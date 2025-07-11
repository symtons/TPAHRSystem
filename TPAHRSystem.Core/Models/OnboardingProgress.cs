// =============================================================================
// OnboardingProgress Model
// File: TPAHRSystem.Core/Models/OnboardingProgress.cs
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingProgress
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal CompletionPercentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "IN_PROGRESS"; // NOT_STARTED, IN_PROGRESS, COMPLETED

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}