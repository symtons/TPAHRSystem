// =============================================================================
// STORED PROCEDURE RESULT MODELS
// File: TPAHRSystem.Core/Models/StoredProcedureResults.cs
// =============================================================================

using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    /// <summary>
    /// Result from sp_CreateEmployeeWithOnboarding stored procedure
    /// </summary>
    public class CreateEmployeeResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int OnboardingTasks { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string? ErrorMessage { get; set; }
        public int? ErrorNumber { get; set; }
        public string? Status { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public string StatusMessage => IsSuccess ? "Employee created successfully" : ErrorMessage ?? "Unknown error";
    }

    /// <summary>
    /// Result from sp_GetEmployeeTasks stored procedure
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
        public bool? CanEmployeeComplete { get; set; }
        public bool? BlocksSystemAccess { get; set; }
        public int? SortOrder { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsCompleted => Status?.ToUpper() == "COMPLETED";

        [NotMapped]
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && !IsCompleted;

        [NotMapped]
        public int DaysUntilDue => DueDate.HasValue ? (int)(DueDate.Value - DateTime.UtcNow).TotalDays : 0;

        [NotMapped]
        public string StatusDisplay => Status?.ToLower() switch
        {
            "pending" => "🔄 Pending",
            "in_progress" => "⏳ In Progress",
            "completed" => "✅ Completed",
            "overdue" => "⚠️ Overdue",
            _ => Status ?? "Unknown"
        };

        [NotMapped]
        public string PriorityDisplay => Priority?.ToLower() switch
        {
            "low" => "🟢 Low",
            "medium" => "🟡 Medium",
            "high" => "🟠 High",
            "urgent" => "🔴 Urgent",
            _ => Priority ?? "Unknown"
        };
    }

    /// <summary>
    /// Result from sp_CompleteOnboardingTask stored procedure
    /// </summary>
    public class TaskCompletionResult
    {
        public string Message { get; set; } = string.Empty;
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Status { get; set; }

        // Computed Properties
        [NotMapped]
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public int RemainingTasks => TotalTasks - CompletedTasks;

        [NotMapped]
        public bool IsFullyCompleted => CompletionPercentage >= 100;

        [NotMapped]
        public string CompletionDisplay => $"{CompletedTasks}/{TotalTasks} ({CompletionPercentage:F1}%)";
    }

    /// <summary>
    /// Result from sp_GetOnboardingStatus stored procedure
    /// </summary>
    public class OnboardingStatusResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? OnboardingStartDate { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public string? CurrentPhase { get; set; }
        public bool? IsOnTrack { get; set; }
        public string? Notes { get; set; }

        // Computed Properties
        [NotMapped]
        public string Status => CompletionPercentage >= 100 ? "COMPLETED" :
                               CompletionPercentage > 0 ? "IN_PROGRESS" : "NOT_STARTED";

        [NotMapped]
        public int DaysInOnboarding => (int)(DateTime.UtcNow - HireDate).TotalDays;

        [NotMapped]
        public bool IsOverdue => ExpectedCompletionDate.HasValue &&
                                ExpectedCompletionDate.Value < DateTime.UtcNow &&
                                CompletionPercentage < 100;

        [NotMapped]
        public string StatusDisplay => Status switch
        {
            "NOT_STARTED" => "🔄 Not Started",
            "IN_PROGRESS" => "⏳ In Progress",
            "COMPLETED" => "✅ Completed",
            _ => Status
        };

        [NotMapped]
        public string ProgressDisplay => $"{CompletedTasks}/{TotalTasks} ({CompletionPercentage:F1}%)";

        [NotMapped]
        public int DaysRemaining => ExpectedCompletionDate.HasValue ?
            (int)(ExpectedCompletionDate.Value - DateTime.UtcNow).TotalDays : 0;
    }

    /// <summary>
    /// Result from sp_CompleteOnboarding stored procedure
    /// </summary>
    public class OnboardingCompletionResult
    {
        public string Message { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime CompletedDate { get; set; }
        public int? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public int? TotalTasksCompleted { get; set; }
        public int? DaysToComplete { get; set; }
        public string? ApprovedByName { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Status { get; set; }
        public int? PendingTasks { get; set; }

        // Computed Properties
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

        // Computed Properties
        [NotMapped]
        public bool IsSuccess => Result?.ToUpper() == "SUCCESS" && string.IsNullOrEmpty(ErrorMessage);

        [NotMapped]
        public string StatusMessage => IsSuccess ? Message : ErrorMessage ?? "Unknown error occurred";
    }

    /// <summary>
    /// Result for department task template queries
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
        public string? CreatedByName { get; set; }

        // Computed Properties
        [NotMapped]
        public string DisplayTitle => $"{Title} ({EstimatedDays} days)";

        [NotMapped]
        public string AccessibilityNote => CanEmployeeComplete ?
            "Employee can complete" : "HR completion required";
    }
}