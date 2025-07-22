// =============================================================================
// COMPLETE FIXED LEAVE REQUEST MODEL
// File: TPAHRSystem.Core/Models/LeaveRequest.cs (REPLACE ENTIRE FILE)
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string LeaveType { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public int DaysRequested { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // FIXED: Changed from ReviewedBy to ReviewedById to avoid foreign key conflict
        public int? ReviewedById { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Additional workflow properties
        [MaxLength(20)]
        public string? WorkflowStatus { get; set; }

        public int? CurrentApprovalStep { get; set; }

        // FIXED: Explicit foreign key attributes to resolve EF Core confusion
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("ReviewedById")]
        public virtual Employee? ReviewedBy { get; set; }

        // Computed Properties (Not mapped to database)
        [NotMapped]
        public int TotalDays => (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;

        [NotMapped]
        public string StatusDisplay => Status switch
        {
            "Pending" => "Pending Approval",
            "Approved" => "Approved",
            "Rejected" => "Rejected",
            "Cancelled" => "Cancelled",
            _ => Status
        };

        [NotMapped]
        public bool CanBeModified => Status == "Pending" && StartDate > DateOnly.FromDateTime(DateTime.Now);

        [NotMapped]
        public bool IsPastDue => StartDate < DateOnly.FromDateTime(DateTime.Now) && Status == "Pending";

        [NotMapped]
        public string FormattedDateRange => $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";

        [NotMapped]
        public string EmployeeName => Employee?.FullName ?? "Unknown";

        [NotMapped]
        public string ReviewerName => ReviewedBy?.FullName ?? "Not Reviewed";
    }
}