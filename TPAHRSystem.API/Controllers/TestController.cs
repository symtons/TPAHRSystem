using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly TPADbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(TPADbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow.ToString("o"),
                api = "TPA HR Management System",
                version = "1.0.0"
            });
        }

        [HttpGet("database-connection")]
        public async Task<IActionResult> DatabaseConnection()
        {
            try
            {
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();

                return Ok(new
                {
                    status = "connected",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    message = "Database connection successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection failed");
                return StatusCode(500, new
                {
                    status = "error",
                    message = "Database connection failed",
                    error = ex.Message
                });
            }
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _context.DashboardStats
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new
                    {
                        id = s.Id,
                        statKey = s.StatKey,
                        statName = s.StatName,
                        statValue = s.StatValue,
                        statColor = s.StatColor,
                        iconName = s.IconName,
                        subtitle = s.Subtitle
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { success = false, message = "Failed to get dashboard stats" });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Employees)
                    .Where(u => u.IsActive)
                    .ToListAsync();

                var result = users.Select(u => new
                {
                    id = u.Id,
                    email = u.Email,
                    role = u.Role,
                    lastLogin = u.LastLogin,
                    employeeName = u.Employees.Any()
                        ? $"{u.Employees.First().FirstName} {u.Employees.First().LastName}"
                        : "No Employee Record"
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { success = false, message = "Failed to get users" });
            }
        }
    }
}
       