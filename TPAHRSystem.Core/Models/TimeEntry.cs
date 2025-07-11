// File: TPAHRSystem.Core/Models/TimeEntry.cs (Replace existing)
namespace TPAHRSystem.Core.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
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