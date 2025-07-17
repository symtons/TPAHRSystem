// =============================================================================
// CORRECTED ONBOARDING CONSTANTS
// File: TPAHRSystem.Core/Models/OnboardingConstants.cs (Replace existing)
// =============================================================================

using System.ComponentModel.DataAnnotations;

namespace TPAHRSystem.Core.Models
{
    public static class OnboardingConstants
    {
        public static class TaskStatuses
        {
            public const string PENDING = "PENDING";
            public const string IN_PROGRESS = "IN_PROGRESS";
            public const string COMPLETED = "COMPLETED";
            public const string OVERDUE = "OVERDUE";
        }

        public static class TaskPriorities
        {
            public const string LOW = "LOW";
            public const string MEDIUM = "MEDIUM";
            public const string HIGH = "HIGH";
        }

        public static class TaskCategories
        {
            public const string ORIENTATION = "ORIENTATION";
            public const string DOCUMENTATION = "DOCUMENTATION";
            public const string TRAINING = "TRAINING";
            public const string PERSONAL = "PERSONAL";
            public const string FINANCIAL = "FINANCIAL";
            public const string LEGAL = "LEGAL";
            public const string CERTIFICATION = "CERTIFICATION";
            public const string EQUIPMENT = "EQUIPMENT";
            public const string GENERAL = "GENERAL";
        }

        public static class NotificationTypes
        {
            public const string TASK_DUE = "TASK_DUE";
            public const string TASK_OVERDUE = "TASK_OVERDUE";
            public const string COMPLETION = "COMPLETION";
            public const string WELCOME = "WELCOME";
            public const string REMINDER = "REMINDER";
            public const string MANAGER_NOTIFICATION = "MANAGER_NOTIFICATION";
        }

        public static class ChecklistStatuses
        {
            public const string ASSIGNED = "ASSIGNED";
            public const string IN_PROGRESS = "IN_PROGRESS";
            public const string COMPLETED = "COMPLETED";
            public const string CANCELLED = "CANCELLED";
        }

        public static class ConfigurationKeys
        {
            public const string DEFAULT_TASK_DUE_DAYS = "DEFAULT_TASK_DUE_DAYS";
            public const string AUTO_ASSIGN_TEMPLATES = "AUTO_ASSIGN_TEMPLATES";
            public const string SEND_WELCOME_EMAIL = "SEND_WELCOME_EMAIL";
            public const string SEND_REMINDER_EMAILS = "SEND_REMINDER_EMAILS";
            public const string REMINDER_DAYS_BEFORE_DUE = "REMINDER_DAYS_BEFORE_DUE";
            public const string REQUIRE_MANAGER_APPROVAL = "REQUIRE_MANAGER_APPROVAL";
            public const string DEFAULT_TASK_PRIORITY = "DEFAULT_TASK_PRIORITY";
            public const string MAX_FILE_UPLOAD_SIZE_MB = "MAX_FILE_UPLOAD_SIZE_MB";
            public const string ALLOWED_FILE_EXTENSIONS = "ALLOWED_FILE_EXTENSIONS";
            public const string ONBOARDING_COMPLETION_THRESHOLD = "ONBOARDING_COMPLETION_THRESHOLD";
        }

        // =============================================================================
        // BUSINESS LOGIC HELPERS
        // =============================================================================

        public static class BusinessLogic
        {
            public static string GetDefaultInstructionsByCategory(string category)
            {
                return category switch
                {
                    TaskCategories.ORIENTATION =>
                        "Please attend the scheduled orientation session. Contact HR if you need to reschedule.",

                    TaskCategories.DOCUMENTATION =>
                        "Complete and submit the required documentation. Ensure all information is accurate and up-to-date.",

                    TaskCategories.FINANCIAL =>
                        "Set up your financial information including direct deposit and tax withholdings. Contact payroll for assistance.",

                    TaskCategories.EQUIPMENT =>
                        "Collect and set up your assigned equipment. Test all functionality and report any issues to IT support.",

                    TaskCategories.TRAINING =>
                        "Complete the assigned training modules. Ensure you understand the material and pass any required assessments.",

                    TaskCategories.PERSONAL =>
                        "Update your personal information and emergency contacts. Verify all details are current.",

                    _ => "Complete this onboarding task as assigned. Contact your supervisor or HR if you need assistance."
                };
            }
        }
    }

    // =============================================================================
    // VALIDATION ATTRIBUTES
    // =============================================================================

    public class OnboardingTaskStatusAttribute : ValidationAttribute
    {
        private static readonly string[] ValidStatuses =
        {
            OnboardingConstants.TaskStatuses.PENDING,
            OnboardingConstants.TaskStatuses.IN_PROGRESS,
            OnboardingConstants.TaskStatuses.COMPLETED,
            OnboardingConstants.TaskStatuses.OVERDUE
        };

        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            return ValidStatuses.Contains(value.ToString()!);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The field {name} must be one of: {string.Join(", ", ValidStatuses)}";
        }
    }

    public class OnboardingTaskPriorityAttribute : ValidationAttribute
    {
        private static readonly string[] ValidPriorities =
        {
            OnboardingConstants.TaskPriorities.LOW,
            OnboardingConstants.TaskPriorities.MEDIUM,
            OnboardingConstants.TaskPriorities.HIGH
        };

        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            return ValidPriorities.Contains(value.ToString()!);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The field {name} must be one of: {string.Join(", ", ValidPriorities)}";
        }
    }

    public class OnboardingTaskCategoryAttribute : ValidationAttribute
    {
        private static readonly string[] ValidCategories =
        {
            OnboardingConstants.TaskCategories.ORIENTATION,
            OnboardingConstants.TaskCategories.DOCUMENTATION,
            OnboardingConstants.TaskCategories.FINANCIAL,
            OnboardingConstants.TaskCategories.PERSONAL,
            OnboardingConstants.TaskCategories.EQUIPMENT,
            OnboardingConstants.TaskCategories.TRAINING
        };

        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            return ValidCategories.Contains(value.ToString()!);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The field {name} must be one of: {string.Join(", ", ValidCategories)}";
        }
    }

    // =============================================================================
    // CUSTOM VALIDATION RESULT CLASS
    // =============================================================================

    public class OnboardingValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}