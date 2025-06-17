// TPAHRSystem.Core/Models/RecentActivity.cs
namespace TPAHRSystem.Core.Models
{
    public class RecentActivity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid ActivityTypeId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public string? Details { get; set; }
        public string? IPAddress { get; set; }
        public bool IsHighlighted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Employee? Employee { get; set; }
        public virtual ActivityType ActivityType { get; set; } = null!;
    }
}