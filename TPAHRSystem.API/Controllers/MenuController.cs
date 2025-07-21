// =============================================================================
// FIXED MENU CONTROLLER - RESOLVE TYPE MAPPING ISSUES
// File: TPAHRSystem.API/Controllers/MenuController.cs (Replace existing)
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly TPADbContext _context;
        private readonly ILogger<MenuController> _logger;

        public MenuController(TPADbContext context, ILogger<MenuController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =============================================================================
        // SESSION-BASED AUTHENTICATION HELPERS
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

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return null;

            return await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
        }

        private async Task<string?> GetDepartmentNameAsync(int? departmentId)
        {
            if (!departmentId.HasValue) return null;

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId.Value);

            return department?.Name;
        }

        private async Task<bool> IsHROrAdmin(User user)
        {
            return user.Role.Contains("HR") || user.Role.Contains("Admin") || user.Role.Contains("SuperAdmin");
        }

        // =============================================================================
        // ONBOARDING-AWARE MENU ENDPOINTS
        // =============================================================================

        /// <summary>
        /// Get user's accessible menu items based on their role, department, and onboarding status
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserMenus()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Please provide a valid session token." });
            }

            var employee = await GetCurrentEmployeeAsync();
            var departmentName = await GetDepartmentNameAsync(employee?.DepartmentId);

            _logger.LogInformation($"Loading menus for user {user.Email} with role {user.Role}");

            // Check if user is in onboarding and should have restricted access
            var hasRestrictedAccess = false;
            var onboardingStatus = "COMPLETED";

            if (employee != null)
            {
                hasRestrictedAccess = employee.IsOnboardingLocked ?? false;
                onboardingStatus = employee.OnboardingStatus ?? "COMPLETED";
            }

            // If user is in onboarding and not HR/Admin, return only onboarding menus
            if (hasRestrictedAccess && !await IsHROrAdmin(user))
            {
                var onboardingMenus = GetOnboardingOnlyMenus();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        userRole = user.Role,
                        userId = user.Id,
                        userEmail = user.Email,
                        departmentId = employee?.DepartmentId,
                        departmentName = departmentName,
                        isOnboardingLocked = hasRestrictedAccess,
                        onboardingStatus = onboardingStatus,
                        menus = onboardingMenus,
                        totalMenus = onboardingMenus.Count,
                        accessLevel = "ONBOARDING_ONLY",
                        lastUpdated = DateTime.UtcNow
                    }
                });
            }

            // Get full role-based menus for completed onboarding or HR/Admin users
            var menuItems = await GetUserMenuItemsAsync(user.Role, employee?.DepartmentId);
            var hierarchicalMenus = BuildMenuHierarchy(menuItems);

            return Ok(new
            {
                success = true,
                data = new
                {
                    userRole = user.Role,
                    userId = user.Id,
                    userEmail = user.Email,
                    departmentId = employee?.DepartmentId,
                    departmentName = departmentName,
                    isOnboardingLocked = hasRestrictedAccess,
                    onboardingStatus = onboardingStatus,
                    menus = hierarchicalMenus,
                    totalMenus = menuItems.Count,
                    accessLevel = hasRestrictedAccess ? "RESTRICTED" : "FULL_ACCESS",
                    lastUpdated = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// Get specific menu permissions for current user
        /// </summary>
        [HttpGet("permissions/{menuName}")]
        public async Task<IActionResult> GetMenuPermissions(string menuName)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                var employee = await GetCurrentEmployeeAsync();
                var hasRestrictedAccess = employee?.IsOnboardingLocked ?? false;

                // If user is in onboarding, only allow onboarding-related permissions
                if (hasRestrictedAccess && !await IsHROrAdmin(user))
                {
                    var isOnboardingMenu = IsOnboardingRelatedMenu(menuName);

                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            menuName = menuName,
                            canView = isOnboardingMenu,
                            canEdit = false, // Employees in onboarding cannot edit
                            canDelete = false,
                            isRestricted = true,
                            reason = isOnboardingMenu ? null : "Access restricted during onboarding"
                        }
                    });
                }

                // Check normal permissions for full access users
                var canView = await CheckUserMenuPermissionAsync(user.Role, menuName, "VIEW");
                var canEdit = await CheckUserMenuPermissionAsync(user.Role, menuName, "EDIT");
                var canDelete = await CheckUserMenuPermissionAsync(user.Role, menuName, "DELETE");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        menuName = menuName,
                        canView = canView,
                        canEdit = canEdit,
                        canDelete = canDelete,
                        isRestricted = false
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permissions for menu: {menuName}");
                return StatusCode(500, new { success = false, message = "Error checking menu permissions" });
            }
        }

        /// <summary>
        /// Check if user can access a specific menu item
        /// </summary>
        [HttpGet("access-check/{menuName}")]
        public async Task<IActionResult> CheckMenuAccess(string menuName)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                var employee = await GetCurrentEmployeeAsync();
                var hasRestrictedAccess = employee?.IsOnboardingLocked ?? false;

                // HR and Admin always have access
                if (await IsHROrAdmin(user))
                {
                    return Ok(new
                    {
                        success = true,
                        hasAccess = true,
                        reason = "HR/Admin access"
                    });
                }

                // If user is in onboarding, check if menu is onboarding-related
                if (hasRestrictedAccess)
                {
                    var isOnboardingMenu = IsOnboardingRelatedMenu(menuName);
                    return Ok(new
                    {
                        success = true,
                        hasAccess = isOnboardingMenu,
                        reason = isOnboardingMenu ? "Onboarding menu access granted" : "Access restricted during onboarding"
                    });
                }

                // Full access for completed onboarding
                var canView = await CheckUserMenuPermissionAsync(user.Role, menuName, "VIEW");
                return Ok(new
                {
                    success = true,
                    hasAccess = canView,
                    reason = canView ? "Full system access" : "Insufficient permissions"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking access for menu: {menuName}");
                return StatusCode(500, new { success = false, message = "Error checking menu access" });
            }
        }

        // =============================================================================
        // MENU DATA METHODS - FIXED TYPE MAPPING
        // =============================================================================

        /// <summary>
        /// Get user menu items based on role and department - FIXED VERSION
        /// </summary>
        private async Task<List<MenuItemDto>> GetUserMenuItemsAsync(string userRole, int? departmentId = null)
        {
            try
            {
                // Simulate menu structure based on role - replace with your actual logic
                var menuItems = new List<MenuItemDto>();

                // Admin and HR get full access
                if (userRole.Contains("Admin") || userRole.Contains("HR"))
                {
                    menuItems = GetFullMenuStructure();
                }
                else
                {
                    // Regular employees get limited access based on their department
                    menuItems = GetEmployeeMenuStructure(departmentId);
                }

                return await Task.FromResult(menuItems);
            }
            catch
            {
                return new List<MenuItemDto>();
            }
        }

        /// <summary>
        /// Check if user has permission for a specific menu action - FIXED VERSION
        /// </summary>
        private async Task<bool> CheckUserMenuPermissionAsync(string userRole, string menuName, string action)
        {
            try
            {
                // Admin and HR have all permissions
                if (userRole.Contains("Admin") || userRole.Contains("HR"))
                {
                    return true;
                }

                // Define basic permissions for regular employees
                var allowedMenus = new[]
                {
                    "Dashboard", "Profile", "Time Entry", "Reports", "Help"
                };

                var canView = allowedMenus.Contains(menuName, StringComparer.OrdinalIgnoreCase);

                return action.ToUpper() switch
                {
                    "VIEW" => canView,
                    "EDIT" => canView && (menuName == "Profile" || menuName == "Time Entry"),
                    "DELETE" => false, // Regular employees can't delete
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        // =============================================================================
        // ONBOARDING MENU HELPERS
        // =============================================================================

        /// <summary>
        /// Get menus available during onboarding (restricted access)
        /// </summary>
        private List<object> GetOnboardingOnlyMenus()
        {
            return new List<object>
            {
                new
                {
                    id = 1,
                    name = "Onboarding",
                    route = "/onboarding",
                    icon = "Users",
                    parentId = (int?)null,
                    sortOrder = 1,
                    isActive = true,
                    children = new List<object>
                    {
                        new
                        {
                            id = 11,
                            name = "My Tasks",
                            route = "/onboarding/my-tasks",
                            icon = "CheckSquare",
                            parentId = 1,
                            sortOrder = 1,
                            isActive = true
                        },
                        new
                        {
                            id = 12,
                            name = "Progress",
                            route = "/onboarding/progress",
                            icon = "TrendingUp",
                            parentId = 1,
                            sortOrder = 2,
                            isActive = true
                        }
                    }
                },
                new
                {
                    id = 2,
                    name = "Profile",
                    route = "/profile",
                    icon = "User",
                    parentId = (int?)null,
                    sortOrder = 2,
                    isActive = true,
                    children = new List<object>
                    {
                        new
                        {
                            id = 21,
                            name = "Personal Information",
                            route = "/profile/personal",
                            icon = "Info",
                            parentId = 2,
                            sortOrder = 1,
                            isActive = true
                        }
                    }
                },
                new
                {
                    id = 3,
                    name = "Help",
                    route = "/help",
                    icon = "HelpCircle",
                    parentId = (int?)null,
                    sortOrder = 3,
                    isActive = true,
                    children = new List<object>
                    {
                        new
                        {
                            id = 31,
                            name = "Getting Started",
                            route = "/help/getting-started",
                            icon = "PlayCircle",
                            parentId = 3,
                            sortOrder = 1,
                            isActive = true
                        },
                        new
                        {
                            id = 32,
                            name = "Contact HR",
                            route = "/help/contact-hr",
                            icon = "Phone",
                            parentId = 3,
                            sortOrder = 2,
                            isActive = true
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Check if a menu is onboarding-related and should be accessible during onboarding
        /// </summary>
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

        /// <summary>
        /// Build hierarchical menu structure from flat menu items - FIXED VERSION
        /// </summary>
        private List<object> BuildMenuHierarchy(List<MenuItemDto> menuItems)
        {
            var parentMenus = menuItems.Where(m => m.ParentId == null).OrderBy(m => m.SortOrder);
            var hierarchicalMenus = new List<object>();

            foreach (var parent in parentMenus)
            {
                var children = menuItems
                    .Where(m => m.ParentId == parent.Id)
                    .OrderBy(m => m.SortOrder)
                    .Select(child => new
                    {
                        id = child.Id,
                        name = child.Name,
                        route = child.Route,
                        icon = child.Icon,
                        parentId = child.ParentId,
                        sortOrder = child.SortOrder,
                        isActive = child.IsActive,
                        requiredPermission = child.RequiredPermission
                    })
                    .ToList();

                hierarchicalMenus.Add(new
                {
                    id = parent.Id,
                    name = parent.Name,
                    route = parent.Route,
                    icon = parent.Icon,
                    parentId = parent.ParentId,
                    sortOrder = parent.SortOrder,
                    isActive = parent.IsActive,
                    requiredPermission = parent.RequiredPermission,
                    children = children
                });
            }

            return hierarchicalMenus;
        }

        /// <summary>
        /// Get full menu structure for admin/HR users - FIXED VERSION
        /// </summary>
        private List<MenuItemDto> GetFullMenuStructure()
        {
            return new List<MenuItemDto>
            {
                // Dashboard
                new MenuItemDto { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "BarChart3", ParentId = null, SortOrder = 1, IsActive = true },
                
                // Employee Management
                new MenuItemDto { Id = 2, Name = "Employee Management", Route = "/employees", Icon = "Users", ParentId = null, SortOrder = 2, IsActive = true },
                new MenuItemDto { Id = 21, Name = "Employee List", Route = "/employees/list", Icon = "List", ParentId = 2, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 22, Name = "Add Employee", Route = "/employees/add", Icon = "UserPlus", ParentId = 2, SortOrder = 2, IsActive = true },
                new MenuItemDto { Id = 23, Name = "Employee Reports", Route = "/employees/reports", Icon = "FileText", ParentId = 2, SortOrder = 3, IsActive = true },
                
                // Onboarding
                new MenuItemDto { Id = 3, Name = "Onboarding", Route = "/onboarding", Icon = "UserCheck", ParentId = null, SortOrder = 3, IsActive = true },
                new MenuItemDto { Id = 31, Name = "Onboarding Overview", Route = "/onboarding/overview", Icon = "Eye", ParentId = 3, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 32, Name = "Task Management", Route = "/onboarding/tasks", Icon = "CheckSquare", ParentId = 3, SortOrder = 2, IsActive = true },
                new MenuItemDto { Id = 33, Name = "Progress Tracking", Route = "/onboarding/progress", Icon = "TrendingUp", ParentId = 3, SortOrder = 3, IsActive = true },
                
                // Time & Attendance
                new MenuItemDto { Id = 4, Name = "Time & Attendance", Route = "/time", Icon = "Clock", ParentId = null, SortOrder = 4, IsActive = true },
                new MenuItemDto { Id = 41, Name = "Time Entry", Route = "/time/entry", Icon = "Plus", ParentId = 4, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 42, Name = "Timesheets", Route = "/time/sheets", Icon = "Calendar", ParentId = 4, SortOrder = 2, IsActive = true },
                new MenuItemDto { Id = 43, Name = "Time Reports", Route = "/time/reports", Icon = "BarChart", ParentId = 4, SortOrder = 3, IsActive = true },
                
                // Administration
                new MenuItemDto { Id = 5, Name = "Administration", Route = "/admin", Icon = "Settings", ParentId = null, SortOrder = 5, IsActive = true },
                new MenuItemDto { Id = 51, Name = "User Management", Route = "/admin/users", Icon = "UserCog", ParentId = 5, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 52, Name = "Department Setup", Route = "/admin/departments", Icon = "Building", ParentId = 5, SortOrder = 2, IsActive = true },
                new MenuItemDto { Id = 53, Name = "System Settings", Route = "/admin/settings", Icon = "Cog", ParentId = 5, SortOrder = 3, IsActive = true },
                
                // Profile
                new MenuItemDto { Id = 7, Name = "Profile", Route = "/profile", Icon = "User", ParentId = null, SortOrder = 7, IsActive = true },
                new MenuItemDto { Id = 71, Name = "Personal Information", Route = "/profile/personal", Icon = "Info", ParentId = 7, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 72, Name = "Account Settings", Route = "/profile/settings", Icon = "Settings", ParentId = 7, SortOrder = 2, IsActive = true },
                
                // Help
                new MenuItemDto { Id = 8, Name = "Help", Route = "/help", Icon = "HelpCircle", ParentId = null, SortOrder = 8, IsActive = true },
                new MenuItemDto { Id = 81, Name = "Documentation", Route = "/help/docs", Icon = "Book", ParentId = 8, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 82, Name = "Support", Route = "/help/support", Icon = "MessageCircle", ParentId = 8, SortOrder = 2, IsActive = true }
            };
        }

        /// <summary>
        /// Get limited menu structure for regular employees - FIXED VERSION
        /// </summary>
        private List<MenuItemDto> GetEmployeeMenuStructure(int? departmentId)
        {
            return new List<MenuItemDto>
            {
                // Dashboard
                new MenuItemDto { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "BarChart3", ParentId = null, SortOrder = 1, IsActive = true },
                
                // Time & Attendance
                new MenuItemDto { Id = 4, Name = "Time & Attendance", Route = "/time", Icon = "Clock", ParentId = null, SortOrder = 2, IsActive = true },
                new MenuItemDto { Id = 41, Name = "Time Entry", Route = "/time/entry", Icon = "Plus", ParentId = 4, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 42, Name = "My Timesheets", Route = "/time/my-sheets", Icon = "Calendar", ParentId = 4, SortOrder = 2, IsActive = true },
                
                // Profile
                new MenuItemDto { Id = 7, Name = "Profile", Route = "/profile", Icon = "User", ParentId = null, SortOrder = 3, IsActive = true },
                new MenuItemDto { Id = 71, Name = "Personal Information", Route = "/profile/personal", Icon = "Info", ParentId = 7, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 72, Name = "Account Settings", Route = "/profile/settings", Icon = "Settings", ParentId = 7, SortOrder = 2, IsActive = true },
                
                // Help
                new MenuItemDto { Id = 8, Name = "Help", Route = "/help", Icon = "HelpCircle", ParentId = null, SortOrder = 4, IsActive = true },
                new MenuItemDto { Id = 81, Name = "Documentation", Route = "/help/docs", Icon = "Book", ParentId = 8, SortOrder = 1, IsActive = true },
                new MenuItemDto { Id = 82, Name = "Support", Route = "/help/support", Icon = "MessageCircle", ParentId = 8, SortOrder = 2, IsActive = true }
            };
        }

        // =============================================================================
        // DASHBOARD INTEGRATION
        // =============================================================================

        /// <summary>
        /// Get dashboard configuration based on onboarding status
        /// </summary>
        [HttpGet("dashboard-config")]
        public async Task<IActionResult> GetDashboardConfig()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                var employee = await GetCurrentEmployeeAsync();
                var hasRestrictedAccess = employee?.IsOnboardingLocked ?? false;

                if (hasRestrictedAccess && !await IsHROrAdmin(user))
                {
                    // Return onboarding-specific dashboard configuration
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            dashboardType = "ONBOARDING",
                            title = "Welcome to TPA HR System",
                            subtitle = "Complete your onboarding to access the full system",
                            showProgressBar = true,
                            showTaskList = true,
                            showWelcomeMessage = true,
                            restrictedAccess = true,
                            availableWidgets = new[]
                            {
                                "onboarding-progress",
                                "pending-tasks",
                                "welcome-message",
                                "hr-contact"
                            }
                        }
                    });
                }

                // Return full dashboard configuration
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        dashboardType = "FULL",
                        title = "TPA HR Dashboard",
                        subtitle = $"Welcome back, {user.Email}",
                        showProgressBar = false,
                        showTaskList = false,
                        showWelcomeMessage = false,
                        restrictedAccess = false,
                        availableWidgets = new[]
                        {
                            "stats-overview",
                            "recent-activity",
                            "notifications",
                            "quick-actions",
                            "calendar-widget"
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard config");
                return StatusCode(500, new { success = false, message = "Error getting dashboard configuration" });
            }
        }

        // =============================================================================
        // HEALTH CHECK ENDPOINTS
        // =============================================================================

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { success = true, message = "Menu Controller is healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var user = await GetCurrentUserAsync();
            var employee = await GetCurrentEmployeeAsync();

            return Ok(new
            {
                success = true,
                message = "Menu Controller is working!",
                timestamp = DateTime.UtcNow,
                authenticatedUser = user?.Email ?? "Not authenticated",
                employee = employee != null ? new
                {
                    id = employee.Id,
                    number = employee.EmployeeNumber,
                    isOnboardingLocked = employee.IsOnboardingLocked,
                    onboardingStatus = employee.OnboardingStatus
                } : null
            });
        }
    }

    // =============================================================================
    // MENU ITEM DTO CLASS (REPLACES THE MISSING MenuItem CLASS)
    // =============================================================================

    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string? RequiredPermission { get; set; }
    }
}