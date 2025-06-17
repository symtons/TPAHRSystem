// TPAHRSystem.Core/Models/DashboardStat.cs
namespace TPAHRSystem.Core.Models
{
    public class DashboardStat
    {
        public int Id { get; set; }
        public string StatKey { get; set; } = string.Empty;
        public string StatName { get; set; } = string.Empty;
        public string StatValue { get; set; } = string.Empty;
        public string StatColor { get; set; } = "primary";
        public string? IconName { get; set; }
        public string? Subtitle { get; set; }
        public string? ApplicableRoles { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}