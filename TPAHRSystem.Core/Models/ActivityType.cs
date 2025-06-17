// TPAHRSystem.Core/Models/ActivityType.cs
namespace TPAHRSystem.Core.Models
{
    public class ActivityType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconName { get; set; }
        public string Color { get; set; } = "#1976d2";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
    }
}