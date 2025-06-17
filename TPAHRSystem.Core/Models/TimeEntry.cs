// TPAHRSystem.Core/Models/TimeEntry.cs
namespace TPAHRSystem.Core.Models
{
    public class TimeEntry
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public decimal? TotalHours { get; set; }
        public string Status { get; set; } = "Active";
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
    }
}