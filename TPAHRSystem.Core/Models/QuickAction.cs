// TPAHRSystem.Core/Models/QuickAction.cs
namespace TPAHRSystem.Core.Models
{
    public class QuickAction
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ActionKey { get; set; } = string.Empty;
        public string? IconName { get; set; }
        public string? Route { get; set; }
        public string Color { get; set; } = "#1976d2";
        public string? ApplicableRoles { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}