using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrustTrade.ViewModels;
using TrustTrade.DAL.Abstract;
using TrustTrade.Data;
using TrustTrade.Models;

public class MarketRepository : IMarketRepository
{
    private readonly TrustTradeDbContext _context;

    public MarketRepository(TrustTradeDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockViewModel>> GetTopMarketAsync(bool isCrypto)
    {
        if (isCrypto)
        {
            return _mockCryptoData
                .OrderByDescending(c => Math.Abs(c.Change))
                .ToList();
        }

        var stockList = await _context.Stocks
            .OrderByDescending(s => Math.Abs(s.DailyChange))
            .ToListAsync();

        var highsLookup = await _context.StockHistories
            .Where(h => stockList.Select(s => s.TickerSymbol).Contains(h.TickerSymbol))
            .OrderByDescending(h => h.Date)
            .GroupBy(h => h.TickerSymbol)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.OrderByDescending(x => x.Date).Take(7).OrderBy(x => x.Date).Select(x => x.HighPrice).ToList()
            );

        return stockList.Select(s => new StockViewModel
        {
            Ticker = s.TickerSymbol,
            Name = "", // You can expand this if needed
            Price = s.StockPrice,
            Change = s.DailyChange,
            LastUpdated = s.LastUpdated,
            Highs = highsLookup.ContainsKey(s.TickerSymbol) ? highsLookup[s.TickerSymbol] : new List<decimal>()
        }).ToList();
    }

    public async Task<List<StockViewModel>> SearchStocksAsync(string searchTerm, bool isCrypto)
    {
        if (isCrypto)
        {
            return _mockCryptoData
                .Where(c => c.Ticker.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                         || c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
        }

        var matchedStocks = await _context.Stocks
            .Where(s => s.TickerSymbol.Contains(searchTerm))
            .OrderByDescending(s => Math.Abs(s.DailyChange))
            .Take(20)
            .ToListAsync();

        var highsLookup = await _context.StockHistories
            .Where(h => matchedStocks.Select(s => s.TickerSymbol).Contains(h.TickerSymbol))
            .OrderByDescending(h => h.Date)
            .GroupBy(h => h.TickerSymbol)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.OrderByDescending(x => x.Date).Take(7).OrderBy(x => x.Date).Select(x => x.HighPrice).ToList()
            );

        return matchedStocks.Select(s => new StockViewModel
        {
            Ticker = s.TickerSymbol,
            Name = "",
            Price = s.StockPrice,
            Change = s.DailyChange,
            LastUpdated = s.LastUpdated,
            Highs = highsLookup.ContainsKey(s.TickerSymbol) ? highsLookup[s.TickerSymbol] : new List<decimal>()
        }).ToList();
    }

    public async Task<List<(DateTime Date, decimal High, decimal Low)>> GetHighLowHistoryAsync(string ticker, int days = 30)
    {
        return await _context.StockHistories
            .Where(h => h.TickerSymbol == ticker)
            .OrderByDescending(h => h.Date)
            .Take(days)
            .OrderBy(h => h.Date)
            .Select(h => new ValueTuple<DateTime, decimal, decimal>(h.Date, h.HighPrice, h.LowPrice))
            .ToListAsync();
    }

    private static readonly List<StockViewModel> _mockCryptoData = new()
    {
        new StockViewModel { Ticker = "BTC", Name = "Bitcoin", Price = 62205.43m, Change = -432.12m },
        new StockViewModel { Ticker = "ETH", Name = "Ethereum", Price = 2940.20m, Change = +56.70m },
        new StockViewModel { Ticker = "SOL", Name = "Solana", Price = 151.02m, Change = +4.90m },
        new StockViewModel { Ticker = "ADA", Name = "Cardano", Price = 0.64m, Change = -0.05m },
        new StockViewModel { Ticker = "DOGE", Name = "Dogecoin", Price = 0.16m, Change = +0.01m },
        new StockViewModel { Ticker = "XRP", Name = "Ripple", Price = 0.56m, Change = -0.02m },
        new StockViewModel { Ticker = "AVAX", Name = "Avalanche", Price = 36.78m, Change = +1.15m },
        new StockViewModel { Ticker = "MATIC", Name = "Polygon", Price = 0.85m, Change = -0.03m },
        new StockViewModel { Ticker = "LTC", Name = "Litecoin", Price = 91.21m, Change = +2.35m },
        new StockViewModel { Ticker = "BNB", Name = "Binance Coin", Price = 589.74m, Change = +12.64m }
    };
}
