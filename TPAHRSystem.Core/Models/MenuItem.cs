// TPAHRSystem.Core/Models/MenuItem.cs
namespace TPAHRSystem.Core.Models
{
    public class MenuItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public Guid? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string? RequiredPermission { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual MenuItem? Parent { get; set; }
        public virtual ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();
        public virtual ICollection<RoleMenuPermission> RolePermissions { get; set; } = new List<RoleMenuPermission>();
    }
}