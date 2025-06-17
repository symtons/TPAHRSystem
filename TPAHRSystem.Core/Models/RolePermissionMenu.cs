// TPAHRSystem.Core/Models/RoleMenuPermission.cs
namespace TPAHRSystem.Core.Models
{
    public class RoleMenuPermission
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public int MenuItemId { get; set; }
        public bool CanView { get; set; } = true;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual MenuItem MenuItem { get; set; } = null!;
    }
}