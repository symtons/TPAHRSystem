// =============================================================================
// CORRECTED ONBOARDING TASK MODEL
// File: TPAHRSystem.Core/Models/OnboardingTask.cs (Replace existing)
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

        // Enhanced Properties
        public DateTime? StartedDate { get; set; }

        [MaxLength(50)]
        public string? CompletedByRole { get; set; } // Track who completed it

        public int? CompletedById { get; set; }

        [Column(TypeName = "decimal(3,1)")]
        public decimal? ActualTimeSpent { get; set; } // Hours spent on task

        public bool RequiresApproval { get; set; } = false;

        public bool IsApproved { get; set; } = false;

        public int? ApprovedById { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [MaxLength(500)]
        public string? ApprovalNotes { get; set; }

        [MaxLength(100)]
        public string? ExternalSystemId { get; set; } // For integration

        public int SortOrder { get; set; } = 0;

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("TemplateId")]
        public virtual OnboardingTemplate? Template { get; set; }

        [ForeignKey("AssignedById")]
        public virtual Employee? AssignedBy { get; set; }

        [ForeignKey("CompletedById")]
        public virtual Employee? CompletedBy { get; set; }

        [ForeignKey("ApprovedById")]
        public virtual Employee? ApprovedBy { get; set; }

        public virtual ICollection<OnboardingDocument> Documents { get; set; } = new List<OnboardingDocument>();

        // Enhanced Computed Properties
        [NotMapped]
        public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value && Status != "COMPLETED";

        [NotMapped]
        public int DaysOverdue
        {
            get
            {
                if (!IsOverdue) return 0;
                return (int)(DateTime.UtcNow - DueDate!.Value).TotalDays;
            }
        }

        [NotMapped]
        public bool IsStarted => StartedDate.HasValue || Status == "IN_PROGRESS";

        [NotMapped]
        public bool IsCompleted => Status == "COMPLETED" && CompletedDate.HasValue;

        [NotMapped]
        public string StatusDisplayName
        {
            get
            {
                return Status switch
                {
                    "PENDING" => "Pending",
                    "IN_PROGRESS" => "In Progress",
                    "COMPLETED" => "Completed",
                    "OVERDUE" => "Overdue",
                    _ => Status
                };
            }
        }

        [NotMapped]
        public string PriorityDisplayName
        {
            get
            {
                return Priority switch
                {
                    "LOW" => "Low",
                    "MEDIUM" => "Medium",
                    "HIGH" => "High",
                    _ => Priority
                };
            }
        }

        [NotMapped]
        public TimeSpan? EstimatedTimeSpan
        {
            get
            {
                if (string.IsNullOrEmpty(EstimatedTime)) return null;

                // Parse common time formats
                var timeStr = EstimatedTime.ToLower();

                if (timeStr.Contains("hour"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(timeStr, @"(\d+(?:\.\d+)?)\s*hours?");
                    if (match.Success && double.TryParse(match.Groups[1].Value, out var hours))
                        return TimeSpan.FromHours(hours);
                }

                if (timeStr.Contains("minute"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(timeStr, @"(\d+)\s*minutes?");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var minutes))
                        return TimeSpan.FromMinutes(minutes);
                }

                if (timeStr.Contains("day"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(timeStr, @"(\d+(?:\.\d+)?)\s*days?");
                    if (match.Success && double.TryParse(match.Groups[1].Value, out var days))
                        return TimeSpan.FromDays(days);
                }

                return null;
            }
        }

        [NotMapped]
        public bool HasRequiredDocuments => Documents.Any(d => d.Required);

        [NotMapped]
        public bool AllRequiredDocumentsUploaded =>
            !HasRequiredDocuments || Documents.Where(d => d.Required).All(d => d.Uploaded);

        [NotMapped]
        public int RequiredDocumentCount => Documents.Count(d => d.Required);

        [NotMapped]
        public int UploadedDocumentCount => Documents.Count(d => d.Required && d.Uploaded);

        // Methods
        public void MarkAsStarted()
        {
            if (Status == "PENDING")
            {
                Status = "IN_PROGRESS";
                StartedDate = DateTime.UtcNow;
            }
        }

        public void MarkAsCompleted(int? completedById = null, string? completedByRole = null)
        {
            Status = "COMPLETED";
            CompletedDate = DateTime.UtcNow;
            CompletedById = completedById;
            CompletedByRole = completedByRole;
        }

        public void UpdateProgress(string newStatus, string? notes = null)
        {
            Status = newStatus;

            if (!string.IsNullOrEmpty(notes))
            {
                Notes = string.IsNullOrEmpty(Notes)
                    ? $"{DateTime.UtcNow:yyyy-MM-dd}: {notes}"
                    : $"{Notes}\n{DateTime.UtcNow:yyyy-MM-dd}: {notes}";
            }

            if (newStatus == "IN_PROGRESS" && !StartedDate.HasValue)
            {
                StartedDate = DateTime.UtcNow;
            }
            else if (newStatus == "COMPLETED" && !CompletedDate.HasValue)
            {
                CompletedDate = DateTime.UtcNow;
            }
        }

        public OnboardingValidationResult ValidateForCompletion()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (RequiresApproval && !IsApproved)
            {
                errors.Add("Task requires approval before completion");
            }

            if (HasRequiredDocuments && !AllRequiredDocumentsUploaded)
            {
                var missingDocs = Documents
                    .Where(d => d.Required && !d.Uploaded)
                    .Select(d => d.Name)
                    .ToList();

                errors.Add($"Required documents missing: {string.Join(", ", missingDocs)}");
            }

            if (IsOverdue)
            {
                warnings.Add($"Task is {DaysOverdue} day(s) overdue");
            }

            return new OnboardingValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }

        public bool ShouldSendReminderNotification(int reminderDaysBefore = 2)
        {
            if (Status != "PENDING" || !DueDate.HasValue) return false;

            var reminderDate = DueDate.Value.AddDays(-reminderDaysBefore);
            var now = DateTime.UtcNow;

            return now.Date >= reminderDate.Date && now.Date < DueDate.Value.Date;
        }

        public string GenerateInstructions()
        {
            if (!string.IsNullOrEmpty(Instructions)) return Instructions;

            // Generate default instructions based on category
            return OnboardingConstants.BusinessLogic.GetDefaultInstructionsByCategory(Category);
        }

        // Static Factory Methods
        public static OnboardingTask CreateFromTemplate(OnboardingTask template, int employeeId, int assignedById)
        {
            return new OnboardingTask
            {
                Title = template.Title,
                Description = template.Description,
                Category = template.Category,
                Priority = template.Priority,
                EstimatedTime = template.EstimatedTime,
                Instructions = template.Instructions,
                RequiresApproval = template.RequiresApproval,
                SortOrder = template.SortOrder,
                EmployeeId = employeeId,
                TemplateId = template.TemplateId ?? template.Id,
                AssignedById = assignedById,
                Status = "PENDING",
                CreatedDate = DateTime.UtcNow,
                IsTemplate = false
            };
        }

        public static OnboardingTask CreateStandardTask(string title, string category, string priority = "MEDIUM")
        {
            return new OnboardingTask
            {
                Title = title,
                Category = category,
                Priority = priority,
                Status = "PENDING",
                CreatedDate = DateTime.UtcNow,
                Instructions = OnboardingConstants.BusinessLogic.GetDefaultInstructionsByCategory(category)
            };
        }
    }
}