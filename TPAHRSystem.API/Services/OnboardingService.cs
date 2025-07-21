// =============================================================================
// ONBOARDING SERVICE - API SERVICES FOLDER
// File: TPAHRSystem.API/Services/OnboardingService.cs
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.API.Services
{
    // =============================================================================
    // STORED PROCEDURE RESULT CLASSES
    // =============================================================================

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

    // =============================================================================
    // SIMPLE DTOs FOR API
    // =============================================================================

    public class CreateEmployeeRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
    }

    public class CreateEmployeeDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int OnboardingTasks { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class OnboardingTaskDto
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
        public bool CanComplete { get; set; } = false;
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != "COMPLETED";
    }

    public class TaskCompletionDto
    {
        public string Message { get; set; } = string.Empty;
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
    }

    public class OnboardingStatusDto
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
        public string Status => CompletionPercentage >= 100 ? "COMPLETED" :
                               CompletionPercentage > 0 ? "IN_PROGRESS" : "NOT_STARTED";
        public int DaysInOnboarding => (int)(DateTime.UtcNow - HireDate).TotalDays;
        public bool IsOverdue => DaysInOnboarding > 14 && CompletionPercentage < 100;
    }

    public class OnboardingCompletionDto
    {
        public string Message { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime CompletedDate { get; set; }
    }

    public class AccessStatusDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsOnboardingLocked { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
        public bool HasRestrictedAccess { get; set; }
        public bool CanAccessFullSystem { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int PendingTasks { get; set; }
    }

    // =============================================================================
    // SERVICE RESULT WRAPPER (CONSISTENT WITH YOUR OTHER SERVICES)
    // =============================================================================

    public class OnboardingServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static OnboardingServiceResult<T> CreateSuccess(T data, string message = "Operation successful")
        {
            return new OnboardingServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static OnboardingServiceResult<T> CreateFailure(string message)
        {
            return new OnboardingServiceResult<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
    }

    // =============================================================================
    // ONBOARDING SERVICE INTERFACE
    // =============================================================================

    public interface IOnboardingService
    {
        // Employee Creation
        Task<OnboardingServiceResult<CreateEmployeeDto>> CreateEmployeeWithOnboardingAsync(CreateEmployeeRequest request, int createdByUserId);

        // Task Management
        Task<OnboardingServiceResult<List<OnboardingTaskDto>>> GetEmployeeTasksAsync(int employeeId, int requestingUserId);
        Task<OnboardingServiceResult<TaskCompletionDto>> CompleteTaskAsync(int taskId, int completedByUserId, string? notes = null);

        // Status and Progress
        Task<OnboardingServiceResult<List<OnboardingStatusDto>>> GetOnboardingStatusAsync(int? employeeId = null);
        Task<OnboardingServiceResult<OnboardingCompletionDto>> CompleteOnboardingAsync(int employeeId, int approvedByUserId);

        // Access Control
        Task<OnboardingServiceResult<AccessStatusDto>> GetEmployeeAccessStatusAsync(int employeeId);
        Task<OnboardingServiceResult<bool>> CanEmployeeAccessMenuAsync(int employeeId, string menuName);

        // Validation
        Task<bool> IsUserHROrAdminAsync(int userId);
        Task<Employee?> GetEmployeeByUserIdAsync(int userId);
    }

    // =============================================================================
    // ONBOARDING SERVICE IMPLEMENTATION
    // =============================================================================

    public class OnboardingService : IOnboardingService
    {
        private readonly TPADbContext _context;
        private readonly ILogger<OnboardingService> _logger;

        public OnboardingService(TPADbContext context, ILogger<OnboardingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =============================================================================
        // EMPLOYEE CREATION
        // =============================================================================

        public async Task<OnboardingServiceResult<CreateEmployeeDto>> CreateEmployeeWithOnboardingAsync(
            CreateEmployeeRequest request, int createdByUserId)
        {
            try
            {
                _logger.LogInformation($"Creating employee with onboarding: {request.FirstName} {request.LastName}");

                // Validate creator permissions
                if (!await IsUserHROrAdminAsync(createdByUserId))
                {
                    return OnboardingServiceResult<CreateEmployeeDto>.CreateFailure("Only HR and Admin staff can create employees");
                }

                // Execute stored procedure
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
                    return OnboardingServiceResult<CreateEmployeeDto>.CreateFailure("Employee creation failed");
                }

                var dto = new CreateEmployeeDto
                {
                    EmployeeId = result.EmployeeId,
                    EmployeeNumber = result.EmployeeNumber,
                    EmployeeName = result.EmployeeName,
                    OnboardingTasks = result.OnboardingTasks,
                    Department = result.Department,
                    Message = result.Message
                };

                return OnboardingServiceResult<CreateEmployeeDto>.CreateSuccess(dto, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee with onboarding");
                return OnboardingServiceResult<CreateEmployeeDto>.CreateFailure("Error creating employee");
            }
        }

        // =============================================================================
        // TASK MANAGEMENT
        // =============================================================================

        public async Task<OnboardingServiceResult<List<OnboardingTaskDto>>> GetEmployeeTasksAsync(
            int employeeId, int requestingUserId)
        {
            try
            {
                // Check permissions
                var requestingEmployee = await GetEmployeeByUserIdAsync(requestingUserId);
                var isHROrAdmin = await IsUserHROrAdminAsync(requestingUserId);

                if (!isHROrAdmin && requestingEmployee?.Id != employeeId)
                {
                    return OnboardingServiceResult<List<OnboardingTaskDto>>.CreateFailure("You can only view your own onboarding tasks");
                }

                var parameter = new SqlParameter("@EmployeeId", employeeId);
                var tasks = await _context.Database
                    .SqlQueryRaw<EmployeeTaskResult>(
                        "EXEC sp_GetEmployeeTasks @EmployeeId",
                        parameter)
                    .ToListAsync();

                var taskDtos = tasks.Select(t => new OnboardingTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Category = t.Category,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    EstimatedTime = t.EstimatedTime,
                    Instructions = t.Instructions,
                    CreatedDate = t.CreatedDate,
                    CompletedDate = t.CompletedDate,
                    Notes = t.Notes,
                    CanComplete = isHROrAdmin
                }).ToList();

                return OnboardingServiceResult<List<OnboardingTaskDto>>.CreateSuccess(taskDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tasks for employee {employeeId}");
                return OnboardingServiceResult<List<OnboardingTaskDto>>.CreateFailure("Error retrieving tasks");
            }
        }

        public async Task<OnboardingServiceResult<TaskCompletionDto>> CompleteTaskAsync(
            int taskId, int completedByUserId, string? notes = null)
        {
            try
            {
                // Validate permissions
                if (!await IsUserHROrAdminAsync(completedByUserId))
                {
                    return OnboardingServiceResult<TaskCompletionDto>.CreateFailure("Only HR and Admin staff can complete onboarding tasks");
                }

                var completedByEmployee = await GetEmployeeByUserIdAsync(completedByUserId);
                if (completedByEmployee == null)
                {
                    return OnboardingServiceResult<TaskCompletionDto>.CreateFailure("HR employee profile required");
                }

                _logger.LogInformation($"HR employee {completedByEmployee.Id} completing task {taskId}");

                var parameters = new[]
                {
                    new SqlParameter("@TaskId", taskId),
                    new SqlParameter("@CompletedByHR", completedByEmployee.Id),
                    new SqlParameter("@Notes", notes ?? "Completed by HR")
                };

                var result = await _context.Database
                    .SqlQueryRaw<TaskCompletionResult>(
                        "EXEC sp_CompleteOnboardingTask @TaskId, @CompletedByHR, @Notes",
                        parameters)
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return OnboardingServiceResult<TaskCompletionDto>.CreateFailure("Task completion failed");
                }

                var dto = new TaskCompletionDto
                {
                    Message = result.Message,
                    CompletedTasks = result.CompletedTasks,
                    TotalTasks = result.TotalTasks,
                    CompletionPercentage = result.CompletionPercentage,
                    OnboardingStatus = result.OnboardingStatus
                };

                return OnboardingServiceResult<TaskCompletionDto>.CreateSuccess(dto, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing task {taskId}");
                return OnboardingServiceResult<TaskCompletionDto>.CreateFailure("Error completing task");
            }
        }

        // =============================================================================
        // STATUS AND PROGRESS
        // =============================================================================

        public async Task<OnboardingServiceResult<List<OnboardingStatusDto>>> GetOnboardingStatusAsync(int? employeeId = null)
        {
            try
            {
                var parameter = new SqlParameter("@EmployeeId", (object?)employeeId ?? DBNull.Value);
                var statuses = await _context.Database
                    .SqlQueryRaw<OnboardingStatusResult>(
                        "EXEC sp_GetOnboardingStatus @EmployeeId",
                        parameter)
                    .ToListAsync();

                var statusDtos = statuses.Select(s => new OnboardingStatusDto
                {
                    EmployeeId = s.EmployeeId,
                    EmployeeNumber = s.EmployeeNumber,
                    EmployeeName = s.EmployeeName,
                    Position = s.Position,
                    Department = s.Department,
                    TotalTasks = s.TotalTasks,
                    CompletedTasks = s.CompletedTasks,
                    PendingTasks = s.PendingTasks,
                    CompletionPercentage = s.CompletionPercentage,
                    HireDate = s.HireDate
                }).ToList();

                return OnboardingServiceResult<List<OnboardingStatusDto>>.CreateSuccess(statusDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting onboarding status");
                return OnboardingServiceResult<List<OnboardingStatusDto>>.CreateFailure("Error retrieving onboarding status");
            }
        }

        public async Task<OnboardingServiceResult<OnboardingCompletionDto>> CompleteOnboardingAsync(
            int employeeId, int approvedByUserId)
        {
            try
            {
                // Validate permissions
                if (!await IsUserHROrAdminAsync(approvedByUserId))
                {
                    return OnboardingServiceResult<OnboardingCompletionDto>.CreateFailure("Only HR and Admin staff can complete onboarding");
                }

                var approvedByEmployee = await GetEmployeeByUserIdAsync(approvedByUserId);
                if (approvedByEmployee == null)
                {
                    return OnboardingServiceResult<OnboardingCompletionDto>.CreateFailure("HR employee profile required");
                }

                _logger.LogInformation($"HR employee {approvedByEmployee.Id} completing onboarding for employee {employeeId}");

                var parameters = new[]
                {
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@ApprovedBy", approvedByEmployee.Id)
                };

                var result = await _context.Database
                    .SqlQueryRaw<OnboardingCompletionResult>(
                        "EXEC sp_CompleteOnboarding @EmployeeId, @ApprovedBy",
                        parameters)
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return OnboardingServiceResult<OnboardingCompletionDto>.CreateFailure("Onboarding completion failed");
                }

                // Update employee's onboarding status to unlock system access
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee != null)
                {
                    employee.OnboardingStatus = "COMPLETED";
                    employee.IsOnboardingLocked = false;
                    await _context.SaveChangesAsync();
                }

                var dto = new OnboardingCompletionDto
                {
                    Message = result.Message,
                    EmployeeNumber = result.EmployeeNumber,
                    EmployeeName = result.EmployeeName,
                    CompletedDate = result.CompletedDate
                };

                return OnboardingServiceResult<OnboardingCompletionDto>.CreateSuccess(dto, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing onboarding for employee {employeeId}");
                return OnboardingServiceResult<OnboardingCompletionDto>.CreateFailure("Error completing onboarding");
            }
        }

        // =============================================================================
        // ACCESS CONTROL
        // =============================================================================

        public async Task<OnboardingServiceResult<AccessStatusDto>> GetEmployeeAccessStatusAsync(int employeeId)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return OnboardingServiceResult<AccessStatusDto>.CreateFailure("Employee not found");
                }

                // Get current onboarding status
                var parameter = new SqlParameter("@EmployeeId", employeeId);
                var status = await _context.Database
                    .SqlQueryRaw<OnboardingStatusResult>(
                        "EXEC sp_GetOnboardingStatus @EmployeeId",
                        parameter)
                    .FirstOrDefaultAsync();

                var isOnboardingLocked = employee.IsOnboardingLocked ?? true;
                var onboardingStatus = employee.OnboardingStatus ?? "PENDING";

                var dto = new AccessStatusDto
                {
                    EmployeeId = employee.Id,
                    EmployeeNumber = employee.EmployeeNumber,
                    IsOnboardingLocked = isOnboardingLocked,
                    OnboardingStatus = onboardingStatus,
                    HasRestrictedAccess = isOnboardingLocked,
                    CanAccessFullSystem = !isOnboardingLocked && onboardingStatus == "COMPLETED",
                    CompletionPercentage = status?.CompletionPercentage ?? 0,
                    PendingTasks = status?.PendingTasks ?? 0
                };

                return OnboardingServiceResult<AccessStatusDto>.CreateSuccess(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting access status for employee {employeeId}");
                return OnboardingServiceResult<AccessStatusDto>.CreateFailure("Error checking access status");
            }
        }

        public async Task<OnboardingServiceResult<bool>> CanEmployeeAccessMenuAsync(int employeeId, string menuName)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return OnboardingServiceResult<bool>.CreateFailure("Employee not found");
                }

                // Check if employee is HR/Admin through their user
                var user = await _context.Users.FindAsync(employee.UserId);
                if (user != null && await IsUserHROrAdminAsync(user.Id))
                {
                    return OnboardingServiceResult<bool>.CreateSuccess(true);
                }

                var isOnboardingLocked = employee.IsOnboardingLocked ?? true;

                // If employee is in onboarding, only allow onboarding-related menus
                if (isOnboardingLocked)
                {
                    var canAccess = IsOnboardingRelatedMenu(menuName);
                    return OnboardingServiceResult<bool>.CreateSuccess(canAccess);
                }

                // Employee has completed onboarding - allow all standard menus
                return OnboardingServiceResult<bool>.CreateSuccess(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking menu access for employee {employeeId}, menu {menuName}");
                return OnboardingServiceResult<bool>.CreateFailure("Error checking menu access");
            }
        }

        // =============================================================================
        // HELPER METHODS
        // =============================================================================

        public async Task<bool> IsUserHROrAdminAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                return user != null && (user.Role.Contains("HR") || user.Role.Contains("Admin") || user.Role.Contains("SuperAdmin"));
            }
            catch
            {
                return false;
            }
        }

        public async Task<Employee?> GetEmployeeByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
            }
            catch
            {
                return null;
            }
        }

        private bool IsOnboardingRelatedMenu(string menuName)
        {
            var onboardingMenus = new[]
            {
                "Onboarding",
                "My Tasks",
                "Progress",
                "Profile",
                "Personal Information",
                "Help",
                "Getting Started",
                "Contact HR"
            };

            return onboardingMenus.Contains(menuName, StringComparer.OrdinalIgnoreCase);
        }
    }
}