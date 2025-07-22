// =============================================================================
// MINIMAL WORKING ONBOARDING CONTROLLER - IMMEDIATE FIX
// File: TPAHRSystem.API/Controllers/OnboardingController.cs
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;
using TPAHRSystem.Application.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnboardingController : ControllerBase
    {
        private readonly TPADbContext _context;
        private readonly ILogger<OnboardingController> _logger;
        private readonly IAuthService _authService;

        public OnboardingController(
            TPADbContext context,
            ILogger<OnboardingController> logger,
            IAuthService authService)
        {
            _context = context;
            _logger = logger;
            _authService = authService;
        }

        // =============================================================================
        // AUTHENTICATION HELPERS
        // =============================================================================

        private string? GetTokenFromHeader()
        {
            return Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var token = GetTokenFromHeader();
            if (string.IsNullOrEmpty(token)) return null;

            var userSession = await _context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionToken == token && s.ExpiresAt > DateTime.UtcNow);

            return userSession?.User;
        }

        private async Task<Employee?> GetEmployeeByUserId(int userId)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // =============================================================================
        // HEALTH CHECK AND DEBUG ENDPOINTS
        // =============================================================================

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { success = true, message = "Onboarding Management API is healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var user = await GetCurrentUserAsync();
            return Ok(new
            {
                success = true,
                message = "Onboarding Management Controller is working!",
                userRole = user?.Role ?? "Not authenticated",
                timestamp = DateTime.UtcNow
            });
        }

        // =============================================================================
        // EMPLOYEE CREATION WITH ONBOARDING - MINIMAL WORKING VERSION
        // =============================================================================

        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployeeWithOnboarding([FromBody] CreateEmployeeRequest request)
        {
            //try
            //{
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return Unauthorized(new { success = false, message = "Authentication required" });

                // Validate admin/HR role
                if (!user.Role.Contains("Admin") && !user.Role.Contains("HR"))
                {
                    return Forbid("Only Admin and HR staff can create employees");
                }

                _logger.LogInformation($"Creating employee with onboarding: {request.FirstName} {request.LastName}");

                // **SAFE APPROACH: Use ExecuteSqlRaw without composition**
                var sql = @"
                    EXEC sp_CreateEmployeeWithOnboarding 
                        @FirstName = {0}, 
                        @LastName = {1}, 
                        @Email = {2}, 
                        @Position = {3}, 
                        @DepartmentId = {4}, 
                        @TemporaryPassword = {5}";

                var parameters = new object[]
                {
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Position,
                    request.DepartmentId,
                    request.TemporaryPassword ?? "TempPass123!"
                };

                // Execute the stored procedure
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                // Since stored procedures don't return data easily with ExecuteSqlRaw,
                // let's get the latest created employee
                var latestEmployee = await _context.Employees
                    .Where(e => e.FirstName == request.FirstName &&
                               e.LastName == request.LastName &&
                               e.Email == request.Email)
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefaultAsync();

                if (latestEmployee != null)
                {
                    // Get onboarding tasks count
                    var tasksCount = await _context.OnboardingTasks
                        .CountAsync(t => t.EmployeeId == latestEmployee.Id && !t.IsTemplate);

                    var result = new
                    {
                        success = true,
                        data = new
                        {
                            employeeId = latestEmployee.Id,
                            employeeNumber = latestEmployee.EmployeeNumber,
                            employeeName = $"{latestEmployee.FirstName} {latestEmployee.LastName}",
                            onboardingTasks = tasksCount,
                            department = latestEmployee.Department?.Name ?? "Unknown",
                            message = "Employee created successfully with onboarding tasks"
                        }
                    };

                    _logger.LogInformation($"Employee created successfully: {latestEmployee.EmployeeNumber}");
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Employee creation may have failed - could not retrieve created employee" });
                }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error creating employee with onboarding");
            //    return StatusCode(500, new { success = false, message = "Error creating employee", error = ex.Message });
            //}
        }

        // =============================================================================
        // ONBOARDING TASKS MANAGEMENT - SAFE EF CORE QUERIES
        // =============================================================================

        [HttpGet("tasks")]
        public async Task<IActionResult> GetOnboardingTasks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] int? employeeId = null)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                _logger.LogInformation($"Getting onboarding tasks for user: {user.Email}");

                var query = _context.OnboardingTasks
                    .Include(t => t.Employee)
                    .Where(t => !t.IsTemplate);

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status == status);
                }

                if (employeeId.HasValue)
                {
                    query = query.Where(t => t.EmployeeId == employeeId.Value);
                }

                var totalCount = await query.CountAsync();

                var tasks = await query
                    .OrderBy(t => t.Priority == "HIGH" ? 1 : t.Priority == "MEDIUM" ? 2 : 3)
                    .ThenBy(t => t.DueDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        id = t.Id,
                        title = t.Title,
                        description = t.Description,
                        category = t.Category,
                        status = t.Status,
                        priority = t.Priority,
                        dueDate = t.DueDate,
                        estimatedTime = t.EstimatedTime,
                        instructions = t.Instructions,
                        createdDate = t.CreatedDate,
                        completedDate = t.CompletedDate,
                        notes = t.Notes,
                        employee = t.Employee != null ? new
                        {
                            id = t.Employee.Id,
                            employeeNumber = t.Employee.EmployeeNumber,
                            firstName = t.Employee.FirstName,
                            lastName = t.Employee.LastName,
                            fullName = $"{t.Employee.FirstName} {t.Employee.LastName}",
                            position = t.Employee.Position
                        } : null
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        tasks,
                        pagination = new
                        {
                            currentPage = page,
                            pageSize,
                            totalCount,
                            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                        }
                    },
                    message = $"Retrieved {tasks.Count} onboarding tasks"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching onboarding tasks");
                return StatusCode(500, new { success = false, message = "Error fetching tasks" });
            }
        }

        [HttpPut("tasks/{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(int taskId, [FromBody] CompleteTaskRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var employee = await GetEmployeeByUserId(user.Id);
                if (employee == null) return BadRequest(new { success = false, message = "Employee profile required" });

                // Validate admin/HR role
                if (!user.Role.Contains("Admin") && !user.Role.Contains("HR"))
                {
                    return Forbid("Only Admin and HR staff can complete onboarding tasks");
                }

                _logger.LogInformation($"Completing onboarding task {taskId} by employee {employee.Id}");

                // **SAFE APPROACH: Update using EF Core directly**
                var task = await _context.OnboardingTasks.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }

                // Update the task
                task.Status = "COMPLETED";
                task.CompletedDate = DateTime.UtcNow;
                task.Notes = string.IsNullOrEmpty(request.Notes) ? "Completed by HR" : request.Notes;

                await _context.SaveChangesAsync();

                // Calculate progress
                var employeeId = task.EmployeeId;
                var totalTasks = await _context.OnboardingTasks
                    .CountAsync(t => t.EmployeeId == employeeId && !t.IsTemplate);
                var completedTasks = await _context.OnboardingTasks
                    .CountAsync(t => t.EmployeeId == employeeId && !t.IsTemplate && t.Status == "COMPLETED");

                var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;
                var onboardingStatus = completionPercentage == 100 ? "COMPLETED" :
                                      completionPercentage > 0 ? "IN_PROGRESS" : "NOT_STARTED";

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        message = "Task completed successfully",
                        completedTasks,
                        totalTasks,
                        completionPercentage = Math.Round(completionPercentage, 2),
                        onboardingStatus
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing onboarding task");
                return StatusCode(500, new { success = false, message = "Error completing task" });
            }
        }

        // =============================================================================
        // EMPLOYEE ONBOARDING (For Employee View)
        // =============================================================================

        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyOnboardingTasks()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var employee = await GetEmployeeByUserId(user.Id);
                if (employee == null)
                {
                    return NotFound(new { success = false, message = "Employee profile not found" });
                }

                _logger.LogInformation($"Getting onboarding tasks for employee ID: {employee.Id}");

                var tasks = await _context.OnboardingTasks
                    .Where(t => t.EmployeeId == employee.Id && !t.IsTemplate)
                    .OrderBy(t => t.DueDate)
                    .ThenBy(t => t.Priority == "HIGH" ? 1 : t.Priority == "MEDIUM" ? 2 : 3)
                    .Select(t => new
                    {
                        id = t.Id,
                        title = t.Title,
                        description = t.Description,
                        category = t.Category,
                        status = t.Status,
                        priority = t.Priority,
                        dueDate = t.DueDate,
                        estimatedTime = t.EstimatedTime,
                        instructions = t.Instructions,
                        createdDate = t.CreatedDate,
                        completedDate = t.CompletedDate,
                        isOverdue = t.DueDate < DateTime.UtcNow && t.Status != "COMPLETED"
                    })
                    .ToListAsync();

                var totalTasks = tasks.Count;
                var completedTasks = tasks.Count(t => t.status == "COMPLETED");
                var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        tasks,
                        summary = new
                        {
                            totalTasks,
                            completedTasks,
                            pendingTasks = totalTasks - completedTasks,
                            completionPercentage = Math.Round(completionPercentage, 2)
                        }
                    },
                    message = $"Retrieved {tasks.Count} onboarding tasks"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching my onboarding tasks");
                return StatusCode(500, new { success = false, message = "Error fetching your tasks" });
            }
        }

        // =============================================================================
        // ONBOARDING OVERVIEW
        // =============================================================================

        [HttpGet("overview")]
        public async Task<IActionResult> GetOnboardingOverview([FromQuery] string? department = null)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                _logger.LogInformation($"Fetching onboarding overview for user: {user.Email}");

                var query = _context.Employees
                    .Include(e => e.Department)
                    .Where(e => e.HireDate >= DateTime.UtcNow.AddMonths(-6)); // Recent employees only

                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(e => e.Department != null && e.Department.Name == department);
                }

                var employees = await query.ToListAsync();

                var employeesWithProgress = new List<object>();

                foreach (var emp in employees)
                {
                    var totalTasks = await _context.OnboardingTasks
                        .CountAsync(t => t.EmployeeId == emp.Id && !t.IsTemplate);
                    var completedTasks = await _context.OnboardingTasks
                        .CountAsync(t => t.EmployeeId == emp.Id && !t.IsTemplate && t.Status == "COMPLETED");

                    var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

                    employeesWithProgress.Add(new
                    {
                        id = emp.Id,
                        employeeNumber = emp.EmployeeNumber,
                        firstName = emp.FirstName,
                        lastName = emp.LastName,
                        fullName = $"{emp.FirstName} {emp.LastName}",
                        position = emp.Position,
                        department = emp.Department?.Name ?? "No Department",
                        hireDate = emp.HireDate,
                        totalTasks,
                        completedTasks,
                        completionPercentage = Math.Round(completionPercentage, 2)
                    });
                }

                var orderedEmployees = employeesWithProgress.OrderByDescending(e => ((dynamic)e).hireDate).ToList();

                var summary = new
                {
                    totalEmployees = orderedEmployees.Count,
                    fullCompleted = orderedEmployees.Count(e => ((dynamic)e).completionPercentage == 100),
                    inProgress = orderedEmployees.Count(e => ((dynamic)e).completionPercentage > 0 && ((dynamic)e).completionPercentage < 100),
                    notStarted = orderedEmployees.Count(e => ((dynamic)e).completionPercentage == 0),
                    averageCompletion = orderedEmployees.Any() ? Math.Round(orderedEmployees.Average(e => ((dynamic)e).completionPercentage), 2) : 0
                };

                return Ok(new
                {
                    success = true,
                    data = new { employees = orderedEmployees, summary },
                    message = "Onboarding overview data retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching onboarding overview");
                return StatusCode(500, new { success = false, message = "Error fetching overview" });
            }
        }
    }

    // =============================================================================
    // REQUEST/RESPONSE MODELS
    // =============================================================================

    public class CreateEmployeeRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Position { get; set; } = string.Empty;

        [Required]
        public int DepartmentId { get; set; }

        public string? TemporaryPassword { get; set; }
    }

    public class CompleteTaskRequest
    {
        public string? Notes { get; set; }
    }
}