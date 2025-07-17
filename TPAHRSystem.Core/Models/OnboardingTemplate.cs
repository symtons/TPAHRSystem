// =============================================================================
// COMPLETE ONBOARDING TEMPLATE MODEL - FIXED ALL COMPILATION ERRORS
// File: TPAHRSystem.Core/Models/OnboardingTemplate.cs (Replace existing)
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ForRole { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ForDepartment { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        public int CreatedById { get; set; }
        public int? ModifiedById { get; set; }

        // Enhanced Properties
        [MaxLength(20)]
        public string TemplateVersion { get; set; } = "1.0";

        [MaxLength(100)]
        public string? EmployeeType { get; set; } // FULL_TIME, PART_TIME, CONTRACT, INTERN

        [MaxLength(100)]
        public string? Location { get; set; }

        public int EstimatedDaysToComplete { get; set; } = 14;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? EstimatedHoursTotal { get; set; }

        public bool RequiresManagerApproval { get; set; } = false;

        public bool AutoAssign { get; set; } = false;

        [MaxLength(500)]
        public string? Prerequisites { get; set; }

        [MaxLength(500)]
        public string? SuccessCriteria { get; set; }

        public int Priority { get; set; } = 1; // For auto-assignment ordering

        public int UsageCount { get; set; } = 0; // Track how many times used

        public DateTime? LastUsedDate { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; } // Comma-separated tags for categorization

        [MaxLength(100)]
        public string? ExternalSystemId { get; set; } // For integration

        public bool IsSystemTemplate { get; set; } = false; // Cannot be deleted

        // Navigation Properties
        [ForeignKey("CreatedById")]
        public virtual Employee CreatedBy { get; set; } = null!;

        [ForeignKey("ModifiedById")]
        public virtual Employee? ModifiedBy { get; set; }

        public virtual ICollection<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();
        public virtual ICollection<OnboardingChecklist> Checklists { get; set; } = new List<OnboardingChecklist>();

        // Computed Properties
        [NotMapped]
        public int TaskCount => Tasks?.Count(t => t.IsTemplate) ?? 0;

        [NotMapped]
        public int RequiredTaskCount => Tasks?.Count(t => t.IsTemplate && t.RequiresApproval) ?? 0;

        [NotMapped]
        public int DocumentCount => Tasks?.Where(t => t.IsTemplate).Sum(t => t.Documents.Count) ?? 0;

        [NotMapped]
        public string[] TagList
        {
            get
            {
                if (string.IsNullOrEmpty(Tags))
                    return Array.Empty<string>();

                return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(tag => tag.Trim())
                          .ToArray();
            }
        }

        [NotMapped]
        public bool IsRecentlyUsed => LastUsedDate.HasValue && LastUsedDate.Value > DateTime.UtcNow.AddDays(-30);

        [NotMapped]
        public bool IsPopular => UsageCount > 10;

        [NotMapped]
        public decimal? CalculatedHoursTotal
        {
            get
            {
                if (EstimatedHoursTotal.HasValue) return EstimatedHoursTotal.Value;

                var totalHours = Tasks?.Where(t => t.IsTemplate)
                    .Sum(t => ParseEstimatedHours(t.EstimatedTime)) ?? 0;

                return totalHours > 0 ? (decimal)totalHours : null;
            }
        }

        // Methods
        public void IncrementUsage()
        {
            UsageCount++;
            LastUsedDate = DateTime.UtcNow;
            if (!EstimatedHoursTotal.HasValue)
            {
                EstimatedHoursTotal = CalculatedHoursTotal;
            }
        }

        public void UpdateModification(int modifiedById)
        {
            ModifiedById = modifiedById;
            ModifiedDate = DateTime.UtcNow;
        }

        public bool MatchesEmployeeCriteria(Employee employee)
        {
            // Check role match (Position property)
            if (!string.Equals(employee.Position, ForRole, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check department match (using Department navigation property)
            if (!string.IsNullOrEmpty(ForDepartment) && employee.Department != null &&
                !string.Equals(employee.Department.Name, ForDepartment, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check employee type match (if specified)
            //if (!string.IsNullOrEmpty(EmployeeType) &&
            //    !string.Equals(employee.EmployeeType, EmployeeType, StringComparison.OrdinalIgnoreCase))
            //    return false;

            //// Check location match (if specified)
            //if (!string.IsNullOrEmpty(Location) &&
            //    !string.Equals(employee.Location, Location, StringComparison.OrdinalIgnoreCase))
            //    return false;

            return true;
        }

        public OnboardingTemplate CreateCopy(string newName, int createdById)
        {
            var copy = new OnboardingTemplate
            {
                Name = newName,
                Description = Description + " (Copy)",
                ForRole = ForRole,
                ForDepartment = ForDepartment,
                EmployeeType = EmployeeType,
                Location = Location,
                EstimatedDaysToComplete = EstimatedDaysToComplete,
                EstimatedHoursTotal = EstimatedHoursTotal,
                RequiresManagerApproval = RequiresManagerApproval,
                Prerequisites = Prerequisites,
                SuccessCriteria = SuccessCriteria,
                Tags = Tags,
                CreatedById = createdById,
                CreatedDate = DateTime.UtcNow,
                TemplateVersion = "1.0",
                IsActive = false // Start inactive for review
            };

            // Copy tasks
            foreach (var task in Tasks.Where(t => t.IsTemplate))
            {
                var taskCopy = new OnboardingTask
                {
                    Title = task.Title,
                    Description = task.Description,
                    Category = task.Category,
                    Priority = task.Priority,
                    EstimatedTime = task.EstimatedTime,
                    Instructions = task.Instructions,
                    RequiresApproval = task.RequiresApproval,
                    SortOrder = task.SortOrder,
                    IsTemplate = true
                };

                copy.Tasks.Add(taskCopy);
            }

            return copy;
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;

            var currentTags = TagList.ToList();
            if (!currentTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                currentTags.Add(tag.Trim());
                Tags = string.Join(", ", currentTags);
            }
        }

        public void RemoveTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;

            var currentTags = TagList.Where(t =>
                !string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)).ToList();

            Tags = currentTags.Count > 0 ? string.Join(", ", currentTags) : null;
        }

        public OnboardingValidationResult ValidateForActivation()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrEmpty(Name))
                errors.Add("Template name is required");

            if (string.IsNullOrEmpty(ForRole))
                errors.Add("Target role is required");

            if (TaskCount == 0)
                errors.Add("Template must have at least one task");

            if (TaskCount > 50)
                warnings.Add("Template has many tasks - consider breaking into multiple templates");

            var duplicateTasks = Tasks.Where(t => t.IsTemplate)
                .GroupBy(t => t.Title)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateTasks.Any())
                warnings.Add($"Duplicate task titles found: {string.Join(", ", duplicateTasks)}");

            return new OnboardingValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }

        private static double ParseEstimatedHours(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr)) return 0;

            timeStr = timeStr.ToLower();

            if (timeStr.Contains("hour"))
            {
                var hourMatch = Regex.Match(timeStr, @"(\d+(?:\.\d+)?)\s*hours?");
                if (hourMatch.Success && double.TryParse(hourMatch.Groups[1].Value, out var hours))
                    return hours;
            }

            if (timeStr.Contains("day"))
            {
                var dayMatch = Regex.Match(timeStr, @"(\d+(?:\.\d+)?)\s*days?");
                if (dayMatch.Success && double.TryParse(dayMatch.Groups[1].Value, out var days))
                    return days * 8;
            }

            if (timeStr.Contains("minute"))
            {
                var minuteMatch = Regex.Match(timeStr, @"(\d+)\s*minutes?");
                if (minuteMatch.Success && int.TryParse(minuteMatch.Groups[1].Value, out var minutes))
                    return minutes / 60.0;
            }

            return 1.0;
        }

        // Static Factory Methods
        public static OnboardingTemplate CreateBasic(string name, string forRole, int createdById)
        {
            return new OnboardingTemplate
            {
                Name = name,
                ForRole = forRole,
                CreatedById = createdById,
                CreatedDate = DateTime.UtcNow,
                TemplateVersion = "1.0",
                IsActive = false // Start inactive for setup
            };
        }

        public static OnboardingTemplate CreateFromExisting(OnboardingTemplate source, string newName, int createdById)
        {
            return source.CreateCopy(newName, createdById);
        }

        public static OnboardingTemplate CreateStandardTemplate(string role, int createdById)
        {
            var template = new OnboardingTemplate
            {
                Name = $"Standard {role} Onboarding",
                Description = $"Standard onboarding process for {role} positions",
                ForRole = role,
                CreatedById = createdById,
                CreatedDate = DateTime.UtcNow,
                TemplateVersion = "1.0",
                EstimatedDaysToComplete = 14,
                IsActive = true,
                IsSystemTemplate = true
            };

            // Add standard tasks based on role
            AddStandardTasks(template, role);

            return template;
        }

        private static void AddStandardTasks(OnboardingTemplate template, string role)
        {
            var standardTasks = new List<OnboardingTask>
            {
                new OnboardingTask
                {
                    Title = "Welcome Orientation",
                    Description = "Attend company welcome orientation session",
                    Category = OnboardingConstants.TaskCategories.ORIENTATION,
                    Priority = OnboardingConstants.TaskPriorities.HIGH,
                    EstimatedTime = "2 hours",
                    Instructions = "Report to HR at 9:00 AM for orientation session",
                    IsTemplate = true,
                    SortOrder = 1
                },
                new OnboardingTask
                {
                    Title = "Complete Tax Forms",
                    Description = "Fill out W-4 and other required tax documentation",
                    Category = OnboardingConstants.TaskCategories.FINANCIAL,
                    Priority = OnboardingConstants.TaskPriorities.HIGH,
                    EstimatedTime = "30 minutes",
                    Instructions = "Complete all tax forms with HR department",
                    IsTemplate = true,
                    SortOrder = 2
                },
                new OnboardingTask
                {
                    Title = "IT Equipment Setup",
                    Description = "Receive and configure work laptop and other equipment",
                    Category = OnboardingConstants.TaskCategories.EQUIPMENT,
                    Priority = OnboardingConstants.TaskPriorities.HIGH,
                    EstimatedTime = "1.5 hours",
                    Instructions = "Visit IT department for equipment assignment and setup",
                    IsTemplate = true,
                    SortOrder = 3
                },
                new OnboardingTask
                {
                    Title = "Safety Training",
                    Description = "Complete mandatory workplace safety training",
                    Category = OnboardingConstants.TaskCategories.TRAINING,
                    Priority = OnboardingConstants.TaskPriorities.MEDIUM,
                    EstimatedTime = "45 minutes",
                    Instructions = "Complete online safety training modules",
                    IsTemplate = true,
                    SortOrder = 4
                }
            };

            // Add role-specific tasks
            switch (role.ToLower())
            {
                case "hr manager":
                    standardTasks.Add(new OnboardingTask
                    {
                        Title = "HRIS System Training",
                        Description = "Learn Human Resources Information System",
                        Category = OnboardingConstants.TaskCategories.TRAINING,
                        Priority = OnboardingConstants.TaskPriorities.HIGH,
                        EstimatedTime = "4 hours",
                        Instructions = "Complete HRIS training with senior HR staff",
                        IsTemplate = true,
                        SortOrder = 5
                    });
                    break;

                case "field staff":
                    standardTasks.Add(new OnboardingTask
                    {
                        Title = "Mobile App Training",
                        Description = "Learn to use company mobile application",
                        Category = OnboardingConstants.TaskCategories.TRAINING,
                        Priority = OnboardingConstants.TaskPriorities.HIGH,
                        EstimatedTime = "1 hour",
                        Instructions = "Download and configure mobile app with supervisor",
                        IsTemplate = true,
                        SortOrder = 5
                    });
                    break;

                case "admin staff":
                    standardTasks.Add(new OnboardingTask
                    {
                        Title = "Office Procedures Training",
                        Description = "Learn office procedures and administrative systems",
                        Category = OnboardingConstants.TaskCategories.TRAINING,
                        Priority = OnboardingConstants.TaskPriorities.MEDIUM,
                        EstimatedTime = "2 hours",
                        Instructions = "Training with office manager on procedures",
                        IsTemplate = true,
                        SortOrder = 5
                    });
                    break;
            }

            template.Tasks = standardTasks;
        }
    }
}