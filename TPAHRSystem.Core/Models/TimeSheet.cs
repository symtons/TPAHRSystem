using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPAHRSystem.Core.Models
{
    public class TimeSheet
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public decimal TotalHours { get; set; } = 0;
        public decimal RegularHours { get; set; } = 0;
        public decimal OvertimeHours { get; set; } = 0;
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected
        public DateTime? SubmittedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Employee? Approver { get; set; }
    }
}
