// =============================================================================
// CORRECTED ONBOARDING CHECKLIST MODEL
// File: TPAHRSystem.Core/Models/OnboardingChecklist.cs (Replace existing)
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingChecklist
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public int TemplateId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "ASSIGNED"; // ASSIGNED, IN_PROGRESS, COMPLETED, CANCELLED

        public int AssignedById { get; set; }

        // Enhanced Properties
        public DateTime? StartedDate { get; set; }

        public DateTime? DueDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, URGENT

        public bool IsActive { get; set; } = true;

        public DateTime? CancelledDate { get; set; }

        public int? CancelledById { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime? LastReminderSent { get; set; }

        public int ReminderCount { get; set; } = 0;

        [MaxLength(100)]
        public string? ExternalSystemId { get; set; }

        public bool RequiresManagerApproval { get; set; } = false;

        public bool IsApproved { get; set; } = false;

        public int? ApprovedById { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [MaxLength(500)]
        public string? ApprovalNotes { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? EstimatedHours { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ActualHours { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("TemplateId")]
        public virtual OnboardingTemplate Template { get; set; } = null!;

        [ForeignKey("AssignedById")]
        public virtual Employee AssignedBy { get; set; } = null!;

        [ForeignKey("CancelledById")]
        public virtual Employee? CancelledBy { get; set; }

        [ForeignKey("ApprovedById")]
        public virtual Employee? ApprovedBy { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsCompleted => Status == "COMPLETED" && CompletedDate.HasValue;

        [NotMapped]
        public bool IsInProgress => Status == "IN_PROGRESS";

        [NotMapped]
        public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value && !IsCompleted;

        [NotMapped]
        public int DaysInProgress
        {
            get
            {
                var startDate = StartedDate ?? AssignedDate;
                var endDate = CompletedDate ?? DateTime.UtcNow;
                return Math.Max(0, (int)(endDate - startDate).TotalDays);
            }
        }

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
        public string StatusDisplayName
        {
            get
            {
                return Status switch
                {
                    "ASSIGNED" => "Assigned",
                    "IN_PROGRESS" => "In Progress",
                    "COMPLETED" => "Completed",
                    "CANCELLED" => "Cancelled",
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
                    "URGENT" => "Urgent",
                    _ => Priority
                };
            }
        }

        [NotMapped]
        public bool CanStart => Status == "ASSIGNED" && IsActive;

        [NotMapped]
        public bool CanComplete => Status == "IN_PROGRESS" && IsActive &&
                                  (!RequiresManagerApproval || IsApproved);

        [NotMapped]
        public bool CanCancel => Status != "COMPLETED" && Status != "CANCELLED" && IsActive;

        [NotMapped]
        public decimal EfficiencyRatio
        {
            get
            {
                if (!EstimatedHours.HasValue || EstimatedHours.Value == 0 || !ActualHours.HasValue)
                    return 1.0m;

                return Math.Round(EstimatedHours.Value / ActualHours.Value, 2);
            }
        }

        // Methods
        public void Start()
        {
            if (CanStart)
            {
                Status = "IN_PROGRESS";
                StartedDate = DateTime.UtcNow;
            }
        }

        public void Complete()
        {
            if (CanComplete)
            {
                Status = "COMPLETED";
                CompletedDate = DateTime.UtcNow;
            }
        }

        public void Cancel(string reason, int cancelledById)
        {
            if (CanCancel)
            {
                Status = "CANCELLED";
                CancelledDate = DateTime.UtcNow;
                CancelledById = cancelledById;
                CancellationReason = reason;
                IsActive = false;
            }
        }

        public void Approve(int approvedById, string? notes = null)
        {
            IsApproved = true;
            ApprovedById = approvedById;
            ApprovedDate = DateTime.UtcNow;
            ApprovalNotes = notes;
        }

        public void AddNote(string note)
        {
            if (string.IsNullOrEmpty(Notes))
            {
                Notes = $"{DateTime.UtcNow:yyyy-MM-dd}: {note}";
            }
            else
            {
                Notes += $"\n{DateTime.UtcNow:yyyy-MM-dd}: {note}";
            }
        }

        public void UpdateProgress(string status, decimal? hoursSpent = null)
        {
            Status = status;

            if (hoursSpent.HasValue)
            {
                ActualHours = (ActualHours ?? 0) + hoursSpent.Value;
            }

            if (status == "IN_PROGRESS" && !StartedDate.HasValue)
            {
                StartedDate = DateTime.UtcNow;
            }
            else if (status == "COMPLETED" && !CompletedDate.HasValue)
            {
                CompletedDate = DateTime.UtcNow;
            }
        }

        public void SendReminder()
        {
            LastReminderSent = DateTime.UtcNow;
            ReminderCount++;
        }

        public bool ShouldSendReminder(int reminderIntervalDays = 3)
        {
            if (IsCompleted || Status == "CANCELLED") return false;

            if (!LastReminderSent.HasValue)
            {
                // Send first reminder if overdue or due within 2 days
                return IsOverdue || (DueDate.HasValue && DueDate.Value <= DateTime.UtcNow.AddDays(2));
            }

            return DateTime.UtcNow >= LastReminderSent.Value.AddDays(reminderIntervalDays);
        }

        public OnboardingValidationResult ValidateForCompletion()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (!CanComplete)
            {
                if (Status != "IN_PROGRESS")
                    errors.Add($"Checklist must be in progress to complete (current status: {StatusDisplayName})");

                if (!IsActive)
                    errors.Add("Checklist is not active");

                if (RequiresManagerApproval && !IsApproved)
                    errors.Add("Manager approval required before completion");
            }

            if (IsOverdue)
                warnings.Add($"Checklist is {DaysOverdue} day(s) overdue");

            if (ActualHours.HasValue && EstimatedHours.HasValue && ActualHours.Value > EstimatedHours.Value * 1.5m)
                warnings.Add("Actual time significantly exceeds estimated time");

            return new OnboardingValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }

        public Dictionary<string, object> GetMetrics()
        {
            return new Dictionary<string, object>
            {
                ["Id"] = Id,
                ["Status"] = StatusDisplayName,
                ["Priority"] = PriorityDisplayName,
                ["DaysInProgress"] = DaysInProgress,
                ["DaysOverdue"] = DaysOverdue,
                ["IsOverdue"] = IsOverdue,
                ["EfficiencyRatio"] = EfficiencyRatio,
                ["ReminderCount"] = ReminderCount,
                ["RequiresApproval"] = RequiresManagerApproval,
                ["IsApproved"] = IsApproved,
                ["EstimatedHours"] = EstimatedHours ?? 0,
                ["ActualHours"] = ActualHours ?? 0
            };
        }

        // Static Factory Methods
        public static OnboardingChecklist CreateFromTemplate(OnboardingTemplate template, int employeeId, int assignedById, DateTime? dueDate = null)
        {
            return new OnboardingChecklist
            {
                EmployeeId = employeeId,
                TemplateId = template.Id,
                AssignedById = assignedById,
                AssignedDate = DateTime.UtcNow,
                DueDate = dueDate ?? DateTime.UtcNow.AddDays(template.EstimatedDaysToComplete),
                Priority = "MEDIUM",
                Status = "ASSIGNED",
                IsActive = true,
                RequiresManagerApproval = template.RequiresManagerApproval,
                EstimatedHours = template.EstimatedHoursTotal
            };
        }

        public static OnboardingChecklist CreateUrgent(int employeeId, int templateId, int assignedById, DateTime dueDate)
        {
            return new OnboardingChecklist
            {
                EmployeeId = employeeId,
                TemplateId = templateId,
                AssignedById = assignedById,
                AssignedDate = DateTime.UtcNow,
                DueDate = dueDate,
                Priority = "URGENT",
                Status = "ASSIGNED",
                IsActive = true
            };
        }
    }
}