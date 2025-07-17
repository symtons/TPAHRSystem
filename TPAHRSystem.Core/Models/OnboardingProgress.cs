// =============================================================================
// CORRECTED ONBOARDING PROGRESS MODEL
// File: TPAHRSystem.Core/Models/OnboardingProgress.cs (Replace existing)
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public int TotalTasks { get; set; } = 0;
        public int CompletedTasks { get; set; } = 0;
        public int PendingTasks { get; set; } = 0;
        public int OverdueTasks { get; set; } = 0;
        public int InProgressTasks { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal CompletionPercentage { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletionDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "NOT_STARTED"; // NOT_STARTED, IN_PROGRESS, COMPLETED

        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Enhanced Properties
        public DateTime? FirstTaskStartedDate { get; set; }

        public DateTime? LastTaskCompletedDate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? EstimatedHoursTotal { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ActualHoursSpent { get; set; }

        public int TasksRequiringApproval { get; set; } = 0;

        public int TasksApproved { get; set; } = 0;

        public int DocumentsRequired { get; set; } = 0;

        public int DocumentsUploaded { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsOnTrack { get; set; } = true;

        [MaxLength(100)]
        public string? CurrentPhase { get; set; } // e.g., "Orientation", "Documentation", "Training"

        public DateTime? ExpectedCompletionDate { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        // Enhanced Computed Properties
        [NotMapped]
        public int DaysInProgress
        {
            get
            {
                var endDate = CompletionDate ?? DateTime.UtcNow;
                return Math.Max(0, (int)(endDate - StartDate).TotalDays);
            }
        }

        [NotMapped]
        public bool IsCompleted => Status == "COMPLETED" && CompletionDate.HasValue;

        [NotMapped]
        public bool HasStarted => FirstTaskStartedDate.HasValue || Status != "NOT_STARTED";

        [NotMapped]
        public bool HasOverdueTasks => OverdueTasks > 0;

        [NotMapped]
        public decimal DocumentCompletionRate
        {
            get
            {
                return DocumentsRequired > 0 ?
                    Math.Round((decimal)DocumentsUploaded / DocumentsRequired * 100, 2) : 100;
            }
        }

        [NotMapped]
        public decimal ApprovalCompletionRate
        {
            get
            {
                return TasksRequiringApproval > 0 ?
                    Math.Round((decimal)TasksApproved / TasksRequiringApproval * 100, 2) : 100;
            }
        }

        [NotMapped]
        public string RiskLevel
        {
            get
            {
                if (IsCompleted) return "COMPLETED";

                var riskFactors = 0;

                // Check for overdue tasks
                if (OverdueTasks > 0) riskFactors += 3;

                // Check if behind schedule
                if (ExpectedCompletionDate.HasValue && DateTime.UtcNow > ExpectedCompletionDate.Value) riskFactors += 2;

                // Check completion rate vs time spent
                if (DaysInProgress > 14 && CompletionPercentage < 50) riskFactors += 2;

                // Check document upload rate
                if (DocumentCompletionRate < 50) riskFactors += 1;

                return riskFactors switch
                {
                    >= 5 => "HIGH",
                    >= 3 => "MEDIUM",
                    >= 1 => "LOW",
                    _ => "NONE"
                };
            }
        }

        [NotMapped]
        public bool IsLateStarter => !HasStarted && DaysInProgress > 2;

        [NotMapped]
        public bool IsAheadOfSchedule
        {
            get
            {
                if (!ExpectedCompletionDate.HasValue) return false;
                var expectedProgress = CalculateExpectedProgress();
                return CompletionPercentage > expectedProgress + 10; // 10% buffer
            }
        }

        [NotMapped]
        public bool IsBehindSchedule
        {
            get
            {
                if (!ExpectedCompletionDate.HasValue) return false;
                var expectedProgress = CalculateExpectedProgress();
                return CompletionPercentage < expectedProgress - 10; // 10% buffer
            }
        }

        [NotMapped]
        public string ScheduleStatus
        {
            get
            {
                if (IsCompleted) return "Completed";
                if (IsAheadOfSchedule) return "Ahead of Schedule";
                if (IsBehindSchedule) return "Behind Schedule";
                return "On Track";
            }
        }

        [NotMapped]
        public decimal EfficiencyRatio
        {
            get
            {
                if (!EstimatedHoursTotal.HasValue || EstimatedHoursTotal.Value == 0 || !ActualHoursSpent.HasValue)
                    return 1.0m;

                return Math.Round(EstimatedHoursTotal.Value / ActualHoursSpent.Value, 2);
            }
        }

        [NotMapped]
        public TimeSpan EstimatedTimeRemaining
        {
            get
            {
                if (IsCompleted || !ExpectedCompletionDate.HasValue) return TimeSpan.Zero;

                var remaining = ExpectedCompletionDate.Value - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }

        // Methods
        public void UpdateProgress(int totalTasks, int completedTasks, int pendingTasks, int overdueTasks, int inProgressTasks)
        {
            TotalTasks = totalTasks;
            CompletedTasks = completedTasks;
            PendingTasks = pendingTasks;
            OverdueTasks = overdueTasks;
            InProgressTasks = inProgressTasks;

            // Calculate completion percentage
            CompletionPercentage = TotalTasks > 0 ? Math.Round((decimal)CompletedTasks / TotalTasks * 100, 2) : 0;

            // Update status based on progress
            if (CompletedTasks == 0)
            {
                Status = "NOT_STARTED";
            }
            else if (CompletedTasks == TotalTasks)
            {
                Status = "COMPLETED";
                if (!CompletionDate.HasValue)
                {
                    CompletionDate = DateTime.UtcNow;
                }
            }
            else
            {
                Status = "IN_PROGRESS";
                if (!FirstTaskStartedDate.HasValue)
                {
                    FirstTaskStartedDate = DateTime.UtcNow;
                }
            }

            // Update tracking flags
            IsOnTrack = OverdueTasks == 0 && !IsBehindSchedule;
            LastUpdated = DateTime.UtcNow;

            if (CompletedTasks > 0)
            {
                LastTaskCompletedDate = DateTime.UtcNow;
            }
        }

        public void UpdateDocumentProgress(int required, int uploaded)
        {
            DocumentsRequired = required;
            DocumentsUploaded = uploaded;
            LastUpdated = DateTime.UtcNow;
        }

        public void UpdateApprovalProgress(int requiring, int approved)
        {
            TasksRequiringApproval = requiring;
            TasksApproved = approved;
            LastUpdated = DateTime.UtcNow;
        }

        public void AddTimeSpent(decimal hours)
        {
            ActualHoursSpent = (ActualHoursSpent ?? 0) + hours;
            LastUpdated = DateTime.UtcNow;
        }

        public void SetCurrentPhase(string phase)
        {
            CurrentPhase = phase;
            LastUpdated = DateTime.UtcNow;
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
            LastUpdated = DateTime.UtcNow;
        }

        public bool ShouldSendCompletionNotification()
        {
            return IsCompleted && CompletionDate.HasValue &&
                   CompletionDate.Value.Date == DateTime.UtcNow.Date;
        }

        public bool ShouldSendOverdueNotification()
        {
            return HasOverdueTasks;
        }

        public bool ShouldSendProgressUpdate()
        {
            // Send progress updates at 25%, 50%, 75%
            var milestones = new[] { 25m, 50m, 75m };
            return milestones.Any(milestone =>
                CompletionPercentage >= milestone &&
                CompletionPercentage < milestone + 5); // 5% buffer to avoid spam
        }

        public List<string> GetProgressMilestones()
        {
            var milestones = new List<string>();

            if (CompletionPercentage >= 25) milestones.Add("Quarter Complete");
            if (CompletionPercentage >= 50) milestones.Add("Half Complete");
            if (CompletionPercentage >= 75) milestones.Add("Three Quarters Complete");
            if (CompletionPercentage >= 100) milestones.Add("Fully Complete");

            return milestones;
        }

        public Dictionary<string, object> GetMetrics()
        {
            return new Dictionary<string, object>
            {
                ["CompletionPercentage"] = CompletionPercentage,
                ["DaysInProgress"] = DaysInProgress,
                ["TasksCompleted"] = CompletedTasks,
                ["TasksTotal"] = TotalTasks,
                ["OverdueTasks"] = OverdueTasks,
                ["DocumentCompletionRate"] = DocumentCompletionRate,
                ["ApprovalCompletionRate"] = ApprovalCompletionRate,
                ["RiskLevel"] = RiskLevel,
                ["ScheduleStatus"] = ScheduleStatus,
                ["EfficiencyRatio"] = EfficiencyRatio,
                ["EstimatedTimeRemaining"] = EstimatedTimeRemaining.ToString(@"dd\.hh\:mm"),
                ["IsOnTrack"] = IsOnTrack
            };
        }

        private decimal CalculateExpectedProgress()
        {
            if (!ExpectedCompletionDate.HasValue) return CompletionPercentage;

            var totalDays = (ExpectedCompletionDate.Value - StartDate).TotalDays;
            var daysPassed = (DateTime.UtcNow - StartDate).TotalDays;

            if (totalDays <= 0) return 100;

            return Math.Min(100, (decimal)(daysPassed / totalDays * 100));
        }

        public static OnboardingProgress CreateNew(int employeeId, int totalTasks, DateTime? expectedCompletion = null)
        {
            return new OnboardingProgress
            {
                EmployeeId = employeeId,
                TotalTasks = totalTasks,
                CompletedTasks = 0,
                PendingTasks = totalTasks,
                OverdueTasks = 0,
                InProgressTasks = 0,
                CompletionPercentage = 0,
                StartDate = DateTime.UtcNow,
                Status = "NOT_STARTED",
                LastUpdated = DateTime.UtcNow,
                ExpectedCompletionDate = expectedCompletion ?? DateTime.UtcNow.AddDays(14),
                IsOnTrack = true
            };
        }
    }
}