// =============================================================================
// SIMPLE DTOs FOR TIME ATTENDANCE
// File: TPAHRSystem.Core/DTOs/TimeAttendanceDTOs.cs
// =============================================================================

namespace TPAHRSystem.Core.DTOs
{
    public class ClockInOutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TimeEntryDto? TimeEntry { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
    }

    public class TimeEntryDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public decimal? TotalHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TimeSheetDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public decimal TotalHours { get; set; }
        public decimal RegularHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public List<TimeEntryDto> TimeEntries { get; set; } = new();
        public string WeekDisplay => $"{WeekStartDate:MMM dd} - {WeekEndDate:MMM dd, yyyy}";
    }

    public class AttendanceSummaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public decimal RegularHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public int DaysWorked { get; set; }
        public int DaysScheduled { get; set; }
        public decimal AttendanceRate { get; set; }
        public int LateArrivals { get; set; }
        public int EarlyDepartures { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ScheduleDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal ScheduledHours { get; set; }
        public bool IsActive { get; set; }
        public DateOnly EffectiveDate { get; set; }
    }

    public class TimeAttendanceStatsDto
    {
        public decimal CurrentWeekHours { get; set; }
        public decimal OvertimeThisWeek { get; set; }
        public decimal AttendanceRate { get; set; }
        public int LateArrivals { get; set; }
        public int PendingTimesheets { get; set; }
        public int ActiveShifts { get; set; }
        public bool IsClockedIn { get; set; }
        public TimeEntryDto? CurrentTimeEntry { get; set; }
    }
}