// =============================================================================
// MOCK TIME ATTENDANCE SERVICE FOR TESTING
// File: TPAHRSystem.API/Services/MockTimeAttendanceService.cs
// =============================================================================

using TPAHRSystem.Core.DTOs;

namespace TPAHRSystem.API.Services
{
    public interface ITimeAttendanceService
    {
        Task<ClockInOutResponse> ClockInAsync(int employeeId, string location);
        Task<ClockInOutResponse> ClockOutAsync(int employeeId);
        Task<TimeAttendanceStatsDto> GetTimeAttendanceStatsAsync(int employeeId);
        Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
        Task<TimeSheetDto?> GetCurrentWeekTimesheetAsync(int employeeId);
        Task<IEnumerable<TimeSheetDto>> GetTimesheetsAsync(int employeeId, int page = 1, int pageSize = 10);
        Task<bool> SubmitTimesheetAsync(int timesheetId, int employeeId);
        Task<IEnumerable<TimeSheetDto>> GetPendingTimesheetsAsync(string role);
        Task<bool> ApproveTimesheetAsync(int timesheetId, int approverId);
        Task<bool> RejectTimesheetAsync(int timesheetId, int approverId, string reason);
        Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceSummaryAsync(DateTime startDate, DateTime endDate, string role);
        Task<IEnumerable<ScheduleDto>> GetEmployeeScheduleAsync(int employeeId);
    }

    public class MockTimeAttendanceService : ITimeAttendanceService
    {
        private readonly ILogger<MockTimeAttendanceService> _logger;

        public MockTimeAttendanceService(ILogger<MockTimeAttendanceService> logger)
        {
            _logger = logger;
        }

        public async Task<ClockInOutResponse> ClockInAsync(int employeeId, string location)
        {
            _logger.LogInformation($"Mock: Clocking in employee {employeeId} at {location}");

            await Task.Delay(500); // Simulate API delay

            return new ClockInOutResponse
            {
                Success = true,
                Message = "Successfully clocked in",
                CurrentStatus = "clocked-in",
                TimeEntry = new TimeEntryDto
                {
                    Id = 1,
                    EmployeeId = employeeId,
                    EmployeeName = "Test Employee",
                    ClockIn = DateTime.Now,
                    Status = "Active",
                    Location = location,
                    CreatedAt = DateTime.Now
                }
            };
        }

        public async Task<ClockInOutResponse> ClockOutAsync(int employeeId)
        {
            _logger.LogInformation($"Mock: Clocking out employee {employeeId}");

            await Task.Delay(500); // Simulate API delay

            return new ClockInOutResponse
            {
                Success = true,
                Message = "Successfully clocked out. Total hours: 8.5",
                CurrentStatus = "clocked-out",
                TimeEntry = new TimeEntryDto
                {
                    Id = 1,
                    EmployeeId = employeeId,
                    EmployeeName = "Test Employee",
                    ClockIn = DateTime.Now.AddHours(-8.5),
                    ClockOut = DateTime.Now,
                    TotalHours = 8.5m,
                    Status = "Completed",
                    Location = "Office",
                    CreatedAt = DateTime.Now.AddHours(-8.5)
                }
            };
        }

        public async Task<TimeAttendanceStatsDto> GetTimeAttendanceStatsAsync(int employeeId)
        {
            _logger.LogInformation($"Mock: Getting stats for employee {employeeId}");

            await Task.Delay(300);

            return new TimeAttendanceStatsDto
            {
                CurrentWeekHours = 32.5m,
                OvertimeThisWeek = 0m,
                AttendanceRate = 96m,
                LateArrivals = 1,
                PendingTimesheets = 0,
                ActiveShifts = 0,
                IsClockedIn = false,
                CurrentTimeEntry = null
            };
        }

        public async Task<IEnumerable<TimeEntryDto>> GetTimeEntriesAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
        {
            _logger.LogInformation($"Mock: Getting time entries for employee {employeeId}");

            await Task.Delay(300);

            var entries = new List<TimeEntryDto>();
            var today = DateTime.Today;

            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(-i);
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    entries.Add(new TimeEntryDto
                    {
                        Id = i + 1,
                        EmployeeId = employeeId,
                        EmployeeName = "Test Employee",
                        ClockIn = date.AddHours(8),
                        ClockOut = date.AddHours(17),
                        TotalHours = 8m,
                        Status = "Completed",
                        Location = "Main Office",
                        CreatedAt = date.AddHours(8)
                    });
                }
            }

            return entries;
        }

        public async Task<TimeSheetDto?> GetCurrentWeekTimesheetAsync(int employeeId)
        {
            _logger.LogInformation($"Mock: Getting current week timesheet for employee {employeeId}");

            await Task.Delay(300);

            var startOfWeek = GetStartOfWeek(DateTime.Today);
            var endOfWeek = startOfWeek.AddDays(6);

            return new TimeSheetDto
            {
                Id = 1,
                EmployeeId = employeeId,
                EmployeeName = "Test Employee",
                WeekStartDate = DateOnly.FromDateTime(startOfWeek),
                WeekEndDate = DateOnly.FromDateTime(endOfWeek),
                TotalHours = 32.5m,
                RegularHours = 32.5m,
                OvertimeHours = 0m,
                Status = "Draft",
                TimeEntries = (await GetTimeEntriesAsync(employeeId)).Take(5).ToList()
            };
        }

        public async Task<IEnumerable<TimeSheetDto>> GetTimesheetsAsync(int employeeId, int page = 1, int pageSize = 10)
        {
            _logger.LogInformation($"Mock: Getting timesheets for employee {employeeId}");

            await Task.Delay(300);

            return new List<TimeSheetDto>
            {
                new TimeSheetDto
                {
                    Id = 2,
                    EmployeeId = employeeId,
                    EmployeeName = "Test Employee",
                    WeekStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-14)),
                    WeekEndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-8)),
                    TotalHours = 40m,
                    RegularHours = 40m,
                    OvertimeHours = 0m,
                    Status = "Approved",
                    SubmittedAt = DateTime.Now.AddDays(-7),
                    ApproverName = "HR Manager"
                }
            };
        }

        public async Task<bool> SubmitTimesheetAsync(int timesheetId, int employeeId)
        {
            _logger.LogInformation($"Mock: Submitting timesheet {timesheetId} for employee {employeeId}");
            await Task.Delay(500);
            return true;
        }

        public async Task<IEnumerable<TimeSheetDto>> GetPendingTimesheetsAsync(string role)
        {
            _logger.LogInformation($"Mock: Getting pending timesheets for role {role}");
            await Task.Delay(300);
            return new List<TimeSheetDto>();
        }

        public async Task<bool> ApproveTimesheetAsync(int timesheetId, int approverId)
        {
            _logger.LogInformation($"Mock: Approving timesheet {timesheetId} by {approverId}");
            await Task.Delay(500);
            return true;
        }

        public async Task<bool> RejectTimesheetAsync(int timesheetId, int approverId, string reason)
        {
            _logger.LogInformation($"Mock: Rejecting timesheet {timesheetId} by {approverId}");
            await Task.Delay(500);
            return true;
        }

        public async Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceSummaryAsync(DateTime startDate, DateTime endDate, string role)
        {
            _logger.LogInformation($"Mock: Getting attendance summary for role {role}");

            await Task.Delay(300);

            return new List<AttendanceSummaryDto>
            {
                new AttendanceSummaryDto
                {
                    EmployeeId = 1,
                    EmployeeName = "John Admin",
                    Department = "Administration",
                    TotalHours = 160m,
                    RegularHours = 160m,
                    OvertimeHours = 0m,
                    DaysWorked = 20,
                    DaysScheduled = 22,
                    AttendanceRate = 91m,
                    LateArrivals = 2,
                    EarlyDepartures = 1,
                    Status = "Active"
                }
            };
        }

        public async Task<IEnumerable<ScheduleDto>> GetEmployeeScheduleAsync(int employeeId)
        {
            _logger.LogInformation($"Mock: Getting schedule for employee {employeeId}");
            await Task.Delay(300);
            return new List<ScheduleDto>();
        }

        private DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }
}