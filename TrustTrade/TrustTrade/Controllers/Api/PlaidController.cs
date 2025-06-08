using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Institutions;
using Going.Plaid.Link;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.Controllers.Api;

/// <summary>
/// Response model for Link token creation
/// </summary>
public class LinkTokenResponse
{
    /// <summary>
    /// The Plaid Link token used to initialize Link
    /// </summary>
    public string LinkToken { get; set; } = string.Empty;
}

/// <summary>
/// Controller for handling Plaid Link integration and brokerage connections
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlaidController : ControllerBase
{
    private readonly PlaidClient _plaidClient;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly TrustTradeDbContext _dbContext;
    private readonly ILogger<PlaidController> _logger;
    private readonly IVerificationHistoryRepository _verificationHistoryRepository;

    /// <summary>
    /// Constructor for PlaidController
    /// </summary>
    public PlaidController(
        PlaidClient plaidClient,
        UserManager<IdentityUser> userManager,
        TrustTradeDbContext dbContext,
        ILogger<PlaidController> logger,
        IVerificationHistoryRepository verificationHistoryRepository
    )
    {
        _plaidClient = plaidClient;
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
        _verificationHistoryRepository = verificationHistoryRepository;
    }

    /// <summary>
    /// Creates a Link token for initializing Plaid Link
    /// </summary>
    /// <returns>JSON response containing the link token</returns>
    [HttpPost("create-link-token")]
    public async Task<ActionResult<LinkTokenResponse>> CreateLinkToken()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var request = new LinkTokenCreateRequest
            {
                User = new Going.Plaid.Entity.LinkTokenCreateRequestUser
                {
                    ClientUserId = user.Id
                },
                ClientName = "TrustTrade",
                Products = new[] { Going.Plaid.Entity.Products.Investments },
                CountryCodes = new[] { Going.Plaid.Entity.CountryCode.Us },
                Language = Going.Plaid.Entity.Language.English,
                Investments = new Going.Plaid.Entity.LinkTokenInvestments
                {
                    AllowManualEntry = true
                }
            };

            var response = await _plaidClient.LinkTokenCreateAsync(request);
            return Ok(new LinkTokenResponse { LinkToken = response.LinkToken });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Link token");
            return StatusCode(500, new { error = "Failed to create Link token" });
        }
    }

    /// <summary>
    /// Handles the exchange of public token after successful Plaid Link flow
    /// </summary>
    /// <param name="publicToken">The public token from Plaid Link</param>
    /// <returns>Success or error response</returns>
    /// <summary>
    /// Request model for public token exchange
    /// </summary>
    public class ExchangePublicTokenRequest
    {
        /// <summary>
        /// The public token received from Plaid Link
        /// </summary>
        public string PublicToken { get; set; } = string.Empty;
    }

    [HttpPost("exchange-public-token")]
    public async Task<IActionResult> ExchangePublicToken([FromBody] ExchangePublicTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Starting public token exchange process");

            // Input validation
            if (request == null || string.IsNullOrEmpty(request.PublicToken))
            {
                _logger.LogError("Null or empty public token received");
                return BadRequest(new { error = "Invalid public token" });
            }

            // User validation
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("Unable to resolve identity user");
                return Unauthorized(new { error = "User authentication failed" });
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError("User has null or empty email: {UserId}", user.Id);
                return StatusCode(500, new { error = "Invalid user account" });
            }

            _logger.LogInformation("Processing token exchange for user: {Email}", user.Email);

            // Get TrustTrade user
            var trustTradeUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email);

            if (trustTradeUser == null)
            {
                _logger.LogError("TrustTrade user not found for {Email}", user.Email);
                return StatusCode(500, new { error = "User record not found" });
            }

            _logger.LogInformation("Found TrustTrade user ID: {UserId}", trustTradeUser.Id);

            // Step 1: Exchange the public token
            _logger.LogInformation("Exchanging public token with Plaid");
            Going.Plaid.Item.ItemPublicTokenExchangeResponse exchangeResponse;
            try
            {
                exchangeResponse = await _plaidClient.ItemPublicTokenExchangeAsync(
                    new Going.Plaid.Item.ItemPublicTokenExchangeRequest
                    {
                        PublicToken = request.PublicToken
                    });

                if (exchangeResponse == null)
                {
                    _logger.LogError("Null response from Plaid token exchange");
                    return StatusCode(500, new { error = "Plaid token exchange failed" });
                }

                if (string.IsNullOrEmpty(exchangeResponse.AccessToken))
                {
                    _logger.LogError("Empty access token in Plaid response");
                    return StatusCode(500, new { error = "Invalid Plaid response" });
                }

                _logger.LogInformation("Successfully exchanged token, received access token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Plaid token exchange");
                return StatusCode(500, new { error = "Plaid API error", details = ex.Message });
            }

            // Step 2: Get item details
            _logger.LogInformation("Retrieving item details from Plaid");
            Going.Plaid.Item.ItemGetResponse itemResponse;
            try
            {
                itemResponse = await _plaidClient.ItemGetAsync(
                    new Going.Plaid.Item.ItemGetRequest
                    {
                        AccessToken = exchangeResponse.AccessToken
                    });

                if (itemResponse == null || itemResponse.Item == null)
                {
                    _logger.LogError("Null item response from Plaid");
                    return StatusCode(500, new { error = "Plaid item retrieval failed" });
                }

                if (string.IsNullOrEmpty(itemResponse.Item.InstitutionId))
                {
                    _logger.LogError("Empty institution ID in Plaid item response");
                    return StatusCode(500, new { error = "Invalid Plaid item data" });
                }

                _logger.LogInformation("Successfully retrieved item for institution: {InstitutionId}",
                    itemResponse.Item.InstitutionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item details from Plaid");
                return StatusCode(500, new { error = "Plaid API error", details = ex.Message });
            }

            // Step 3: Get institution details
            _logger.LogInformation("Retrieving institution details from Plaid");
            Going.Plaid.Institutions.InstitutionsGetByIdResponse institutionResponse;
            string institutionName = "Unknown Institution"; // Default fallback

            try
            {
                institutionResponse = await _plaidClient.InstitutionsGetByIdAsync(
                    new Going.Plaid.Institutions.InstitutionsGetByIdRequest
                    {
                        InstitutionId = itemResponse.Item.InstitutionId,
                        CountryCodes = new[] { Going.Plaid.Entity.CountryCode.Us }
                    });

                if (institutionResponse?.Institution?.Name != null)
                {
                    institutionName = institutionResponse.Institution.Name;
                    _logger.LogInformation("Successfully retrieved institution: {InstitutionName}",
                        institutionName);
                }
            }
            catch (Exception ex)
            {
                // Log but continue - this is a non-critical error
                _logger.LogWarning(ex, "Unable to retrieve institution details from Plaid, using fallback name");

                // Try to extract institution name from the Item if possible
                if (!string.IsNullOrEmpty(itemResponse?.Item?.InstitutionId))
                {
                    // For "ins_109508", extract "109508" and create a readable name
                    var idParts = itemResponse.Item.InstitutionId.Split('_');
                    if (idParts.Length > 1)
                    {
                        institutionName = $"Institution {idParts[1]}";
                    }
                }
            }

            // Step 4: Save data to database
            _logger.LogInformation("Saving Plaid connection to database");

            // Use execution strategy to handle SQL retries properly
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                // Use transaction within execution strategy
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Create and save connection
                        var plaidConnection = new PlaidConnection
                        {
                            UserId = trustTradeUser.Id,
                            AccessToken = exchangeResponse.AccessToken,
                            ItemId = exchangeResponse.ItemId,
                            InstitutionId = itemResponse.Item.InstitutionId,
                            InstitutionName = institutionName,
                            LastSyncTimestamp = DateTime.UtcNow
                        };

                        _dbContext.PlaidConnections.Add(plaidConnection);

                        // Update user Plaid status
                        trustTradeUser.PlaidEnabled = true;
                        trustTradeUser.PlaidStatus = "Connected";
                        trustTradeUser.LastPlaidSync = DateTime.UtcNow;

                        // Handle verification status
                        bool wasVerified = trustTradeUser.IsVerified ?? false;
                        trustTradeUser.IsVerified = true;

                        // Save changes before attempting to add verification record
                        await _dbContext.SaveChangesAsync();

                        // Record verification history if status changed and repository exists
                        if (!wasVerified && _verificationHistoryRepository != null)
                        {
                            try
                            {
                                _logger.LogInformation("Adding verification record for user {UserId}",
                                    trustTradeUser.Id);

                                await _verificationHistoryRepository.AddVerificationRecordAsync(
                                    trustTradeUser.Id,
                                    true,
                                    "Plaid connection established",
                                    "Plaid");
                            }
                            catch (Exception ex)
                            {
                                // Log but don't fail if verification history update fails
                                _logger.LogError(ex, "Error adding verification record, continuing with connection");
                            }
                        }

                        // Commit transaction
                        await transaction.CommitAsync();

                        _logger.LogInformation("Successfully completed Plaid connection for user {UserId}",
                            trustTradeUser.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Database error during Plaid connection");
                        throw; // Re-throw to be caught by outer execution strategy
                    }
                }
            });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            string details = ex.Message;
            string innerDetails = ex.InnerException?.Message ?? "No inner exception";
            string fullStack = ex.ToString();

            _logger.LogError(ex, "Error in token exchange. Details: {Details}, Inner: {Inner}, Stack: {Stack}",
                details, innerDetails, fullStack);

            return StatusCode(500, new
            {
                error = "Failed to exchange public token",
                details = details,
                innerDetails = innerDetails,
                fullStackTrace = fullStack.Length > 500 ? fullStack.Substring(0, 500) + "..." : fullStack
            });
        }
    }
}