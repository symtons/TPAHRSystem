using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class Employee
    {
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

        // Navigation Properties
        public User? User { get; set; }
        public Department? Department { get; set; }
        public Employee? Manager { get; set; }
        public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

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

        // Computed Properties
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
    }
}