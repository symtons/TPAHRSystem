// =============================================================================
// CORRECTED ONBOARDING DOCUMENT MODEL
// File: TPAHRSystem.Core/Models/OnboardingDocument.cs (Replace existing)
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

        // Enhanced Properties
        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsApproved { get; set; } = false;

        public int? ApprovedById { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [MaxLength(500)]
        public string? ApprovalNotes { get; set; }

        public bool IsRejected { get; set; } = false;

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [MaxLength(32)]
        public string? FileHash { get; set; } // For duplicate detection

        public bool IsVirusScanRequired { get; set; } = false;

        public bool IsVirusScanPassed { get; set; } = false;

        public DateTime? VirusScanDate { get; set; }

        [MaxLength(500)]
        public string? VirusScanResult { get; set; }

        public int Version { get; set; } = 1;

        public int? PreviousVersionId { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; }

        public bool IsConfidential { get; set; } = false;

        [MaxLength(100)]
        public string? ExternalSystemId { get; set; }

        public DateTime? LastAccessedDate { get; set; }

        public int AccessCount { get; set; } = 0;

        // Navigation Properties
        [ForeignKey("TaskId")]
        public virtual OnboardingTask Task { get; set; } = null!;

        [ForeignKey("UploadedById")]
        public virtual Employee? UploadedBy { get; set; }

        [ForeignKey("ApprovedById")]
        public virtual Employee? ApprovedBy { get; set; }

        [ForeignKey("PreviousVersionId")]
        public virtual OnboardingDocument? PreviousVersion { get; set; }

        public virtual ICollection<OnboardingDocument> NextVersions { get; set; } = new List<OnboardingDocument>();

        // Enhanced Computed Properties
        [NotMapped]
        public string FileSizeDisplay
        {
            get
            {
                if (FileSize == 0) return "0 B";

                string[] sizes = { "B", "KB", "MB", "GB" };
                var order = 0;
                var size = (double)FileSize;

                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size /= 1024;
                }

                return $"{size:0.##} {sizes[order]}";
            }
        }

        [NotMapped]
        public string StatusDisplayName
        {
            get
            {
                if (IsRejected) return "Rejected";
                if (!Uploaded) return Required ? "Required - Not Uploaded" : "Optional - Not Uploaded";
                if (IsApproved) return "Approved";
                if (IsVirusScanRequired && !IsVirusScanPassed) return "Pending Virus Scan";
                return "Uploaded - Pending Review";
            }
        }

        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate.Value;

        [NotMapped]
        public bool IsExpiringSoon => ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate.Value.AddDays(-30);

        [NotMapped]
        public bool CanBeApproved => Uploaded && !IsRejected && !IsApproved &&
                                    (!IsVirusScanRequired || IsVirusScanPassed);

        [NotMapped]
        public bool HasLatestVersion => !NextVersions.Any();

        [NotMapped]
        public string SecurityStatus
        {
            get
            {
                if (!Uploaded) return "N/A";
                if (IsVirusScanRequired && !IsVirusScanPassed) return "Pending Scan";
                if (IsVirusScanRequired && IsVirusScanPassed) return "Scan Passed";
                return "No Scan Required";
            }
        }

        // Methods
        public void MarkAsUploaded(string filePath, string fileName, string contentType, long fileSize, int uploadedById)
        {
            Uploaded = true;
            FilePath = filePath;
            FileName = fileName;
            ContentType = contentType;
            FileSize = fileSize;
            UploadedDate = DateTime.UtcNow;
            UploadedById = uploadedById;
            IsRejected = false; // Reset rejection status on new upload
            RejectionReason = null;
        }

        public void Approve(int approvedById, string? notes = null)
        {
            if (!CanBeApproved) return;

            IsApproved = true;
            ApprovedById = approvedById;
            ApprovedDate = DateTime.UtcNow;
            ApprovalNotes = notes;
            IsRejected = false;
        }

        public void Reject(string reason, int? rejectedById = null)
        {
            IsRejected = true;
            RejectionReason = reason;
            IsApproved = false;
            ApprovedById = rejectedById;
            ApprovedDate = DateTime.UtcNow;
        }

        public void UpdateVirusScanResult(bool passed, string? result = null)
        {
            IsVirusScanPassed = passed;
            VirusScanDate = DateTime.UtcNow;
            VirusScanResult = result;

            if (!passed)
            {
                IsRejected = true;
                RejectionReason = "Failed virus scan: " + result;
            }
        }

        public void RecordAccess()
        {
            LastAccessedDate = DateTime.UtcNow;
            AccessCount++;
        }

        public OnboardingDocument CreateNewVersion(string filePath, string fileName, string contentType, long fileSize, int uploadedById)
        {
            var newVersion = new OnboardingDocument
            {
                Name = Name,
                DocumentType = DocumentType,
                Required = Required,
                Description = Description,
                Instructions = Instructions,
                ExpiryDate = ExpiryDate,
                TaskId = TaskId,
                IsVirusScanRequired = IsVirusScanRequired,
                IsConfidential = IsConfidential,
                Tags = Tags,
                Version = Version + 1,
                PreviousVersionId = Id
            };

            newVersion.MarkAsUploaded(filePath, fileName, contentType, fileSize, uploadedById);
            return newVersion;
        }

        public OnboardingValidationResult ValidateForUpload()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrEmpty(Name))
                errors.Add("Document name is required");

            if (string.IsNullOrEmpty(DocumentType))
                errors.Add("Document type is required");

            if (FileSize > 50 * 1024 * 1024) // 50MB limit
                errors.Add("File size exceeds 50MB limit");

            if (IsExpired)
                warnings.Add("Document has expired and may need to be updated");

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".txt" };
            var extension = Path.GetExtension(FileName)?.ToLower();

            if (!string.IsNullOrEmpty(FileName) && !allowedExtensions.Contains(extension))
                warnings.Add($"File type '{extension}' may not be supported");

            return new OnboardingValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors,
                Warnings = warnings
            };
        }

        public Dictionary<string, object> GetMetadata()
        {
            return new Dictionary<string, object>
            {
                ["Id"] = Id,
                ["Name"] = Name,
                ["DocumentType"] = DocumentType,
                ["Required"] = Required,
                ["Status"] = StatusDisplayName,
                ["FileSize"] = FileSizeDisplay,
                ["UploadedDate"] = UploadedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not uploaded",
                ["IsApproved"] = IsApproved,
                ["IsExpired"] = IsExpired,
                ["Version"] = Version,
                ["AccessCount"] = AccessCount,
                ["SecurityStatus"] = SecurityStatus
            };
        }

        // Static Factory Methods
        public static OnboardingDocument CreateRequired(string name, string documentType, int taskId, string? description = null)
        {
            return new OnboardingDocument
            {
                Name = name,
                DocumentType = documentType,
                TaskId = taskId,
                Required = true,
                Description = description,
                Instructions = $"Please upload your {name.ToLower()}. This document is required to proceed with onboarding."
            };
        }

        public static OnboardingDocument CreateOptional(string name, string documentType, int taskId, string? description = null)
        {
            return new OnboardingDocument
            {
                Name = name,
                DocumentType = documentType,
                TaskId = taskId,
                Required = false,
                Description = description,
                Instructions = $"Please upload your {name.ToLower()} if available. This document is optional."
            };
        }

        public static OnboardingDocument CreateWithExpiry(string name, string documentType, int taskId, DateTime expiryDate, bool required = true)
        {
            return new OnboardingDocument
            {
                Name = name,
                DocumentType = documentType,
                TaskId = taskId,
                Required = required,
                ExpiryDate = expiryDate,
                Instructions = $"Please upload your {name.ToLower()}. This document expires on {expiryDate:yyyy-MM-dd}."
            };
        }
    }
}