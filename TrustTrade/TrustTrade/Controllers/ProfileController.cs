using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.ViewModels;

namespace TrustTrade.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly TrustTradeDbContext _context;
        private readonly IHoldingsRepository _holdingsRepository;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            TrustTradeDbContext context,
            IHoldingsRepository holdingsRepository,
            ILogger<ProfileController> logger)
        {
            _context = context;
            _holdingsRepository = holdingsRepository;
            _logger = logger;
        }

        // route to get to logged in users profile "/Profile"
        // This is the method for accessing the owner's profile
        [HttpGet("/Profile")]
        public async Task<IActionResult> MyProfile()
        {
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.FollowerFollowerUsers)
                .Include(u => u.FollowerFollowingUsers)
                .FirstOrDefaultAsync(u => u.IdentityId == identityId);

            if (user == null)
            {
                return NotFound();
            }

            var holdings = await _holdingsRepository.GetHoldingsForUserAsync(user.Id);
            var holdingViewModels = holdings.Select(h => new HoldingViewModel
            {
                Symbol = h.Symbol,
                Quantity = h.Quantity,
                CurrentPrice = h.CurrentPrice,
                CostBasis = h.CostBasis,
                Institution = h.PlaidConnection.InstitutionName,
                TypeOfSecurity = h.TypeOfSecurity
            }).ToList();

            var model = new ProfileViewModel
            {
                IdentityId = user.IdentityId,
                ProfileName = user.ProfileName,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                IsVerified = user.IsVerified ?? false,
                PlaidEnabled = user.PlaidEnabled ?? false,
                LastPlaidSync = user.LastPlaidSync,
                FollowersCount = user.FollowerFollowerUsers?.Count ?? 0,
                FollowingCount = user.FollowerFollowingUsers?.Count ?? 0,
                Followers = user.FollowerFollowerUsers?.Select(f => f.FollowerUser.ProfileName).ToList() ?? new List<string>(),
                Following = user.FollowerFollowingUsers?.Select(f => f.FollowingUser.ProfileName).ToList() ?? new List<string>(),
                Holdings = holdingViewModels,
                LastHoldingsUpdate = holdings.Any() ? holdings.Max(h => h.LastUpdated) : null,
                UserTag = user.UserTag
            };

            return View("Profile",model);
        }

        // In order to access the profile of a user, use the route below
        // This is the method for accessing a non-owners profile
        [AllowAnonymous]
        [HttpGet("/Profile/User/{username}", Name = "UserProfileRoute")]
        public async Task<IActionResult> UserProfile(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(MyProfile));
            }

            var user = await _context.Users
                .Include(u => u.FollowerFollowerUsers)
                .Include(u => u.FollowerFollowingUsers)
                .FirstOrDefaultAsync(u => u.ProfileName == username);

            if (user == null)
            {
                return NotFound();
            }

            var holdings = await _holdingsRepository.GetHoldingsForUserAsync(user.Id);
            var holdingViewModels = holdings.Select(h => new HoldingViewModel
            {
                Symbol = h.Symbol,
                Quantity = h.Quantity,
                CurrentPrice = h.CurrentPrice,
                CostBasis = h.CostBasis,
                Institution = h.PlaidConnection.InstitutionName,
                TypeOfSecurity = h.TypeOfSecurity
            }).ToList();

            var model = new ProfileViewModel
            {
                IdentityId = user.IdentityId,
                ProfileName = user.ProfileName,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                IsVerified = user.IsVerified ?? false,
                PlaidEnabled = user.PlaidEnabled ?? false,
                LastPlaidSync = user.LastPlaidSync,
                FollowersCount = user.FollowerFollowerUsers?.Count ?? 0,
                FollowingCount = user.FollowerFollowingUsers?.Count ?? 0,
                Followers = user.FollowerFollowerUsers?.Select(f => f.FollowerUser.ProfileName).ToList() ?? new List<string>(),
                Following = user.FollowerFollowingUsers?.Select(f => f.FollowingUser.ProfileName).ToList() ?? new List<string>(),
                Holdings = holdingViewModels,
                LastHoldingsUpdate = holdings.Any() ? holdings.Max(h => h.LastUpdated) : null,
                UserTag = user.UserTag
            };

            return View("Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> RefreshHoldings()
        {
            try
            {
                var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(identityId))
                {
                    return Unauthorized();
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.IdentityId == identityId);

                if (user == null)
                {
                    return NotFound();
                }

                var success = await _holdingsRepository.RefreshHoldingsAsync(user.Id);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to refresh holdings" });
                }

                return RedirectToAction(nameof(MyProfile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing holdings");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Updates the user's profile information
        /// </summary>
        /// <param name="bio">The user's updated bio</param>
        /// <param name="userTag">The user's selected trading preference</param>
        /// <returns>Redirects to Profile page</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string bio, string userTag)
        {
            try
            {
                var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(identityId))
                {
                    return Unauthorized();
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.IdentityId == identityId);

                if (user == null)
                {
                    return NotFound();
                }

                user.Bio = bio;
                user.UserTag = userTag;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyProfile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }
    }
}
