// =============================================================================
// CONSOLIDATED STORED PROCEDURE RESULT MODELS - SINGLE SOURCE OF TRUTH
// File: TPAHRSystem.Core/Models/StoredProcedureResults.cs
// Replace the ENTIRE contents of this file with this code
// =============================================================================

using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    /// <summary>
    /// Result from sp_CreateEmployeeWithOnboarding stored procedure
    /// This matches the actual stored procedure output exactly
    /// </summary>
    public class CreateEmployeeResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int OnboardingTasks { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Error handling properties (when procedure fails)
        public string? ErrorMessage { get; set; }
        public int? ErrorNumber { get; set; }
        public string? Status { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage) && ErrorNumber == null;

        [NotMapped]
        public string StatusMessage => IsSuccess ? "Employee created successfully" : ErrorMessage ?? "Unknown error";
    }

    /// <summary>
    /// Result from sp_GetEmployeeTasks stored procedure
    /// Matches the actual OnboardingTasks table structure
    /// </summary>
    public class EmployeeTaskResult
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
        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }

        // Note: Removed CanEmployeeComplete, BlocksSystemAccess, SortOrder 
        // because these don't exist in the OnboardingTasks table

        // Computed Properties
        [NotMapped]
        public bool IsCompleted => Status?.ToUpper() == "COMPLETED";

        [NotMapped]
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsCompleted;

        [NotMapped]
        public int DaysUntilDue => DueDate.HasValue ?
            (int)(DueDate.Value - DateTime.UtcNow).TotalDays : 0;

        [NotMapped]
        public string StatusBadgeClass => Status?.ToUpper() switch
        {
            "COMPLETED" => "success",
            "IN_PROGRESS" => "warning",
            "PENDING" => "secondary",
            "OVERDUE" => "danger",
            _ => "secondary"
        };

        [NotMapped]
        public string PriorityBadgeClass => Priority?.ToUpper() switch
        {
            "HIGH" => "danger",
            "MEDIUM" => "warning",
            "LOW" => "success",
            _ => "secondary"
        };
    }

    /// <summary>
    /// Result from task completion stored procedures
    /// </summary>
    public class TaskCompletionResult
    {
        public string Message { get; set; } = string.Empty;
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        [NotMapped]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public string CompletionSummary => IsSuccess ?
            $"{CompletedTasks}/{TotalTasks} tasks completed ({CompletionPercentage:F1}%)" :
            ErrorMessage ?? "Completion failed";
    }

    /// <summary>
    /// Result from onboarding completion stored procedure
    /// </summary>
    public class OnboardingCompletionResult
    {
        public string Message { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public int? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public int? TotalTasksCompleted { get; set; }
        public int? DaysToComplete { get; set; }
        public string? ApprovedByName { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Status { get; set; }
        public int? PendingTasks { get; set; }

        [NotMapped]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public string CompletionSummary => IsSuccess ?
            $"Onboarding completed in {DaysToComplete} days with {TotalTasksCompleted} tasks" :
            ErrorMessage ?? "Completion failed";

        [NotMapped]
        public bool CanComplete => (PendingTasks ?? 0) == 0;
    }

    /// <summary>
    /// Result from onboarding status stored procedures
    /// </summary>
    public class OnboardingStatusResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string OnboardingStatus { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public DateTime HireDate { get; set; }
        public string? ErrorMessage { get; set; }

        [NotMapped]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public bool IsCompleted => OnboardingStatus?.ToUpper() == "COMPLETED";

        [NotMapped]
        public bool IsOnTrack => CompletionPercentage >= 70 || IsCompleted;

        [NotMapped]
        public int DaysUntilExpected => ExpectedCompletionDate.HasValue ?
            (int)(ExpectedCompletionDate.Value - DateTime.UtcNow).TotalDays : 0;

        [NotMapped]
        public int DaysInOnboarding => (int)(DateTime.UtcNow - HireDate).TotalDays;

        [NotMapped]
        public string StatusBadge => OnboardingStatus?.ToUpper() switch
        {
            "COMPLETED" => "success",
            "IN_PROGRESS" => "warning",
            "NOT_STARTED" => "secondary",
            "OVERDUE" => "danger",
            _ => "secondary"
        };

        [NotMapped]
        public string ProgressSummary => $"{CompletedTasks}/{TotalTasks} tasks ({CompletionPercentage:F1}%)";
    }

    /// <summary>
    /// Generic result for stored procedures that return simple status
    /// </summary>
    public class StoredProcedureResult
    {
        public string Result { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public int? ErrorNumber { get; set; }
        public string? Status { get; set; }
        public DateTime? Timestamp { get; set; }

        [NotMapped]
        public bool IsSuccess => Result?.ToUpper() == "SUCCESS" && string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public string StatusMessage => IsSuccess ? Message : ErrorMessage ?? "Unknown error occurred";
    }

    /// <summary>
    /// Result for department task template queries
    /// Used when retrieving onboarding task templates
    /// </summary>
    public class DepartmentTaskTemplateResult
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
        public string? ErrorMessage { get; set; }

        [NotMapped]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public string FormattedEstimatedTime => $"{EstimatedDays} day{(EstimatedDays == 1 ? "" : "s")}";

        [NotMapped]
        public string StatusBadge => IsActive ? "Active" : "Inactive";

        [NotMapped]
        public string PriorityBadgeClass => Priority?.ToUpper() switch
        {
            "HIGH" => "danger",
            "MEDIUM" => "warning",
            "LOW" => "success",
            _ => "secondary"
        };

        [NotMapped]
        public string CategoryBadgeClass => Category?.ToUpper() switch
        {
            "DOCUMENTATION" => "primary",
            "TRAINING" => "info",
            "EQUIPMENT" => "warning",
            "HR" => "success",
            "IT" => "dark",
            "FINANCIAL" => "secondary",
            _ => "light"
        };
    }

    /// <summary>
    /// Result for employee hierarchy and reporting structure queries
    /// </summary>
    public class EmployeeHierarchyResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? JobTitle { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool UserActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public int DaysEmployed { get; set; }

        [NotMapped]
        public bool HasManager => ManagerId.HasValue;

        [NotMapped]
        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : $"{FirstName} {LastName}";

        [NotMapped]
        public string EmploymentDuration => DaysEmployed switch
        {
            < 30 => $"{DaysEmployed} days",
            < 365 => $"{DaysEmployed / 30} months",
            _ => $"{DaysEmployed / 365} years"
        };

        [NotMapped]
        public string StatusIndicator => UserActive ? "Active" : "Inactive";
    }

    /// <summary>
    /// Result for bulk operations on employees or tasks
    /// </summary>
    public class BulkOperationResult
    {
        public int SuccessfulOperations { get; set; }
        public int FailedOperations { get; set; }
        public int TotalOperations { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? ErrorMessage { get; set; }

        [NotMapped]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public decimal SuccessRate => TotalOperations > 0 ?
            (decimal)SuccessfulOperations / TotalOperations * 100 : 0;

        [NotMapped]
        public string OperationSummary => $"{SuccessfulOperations}/{TotalOperations} operations completed ({SuccessRate:F1}%)";

        [NotMapped]
        public bool HasErrors => Errors.Any();

        [NotMapped]
        public bool HasWarnings => Warnings.Any();
    }
}