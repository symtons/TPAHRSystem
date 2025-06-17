// TPAHRSystem.Core/Models/Department.cs
namespace TPAHRSystem.Core.Models
{
    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}