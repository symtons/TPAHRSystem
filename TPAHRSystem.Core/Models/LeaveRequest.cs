// TPAHRSystem.Core/Models/LeaveRequest.cs
namespace TPAHRSystem.Core.Models
{
    public class LeaveRequest
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysRequested { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Employee? ReviewedByEmployee { get; set; }
    }
}