// TPAHRSystem.Core/Models/Employee.cs
namespace TPAHRSystem.Core.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int DepartmentId { get; set; }
        public string? JobTitle { get; set; }
        public string? Position { get; set; }
        public string? EmployeeType { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsActive {  get; set; }
        public string Status { get; set; } = "Active";
        public int? ManagerId { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties (Essential only)
        public virtual User? User { get; set; }
        public virtual Department? Department { get; set; }
        public virtual Employee? Manager { get; set; }
        public virtual ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

        // Temporarily removed until we create these models:
        // public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        // public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        // public virtual ICollection<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();

        // Computed Properties
        public string FullName => $"{FirstName} {LastName}";
    }
}