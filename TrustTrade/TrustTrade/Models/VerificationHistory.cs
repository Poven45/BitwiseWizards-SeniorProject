using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models
{
    /// <summary>
    /// Represents the history of user verification status changes
    /// </summary>
    public class VerificationHistory
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// The verification status (true = verified, false = unverified)
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// The timestamp when this verification status change occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional reason for the verification status change
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Optional source of the verification (e.g., "Plaid", "Admin", "System")
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Navigation property to the associated user
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}