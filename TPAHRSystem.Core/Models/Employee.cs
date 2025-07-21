using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class Employee
    {
        // =============================================================================
        // EXISTING ONBOARDING PROPERTIES (MOVED TO TOP)
        // =============================================================================

        [StringLength(20)]
        public string? OnboardingStatus { get; set; } = "PENDING";

        public bool? IsOnboardingLocked { get; set; } = true;

        // =============================================================================
        // ADDITIONAL ONBOARDING PROPERTIES
        // =============================================================================

        /// <summary>
        /// Date when onboarding process started
        /// </summary>
        public DateTime? OnboardingStartDate { get; set; }

        /// <summary>
        /// Expected onboarding completion date
        /// </summary>
        public DateTime? OnboardingExpectedDate { get; set; }

        /// <summary>
        /// Actual onboarding completion date
        /// </summary>
        public DateTime? OnboardingCompletionDate { get; set; }

        /// <summary>
        /// Current onboarding phase - Getting Started, Documentation, Training, Final Steps
        /// </summary>
        [StringLength(50)]
        public string? OnboardingPhase { get; set; }

        /// <summary>
        /// Onboarding completion percentage (0-100)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? OnboardingCompletionPercentage { get; set; } = 0;

        /// <summary>
        /// Number of completed onboarding tasks
        /// </summary>
        public int? OnboardingTasksCompleted { get; set; } = 0;

        /// <summary>
        /// Total number of onboarding tasks assigned
        /// </summary>
        public int? OnboardingTasksTotal { get; set; } = 0;

        /// <summary>
        /// Whether onboarding is on track for completion
        /// </summary>
        public bool? IsOnboardingOnTrack { get; set; } = true;

        /// <summary>
        /// Notes about onboarding progress
        /// </summary>
        [StringLength(1000)]
        public string? OnboardingNotes { get; set; }

        /// <summary>
        /// HR employee who approved final onboarding completion
        /// </summary>
        public int? OnboardingApprovedById { get; set; }

        /// <summary>
        /// Date of final onboarding approval
        /// </summary>
        public DateTime? OnboardingApprovedDate { get; set; }

        /// <summary>
        /// Employee's assigned onboarding mentor/buddy
        /// </summary>
        public int? OnboardingMentorId { get; set; }

        /// <summary>
        /// Last date onboarding reminder was sent
        /// </summary>
        public DateTime? LastOnboardingReminderDate { get; set; }

        /// <summary>
        /// Number of onboarding reminders sent
        /// </summary>
        public int? OnboardingReminderCount { get; set; } = 0;

        // =============================================================================
        // EXISTING PROPERTIES (UNCHANGED)
        // =============================================================================

        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        public DateTime HireDate { get; set; }

        public DateTime? TerminationDate { get; set; }

        [StringLength(50)]
        public string? JobTitle { get; set; }

        [StringLength(50)]
        public string? Position { get; set; }

        [StringLength(50)]
        public string? WorkLocation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Salary { get; set; }

        [StringLength(20)]
        public string? EmploymentStatus { get; set; } = "Active";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        public int? ManagerId { get; set; }

        // =============================================================================
        // EXISTING NAVIGATION PROPERTIES (UNCHANGED)
        // =============================================================================

        public User? User { get; set; }
        public Department? Department { get; set; }
        public Employee? Manager { get; set; }
        public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

        // =============================================================================
        // ADDITIONAL ONBOARDING NAVIGATION PROPERTIES
        // =============================================================================

        /// <summary>
        /// HR employee who approved onboarding completion
        /// </summary>
        [ForeignKey("OnboardingApprovedById")]
        public virtual Employee? OnboardingApprovedBy { get; set; }

        /// <summary>
        /// Employee's onboarding mentor
        /// </summary>
        [ForeignKey("OnboardingMentorId")]
        public virtual Employee? OnboardingMentor { get; set; }

        /// <summary>
        /// Employees this person is mentoring
        /// </summary>
        [InverseProperty("OnboardingMentor")]
        public virtual ICollection<Employee> OnboardingMentees { get; set; } = new List<Employee>();

        /// <summary>
        /// Employees this person approved onboarding for
        /// </summary>
        [InverseProperty("OnboardingApprovedBy")]
        public virtual ICollection<Employee> OnboardingApprovals { get; set; } = new List<Employee>();

        // =============================================================================
        // EXISTING TEMPORARY NAVIGATION PROPERTIES (UNCHANGED)
        // =============================================================================

        // TEMPORARY: Ignore these navigation properties to fix EF error
        [NotMapped]
        public ICollection<OnboardingTask> AssignedTasks { get; set; } = new List<OnboardingTask>();

        [NotMapped]
        public ICollection<OnboardingProgress> OnboardingProgress { get; set; } = new List<OnboardingProgress>();

        [NotMapped]
        public ICollection<OnboardingChecklist> OnboardingChecklists { get; set; } = new List<OnboardingChecklist>();

        [NotMapped]
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

        [NotMapped]
        public ICollection<TimeSheet> TimeSheets { get; set; } = new List<TimeSheet>();

        [NotMapped]
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        [NotMapped]
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

        public ICollection<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();

        // =============================================================================
        // EXISTING COMPUTED PROPERTIES (UNCHANGED)
        // =============================================================================

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string DisplayName => $"{LastName}, {FirstName}";

        [NotMapped]
        public int? YearsOfService
        {
            get
            {
                if (TerminationDate.HasValue)
                {
                    return (int)((TerminationDate.Value - HireDate).TotalDays / 365);
                }
                return (int)((DateTime.UtcNow - HireDate).TotalDays / 365);
            }
        }

        // =============================================================================
        // NEW ONBOARDING COMPUTED PROPERTIES
        // =============================================================================

        /// <summary>
        /// Whether employee is currently in onboarding process
        /// </summary>
        [NotMapped]
        public bool IsInOnboarding => OnboardingStatus != "COMPLETED" && OnboardingStatus != "CANCELLED";

        /// <summary>
        /// Whether employee has completed onboarding
        /// </summary>
        [NotMapped]
        public bool HasCompletedOnboarding => OnboardingStatus == "COMPLETED";

        /// <summary>
        /// Number of days since hire
        /// </summary>
        [NotMapped]
        public int DaysSinceHire => (int)(DateTime.UtcNow - HireDate).TotalDays;

        /// <summary>
        /// Number of days in onboarding process
        /// </summary>
        [NotMapped]
        public int DaysInOnboarding => OnboardingStartDate.HasValue ?
            (int)(DateTime.UtcNow - OnboardingStartDate.Value).TotalDays : DaysSinceHire;

        /// <summary>
        /// Whether onboarding is overdue
        /// </summary>
        [NotMapped]
        public bool IsOnboardingOverdue => OnboardingExpectedDate.HasValue &&
            OnboardingExpectedDate.Value < DateTime.UtcNow &&
            !HasCompletedOnboarding;

        /// <summary>
        /// Number of pending onboarding tasks
        /// </summary>
        [NotMapped]
        public int OnboardingTasksPending => (OnboardingTasksTotal ?? 0) - (OnboardingTasksCompleted ?? 0);

        /// <summary>
        /// Onboarding progress display string
        /// </summary>
        [NotMapped]
        public string OnboardingProgressDisplay =>
            $"{OnboardingTasksCompleted ?? 0}/{OnboardingTasksTotal ?? 0} ({OnboardingCompletionPercentage ?? 0:F1}%)";

        /// <summary>
        /// Onboarding status display with emoji
        /// </summary>
        [NotMapped]
        public string OnboardingStatusDisplay => OnboardingStatus?.ToUpper() switch
        {
            "PENDING" => "🔄 Pending",
            "IN_PROGRESS" => "⏳ In Progress",
            "COMPLETED" => "✅ Completed",
            "CANCELLED" => "❌ Cancelled",
            _ => OnboardingStatus ?? "Unknown"
        };

        /// <summary>
        /// Current access level based on onboarding status
        /// </summary>
        [NotMapped]
        public string AccessLevel => (IsOnboardingLocked ?? true) ? "RESTRICTED" : "FULL";

        /// <summary>
        /// Whether employee can access full system
        /// </summary>
        [NotMapped]
        public bool CanAccessFullSystem => !(IsOnboardingLocked ?? true) && HasCompletedOnboarding;

        /// <summary>
        /// Days until expected onboarding completion
        /// </summary>
        [NotMapped]
        public int DaysUntilOnboardingDue => OnboardingExpectedDate.HasValue ?
            (int)(OnboardingExpectedDate.Value - DateTime.UtcNow).TotalDays : 0;

        /// <summary>
        /// Onboarding time summary
        /// </summary>
        [NotMapped]
        public string OnboardingTimeSummary
        {
            get
            {
                if (HasCompletedOnboarding && OnboardingCompletionDate.HasValue)
                {
                    var daysToComplete = (int)(OnboardingCompletionDate.Value - HireDate).TotalDays;
                    return $"Completed in {daysToComplete} days";
                }
                else if (IsOnboardingOverdue)
                {
                    return $"Overdue by {Math.Abs(DaysUntilOnboardingDue)} days";
                }
                else if (DaysUntilOnboardingDue > 0)
                {
                    return $"{DaysUntilOnboardingDue} days remaining";
                }
                else
                {
                    return $"{DaysInOnboarding} days in progress";
                }
            }
        }
    }
}