// =============================================================================
// CORRECTED EMPLOYEE MODEL - MATCHES DATABASE STRUCTURE
// File: TPAHRSystem.Core/Models/Employee.cs (REPLACE ENTIRE FILE)
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool? IsOnboardingLocked { get; set; } = true;


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

        public DateTime HireDate { get; set; } = DateTime.UtcNow;

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

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // **ADDED: Only the onboarding column that actually exists in database**
        public DateTime? OnboardingCompletedDate { get; set; }

        // Foreign Keys
        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        public int? ManagerId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [ForeignKey("ManagerId")]
        public virtual Employee? Manager { get; set; }

        // Navigation Properties for related entities
        public virtual ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
        public virtual ICollection<OnboardingTask> OnboardingTasks { get; set; } = new List<OnboardingTask>();
        public virtual ICollection<OnboardingChecklist> OnboardingChecklists { get; set; } = new List<OnboardingChecklist>();
        public virtual ICollection<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();

        // **COMPUTED PROPERTIES - NOT MAPPED TO DATABASE**
        // These are calculated from related OnboardingTasks, not stored in Employee table
        [NotMapped]
        public string OnboardingStatus
        {
            get
            {
                if (OnboardingTasks == null || !OnboardingTasks.Any())
                    return "NOT_STARTED";

                var totalTasks = OnboardingTasks.Count(t => !t.IsTemplate);
                var completedTasks = OnboardingTasks.Count(t => !t.IsTemplate && t.Status == "COMPLETED");

                if (completedTasks == totalTasks) return "COMPLETED";
                if (completedTasks > 0) return "IN_PROGRESS";
                return "NOT_STARTED";
            }
        }

        [NotMapped]
        public decimal OnboardingCompletionPercentage
        {
            get
            {
                if (OnboardingTasks == null || !OnboardingTasks.Any())
                    return 0;

                var totalTasks = OnboardingTasks.Count(t => !t.IsTemplate);
                if (totalTasks == 0) return 0;

                var completedTasks = OnboardingTasks.Count(t => !t.IsTemplate && t.Status == "COMPLETED");
                return Math.Round((decimal)completedTasks / totalTasks * 100, 2);
            }
        }

        [NotMapped]
        public int OnboardingTasksTotal
        {
            get => OnboardingTasks?.Count(t => !t.IsTemplate) ?? 0;
        }

        [NotMapped]
        public int OnboardingTasksCompleted
        {
            get => OnboardingTasks?.Count(t => !t.IsTemplate && t.Status == "COMPLETED") ?? 0;
        }
       

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string DisplayName => $"{FirstName} {LastName} ({EmployeeNumber})";
    }
}