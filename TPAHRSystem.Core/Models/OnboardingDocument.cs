// =============================================================================
// OnboardingDocument Model
// File: TPAHRSystem.Core/Models/OnboardingDocument.cs
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class OnboardingDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DocumentType { get; set; } = string.Empty;

        public bool Required { get; set; } = false;
        public bool Uploaded { get; set; } = false;

        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime? UploadedDate { get; set; }

        // Foreign Keys
        public int TaskId { get; set; }
        public int? UploadedById { get; set; }

        // Navigation Properties
        [ForeignKey("TaskId")]
        public virtual OnboardingTask Task { get; set; } = null!;

        [ForeignKey("UploadedById")]
        public virtual Employee? UploadedBy { get; set; }
    }
}