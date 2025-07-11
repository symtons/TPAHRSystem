// =============================================================================
// ENHANCED ONBOARDING CONTROLLER FOR HR/ADMIN MANAGEMENT
// File: TPAHRSystem.API/Controllers/OnboardingController.cs (Replace existing)
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;
using TPAHRSystem.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnboardingController : ControllerBase
    {
        private readonly TPADbContext _context;
        private readonly ILogger<OnboardingController> _logger;
        private readonly IAuthService _authService;

        public OnboardingController(TPADbContext context, ILogger<OnboardingController> logger, IAuthService authService)
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
                timestamp = DateTime.UtcNow,
                controller = "OnboardingManagement",
                version = "2.0.0",
                authenticated = user != null,
                userId = user?.Id
            });
        }

        // =============================================================================
        // TEMPLATE MANAGEMENT ENDPOINTS
        // =============================================================================

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                _logger.LogInformation($"Fetching onboarding templates for user: {user.Email}");

                var templates = await _context.OnboardingTemplates
                    .Include(t => t.CreatedBy)
                    .Include(t => t.Tasks.Where(task => task.IsTemplate))
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.ForRole)
                    .ThenBy(t => t.Name)
                    .ToListAsync();

                var templateData = templates.Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    description = t.Description,
                    forRole = t.ForRole,
                    forDepartment = t.ForDepartment,
                    isActive = t.IsActive,
                    createdDate = t.CreatedDate,
                    modifiedDate = t.ModifiedDate,
                    createdBy = t.CreatedBy != null ? new
                    {
                        id = t.CreatedBy.Id,
                        name = $"{t.CreatedBy.FirstName} {t.CreatedBy.LastName}"
                    } : null,
                    taskCount = t.Tasks.Count,
                    tasks = t.Tasks.Select(task => new
                    {
                        id = task.Id,
                        title = task.Title,
                        description = task.Description,
                        category = task.Category,
                        priority = task.Priority,
                        estimatedTime = task.EstimatedTime,
                        instructions = task.Instructions
                    }).ToList()
                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = templateData,
                    message = $"Found {templateData.Count} onboarding templates"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching onboarding templates");
                return StatusCode(500, new { success = false, message = "Error fetching templates" });
            }
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateRequest request)
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
                    return Forbid("Only Admin and HR staff can create templates");
                }

                _logger.LogInformation($"Creating onboarding template: {request.Name}");

                var template = new OnboardingTemplate
                {
                    Name = request.Name,
                    Description = request.Description,
                    ForRole = request.ForRole,
                    ForDepartment = request.ForDepartment,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = employee.Id
                };

                _context.OnboardingTemplates.Add(template);
                await _context.SaveChangesAsync();

                // Add tasks to template if provided
                if (request.Tasks?.Any() == true)
                {
                    foreach (var taskRequest in request.Tasks)
                    {
                        var task = new OnboardingTask
                        {
                            Title = taskRequest.Title,
                            Description = taskRequest.Description,
                            Category = taskRequest.Category,
                            Priority = taskRequest.Priority,
                            EstimatedTime = taskRequest.EstimatedTime,
                            Instructions = taskRequest.Instructions,
                            Status = "PENDING",
                            CreatedDate = DateTime.UtcNow,
                            IsTemplate = true,
                            TemplateId = template.Id,
                            AssignedById = employee.Id
                        };

                        _context.OnboardingTasks.Add(task);
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    data = new { templateId = template.Id, name = template.Name },
                    message = "Onboarding template created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating onboarding template");
                return StatusCode(500, new { success = false, message = "Error creating template" });
            }
        }

        [HttpPut("templates/{templateId}")]
        public async Task<IActionResult> UpdateTemplate(int templateId, [FromBody] UpdateTemplateRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                // Validate admin/HR role
                if (!user.Role.Contains("Admin") && !user.Role.Contains("HR"))
                {
                    return Forbid("Only Admin and HR staff can update templates");
                }

                var template = await _context.OnboardingTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return NotFound(new { success = false, message = "Template not found" });
                }

                _logger.LogInformation($"Updating onboarding template: {templateId}");

                template.Name = request.Name ?? template.Name;
                template.Description = request.Description ?? template.Description;
                template.ForRole = request.ForRole ?? template.ForRole;
                template.ForDepartment = request.ForDepartment ?? template.ForDepartment;
                template.IsActive = request.IsActive ?? template.IsActive;
                template.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    data = new { templateId = template.Id, name = template.Name },
                    message = "Template updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template");
                return StatusCode(500, new { success = false, message = "Error updating template" });
            }
        }

        [HttpDelete("templates/{templateId}")]
        public async Task<IActionResult> DeleteTemplate(int templateId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                // Validate admin role
                if (!user.Role.Contains("Admin"))
                {
                    return Forbid("Only Admin can delete templates");
                }

                var template = await _context.OnboardingTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return NotFound(new { success = false, message = "Template not found" });
                }

                _logger.LogInformation($"Deactivating onboarding template: {templateId}");

                // Soft delete - deactivate instead of removing
                template.IsActive = false;
                template.ModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Template deactivated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template");
                return StatusCode(500, new { success = false, message = "Error deleting template" });
            }
        }

        // =============================================================================
        // EMPLOYEE ASSIGNMENT ENDPOINTS
        // =============================================================================

        [HttpPost("assign-template")]
        public async Task<IActionResult> AssignTemplate([FromBody] AssignTemplateRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var assignedBy = await GetEmployeeByUserId(user.Id);
                if (assignedBy == null) return BadRequest(new { success = false, message = "Employee profile required" });

                // Validate admin/HR role
                if (!user.Role.Contains("Admin") && !user.Role.Contains("HR"))
                {
                    return Forbid("Only Admin and HR staff can assign templates");
                }

                _logger.LogInformation($"Assigning template {request.TemplateId} to employee {request.EmployeeId}");

                // Check if employee already has onboarding assigned
                var existingAssignment = await _context.OnboardingChecklists
                    .FirstOrDefaultAsync(c => c.EmployeeId == request.EmployeeId);

                if (existingAssignment != null)
                {
                    return BadRequest(new { success = false, message = "Employee already has onboarding template assigned" });
                }

                // Execute stored procedure
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC AssignOnboardingTemplate @EmployeeId = {0}, @TemplateId = {1}, @AssignedById = {2}",
                    request.EmployeeId, request.TemplateId, assignedBy.Id);

                return Ok(new
                {
                    success = true,
                    message = "Template assigned successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning template");
                return StatusCode(500, new { success = false, message = "Error assigning template: " + ex.Message });
            }
        }

        [HttpPost("bulk-assign")]
        public async Task<IActionResult> BulkAssignTemplate([FromBody] BulkAssignRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var assignedBy = await GetEmployeeByUserId(user.Id);
                if (assignedBy == null) return BadRequest(new { success = false, message = "Employee profile required" });

                // Validate admin/HR role
                if (!user.Role.Contains("Admin") && !user.Role.Contains("HR"))
                {
                    return Forbid("Only Admin and HR staff can bulk assign templates");
                }

                _logger.LogInformation($"Bulk assigning template {request.TemplateId} to {request.EmployeeIds.Count} employees");

                var results = new List<object>();
                var successCount = 0;
                var errorCount = 0;

                foreach (var employeeId in request.EmployeeIds)
                {
                    try
                    {
                        // Check if employee already has onboarding
                        var existing = await _context.OnboardingChecklists
                            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId);

                        if (existing != null)
                        {
                            results.Add(new { employeeId, success = false, message = "Already has onboarding assigned" });
                            errorCount++;
                            continue;
                        }

                        // Execute assignment
                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC AssignOnboardingTemplate @EmployeeId = {0}, @TemplateId = {1}, @AssignedById = {2}",
                            employeeId, request.TemplateId, assignedBy.Id);

                        results.Add(new { employeeId, success = true, message = "Assigned successfully" });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        results.Add(new { employeeId, success = false, message = ex.Message });
                        errorCount++;
                    }
                }

                return Ok(new
                {
                    success = true,
                    data = new { results, successCount, errorCount },
                    message = $"Bulk assignment completed: {successCount} successful, {errorCount} errors"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk assignment");
                return StatusCode(500, new { success = false, message = "Error in bulk assignment" });
            }
        }

        // =============================================================================
        // ONBOARDING OVERVIEW AND ANALYTICS
        // =============================================================================

        [HttpGet("overview")]
        public async Task<IActionResult> GetOnboardingOverview([FromQuery] string? role = null, [FromQuery] string? department = null, [FromQuery] string? status = null)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                _logger.LogInformation($"Fetching onboarding overview for user: {user.Email}");

                // Use the OnboardingOverview view
                var query = _context.Set<OnboardingOverviewView>().AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(role))
                    query = query.Where(o => o.EmployeeName.Contains(role)); // You might want to join with Employee table for actual role filtering

                if (!string.IsNullOrEmpty(department))
                    query = query.Where(o => o.Department == department);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(o => o.OnboardingStatus == status);

                var overviewData = await query
                    .OrderByDescending(o => o.OnboardingStartDate)
                    .ToListAsync();

                // Get summary statistics
                var stats = new
                {
                    totalEmployees = overviewData.Count,
                    activeOnboarding = overviewData.Count(o => o.OnboardingStatus == "IN_PROGRESS"),
                    completedOnboarding = overviewData.Count(o => o.OnboardingStatus == "COMPLETED"),
                    notStarted = overviewData.Count(o => o.OnboardingStatus == "NOT_STARTED"),
                    averageCompletion = overviewData.Any() ? overviewData.Average(o => o.CompletionPercentage ?? 0) : 0,
                    averageDaysToComplete = overviewData.Where(o => o.OnboardingStatus == "COMPLETED").Any() ?
                        overviewData.Where(o => o.OnboardingStatus == "COMPLETED").Average(o => o.DaysInOnboarding ?? 0) : 0
                };

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        statistics = stats,
                        employees = overviewData.Select(o => new
                        {
                            employeeId = o.EmployeeId,
                            employeeName = o.EmployeeName,
                            email = o.Email,
                            position = o.Position,
                            department = o.Department,
                            hireDate = o.HireDate,
                            onboarding = new
                            {
                                status = o.OnboardingStatus,
                                completionPercentage = o.CompletionPercentage,
                                totalTasks = o.TotalTasks,
                                completedTasks = o.CompletedTasks,
                                pendingTasks = o.PendingTasks,
                                overdueTasks = o.OverdueTasks,
                                startDate = o.OnboardingStartDate,
                                completionDate = o.OnboardingCompletionDate,
                                daysInOnboarding = o.DaysInOnboarding,
                                templateName = o.TemplateName
                            }
                        }).ToList()
                    },
                    message = $"Found {overviewData.Count} employees with onboarding data"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching onboarding overview");
                return StatusCode(500, new { success = false, message = "Error fetching overview" });
            }
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetOnboardingAnalytics()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                _logger.LogInformation($"Fetching onboarding analytics for user: {user.Email}");

                // Get task summary by category
                var taskSummary = await _context.Set<OnboardingTaskSummaryView>().ToListAsync();

                // Get template usage statistics
                var templateStats = await _context.Set<TemplateUsageStatsView>().ToListAsync();

                // Get recent activity
                var recentActivities = await _context.OnboardingTasks
                    .Include(t => t.Employee)
                    .Where(t => !t.IsTemplate && t.CompletedDate.HasValue)
                    .OrderByDescending(t => t.CompletedDate)
                    .Take(10)
                    .Select(t => new
                    {
                        taskTitle = t.Title,
                        employeeName = $"{t.Employee!.FirstName} {t.Employee.LastName}",
                        completedDate = t.CompletedDate,
                        category = t.Category
                    })
                    .ToListAsync();

                // Get overdue tasks
                var overdueTasks = await _context.OnboardingTasks
                    .Include(t => t.Employee)
                    .Where(t => !t.IsTemplate && t.Status == "PENDING" && t.DueDate < DateTime.UtcNow)
                    .Select(t => new
                    {
                        taskId = t.Id,
                        taskTitle = t.Title,
                        employeeName = $"{t.Employee!.FirstName} {t.Employee.LastName}",
                        dueDate = t.DueDate,
                        priority = t.Priority,
                        category = t.Category,
                        daysOverdue = (int)(DateTime.UtcNow - t.DueDate!.Value).TotalDays
                    })
                    .OrderByDescending(t => t.daysOverdue)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        taskSummary = taskSummary.Select(ts => new
                        {
                            category = ts.Category,
                            totalTasks = ts.TotalTasks,
                            completedTasks = ts.CompletedTasks,
                            pendingTasks = ts.PendingTasks,
                            inProgressTasks = ts.InProgressTasks,
                            overdueTasks = ts.OverdueTasks,
                            avgCompletionDays = ts.AvgCompletionDays
                        }).ToList(),
                        templateStats = templateStats.Select(ts => new
                        {
                            templateId = ts.TemplateId,
                            templateName = ts.TemplateName,
                            forRole = ts.ForRole,
                            forDepartment = ts.ForDepartment,
                            timesAssigned = ts.TimesAssigned,
                            completedAssignments = ts.CompletedAssignments,
                            avgCompletionRate = ts.AvgCompletionRate,
                            avgCompletionDays = ts.AvgCompletionDays
                        }).ToList(),
                        recentActivities,
                        overdueTasks
                    },
                    message = "Analytics data retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching analytics");
                return StatusCode(500, new { success = false, message = "Error fetching analytics" });
            }
        }

        // =============================================================================
        // TASK MANAGEMENT ENDPOINTS
        // =============================================================================

        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks([FromQuery] int? employeeId = null, [FromQuery] string? status = null, [FromQuery] string? category = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                _logger.LogInformation($"Fetching onboarding tasks - Page: {page}, PageSize: {pageSize}");

                var query = _context.OnboardingTasks
                    .Include(t => t.Employee)
                    .Include(t => t.Documents)
                    .Where(t => !t.IsTemplate);

                // Apply filters
                if (employeeId.HasValue)
                    query = query.Where(t => t.EmployeeId == employeeId.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(t => t.Status == status);

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(t => t.Category == category);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination and ordering
                var tasks = await query
                    .OrderBy(t => t.DueDate)
                    .ThenBy(t => t.Priority == "HIGH" ? 1 : t.Priority == "MEDIUM" ? 2 : 3)
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
                            name = $"{t.Employee.FirstName} {t.Employee.LastName}",
                            email = t.Employee.Email,
                            position = t.Employee.Position
                        } : null,
                        documentsRequired = t.Documents.Count(d => d.Required),
                        documentsUploaded = t.Documents.Count(d => d.Uploaded),
                        isOverdue = t.Status == "PENDING" && t.DueDate < DateTime.UtcNow
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
                            page,
                            pageSize,
                            totalCount,
                            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                        }
                    },
                    message = $"Found {tasks.Count} tasks"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks");
                return StatusCode(500, new { success = false, message = "Error fetching tasks" });
            }
        }

        [HttpPut("tasks/{taskId}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskStatusRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var task = await _context.OnboardingTasks.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }

                _logger.LogInformation($"Updating task {taskId} status to {request.Status}");

                task.Status = request.Status;
                task.Notes = request.Notes ?? task.Notes;

                if (request.Status == "COMPLETED" && task.CompletedDate == null)
                {
                    task.CompletedDate = DateTime.UtcNow;
                }
                else if (request.Status != "COMPLETED")
                {
                    task.CompletedDate = null;
                }

                await _context.SaveChangesAsync();

                // Update progress will be handled by trigger
                return Ok(new
                {
                    success = true,
                    message = "Task status updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status");
                return StatusCode(500, new { success = false, message = "Error updating task status" });
            }
        }

        [HttpPost("tasks/bulk-update")]
        public async Task<IActionResult> BulkUpdateTasks([FromBody] BulkUpdateTasksRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                // Validate admin/HR role
                if (!user.Role.Contains("Admin") && !user.Role.Contains("HR"))
                {
                    return Forbid("Only Admin and HR staff can bulk update tasks");
                }

                _logger.LogInformation($"Bulk updating {request.TaskIds.Count} tasks");

                var tasks = await _context.OnboardingTasks
                    .Where(t => request.TaskIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var task in tasks)
                {
                    if (!string.IsNullOrEmpty(request.Status))
                    {
                        task.Status = request.Status;
                        if (request.Status == "COMPLETED" && task.CompletedDate == null)
                        {
                            task.CompletedDate = DateTime.UtcNow;
                        }
                    }

                    if (!string.IsNullOrEmpty(request.Priority))
                        task.Priority = request.Priority;

                    if (request.DueDate.HasValue)
                        task.DueDate = request.DueDate;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    data = new { updatedCount = tasks.Count },
                    message = $"Successfully updated {tasks.Count} tasks"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk update");
                return StatusCode(500, new { success = false, message = "Error in bulk update" });
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
                    .Include(t => t.Documents)
                    .Where(t => t.EmployeeId == employee.Id && !t.IsTemplate)
                    .OrderBy(t => t.DueDate)
                    .ThenBy(t => t.Priority == "HIGH" ? 1 : t.Priority == "MEDIUM" ? 2 : 3)
                    .ToListAsync();

                var progress = await _context.OnboardingProgress
                    .FirstOrDefaultAsync(p => p.EmployeeId == employee.Id);

                var taskData = tasks.Select(t => new
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
                    documents = t.Documents.Select(d => new
                    {
                        id = d.Id,
                        name = d.Name,
                        documentType = d.DocumentType,
                        required = d.Required,
                        uploaded = d.Uploaded,
                        uploadedDate = d.UploadedDate
                    }).ToList(),
                    isOverdue = t.Status == "PENDING" && t.DueDate < DateTime.UtcNow
                }).ToList();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        employee = new
                        {
                            id = employee.Id,
                            name = $"{employee.FirstName} {employee.LastName}",
                            email = employee.Email,
                            position = employee.Position,
                            department = employee.Department
                        },
                        progress = progress != null ? new
                        {
                            totalTasks = progress.TotalTasks,
                            completedTasks = progress.CompletedTasks,
                            pendingTasks = progress.PendingTasks,
                            overdueTasks = progress.OverdueTasks,
                            completionPercentage = progress.CompletionPercentage,
                            status = progress.Status,
                            startDate = progress.StartDate,
                            completionDate = progress.CompletionDate,
                            lastUpdated = progress.LastUpdated
                        } : null,
                        tasks = taskData
                    },
                    message = $"Found {taskData.Count} onboarding tasks"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee onboarding tasks");
                return StatusCode(500, new { success = false, message = "Error fetching tasks" });
            }
        }

        // =============================================================================
        // DOCUMENT MANAGEMENT ENDPOINTS
        // =============================================================================

        [HttpGet("tasks/{taskId}/documents")]
        public async Task<IActionResult> GetTaskDocuments(int taskId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var documents = await _context.OnboardingDocuments
                    .Include(d => d.UploadedBy)
                    .Where(d => d.TaskId == taskId)
                    .Select(d => new
                    {
                        id = d.Id,
                        name = d.Name,
                        documentType = d.DocumentType,
                        required = d.Required,
                        uploaded = d.Uploaded,
                        fileName = d.FileName,
                        fileSize = d.FileSize,
                        uploadedDate = d.UploadedDate,
                        uploadedBy = d.UploadedBy != null ? new
                        {
                            id = d.UploadedBy.Id,
                            name = $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}"
                        } : null
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = documents,
                    message = $"Found {documents.Count} documents for task"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task documents");
                return StatusCode(500, new { success = false, message = "Error fetching documents" });
            }
        }

        [HttpPost("tasks/{taskId}/documents/upload")]
        public async Task<IActionResult> UploadDocument(int taskId, [FromForm] DocumentUploadRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var employee = await GetEmployeeByUserId(user.Id);
                if (employee == null) return BadRequest(new { success = false, message = "Employee profile required" });

                var task = await _context.OnboardingTasks.FindAsync(taskId);
                if (task == null)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file provided" });
                }

                _logger.LogInformation($"Uploading document for task {taskId}");

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "onboarding");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(request.File.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // Save document record
                var document = new OnboardingDocument
                {
                    Name = request.DocumentName ?? request.File.FileName,
                    DocumentType = request.DocumentType ?? "GENERAL",
                    Required = false,
                    Uploaded = true,
                    FilePath = filePath,
                    FileName = request.File.FileName,
                    ContentType = request.File.ContentType,
                    FileSize = request.File.Length,
                    UploadedDate = DateTime.UtcNow,
                    TaskId = taskId,
                    UploadedById = employee.Id
                };

                _context.OnboardingDocuments.Add(document);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        documentId = document.Id,
                        fileName = document.FileName,
                        fileSize = document.FileSize
                    },
                    message = "Document uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, new { success = false, message = "Error uploading document" });
            }
        }

        [HttpGet("documents/{documentId}/download")]
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var document = await _context.OnboardingDocuments.FindAsync(documentId);
                if (document == null)
                {
                    return NotFound(new { success = false, message = "Document not found" });
                }

                if (!System.IO.File.Exists(document.FilePath))
                {
                    return NotFound(new { success = false, message = "Physical file not found" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
                return File(fileBytes, document.ContentType ?? "application/octet-stream", document.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document");
                return StatusCode(500, new { success = false, message = "Error downloading document" });
            }
        }

        // =============================================================================
        // REPORTING ENDPOINTS
        // =============================================================================

        [HttpGet("reports/completion-by-department")]
        public async Task<IActionResult> GetCompletionByDepartment()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var departmentStats = await (from e in _context.Employees
                                             join op in _context.OnboardingProgress on e.Id equals op.EmployeeId into prog
                                             from p in prog.DefaultIfEmpty()
                                             where e.IsActive
                                             group new { e, p } by e.Department into g
                                             select new
                                             {
                                                 department = g.Key,
                                                 totalEmployees = g.Count(),
                                                 withOnboarding = g.Count(x => x.p != null),
                                                 averageCompletion = g.Where(x => x.p != null).Average(x => (double?)x.p.CompletionPercentage) ?? 0,
                                                 completedCount = g.Count(x => x.p != null && x.p.Status == "COMPLETED"),
                                                 inProgressCount = g.Count(x => x.p != null && x.p.Status == "IN_PROGRESS"),
                                                 notStartedCount = g.Count(x => x.p != null && x.p.Status == "NOT_STARTED")
                                             }).ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = departmentStats,
                    message = $"Department completion report generated for {departmentStats.Count} departments"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating department report");
                return StatusCode(500, new { success = false, message = "Error generating report" });
            }
        }

        [HttpGet("reports/overdue-tasks")]
        public async Task<IActionResult> GetOverdueTasks()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var overdueTasks = await _context.OnboardingTasks
                    .Include(t => t.Employee)
                    .Where(t => !t.IsTemplate && t.Status == "PENDING" && t.DueDate < DateTime.UtcNow)
                    .Select(t => new
                    {
                        taskId = t.Id,
                        taskTitle = t.Title,
                        category = t.Category,
                        priority = t.Priority,
                        dueDate = t.DueDate,
                        employee = new
                        {
                            id = t.Employee!.Id,
                            name = $"{t.Employee.FirstName} {t.Employee.LastName}",
                            email = t.Employee.Email,
                            department = t.Employee.Department,
                            position = t.Employee.Position
                        },
                        daysOverdue = (int)(DateTime.UtcNow - t.DueDate!.Value).TotalDays
                    })
                    .OrderByDescending(t => t.daysOverdue)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = overdueTasks,
                    message = $"Found {overdueTasks.Count} overdue tasks"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching overdue tasks");
                return StatusCode(500, new { success = false, message = "Error fetching overdue tasks" });
            }
        }

        [HttpGet("reports/template-effectiveness")]
        public async Task<IActionResult> GetTemplateEffectiveness()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return Unauthorized(new { success = false, message = "Authentication required" });

                var templateStats = await _context.Set<TemplateUsageStatsView>()
                    .OrderByDescending(t => t.TimesAssigned)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = templateStats.Select(t => new
                    {
                        templateId = t.TemplateId,
                        templateName = t.TemplateName,
                        forRole = t.ForRole,
                        forDepartment = t.ForDepartment,
                        timesAssigned = t.TimesAssigned,
                        completedAssignments = t.CompletedAssignments,
                        completionRate = t.CompletedAssignments > 0 ? (double)t.CompletedAssignments / t.TimesAssigned * 100 : 0,
                        avgCompletionPercentage = t.AvgCompletionRate,
                        avgDaysToComplete = t.AvgCompletionDays,
                        effectiveness = CalculateTemplateEffectiveness(t.TimesAssigned, t.CompletedAssignments, t.AvgCompletionRate, t.AvgCompletionDays)
                    }).ToList(),
                    message = $"Template effectiveness report generated for {templateStats.Count} templates"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating template effectiveness report");
                return StatusCode(500, new { success = false, message = "Error generating report" });
            }
        }

        // =============================================================================
        // HELPER METHODS
        // =============================================================================

        private string CalculateTemplateEffectiveness(int timesAssigned, int completedAssignments, decimal? avgCompletionRate, decimal? avgCompletionDays)
        {
            if (timesAssigned == 0) return "Not Used";

            var completionRatio = (double)completedAssignments / timesAssigned;
            var avgCompletion = (double)(avgCompletionRate ?? 0);
            var avgDays = (double)(avgCompletionDays ?? 30);

            // Simple effectiveness scoring
            var score = (completionRatio * 0.4) + (avgCompletion / 100 * 0.4) + (Math.Max(0, 30 - avgDays) / 30 * 0.2);

            return score switch
            {
                >= 0.8 => "Excellent",
                >= 0.6 => "Good",
                >= 0.4 => "Average",
                >= 0.2 => "Poor",
                _ => "Very Poor"
            };
        }
    }

    // =============================================================================
    // REQUEST/RESPONSE MODELS
    // =============================================================================

    public class CreateTemplateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string ForRole { get; set; } = string.Empty;
        public string? ForDepartment { get; set; }
        public List<CreateTaskRequest>? Tasks { get; set; }
    }

    public class CreateTaskRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string Category { get; set; } = string.Empty;
        [Required]
        public string Priority { get; set; } = string.Empty;
        public string? EstimatedTime { get; set; }
        public string? Instructions { get; set; }
    }

    public class UpdateTemplateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ForRole { get; set; }
        public string? ForDepartment { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AssignTemplateRequest
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int TemplateId { get; set; }
    }

    public class BulkAssignRequest
    {
        [Required]
        public List<int> EmployeeIds { get; set; } = new();
        [Required]
        public int TemplateId { get; set; }
    }

    public class UpdateTaskStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class BulkUpdateTasksRequest
    {
        [Required]
        public List<int> TaskIds { get; set; } = new();
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class DocumentUploadRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        public string? DocumentName { get; set; }
        public string? DocumentType { get; set; }
    }

    // =============================================================================
    // VIEW MODELS (These should match your SQL views)
    // =============================================================================

    public class OnboardingOverviewView
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public int? TotalTasks { get; set; }
        public int? CompletedTasks { get; set; }
        public int? PendingTasks { get; set; }
        public int? OverdueTasks { get; set; }
        public decimal? CompletionPercentage { get; set; }
        public DateTime? OnboardingStartDate { get; set; }
        public DateTime? OnboardingCompletionDate { get; set; }
        public string? OnboardingStatus { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int? DaysInOnboarding { get; set; }
        public string? TemplateName { get; set; }
    }

    public class OnboardingTaskSummaryView
    {
        public string Category { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal? AvgCompletionDays { get; set; }
    }

    public class TemplateUsageStatsView
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string ForRole { get; set; } = string.Empty;
        public string? ForDepartment { get; set; }
        public int TimesAssigned { get; set; }
        public int CompletedAssignments { get; set; }
        public decimal? AvgCompletionRate { get; set; }
        public decimal? AvgCompletionDays { get; set; }
    }
}