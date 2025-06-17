using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly TPADbContext _context;

        public TestController(TPADbContext context)
        {
            _context = context;
        }

        [HttpGet("database-connection")]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (canConnect)
                {
                    var userCount = await _context.Users.CountAsync();
                    var departmentCount = await _context.Departments.CountAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Database connection successful!",
                        data = new
                        {
                            userCount,
                            departmentCount,
                            databaseName = _context.Database.GetDbConnection().Database,
                            serverName = _context.Database.GetDbConnection().DataSource
                        }
                    });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Cannot connect to database" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("seed-data")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                await DataSeeder.SeedAsync(_context);
                return Ok(new { success = true, message = "Data seeded successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
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
                    .ToListAsync();

                return Ok(new { success = true, count = stats.Count, data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                api = "TPA HR Management System",
                version = "1.0.0"
            });
        }
    }
}