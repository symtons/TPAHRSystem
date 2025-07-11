// =============================================================================
// COMPLETE TIME ATTENDANCE CONTROLLER
// File: TPAHRSystem.API/Controllers/TimeAttendanceController.cs
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TPAHRSystem.API.Services;

namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeAttendanceController : ControllerBase
    {
        private readonly ITimeAttendanceService _timeAttendanceService;
        private readonly ILogger<TimeAttendanceController> _logger;

        public TimeAttendanceController(ITimeAttendanceService timeAttendanceService, ILogger<TimeAttendanceController> logger)
        {
            _timeAttendanceService = timeAttendanceService;
            _logger = logger;
        }

        // Test endpoint - ALWAYS ADD THIS FIRST
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                success = true,
                message = "Time and Attendance controller is working!",
                timestamp = DateTime.UtcNow,
                controller = "TimeAttendance",
                version = "1.0.0"
            });
        }

        // Clock In/Out Endpoints
        [HttpPost("clock-in")]
        public async Task<IActionResult> ClockIn([FromBody] ClockInRequest request)
        {
            try
            {
                _logger.LogInformation($"Clock in request for employee {request.EmployeeId}");

                var result = await _timeAttendanceService.ClockInAsync(request.EmployeeId, request.Location);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing clock in for employee {request.EmployeeId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("clock-out")]
        public async Task<IActionResult> ClockOut([FromBody] ClockOutRequest request)
        {
            try
            {
                _logger.LogInformation($"Clock out request for employee {request.EmployeeId}");

                var result = await _timeAttendanceService.ClockOutAsync(request.EmployeeId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing clock out for employee {request.EmployeeId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // Stats and Summary Endpoints
        [HttpGet("stats/{employeeId}")]
        public async Task<IActionResult> GetTimeAttendanceStats(int employeeId)
        {
            try
            {
                var stats = await _timeAttendanceService.GetTimeAttendanceStatsAsync(employeeId);
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting time attendance stats for employee {employeeId}");
                return StatusCode(500, new { success = false, message = "Failed to get time attendance stats" });
            }
        }

        [HttpGet("current-status/{employeeId}")]
        public async Task<IActionResult> GetCurrentStatus(int employeeId)
        {
            try
            {
                var stats = await _timeAttendanceService.GetTimeAttendanceStatsAsync(employeeId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        isClockedIn = stats.IsClockedIn,
                        currentTimeEntry = stats.CurrentTimeEntry,
                        currentWeekHours = stats.CurrentWeekHours
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting current status for employee {employeeId}");
                return StatusCode(500, new { success = false, message = "Failed to get current status" });
            }
        }

        // Time Entries Endpoints
        [HttpGet("entries/{employeeId}")]
        public async Task<IActionResult> GetTimeEntries(int employeeId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var entries = await _timeAttendanceService.GetTimeEntriesAsync(employeeId, startDate, endDate);
                return Ok(new { success = true, data = entries });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting time entries for employee {employeeId}");
                return StatusCode(500, new { success = false, message = "Failed to get time entries" });
            }
        }

        // Timesheet Endpoints
        [HttpGet("timesheet/current/{employeeId}")]
        public async Task<IActionResult> GetCurrentWeekTimesheet(int employeeId)
        {
            try
            {
                var timesheet = await _timeAttendanceService.GetCurrentWeekTimesheetAsync(employeeId);

                if (timesheet == null)
                {
                    return Ok(new { success = true, data = (object?)null, message = "No time entries found for current week" });
                }

                return Ok(new { success = true, data = timesheet });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting current week timesheet for employee {employeeId}");
                return StatusCode(500, new { success = false, message = "Failed to get current week timesheet" });
            }
        }

        [HttpGet("timesheets/{employeeId}")]
        public async Task<IActionResult> GetTimesheets(int employeeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var timesheets = await _timeAttendanceService.GetTimesheetsAsync(employeeId, page, pageSize);
                return Ok(new { success = true, data = timesheets });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting timesheets for employee {employeeId}");
                return StatusCode(500, new { success = false, message = "Failed to get timesheets" });
            }
        }

        [HttpPost("timesheet/{timesheetId}/submit")]
        public async Task<IActionResult> SubmitTimesheet(int timesheetId, [FromBody] SubmitTimesheetRequest request)
        {
            try
            {
                var success = await _timeAttendanceService.SubmitTimesheetAsync(timesheetId, request.EmployeeId);

                if (success)
                {
                    return Ok(new { success = true, message = "Timesheet submitted successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to submit timesheet" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting timesheet {timesheetId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // Management Endpoints (HR/Admin)
        [HttpGet("pending-timesheets")]
        public async Task<IActionResult> GetPendingTimesheets([FromQuery] string role)
        {
            try
            {
                var timesheets = await _timeAttendanceService.GetPendingTimesheetsAsync(role);
                return Ok(new { success = true, data = timesheets });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending timesheets");
                return StatusCode(500, new { success = false, message = "Failed to get pending timesheets" });
            }
        }

        [HttpPost("timesheet/{timesheetId}/approve")]
        public async Task<IActionResult> ApproveTimesheet(int timesheetId, [FromBody] ApproveTimesheetRequest request)
        {
            try
            {
                var success = await _timeAttendanceService.ApproveTimesheetAsync(timesheetId, request.ApproverId);

                if (success)
                {
                    return Ok(new { success = true, message = "Timesheet approved successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to approve timesheet" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving timesheet {timesheetId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("timesheet/{timesheetId}/reject")]
        public async Task<IActionResult> RejectTimesheet(int timesheetId, [FromBody] RejectTimesheetRequest request)
        {
            try
            {
                var success = await _timeAttendanceService.RejectTimesheetAsync(timesheetId, request.ApproverId, request.Reason);

                if (success)
                {
                    return Ok(new { success = true, message = "Timesheet rejected successfully" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Failed to reject timesheet" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting timesheet {timesheetId}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // Attendance Reports
        [HttpGet("attendance-summary")]
        public async Task<IActionResult> GetAttendanceSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string role)
        {
            try
            {
                var summary = await _timeAttendanceService.GetAttendanceSummaryAsync(startDate, endDate, role);
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance summary");
                return StatusCode(500, new { success = false, message = "Failed to get attendance summary" });
            }
        }

        // Schedule Endpoints
        [HttpGet("schedule/{employeeId}")]
        public async Task<IActionResult> GetEmployeeSchedule(int employeeId)
        {
            try
            {
                var schedule = await _timeAttendanceService.GetEmployeeScheduleAsync(employeeId);
                return Ok(new { success = true, data = schedule });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting schedule for employee {employeeId}");
                return StatusCode(500, new { success = false, message = "Failed to get employee schedule" });
            }
        }
    }

    // Request DTOs
    public class ClockInRequest
    {
        public int EmployeeId { get; set; }
        public string Location { get; set; } = "Office";
    }

    public class ClockOutRequest
    {
        public int EmployeeId { get; set; }
    }

    public class SubmitTimesheetRequest
    {
        public int EmployeeId { get; set; }
    }

    public class ApproveTimesheetRequest
    {
        public int ApproverId { get; set; }
    }

    public class RejectTimesheetRequest
    {
        public int ApproverId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}