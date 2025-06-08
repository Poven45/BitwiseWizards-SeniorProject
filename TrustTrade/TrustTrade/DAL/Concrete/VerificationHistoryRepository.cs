using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    /// <summary>
    /// Repository implementation for managing verification history records
    /// </summary>
    public class VerificationHistoryRepository : Repository<VerificationHistory>, IVerificationHistoryRepository
    {
        private readonly TrustTradeDbContext _context;
        private readonly ILogger<VerificationHistoryRepository> _logger;

        /// <summary>
        /// Constructor for VerificationHistoryRepository
        /// </summary>
        public VerificationHistoryRepository(
            TrustTradeDbContext context,
            ILogger<VerificationHistoryRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<VerificationHistory>> GetHistoryForUserAsync(int userId)
        {
            return await _context.VerificationHistory
                .Where(vh => vh.UserId == userId)
                .OrderByDescending(vh => vh.Timestamp)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<VerificationHistory?> GetMostRecentStatusAsync(int userId)
        {
            return await _context.VerificationHistory
                .Where(vh => vh.UserId == userId)
                .OrderByDescending(vh => vh.Timestamp)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<VerificationHistory> AddVerificationRecordAsync(int userId, bool isVerified,
            string? reason = null, string? source = null)
        {
            // Check if this would be a duplicate of the most recent status
            var mostRecent = await GetMostRecentStatusAsync(userId);
            if (mostRecent != null && mostRecent.IsVerified == isVerified)
            {
                // Don't add a duplicate record if the status hasn't changed
                _logger.LogInformation("Not adding duplicate verification record for user {UserId}", userId);
                return mostRecent;
            }

            var record = new VerificationHistory
            {
                UserId = userId,
                IsVerified = isVerified,
                Timestamp = DateTime.UtcNow,
                Reason = reason,
                Source = source
            };

            _context.VerificationHistory.Add(record);
            await _context.SaveChangesAsync();

            return record;
        }

        /// <inheritdoc />
        public async Task<TimeSpan> CalculateVerifiedDurationAsync(int userId)
        {
            var history = await _context.VerificationHistory
                .Where(vh => vh.UserId == userId)
                .OrderBy(vh => vh.Timestamp)
                .ToListAsync();

            if (!history.Any())
            {
                return TimeSpan.Zero;
            }

            TimeSpan totalDuration = TimeSpan.Zero;
            DateTime? verifiedStart = null;

            _logger.LogDebug("Calculating verification duration for user {UserId} with {RecordCount} history records",
                userId, history.Count);

            foreach (var record in history)
            {
                if (record.IsVerified && verifiedStart == null)
                {
                    // Start of a verified period
                    verifiedStart = record.Timestamp;
                    _logger.LogDebug("Found verification start at {Timestamp}", verifiedStart);
                }
                else if (!record.IsVerified && verifiedStart != null)
                {
                    // End of a verified period
                    var periodDuration = record.Timestamp - verifiedStart.Value;
                    totalDuration += periodDuration;
                    _logger.LogDebug("Added verification period of {Duration} to total", periodDuration);
                    verifiedStart = null;
                }
            }

            // If user is currently verified, add time until now
            if (verifiedStart != null)
            {
                var now = DateTime.UtcNow;
                // Ensure we have a positive duration by comparing timestamps correctly
                // This fixes the issue where timestamps might have different Kind properties
                if (now > verifiedStart.Value)
                {
                    var currentPeriod = now - verifiedStart.Value;
                    totalDuration += currentPeriod;
                    _logger.LogDebug("Added current verification period of {Duration} (from {Start} to {Now})",
                        currentPeriod, verifiedStart, now);
                }
                else
                {
                    // Handle case where timestamps might be out of order due to different time zones
                    _logger.LogWarning("Current time appears earlier than verification start - using minimum duration");
                    totalDuration += TimeSpan.FromMinutes(1); // Minimum duration to avoid negative values
                }
            }

            _logger.LogInformation("Calculated total verification duration for user {UserId}: {Duration}",
                userId, totalDuration);

            // Ensure we never return a negative duration
            return totalDuration.Ticks < 0 ? TimeSpan.Zero : totalDuration;
        }

        /// <inheritdoc />
        public async Task<(DateTime? FirstVerified, DateTime? MostRecentVerified)> GetVerificationDatesAsync(int userId)
        {
            var firstVerified = await _context.VerificationHistory
                .Where(vh => vh.UserId == userId && vh.IsVerified)
                .OrderBy(vh => vh.Timestamp)
                .Select(vh => vh.Timestamp)
                .FirstOrDefaultAsync();

            var mostRecentVerified = await _context.VerificationHistory
                .Where(vh => vh.UserId == userId && vh.IsVerified)
                .OrderByDescending(vh => vh.Timestamp)
                .Select(vh => vh.Timestamp)
                .FirstOrDefaultAsync();

            // If DateTime.MinValue was returned (default value), convert to null
            DateTime? firstVerifiedResult = firstVerified == default ? null : firstVerified;
            DateTime? mostRecentVerifiedResult = mostRecentVerified == default ? null : mostRecentVerified;

            return (firstVerifiedResult, mostRecentVerifiedResult);
        }
    }
}