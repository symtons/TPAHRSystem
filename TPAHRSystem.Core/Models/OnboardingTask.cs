// =============================================================================
// OnboardingTask Model
// File: TPAHRSystem.Core/Models/OnboardingTask.cs
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // ORIENTATION, DOCUMENTATION, FINANCIAL, etc.

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, IN_PROGRESS, COMPLETED, OVERDUE

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH

        public DateTime? DueDate { get; set; }

        [MaxLength(100)]
        public string EstimatedTime { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Instructions { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;

        public bool IsTemplate { get; set; } = false;

        // Foreign Keys
        public int? EmployeeId { get; set; }
        public int? TemplateId { get; set; }
        public int? AssignedById { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("TemplateId")]
        public virtual OnboardingTemplate? Template { get; set; }

        [ForeignKey("AssignedById")]
        public virtual Employee? AssignedBy { get; set; }

        public virtual ICollection<OnboardingDocument> Documents { get; set; } = new List<OnboardingDocument>();
    }
}