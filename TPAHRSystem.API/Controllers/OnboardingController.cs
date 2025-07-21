// =============================================================================
// FIX 1: UPDATED ONBOARDING CONTROLLER - RESOLVE NAMESPACE CONFLICTS
// File: TPAHRSystem.API/Controllers/OnboardingController.cs
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;
using AuthService = TPAHRSystem.Application.Services.IAuthService; // Alias to resolve conflict
using OnboardingService = TPAHRSystem.API.Services.IOnboardingService; // Alias to resolve conflict
using TPAHRSystem.API.Services;
using System.ComponentModel.DataAnnotations;

namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnboardingController : ControllerBase
    {
        private readonly TPADbContext _context;
        private readonly ILogger<OnboardingController> _logger;
        private readonly AuthService _authService; // Using alias
        private readonly OnboardingService _onboardingService; // Using alias

        public OnboardingController(
            TPADbContext context,
            ILogger<OnboardingController> logger,
            AuthService authService,
            OnboardingService onboardingService)
        {
            _context = context;
            _logger = logger;
            _authService = authService;
            _onboardingService = onboardingService;
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

        // =============================================================================
        // SIMPLIFIED ENDPOINTS - DIRECT DATABASE CALLS
        // =============================================================================

        /// <summary>
        /// Create new employee with automatic onboarding task assignment
        /// </summary>
        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployeeWithOnboarding([FromBody] CreateEmployeeRequest request)
        {
            //try
            //{
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                // Check if user is HR/Admin
                if (!user.Role.Contains("HR") && !user.Role.Contains("Admin"))
                {
                    return Forbid("Only HR and Admin staff can create employees");
                }

                _logger.LogInformation($"Creating employee with onboarding: {request.FirstName} {request.LastName}");

                // Execute stored procedure directly
                var parameters = new[]
                {
                    new SqlParameter("@FirstName", request.FirstName),
                    new SqlParameter("@LastName", request.LastName),
                    new SqlParameter("@Email", request.Email),
                    new SqlParameter("@Position", request.Position),
                    new SqlParameter("@DepartmentId", request.DepartmentId)
                };

                var result = await _context.Database
                    .SqlQueryRaw<CreateEmployeeResult>(
                        "EXEC sp_CreateEmployeeWithOnboarding @FirstName, @LastName, @Email, @Position, @DepartmentId",
                        parameters)
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return BadRequest(new { success = false, message = "Employee creation failed" });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = new
                    {
                        employeeId = result.EmployeeId,
                        employeeNumber = result.EmployeeNumber,
                        employeeName = result.EmployeeName,
                        onboardingTasks = result.OnboardingTasks,
                        department = result.Department
                    }
                });
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error creating employee with onboarding");
            //    return StatusCode(500, new { success = false, message = "Error creating employee" });
            //}
        }

        /// <summary>
        /// Get onboarding tasks for an employee
        /// </summary>
        [HttpGet("tasks/{employeeId}")]
        public async Task<IActionResult> GetEmployeeTasks(int employeeId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var currentEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
                var isHROrAdmin = user.Role.Contains("HR") || user.Role.Contains("Admin");

                // HR can view any employee's tasks, employees can only view their own
                if (!isHROrAdmin && currentEmployee?.Id != employeeId)
                {
                    return Forbid("You can only view your own onboarding tasks");
                }

                var parameter = new SqlParameter("@EmployeeId", employeeId);
                var tasks = await _context.Database
                    .SqlQueryRaw<EmployeeTaskResult>(
                        "EXEC sp_GetEmployeeTasks @EmployeeId",
                        parameter)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = tasks,
                    summary = new
                    {
                        totalTasks = tasks.Count,
                        completedTasks = tasks.Count(t => t.Status == "COMPLETED"),
                        pendingTasks = tasks.Count(t => t.Status == "PENDING"),
                        overdueTasks = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != "COMPLETED"),
                        completionPercentage = tasks.Count > 0 ?
                            Math.Round((decimal)tasks.Count(t => t.Status == "COMPLETED") / tasks.Count * 100, 2) : 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tasks for employee {employeeId}");
                return StatusCode(500, new { success = false, message = "Error retrieving tasks" });
            }
        }

        /// <summary>
        /// Get current user's onboarding tasks
        /// </summary>
        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
                if (employee == null) return BadRequest(new { success = false, message = "Employee profile not found" });

                return await GetEmployeeTasks(employee.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user's tasks");
                return StatusCode(500, new { success = false, message = "Error retrieving your tasks" });
            }
        }

        /// <summary>
        /// HR completes an onboarding task for an employee
        /// </summary>
        [HttpPost("complete-task/{taskId}")]
        public async Task<IActionResult> CompleteTask(int taskId, [FromBody] CompleteTaskRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                if (!user.Role.Contains("HR") && !user.Role.Contains("Admin"))
                {
                    return Forbid("Only HR and Admin staff can complete onboarding tasks");
                }

                var hrEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
                if (hrEmployee == null) return BadRequest(new { success = false, message = "HR employee profile required" });

                _logger.LogInformation($"HR {user.Email} completing task {taskId}");

                var parameters = new[]
                {
                    new SqlParameter("@TaskId", taskId),
                    new SqlParameter("@CompletedByHR", hrEmployee.Id),
                    new SqlParameter("@Notes", request.Notes ?? "Completed by HR")
                };

                var result = await _context.Database
                    .SqlQueryRaw<TaskCompletionResult>(
                        "EXEC sp_CompleteOnboardingTask @TaskId, @CompletedByHR, @Notes",
                        parameters)
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return BadRequest(new { success = false, message = "Task completion failed" });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = new
                    {
                        completedTasks = result.CompletedTasks,
                        totalTasks = result.TotalTasks,
                        completionPercentage = result.CompletionPercentage,
                        onboardingStatus = result.OnboardingStatus
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing task {taskId}");
                return StatusCode(500, new { success = false, message = "Error completing task" });
            }
        }

        /// <summary>
        /// Get onboarding status for all employees (HR view)
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetOnboardingStatus([FromQuery] int? employeeId = null)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                if (!user.Role.Contains("HR") && !user.Role.Contains("Admin"))
                {
                    return Forbid("Only HR and Admin staff can view onboarding status");
                }

                var parameter = new SqlParameter("@EmployeeId", (object?)employeeId ?? DBNull.Value);
                var statuses = await _context.Database
                    .SqlQueryRaw<OnboardingStatusResult>(
                        "EXEC sp_GetOnboardingStatus @EmployeeId",
                        parameter)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = statuses,
                    summary = new
                    {
                        totalEmployees = statuses.Count,
                        averageCompletion = statuses.Any() ? statuses.Average(s => s.CompletionPercentage) : 0,
                        fullyCompleted = statuses.Count(s => s.CompletionPercentage >= 100),
                        inProgress = statuses.Count(s => s.CompletionPercentage > 0 && s.CompletionPercentage < 100),
                        notStarted = statuses.Count(s => s.CompletionPercentage == 0)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting onboarding status");
                return StatusCode(500, new { success = false, message = "Error retrieving onboarding status" });
            }
        }

        /// <summary>
        /// Complete entire onboarding process (final approval)
        /// </summary>
        [HttpPost("complete-onboarding/{employeeId}")]
        public async Task<IActionResult> CompleteOnboarding(int employeeId, [FromBody] CompleteOnboardingRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                if (!user.Role.Contains("HR") && !user.Role.Contains("Admin"))
                {
                    return Forbid("Only HR and Admin staff can complete onboarding");
                }

                var hrEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
                if (hrEmployee == null) return BadRequest(new { success = false, message = "HR employee profile required" });

                _logger.LogInformation($"HR {user.Email} completing onboarding for employee {employeeId}");

                var parameters = new[]
                {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@ApprovedBy", hrEmployee.Id)
                };

                var result = await _context.Database
                    .SqlQueryRaw<OnboardingCompletionResult>(
                        "EXEC sp_CompleteOnboarding @EmployeeId, @ApprovedBy",
                        parameters)
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return BadRequest(new { success = false, message = "Onboarding completion failed" });
                }

                // Update employee's onboarding status to unlock system access
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee != null)
                {
                    employee.OnboardingStatus = "COMPLETED";
                    employee.IsOnboardingLocked = false;
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = new
                    {
                        employeeNumber = result.EmployeeNumber,
                        employeeName = result.EmployeeName,
                        completedDate = result.CompletedDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing onboarding for employee {employeeId}");
                return StatusCode(500, new { success = false, message = "Error completing onboarding" });
            }
        }

        /// <summary>
        /// Check if employee should have restricted access (onboarding only)
        /// </summary>
        [HttpGet("access-status")]
        public async Task<IActionResult> GetAccessStatus()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
                if (employee == null) return BadRequest(new { success = false, message = "Employee profile not found" });

                // Get current onboarding status
                var parameter = new SqlParameter("@EmployeeId", employee.Id);
                var status = await _context.Database
                    .SqlQueryRaw<OnboardingStatusResult>(
                        "EXEC sp_GetOnboardingStatus @EmployeeId",
                        parameter)
                    .FirstOrDefaultAsync();

                var isOnboardingLocked = employee.IsOnboardingLocked ?? true;
                var onboardingStatus = employee.OnboardingStatus ?? "PENDING";

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        employeeId = employee.Id,
                        employeeNumber = employee.EmployeeNumber,
                        isOnboardingLocked = isOnboardingLocked,
                        onboardingStatus = onboardingStatus,
                        hasRestrictedAccess = isOnboardingLocked,
                        canAccessFullSystem = !isOnboardingLocked && onboardingStatus == "COMPLETED",
                        completionPercentage = status?.CompletionPercentage ?? 0,
                        pendingTasks = status?.PendingTasks ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access status");
                return StatusCode(500, new { success = false, message = "Error checking access status" });
            }
        }

        /// <summary>
        /// Get available departments for employee creation
        /// </summary>
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var departments = await _context.Departments
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .Select(d => new {
                        id = d.Id,
                        name = d.Name,
                        description = d.Description
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = departments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                return StatusCode(500, new { success = false, message = "Error retrieving departments" });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { success = true, message = "Onboarding Management API is healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var user = await GetCurrentUserAsync();
            var employee = user != null ? await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id) : null;

            return Ok(new
            {
                success = true,
                message = "Onboarding Management Controller is working!",
                timestamp = DateTime.UtcNow,
                authenticatedUser = user?.Email ?? "Not authenticated",
                employee = employee != null ? new
                {
                    id = employee.Id,
                    number = employee.EmployeeNumber,
                    name = $"{employee.FirstName} {employee.LastName}"
                } : null,
                endpointsAvailable = new[]
                {
                    "POST /api/onboarding/create-employee",
                    "GET /api/onboarding/tasks/{employeeId}",
                    "GET /api/onboarding/my-tasks",
                    "POST /api/onboarding/complete-task/{taskId}",
                    "GET /api/onboarding/status",
                    "POST /api/onboarding/complete-onboarding/{employeeId}",
                    "GET /api/onboarding/access-status",
                    "GET /api/onboarding/departments"
                }
            });
        }
    }

    // =============================================================================
    // ALL REQUIRED CLASSES IN ONE FILE TO AVOID MISSING REFERENCES
    // =============================================================================

    public class CreateEmployeeRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [Required]
        public int DepartmentId { get; set; }
    }

    public class CompleteTaskRequest
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CompleteOnboardingRequest
    {
        [MaxLength(500)]
        public string? FinalNotes { get; set; }
    }

    public class CreateEmployeeResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int OnboardingTasks { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class EmployeeTaskResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string EstimatedTime { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class TaskCompletionResult
    {
        public string Message { get; set; } = string.Empty;
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
    }

    public class OnboardingStatusResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class OnboardingCompletionResult
    {
        public string Message { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime CompletedDate { get; set; }
    }
}