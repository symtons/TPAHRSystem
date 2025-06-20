// TPAHRSystem.Core/Models/RecentActivity.cs
namespace TPAHRSystem.Core.Models
{
    public class RecentActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? EmployeeId { get; set; }
        public int ActivityTypeId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Employee? Employee { get; set; }
        public virtual ActivityType ActivityType { get; set; } = null!;
    }
}