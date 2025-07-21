// =============================================================================
// ONBOARDING TASK TEMPLATE MODEL
// File: TPAHRSystem.Core/Models/OnboardingTaskTemplate.cs
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingTaskTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // ORIENTATION, DOCUMENTATION, TRAINING, SETUP, MEETING, REVIEW

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, URGENT

        [Required]
        [Range(1, 365)]
        public int EstimatedDays { get; set; } = 1;

        [MaxLength(2000)]
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// Whether employees can complete this task themselves (true) or if only HR can complete it (false)
        /// </summary>
        public bool CanEmployeeComplete { get; set; } = false;

        /// <summary>
        /// Whether this task blocks system access until completed
        /// </summary>
        public bool BlocksSystemAccess { get; set; } = false;

        [Required]
        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }

        public int? CreatedById { get; set; }

        public int? ModifiedById { get; set; }

        // Additional Properties
        [MaxLength(100)]
        public string? TemplateVersion { get; set; } = "1.0";

        [MaxLength(500)]
        public string? Prerequisites { get; set; }

        [MaxLength(500)]
        public string? CompletionCriteria { get; set; }

        public bool RequiresApproval { get; set; } = false;

        public bool IsSystemTemplate { get; set; } = false; // Cannot be deleted if true

        [MaxLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

        [Column(TypeName = "decimal(5,2)")]
        public decimal? EstimatedHours { get; set; }

        /// <summary>
        /// Whether this task requires document uploads
        /// </summary>
        public bool RequiresDocuments { get; set; } = false;

        /// <summary>
        /// Number of documents required for this task
        /// </summary>
        public int RequiredDocumentCount { get; set; } = 0;

        /// <summary>
        /// Whether this task can be completed remotely
        /// </summary>
        public bool CanCompleteRemotely { get; set; } = true;

        /// <summary>
        /// Whether this task requires in-person presence
        /// </summary>
        public bool RequiresInPersonPresence { get; set; } = false;

        /// <summary>
        /// Specific role/position this task applies to
        /// </summary>
        [MaxLength(100)]
        public string? ApplicableRole { get; set; }

        /// <summary>
        /// Employment type this task applies to (FULL_TIME, PART_TIME, CONTRACT, INTERN)
        /// </summary>
        [MaxLength(50)]
        public string? ApplicableEmploymentType { get; set; }

        /// <summary>
        /// Work location this task applies to (OFFICE, REMOTE, HYBRID, FIELD)
        /// </summary>
        [MaxLength(50)]
        public string? ApplicableWorkLocation { get; set; }

        // Notification Properties
        public bool SendReminderNotifications { get; set; } = true;

        public int? ReminderDaysBefore { get; set; } = 1; // Days before due date to send reminder

        public bool NotifyManagerOnCompletion { get; set; } = false;

        public bool NotifyHROnCompletion { get; set; } = true;

        // Navigation Properties
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("CreatedById")]
        public virtual Employee? CreatedBy { get; set; }

        [ForeignKey("ModifiedById")]
        public virtual Employee? ModifiedBy { get; set; }

        // Related collections
        public virtual ICollection<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();

        // Computed Properties
        [NotMapped]
        public string DisplayName => $"{Title} ({Department?.Name ?? "Unknown Dept"})";

        [NotMapped]
        public string PriorityDisplay => Priority?.ToLower() switch
        {
            "low" => "🟢 Low",
            "medium" => "🟡 Medium",
            "high" => "🟠 High",
            "urgent" => "🔴 Urgent",
            _ => Priority ?? "Medium"
        };

        [NotMapped]
        public string CategoryDisplay => Category?.ToLower() switch
        {
            "orientation" => "🎯 Orientation",
            "documentation" => "📄 Documentation",
            "training" => "📚 Training",
            "setup" => "⚙️ Setup",
            "meeting" => "👥 Meeting",
            "review" => "🔍 Review",
            _ => Category ?? "General"
        };

        [NotMapped]
        public bool IsOverdue => EstimatedDays > 0 && CreatedDate.AddDays(EstimatedDays) < DateTime.UtcNow;

        [NotMapped]
        public string TimeEstimate
        {
            get
            {
                if (EstimatedHours.HasValue)
                {
                    var hours = EstimatedHours.Value;
                    if (hours < 1)
                        return $"{hours * 60:F0} minutes";
                    else if (hours < 8)
                        return $"{hours:F1} hours";
                    else
                        return $"{hours / 8:F1} days";
                }
                return $"{EstimatedDays} day(s)";
            }
        }

        [NotMapped]
        public string CompletionMethod => CanEmployeeComplete ?
            "👤 Employee can complete" : "👥 HR completion required";

        [NotMapped]
        public string AccessImpact => BlocksSystemAccess ?
            "🚫 Blocks system access" : "✅ No access restriction";

        [NotMapped]
        public string DocumentRequirement => RequiresDocuments ?
            $"📎 Requires {RequiredDocumentCount} document(s)" : "📄 No documents required";

        [NotMapped]
        public string LocationRequirement => RequiresInPersonPresence ?
            "🏢 In-person required" : (CanCompleteRemotely ? "🏠 Can complete remotely" : "🏢 Office preferred");

        [NotMapped]
        public bool CanBeDeleted => !IsSystemTemplate && !Tasks.Any();

        [NotMapped]
        public int UsageCount => Tasks.Count;

        [NotMapped]
        public string UsageSummary => $"Used {UsageCount} times";

        [NotMapped]
        public string ApplicabilitySummary
        {
            get
            {
                var parts = new List<string>();

                if (!string.IsNullOrEmpty(ApplicableRole))
                    parts.Add($"Role: {ApplicableRole}");

                if (!string.IsNullOrEmpty(ApplicableEmploymentType))
                    parts.Add($"Type: {ApplicableEmploymentType}");

                if (!string.IsNullOrEmpty(ApplicableWorkLocation))
                    parts.Add($"Location: {ApplicableWorkLocation}");

                return parts.Any() ? string.Join(", ", parts) : "All employees";
            }
        }

        [NotMapped]
        public string NotificationSummary => SendReminderNotifications ?
            $"Reminder {ReminderDaysBefore} day(s) before due" : "No reminders";

        [NotMapped]
        public List<string> RequirementsList
        {
            get
            {
                var requirements = new List<string>();

                if (RequiresApproval) requirements.Add("Requires approval");
                if (RequiresDocuments) requirements.Add($"{RequiredDocumentCount} document(s)");
                if (RequiresInPersonPresence) requirements.Add("In-person presence");
                if (BlocksSystemAccess) requirements.Add("Blocks system access");

                return requirements;
            }
        }

        [NotMapped]
        public bool HasSpecialRequirements => RequirementsList.Any();
    }
}