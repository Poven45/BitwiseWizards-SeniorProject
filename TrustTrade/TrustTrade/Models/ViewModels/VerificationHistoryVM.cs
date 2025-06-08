using System;

namespace TrustTrade.Models.ViewModels
{
    /// <summary>
    /// View model for a single verification history record
    /// </summary>
    public class VerificationHistoryItem
    {
        /// <summary>
        /// Verification status
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// When this status change occurred
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Reason for the status change, if any
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Source of the verification, if any
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Duration since this status change
        /// </summary>
        public string TimeSince => GetTimeSince(Timestamp);

        /// <summary>
        /// Calculates a human-readable time difference from a timestamp to now
        /// </summary>
        private string GetTimeSince(DateTime timestamp)
        {
            TimeSpan diff = DateTime.UtcNow - timestamp;

            if (diff.TotalDays > 365)
            {
                int years = (int)(diff.TotalDays / 365);
                return $"{years} {(years == 1 ? "year" : "years")} ago";
            }
            else if (diff.TotalDays > 30)
            {
                int months = (int)(diff.TotalDays / 30);
                return $"{months} {(months == 1 ? "month" : "months")} ago";
            }
            else if (diff.TotalDays > 1)
            {
                return $"{(int)diff.TotalDays} {((int)diff.TotalDays == 1 ? "day" : "days")} ago";
            }
            else if (diff.TotalHours > 1)
            {
                return $"{(int)diff.TotalHours} {((int)diff.TotalHours == 1 ? "hour" : "hours")} ago";
            }
            else if (diff.TotalMinutes > 1)
            {
                return $"{(int)diff.TotalMinutes} {((int)diff.TotalMinutes == 1 ? "minute" : "minutes")} ago";
            }
            else
            {
                return "Just now";
            }
        }
    }
}