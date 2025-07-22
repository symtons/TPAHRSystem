// =============================================================================
// FIXED TIMESHEET MODEL - RESOLVE FOREIGN KEY CONFLICT
// File: TPAHRSystem.Core/Models/TimeSheet.cs (REPLACE ENTIRE FILE)
// =============================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class TimeSheet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public DateOnly WeekStartDate { get; set; }

        [Required]
        public DateOnly WeekEndDate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalHours { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal RegularHours { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal OvertimeHours { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected

        public DateTime? SubmittedAt { get; set; }

        // FIXED: Changed from ApprovedBy to ApprovedById to avoid foreign key conflict
        public int? ApprovedById { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Additional properties for better timesheet management
        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? PayPeriod { get; set; }

        public bool IsLocked { get; set; } = false;

        // FIXED: Explicit foreign key attributes to resolve EF Core confusion
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("ApprovedById")]
        public virtual Employee? Approver { get; set; }

        // Computed Properties (Not mapped to database)
        [NotMapped]
        public string StatusDisplay => Status switch
        {
            "Draft" => "Draft",
            "Submitted" => "Submitted for Approval",
            "Approved" => "Approved",
            "Rejected" => "Rejected",
            _ => Status
        };

        [NotMapped]
        public bool CanBeModified => Status == "Draft" && !IsLocked;

        [NotMapped]
        public bool CanBeSubmitted => Status == "Draft" && TotalHours > 0 && !IsLocked;

        [NotMapped]
        public string WeekPeriod => $"{WeekStartDate:MMM dd} - {WeekEndDate:MMM dd, yyyy}";

        [NotMapped]
        public string EmployeeName => Employee?.FullName ?? "Unknown";

        [NotMapped]
        public string ApproverName => Approver?.FullName ?? "Not Approved";

        [NotMapped]
        public bool HasOvertime => OvertimeHours > 0;

        [NotMapped]
        public decimal OvertimeRate => 1.5m; // 1.5x regular rate for overtime

        [NotMapped]
        public bool IsCurrentWeek
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                return today >= WeekStartDate && today <= WeekEndDate;
            }
        }
    }
}