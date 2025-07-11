// =============================================================================
// ONBOARDING MANAGEMENT DTOs
// File: TPAHRSystem.Core/DTOs/OnboardingDTOs.cs (New file)
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TPAHRSystem.Core.DTOs
{
    // =============================================================================
    // TEMPLATE MANAGEMENT DTOs
    // =============================================================================

    public class OnboardingTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ForRole { get; set; } = string.Empty;
        public string? ForDepartment { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public EmployeeBasicDto? CreatedBy { get; set; }
        public int TaskCount { get; set; }
        public List<OnboardingTaskTemplateDto> Tasks { get; set; } = new();
    }

    public class CreateOnboardingTemplateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string ForRole { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ForDepartment { get; set; }

        public List<CreateTaskTemplateDto>? Tasks { get; set; }
    }

    public class UpdateOnboardingTemplateDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? ForRole { get; set; }

        [StringLength(50)]
        public string? ForDepartment { get; set; }

        public bool? IsActive { get; set; }
    }

    public class CreateTaskTemplateDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EstimatedTime { get; set; }

        [StringLength(2000)]
        public string? Instructions { get; set; }

        public List<CreateDocumentTemplateDto>? RequiredDocuments { get; set; }
    }

    public class CreateDocumentTemplateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        public bool Required { get; set; } = true;
    }

    // =============================================================================
    // TASK MANAGEMENT DTOs
    // =============================================================================

    public class OnboardingTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string? EstimatedTime { get; set; }
        public string? Instructions { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Notes { get; set; }
        public EmployeeBasicDto? Employee { get; set; }
        public List<OnboardingDocumentDto> Documents { get; set; } = new();
        public bool IsOverdue { get; set; }
        public int DocumentsRequired { get; set; }
        public int DocumentsUploaded { get; set; }
    }

    public class OnboardingTaskTemplateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? EstimatedTime { get; set; }
        public string? Instructions { get; set; }
        public List<OnboardingDocumentDto> RequiredDocuments { get; set; } = new();
    }

    public class UpdateTaskStatusDto
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class BulkUpdateTasksDto
    {
        [Required]
        public List<int> TaskIds { get; set; } = new();

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(20)]
        public string? Priority { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // =============================================================================
    // ASSIGNMENT DTOs
    // =============================================================================

    public class AssignTemplateDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public DateTime? StartDate { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkAssignTemplateDto
    {
        [Required]
        public List<int> EmployeeIds { get; set; } = new();

        [Required]
        public int TemplateId { get; set; }

        public DateTime? StartDate { get; set; }
        public string? Notes { get; set; }
    }

    public class OnboardingAssignmentDto
    {
        public int Id { get; set; }
        public EmployeeBasicDto Employee { get; set; } = new();
        public OnboardingTemplateDto Template { get; set; } = new();
        public DateTime AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public EmployeeBasicDto AssignedBy { get; set; } = new();
    }

    // =============================================================================
    // PROGRESS AND OVERVIEW DTOs
    // =============================================================================

    public class OnboardingProgressDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class EmployeeOnboardingOverviewDto
    {
        public EmployeeBasicDto Employee { get; set; } = new();
        public OnboardingProgressDto? Progress { get; set; }
        public List<OnboardingTaskDto> Tasks { get; set; } = new();
        public OnboardingTemplateDto? Template { get; set; }
        public int DaysInOnboarding { get; set; }
        public List<OnboardingDocumentDto> PendingDocuments { get; set; } = new();
        public List<OnboardingTaskDto> OverdueTasks { get; set; } = new();
    }

    public class OnboardingOverviewStatsDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveOnboarding { get; set; }
        public int CompletedOnboarding { get; set; }
        public int NotStarted { get; set; }
        public decimal AverageCompletion { get; set; }
        public decimal AverageDaysToComplete { get; set; }
        public int OverdueTasks { get; set; }
        public List<DepartmentStatsDto> DepartmentBreakdown { get; set; } = new();
    }

    public class DepartmentStatsDto
    {
        public string Department { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int WithOnboarding { get; set; }
        public decimal AverageCompletion { get; set; }
        public int CompletedCount { get; set; }
        public int InProgressCount { get; set; }
        public int NotStartedCount { get; set; }
    }

    // =============================================================================
    // DOCUMENT MANAGEMENT DTOs
    // =============================================================================

    public class OnboardingDocumentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool Uploaded { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTime? UploadedDate { get; set; }
        public EmployeeBasicDto? UploadedBy { get; set; }
    }

    public class DocumentUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [StringLength(200)]
        public string? DocumentName { get; set; }

        [StringLength(50)]
        public string DocumentType { get; set; } = "GENERAL";
    }

    // =============================================================================
    // ANALYTICS AND REPORTING DTOs
    // =============================================================================

    public class OnboardingAnalyticsDto
    {
        public List<TaskCategoryStatsDto> TaskSummary { get; set; } = new();
        public List<TemplateEffectivenessDto> TemplateStats { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<OverdueTaskDto> OverdueTasks { get; set; } = new();
        public OnboardingTrendsDto Trends { get; set; } = new();
    }

    public class TaskCategoryStatsDto
    {
        public string Category { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal? AvgCompletionDays { get; set; }
    }

    public class TemplateEffectivenessDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string ForRole { get; set; } = string.Empty;
        public string? ForDepartment { get; set; }
        public int TimesAssigned { get; set; }
        public int CompletedAssignments { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal? AvgCompletionPercentage { get; set; }
        public decimal? AvgDaysToComplete { get; set; }
        public string EffectivenessRating { get; set; } = string.Empty;
    }

    public class RecentActivityDto
    {
        public string TaskTitle { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime CompletedDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
    }

    public class OverdueTaskDto
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public EmployeeBasicDto Employee { get; set; } = new();
        public int DaysOverdue { get; set; }
    }

    public class OnboardingTrendsDto
    {
        public List<MonthlyTrendDto> MonthlyCompletion { get; set; } = new();
        public List<DepartmentTrendDto> DepartmentTrends { get; set; } = new();
        public decimal YearOverYearGrowth { get; set; }
        public decimal AverageTimeToComplete { get; set; }
    }

    public class MonthlyTrendDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int NewHires { get; set; }
        public int CompletedOnboarding { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AvgDaysToComplete { get; set; }
    }

    public class DepartmentTrendDto
    {
        public string Department { get; set; } = string.Empty;
        public decimal CompletionRate { get; set; }
        public decimal AvgDaysToComplete { get; set; }
        public int TotalEmployees { get; set; }
        public string Trend { get; set; } = string.Empty; // "Improving", "Declining", "Stable"
    }

    // =============================================================================
    // FILTER AND SEARCH DTOs
    // =============================================================================

    public class OnboardingFilterDto
    {
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Status { get; set; }
        public string? TemplateId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? CompletionDateFrom { get; set; }
        public DateTime? CompletionDateTo { get; set; }
        public decimal? MinCompletionPercentage { get; set; }
        public decimal? MaxCompletionPercentage { get; set; }
        public bool? HasOverdueTasks { get; set; }
    }

    public class TaskFilterDto
    {
        public int? EmployeeId { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public bool? IsOverdue { get; set; }
        public bool? HasDocuments { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class PaginationDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
    }

    public class PagedResultDto<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationInfoDto Pagination { get; set; } = new();
    }

    public class PaginationInfoDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    // =============================================================================
    // COMMON DTOs
    // =============================================================================

    public class EmployeeBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Department { get; set; }
        public DateTime? HireDate { get; set; }
        public string? EmployeeNumber { get; set; }
    }

    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class BulkOperationResultDto
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<BulkOperationItemDto> Results { get; set; } = new();
    }

    public class BulkOperationItemDto
    {
        public int ItemId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // =============================================================================
    // NOTIFICATION AND COMMUNICATION DTOs
    // =============================================================================

    public class OnboardingNotificationDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "TASK_DUE", "TASK_OVERDUE", "COMPLETION", "WELCOME"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public int? TaskId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
    }

    public class SendNotificationDto
    {
        [Required]
        public List<int> EmployeeIds { get; set; } = new();

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string Type { get; set; } = "GENERAL";

        public int? TaskId { get; set; }
    }

    // =============================================================================
    // CHECKLIST AND WORKFLOW DTOs
    // =============================================================================

    public class OnboardingChecklistDto
    {
        public int Id { get; set; }
        public EmployeeBasicDto Employee { get; set; } = new();
        public OnboardingTemplateDto Template { get; set; } = new();
        public DateTime AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public EmployeeBasicDto AssignedBy { get; set; } = new();
        public List<OnboardingTaskDto> Tasks { get; set; } = new();
        public OnboardingProgressDto? Progress { get; set; }
    }

    public class WorkflowStepDto
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public List<OnboardingTaskDto> Tasks { get; set; } = new();
        public bool CanProceed { get; set; }
    }

    // =============================================================================
    // INTEGRATION DTOs
    // =============================================================================

    public class HRISIntegrationDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public string ManagerEmail { get; set; } = string.Empty;
        public Dictionary<string, object> CustomFields { get; set; } = new();
    }

    public class AutoAssignmentRuleDto
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int TemplateId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Priority { get; set; }
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

    public class OnboardingValidationResultDto
    {
        public bool IsValid { get; set; }
        public List<ValidationErrorDto> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> ValidationContext { get; set; } = new();
    }

    // =============================================================================
    // CONFIGURATION DTOs
    // =============================================================================

    public class OnboardingConfigurationDto
    {
        public int DefaultTaskDueDays { get; set; } = 7;
        public bool AutoAssignTemplates { get; set; } = true;
        public bool SendWelcomeEmail { get; set; } = true;
        public bool SendReminderEmails { get; set; } = true;
        public int ReminderDaysBeforeDue { get; set; } = 2;
        public bool RequireManagerApproval { get; set; } = false;
        public string DefaultTaskPriority { get; set; } = "MEDIUM";
        public List<string> RequiredCategories { get; set; } = new();
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }

    // =============================================================================
    // AUDIT AND TRACKING DTOs
    // =============================================================================

    public class OnboardingAuditLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public EmployeeBasicDto PerformedBy { get; set; } = new();
        public string? Notes { get; set; }
    }

    public class OnboardingMetricsDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalOnboardingStarted { get; set; }
        public int TotalOnboardingCompleted { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageTimeToComplete { get; set; }
        public decimal EmployeeSatisfactionScore { get; set; }
        public int TasksCompleted { get; set; }
        public int DocumentsUploaded { get; set; }
        public List<DepartmentMetricsDto> DepartmentMetrics { get; set; } = new();
    }

    public class DepartmentMetricsDto
    {
        public string Department { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageTimeToComplete { get; set; }
        public decimal SatisfactionScore { get; set; }
    }
}