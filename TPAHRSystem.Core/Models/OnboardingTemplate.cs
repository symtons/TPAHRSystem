// =============================================================================
// OnboardingTemplate Model
// File: TPAHRSystem.Core/Models/OnboardingTemplate.cs
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ForRole { get; set; } = string.Empty; // Employee role this template is for

        [MaxLength(50)]
        public string ForDepartment { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        public int CreatedById { get; set; }

        // Navigation Properties
        [ForeignKey("CreatedById")]
        public virtual Employee CreatedBy { get; set; } = null!;

        public virtual ICollection<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();
    }
}