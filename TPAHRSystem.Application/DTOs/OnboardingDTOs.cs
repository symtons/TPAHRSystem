// =============================================================================
// ONBOARDING DATA TRANSFER OBJECTS (DTOs)
// File: TPAHRSystem.Application/DTOs/OnboardingDTOs.cs
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TPAHRSystem.Application.DTOs
{
    // =============================================================================
    // REQUEST DTOs
    // =============================================================================

    public class CreateEmployeeRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [Required]
        public int DepartmentId { get; set; }
    }

    public class CompleteTaskRequest
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CompleteOnboardingRequest
    {
        [MaxLength(500)]
        public string? FinalNotes { get; set; }
    }

    // =============================================================================
    // RESPONSE DTOs
    // =============================================================================

    public class CreateEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int OnboardingTasks { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class OnboardingTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string EstimatedTime { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool CanComplete { get; set; } = false;
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != "COMPLETED";
        public int DaysUntilDue => DueDate.HasValue ? (int)(DueDate.Value - DateTime.UtcNow).TotalDays : 0;
    }

    public class TaskCompletionDto
    {
        public string Message { get; set; } = string.Empty;
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
    }

    public class OnboardingStatusDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime HireDate { get; set; }
        public string Status => CompletionPercentage >= 100 ? "COMPLETED" :
                               CompletionPercentage > 0 ? "IN_PROGRESS" : "NOT_STARTED";
        public int DaysInOnboarding => (int)(DateTime.UtcNow - HireDate).TotalDays;
        public bool IsOverdue => DaysInOnboarding > 14 && CompletionPercentage < 100; // Assuming 14 days is standard
    }

    public class OnboardingCompletionDto
    {
        public string Message { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime CompletedDate { get; set; }
    }

    public class AccessStatusDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsOnboardingLocked { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
        public bool HasRestrictedAccess { get; set; }
        public bool CanAccessFullSystem { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int PendingTasks { get; set; }
    }

    // =============================================================================
    // OVERVIEW AND ANALYTICS DTOs
    // =============================================================================

    public class OnboardingOverviewDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveOnboarding { get; set; }
        public int CompletedOnboarding { get; set; }
        public int NotStarted { get; set; }
        public int OverdueOnboarding { get; set; }
        public decimal AverageCompletionPercentage { get; set; }
        public double AverageDaysToComplete { get; set; }
        public List<OnboardingStatusDto> Employees { get; set; } = new();
        public Dictionary<string, int> TasksByCategory { get; set; } = new();
        public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
    }

    public class OnboardingAnalyticsDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int NewHires { get; set; }
        public int CompletedOnboarding { get; set; }
        public decimal CompletionRate => NewHires > 0 ? (decimal)CompletedOnboarding / NewHires * 100 : 0;
        public double AverageCompletionTime { get; set; }
        public Dictionary<string, decimal> CompletionRateByDepartment { get; set; } = new();
        public Dictionary<string, double> AverageTimeByDepartment { get; set; } = new();
        public List<string> CommonDelays { get; set; } = new();
    }

    // =============================================================================
    // TASK TEMPLATE DTOs
    // =============================================================================

    public class OnboardingTaskTemplateDto
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int EstimatedDays { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public bool CanEmployeeComplete { get; set; }
        public bool BlocksSystemAccess { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateTaskTemplateRequest
    {
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "MEDIUM";

        [Range(1, 30)]
        public int EstimatedDays { get; set; } = 1;

        [MaxLength(2000)]
        public string Instructions { get; set; } = string.Empty;

        public bool CanEmployeeComplete { get; set; } = false;
        public bool BlocksSystemAccess { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    // =============================================================================
    // DASHBOARD DTOs
    // =============================================================================

    public class OnboardingDashboardDto
    {
        public string DashboardType { get; set; } = string.Empty; // ONBOARDING, FULL
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public bool ShowProgressBar { get; set; }
        public bool ShowTaskList { get; set; }
        public bool ShowWelcomeMessage { get; set; }
        public bool RestrictedAccess { get; set; }
        public List<string> AvailableWidgets { get; set; } = new();
        public OnboardingProgressDto? Progress { get; set; }
        public List<OnboardingTaskDto> RecentTasks { get; set; } = new();
    }

    public class OnboardingProgressDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public decimal CompletionPercentage { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public bool IsOnTrack { get; set; }
        public string CurrentPhase { get; set; } = string.Empty;
        public List<string> NextSteps { get; set; } = new();
    }

    // =============================================================================
    // NOTIFICATION AND COMMUNICATION DTOs
    // =============================================================================

    public class OnboardingNotificationDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Type { get; set; } = string.Empty; // WELCOME, TASK_DUE, OVERDUE, COMPLETION
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string Priority { get; set; } = "NORMAL";
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class SendNotificationRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Priority { get; set; } = "NORMAL";

        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    // =============================================================================
    // REPORTING DTOs
    // =============================================================================

    public class OnboardingReportDto
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public OnboardingOverviewDto Overview { get; set; } = new();
        public OnboardingAnalyticsDto Analytics { get; set; } = new();
        public List<OnboardingStatusDto> DetailedStatus { get; set; } = new();
        public Dictionary<string, object> CustomMetrics { get; set; } = new();
    }

    public class GenerateReportRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReportType { get; set; } = string.Empty; // OVERVIEW, ANALYTICS, DETAILED

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        public int? DepartmentId { get; set; }
        public string? Status { get; set; }
        public List<string> IncludeMetrics { get; set; } = new();
    }

    // =============================================================================
    // VALIDATION AND ERROR DTOs
    // =============================================================================

    public class ValidationErrorDto
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? AttemptedValue { get; set; }
    }

    public class OnboardingErrorDto
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<ValidationErrorDto> ValidationErrors { get; set; } = new();
        public Dictionary<string, object> Context { get; set; } = new();
    }
}