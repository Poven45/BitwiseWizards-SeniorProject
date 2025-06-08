using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models
{
    public class PortfolioVisibilitySettings
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }

        // Global setting to hide detailed information (quantities, cost basis, etc.)
        public bool HideDetailedInformation { get; set; } = false;

        // Global setting to hide all positions by default
        public bool HideAllPositions { get; set; } = false;

        public DateTime? LastUpdated { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    }
}