// File: TrustTrade/DAL/Concrete/HoldingsRepository.cs

using Going.Plaid;
using Going.Plaid.Investments;
using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

/// <summary>
/// Repository implementation for managing investment holdings
/// </summary>
public class HoldingsRepository : Repository<InvestmentPosition>, IHoldingsRepository
{
    private readonly TrustTradeDbContext _context;
    private readonly PlaidClient _plaidClient;
    private readonly ILogger<HoldingsRepository> _logger;

    /// <summary>
    /// Constructor for HoldingsRepository
    /// </summary>
    public HoldingsRepository(
        TrustTradeDbContext context,
        PlaidClient plaidClient,
        ILogger<HoldingsRepository> logger) : base(context)
    {
        _context = context;
        _plaidClient = plaidClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<InvestmentPosition>> GetHoldingsForUserAsync(int userId)
    {
        return await _context.InvestmentPositions
            .Include(ip => ip.PlaidConnection)
            .Where(ip => ip.PlaidConnection.UserId == userId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> RefreshHoldingsAsync(int userId)
{
    try
    {
        // Get user's Plaid connections
        var plaidConnections = await _context.PlaidConnections
            .Where(pc => pc.UserId == userId)
            .ToListAsync();

        // Dictionary to store visibility settings before removing holdings
        var visibilitySettings = new Dictionary<string, bool>();

        // Get existing holdings to preserve visibility settings
        var existingHoldings = await _context.InvestmentPositions
            .Include(ip => ip.PlaidConnection)
            .Where(ip => ip.PlaidConnection.UserId == userId)
            .ToListAsync();

        // Store current visibility settings
        foreach (var holding in existingHoldings)
        {
            // Use SecurityId as the unique key
            visibilitySettings[holding.SecurityId] = holding.IsHidden;
        }

        // Remove ALL existing holdings for this user
        _context.InvestmentPositions.RemoveRange(existingHoldings);

        foreach (var connection in plaidConnections)
        {
            // Set access token for this connection
            _plaidClient.AccessToken = connection.AccessToken;

            // Get holdings from Plaid
            var holdingsResponse = await _plaidClient.InvestmentsHoldingsGetAsync(
                new InvestmentsHoldingsGetRequest());

            if (holdingsResponse.Error != null)
            {
                _logger.LogError("Error getting holdings for connection {ConnectionId}: {Error}",
                    connection.Id, holdingsResponse.Error.ErrorMessage);
                continue;
            }

            // Add new holdings
            foreach (var holding in holdingsResponse.Holdings)
            {
                var security = holdingsResponse.Securities
                    .FirstOrDefault(s => s.SecurityId == holding.SecurityId);

                if (security == null) continue;
                if (security.Type == "cryptocurrency") continue;

                var position = new InvestmentPosition
                {
                    PlaidConnectionId = connection.Id,
                    SecurityId = holding.SecurityId,
                    Symbol = security.TickerSymbol ?? security.SecurityId,
                    Quantity = holding.Quantity,
                    CostBasis = holding.CostBasis ?? 0,
                    CurrentPrice = holding.InstitutionPrice,
                    LastUpdated = DateTime.UtcNow,
                    TypeOfSecurity = security.Type,
                    // Apply previous visibility setting if it exists
                    IsHidden = visibilitySettings.ContainsKey(holding.SecurityId) 
                        ? visibilitySettings[holding.SecurityId] 
                        : false
                };

                _context.InvestmentPositions.Add(position);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error refreshing holdings for user {UserId}", userId);
        return false;
    }
}

    /// <inheritdoc />
    public async Task<int> RemoveHoldingsForUserAsync(int userId)
    {
        var holdings = await _context.InvestmentPositions
            .Include(ip => ip.PlaidConnection)
            .Where(ip => ip.PlaidConnection.UserId == userId)
            .ToListAsync();

        _context.InvestmentPositions.RemoveRange(holdings);
        return await _context.SaveChangesAsync();
    }
}