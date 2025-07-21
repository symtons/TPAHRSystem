// =============================================================================
// ONBOARDING TEMPLATE MODEL - MISSING CLASS
// File: TPAHRSystem.Core/Models/OnboardingTemplate.cs
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ForRole { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ForDepartment { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        public int CreatedById { get; set; }

        public int? ModifiedById { get; set; }

        // Enhanced Properties
        [MaxLength(20)]
        public string TemplateVersion { get; set; } = "1.0";

        [MaxLength(100)]
        public string? EmployeeType { get; set; } // FULL_TIME, PART_TIME, CONTRACT, INTERN

        [MaxLength(100)]
        public string? Location { get; set; }

        public int EstimatedDaysToComplete { get; set; } = 14;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? EstimatedHoursTotal { get; set; }

        public bool RequiresManagerApproval { get; set; } = false;

        public bool AutoAssign { get; set; } = false;

        [MaxLength(500)]
        public string? Prerequisites { get; set; }

        [MaxLength(500)]
        public string? SuccessCriteria { get; set; }

        public int Priority { get; set; } = 1; // For auto-assignment ordering

        public int UsageCount { get; set; } = 0; // Track how many times used

        public DateTime? LastUsedDate { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; } // Comma-separated tags for categorization

        [MaxLength(100)]
        public string? ExternalSystemId { get; set; } // For integration

        public bool IsSystemTemplate { get; set; } = false; // Cannot be deleted

        // Navigation Properties
        [ForeignKey("CreatedById")]
        public virtual Employee CreatedBy { get; set; } = null!;

        [ForeignKey("ModifiedById")]
        public virtual Employee? ModifiedBy { get; set; }

        // Related collections
        public virtual ICollection<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();

        public virtual ICollection<OnboardingChecklist> Checklists { get; set; } = new List<OnboardingChecklist>();

        // Computed Properties
        [NotMapped]
        public string DisplayName => $"{Name} (v{TemplateVersion})";

        [NotMapped]
        public bool CanBeDeleted => !IsSystemTemplate && UsageCount == 0;

        [NotMapped]
        public string StatusSummary => IsActive ? "Active" : "Inactive";

        [NotMapped]
        public int DaysSinceCreated => (int)(DateTime.UtcNow - CreatedDate).TotalDays;

        [NotMapped]
        public string UsageSummary => $"Used {UsageCount} times" +
            (LastUsedDate.HasValue ? $", last used {LastUsedDate.Value:MMM dd, yyyy}" : ", never used");
    }
}