using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPAHRSystem.Core.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public DateOnly EffectiveDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;

        // Computed Properties
        public string DayName => ((DayOfWeek)DayOfWeek).ToString();
        public decimal ScheduledHours => (decimal)(EndTime - StartTime).TotalHours;
    }
}