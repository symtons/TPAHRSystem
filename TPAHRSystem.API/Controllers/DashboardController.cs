// =============================================================================

// File: TPAHRSystem.API/Controllers/DashboardController.cs (Replace existing)
// =============================================================================


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Infrastructure.Data;
using TPAHRSystem.Core.Models;


namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly TPADbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(TPADbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("stats/{role}")]
        public async Task<IActionResult> GetDashboardStats(string role)
        {
            try
            {
                _logger.LogInformation($"Getting dashboard stats for role: {role}");

                var stats = await _context.DashboardStats
                    .Where(s => s.IsActive &&
                               (s.ApplicableRoles == null ||
                                s.ApplicableRoles.Contains(role)))
                    .OrderBy(s => s.SortOrder)
                    .ToListAsync();

                var result = stats.Select(s => new
                {
                    title = s.StatName,
                    value = s.StatValue,
                    subtitle = s.Subtitle,
                    icon = s.IconName,
                    color = s.StatColor
                });

                _logger.LogInformation($"Found {result.Count()} stats for role: {role}");
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats for role: {Role}", role);
                return StatusCode(500, new { success = false, message = $"Failed to get dashboard stats: {ex.Message}" });
            }
        }

        [HttpGet("quick-actions/{role}")]
        public async Task<IActionResult> GetQuickActions(string role)
        {
            try
            {
                _logger.LogInformation($"Getting quick actions for role: {role}");

                var actions = await _context.QuickActions
                    .Where(qa => qa.IsActive &&
                                (qa.ApplicableRoles == null ||
                                 qa.ApplicableRoles.Contains(role)))
                    .OrderBy(qa => qa.SortOrder)
                    .ToListAsync();

                var result = actions.Select(qa => new
                {
                    key = qa.ActionKey,
                    label = qa.Title,
                    icon = qa.IconName,
                    color = qa.Color,
                    route = qa.Route
                });

                _logger.LogInformation($"Found {result.Count()} actions for role: {role}");
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quick actions for role: {Role}", role);
                return StatusCode(500, new { success = false, message = $"Failed to get quick actions: {ex.Message}" });
            }
        }

        [HttpGet("recent-activities/{userId}")]
        public async Task<IActionResult> GetRecentActivities(int userId, [FromQuery] string role)
        {
            try
            {
                _logger.LogInformation($"Getting recent activities for user: {userId}, role: {role}");

                var query = _context.RecentActivities
                    .Include(ra => ra.User)
                    .Include(ra => ra.Employee)
                    .Include(ra => ra.ActivityType)
                    .AsQueryable();

                // Filter based on role
                switch (role)
                {
                    case "Admin":
                        // Admins see all activities
                        break;
                    case "HR Manager":
                        // HR sees HR-related activities
                        var hrActivityTypes = new[] { "USER_LOGIN", "LEAVE_REQUESTED", "LEAVE_APPROVED", "EMPLOYEE_CREATED", "ONBOARDING_TASK" };
                        query = query.Where(ra => hrActivityTypes.Contains(ra.ActivityType.Name));
                        break;
                    case "Employee (Admin Staff)":
                    case "Employee (Field Staff)":
                        // Employees see their own activities and general announcements
                        query = query.Where(ra => ra.UserId == userId ||
                                                 ra.ActivityType.Name.Contains("SYSTEM"));
                        break;
                }

                var rawActivities = await query
                    .OrderByDescending(ra => ra.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                var activities = rawActivities.Select(ra => new
                {
                    id = ra.Id,
                    user = ra.Employee?.FullName ?? ra.User.Email.Split('@')[0],
                    action = ra.Action,
                    details = ra.Details,
                    time = ra.CreatedAt,
                    avatar = ra.Employee?.FirstName?.Substring(0, 1) ?? ra.User.Email.Substring(0, 1).ToUpper(),
                    color = ra.ActivityType.Color,
                    type = ra.ActivityType.Name,
                    isNew = ra.CreatedAt > DateTime.UtcNow.AddHours(-1)
                }).ToList();

                _logger.LogInformation($"Found {activities.Count} activities for user: {userId}");
                return Ok(new { success = true, data = activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities for user: {UserId}", userId);
                return StatusCode(500, new { success = false, message = $"Failed to get recent activities: {ex.Message}" });
            }
        }

        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetDashboardSummary(int userId, [FromQuery] string role)
        {
            try
            {
                _logger.LogInformation($"Getting dashboard summary for user: {userId}, role: {role}");

                // Get stats
                var stats = await _context.DashboardStats
                    .Where(s => s.IsActive &&
                               (s.ApplicableRoles == null ||
                                s.ApplicableRoles.Contains(role)))
                    .OrderBy(s => s.SortOrder)
                    .ToListAsync();

                // Get quick actions
                var actions = await _context.QuickActions
                    .Where(qa => qa.IsActive &&
                                (qa.ApplicableRoles == null ||
                                 qa.ApplicableRoles.Contains(role)))
                    .OrderBy(qa => qa.SortOrder)
                    .ToListAsync();

                // Get recent activities (simplified for summary)
                var recentActivities = await _context.RecentActivities
                    .Include(ra => ra.User)
                    .Include(ra => ra.Employee)
                    .Include(ra => ra.ActivityType)
                    .OrderByDescending(ra => ra.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                var summary = new
                {
                    stats = stats.Select(s => new
                    {
                        title = s.StatName,
                        value = s.StatValue,
                        subtitle = s.Subtitle,
                        icon = s.IconName,
                        color = s.StatColor
                    }),
                    quickActions = actions.Select(qa => new
                    {
                        key = qa.ActionKey,
                        label = qa.Title,
                        icon = qa.IconName,
                        color = qa.Color,
                        route = qa.Route
                    }),
                    recentActivities = recentActivities.Select(ra => new
                    {
                        id = ra.Id,
                        user = ra.Employee?.FullName ?? ra.User.Email.Split('@')[0],
                        action = ra.Action,
                        details = ra.Details,
                        time = ra.CreatedAt,
                        avatar = ra.Employee?.FirstName?.Substring(0, 1) ?? ra.User.Email.Substring(0, 1).ToUpper(),
                        color = ra.ActivityType.Color,
                        type = ra.ActivityType.Name,
                        isNew = ra.CreatedAt > DateTime.UtcNow.AddHours(-1)
                    })
                };

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary for user: {UserId}", userId);
                return StatusCode(500, new { success = false, message = $"Failed to get dashboard summary: {ex.Message}" });
            }
        }

        // Add a test endpoint to verify the controller is working
        [HttpGet("test")]
        public IActionResult Test()
        {



            return Ok(new
            {
                success = true,
                message = "Dashboard controller is working!",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
///