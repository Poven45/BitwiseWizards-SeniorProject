using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using TrustTrade.Helpers;
using TrustTrade.Services.Web.Interfaces;

namespace TrustTrade.Controllers
{
    public class PostsController : Controller
    {
        private readonly ILogger<PostsController> _logger;
        private readonly IUserService _userService;
        private readonly IHoldingsRepository _holdingsRepository;
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;

        public PostsController(
            ILogger<PostsController> logger,
            IUserService userService,
            IHoldingsRepository holdingsRepository,
            IPostRepository postRepository,
            ITagRepository tagRepository)
        {
            _logger = logger;
            _userService = userService;
            _holdingsRepository = holdingsRepository;
            _postRepository = postRepository;
            _tagRepository = tagRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Retrieve all existing tags for the view model
            var vm = new CreatePostVM
            {
                ExistingTags = await _tagRepository.GetAllTagNamesAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreatePostVM createPostVM)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate the existing tags in case of validation errors
                createPostVM.ExistingTags = await _tagRepository.GetAllTagNamesAsync();
                return View(createPostVM);
            }

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Map the CreatePostVM to the Post entity
            var post = new Post
            {
                UserId = user.Id,
                Title = createPostVM.Title,
                Content = createPostVM.Content,
                IsPublic = createPostVM.IsPublic ?? false, // Should never be null due to validation
                                                           // PortfolioValueAtPosting will be set below if available
            };

            // Only attempt to refresh and calculate portfolio value if Plaid is enabled
            if (user.PlaidEnabled == true)
            {
                try
                {
                    // Refresh the user's holdings using the repository method
                    bool refreshSuccess = await _holdingsRepository.RefreshHoldingsAsync(user.Id);

                    if (refreshSuccess)
                    {
                        // Get the latest holdings after refresh
                        var holdings = await _holdingsRepository.GetHoldingsForUserAsync(user.Id);

                        // Calculate total portfolio value by summing (quantity * current price)
                        decimal totalPortfolioValue = 0;
                        foreach (var holding in holdings)
                        {
                            totalPortfolioValue += holding.Quantity * holding.CurrentPrice;
                        }

                        // Set the portfolio value in the post
                        post.PortfolioValueAtPosting = totalPortfolioValue;

                        // Log the portfolio value for debugging
                        _logger.LogInformation(
                            $"User {user.Username} portfolio value at posting: {totalPortfolioValue:C}");
                    }
                    else
                    {
                        // Log warning if refresh fails but continue with post creation
                        _logger.LogWarning(
                            $"Failed to refresh holdings for user {user.Username} during post creation");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't prevent post creation
                    _logger.LogError(ex, $"Error calculating portfolio value for user {user.Username}");
                }
            }
            else
            {
                _logger.LogInformation(
                    $"User {user.Username} does not have Plaid enabled, skipping portfolio value calculation");
            }

            // Add the selected tags to the post
            foreach (string tagName in createPostVM.SelectedTags)
            {
                Tag? tag = await _tagRepository.FindByTagNameAsync(tagName);
                if (tag == null)
                {
                    // If the tag doesn't exist, create it and add it to the database
                    tag = new Tag { TagName = tagName };
                    await _tagRepository.AddOrUpdateAsync(tag);
                }

                // Add the tag to the post and the post to the tag
                post.Tags.Add(tag);
                tag.Posts.Add(post);
            }

            // Save the post to the database
            await _postRepository.AddOrUpdateAsync(post);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(int id)
        {
            // Retrieve the post from the repository
            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            var isPlaidEnabled = post.User.PlaidEnabled ?? false;
            string? portfolioValue = null;

            // Retreive and format the portfolio value if Plaid is enabled
            if (isPlaidEnabled)
            {
                if (post.PortfolioValueAtPosting.HasValue)
                {
                    portfolioValue = FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(post.PortfolioValueAtPosting.Value);
                }
                else
                {
                    portfolioValue = "$0";
                }
            }

            bool isOwnedByCurrentUser = false;
            bool isLikedByCurrentUser = false;
            User? user = await _userService.GetCurrentUserAsync(User);
            if (user != null && user.Id == post.UserId)
            {
                isOwnedByCurrentUser = true;

                isLikedByCurrentUser = post.Likes.Any(l => l.UserId == user.Id);
            }

            // Map the post to the view model
            var vm = new PostDetailsVM
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Username = post.User.Username,
                TimeAgo = TimeAgoHelper.GetTimeAgo(post.CreatedAt),
                Tags = post.Tags.Select(t => t.TagName).ToList(),
                LikeCount = post.Likes.Count,
                IsLikedByCurrentUser = isLikedByCurrentUser,
                CommentCount = post.Comments.Count,
                IsPlaidEnabled = isPlaidEnabled,
                PortfolioValueAtPosting = portfolioValue,
                IsOwnedByCurrentUser = isOwnedByCurrentUser,
                ProfilePicture = post.User.ProfilePicture
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Ensure the user is the author of the post
            if (post.UserId != user.Id) return Unauthorized();

            var vm = new PostEditVM
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                IsPublic = post.IsPublic,
                AvailableTags = await _tagRepository.GetAllTagNamesAsync(),
                SelectedTags = post.Tags.Select(t => t.TagName).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, PostEditVM postEditVM)
        {
            if (!ModelState.IsValid)
            {
                postEditVM.AvailableTags = await _tagRepository.GetAllTagNamesAsync();
                return View(postEditVM);
            }

            if (id != postEditVM.Id) return BadRequest("Post ID mismatch");

            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Ensure the user is the author of the post
            if (post.UserId != user.Id) return Unauthorized();

            // Update the post with the new values
            post.Title = postEditVM.Title;
            post.Content = postEditVM.Content;
            post.IsPublic = postEditVM.IsPublic;

            // Clear existing post tags and add the new ones
            post.Tags.Clear();
            foreach (string tagName in postEditVM.SelectedTags)
            {
                Tag? tag = await _tagRepository.FindByTagNameAsync(tagName);
                if (tag == null)
                {
                    // If the tag doesn't exist, create it and add it to the database
                    tag = new Tag { TagName = tagName };
                    await _tagRepository.AddOrUpdateAsync(tag);
                }

                post.Tags.Add(tag);
            }

            // Save the updated post
            await _postRepository.AddOrUpdateAsync(post);
            return RedirectToAction("Details", new { id = post.Id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Ensure the user is the author of the post
            if (post.UserId != user.Id) return Unauthorized();

            await _postRepository.DeleteAsync(post);

            return RedirectToAction("Index", "Home");
        }
    }
}