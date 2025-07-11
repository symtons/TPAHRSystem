// =============================================================================
// OnboardingChecklist Model
// File: TPAHRSystem.Core/Models/OnboardingChecklist.cs
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingChecklist
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public int TemplateId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "ASSIGNED"; // ASSIGNED, IN_PROGRESS, COMPLETED

        public int AssignedById { get; set; }

        // Navigation Properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("TemplateId")]
        public virtual OnboardingTemplate Template { get; set; } = null!;

        [ForeignKey("AssignedById")]
        public virtual Employee AssignedBy { get; set; } = null!;
    }
}