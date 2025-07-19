// =============================================================================
// SESSION-BASED MENU CONTROLLER - WORKS WITH YOUR EXISTING AUTH SYSTEM
// File: TPAHRSystem.API/Controllers/MenuController.cs (REPLACE ENTIRE FILE)
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
            try
            {
                var token = GetTokenFromHeader();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No authorization token provided");
                    return null;
                }

                _logger.LogInformation($"Looking up user with token: {token.Substring(0, Math.Min(10, token.Length))}...");

                var userSession = await _context.UserSessions
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.SessionToken == token &&
                                             s.ExpiresAt > DateTime.UtcNow &&
                                             s.IsActive);

                if (userSession?.User != null)
                {
                    _logger.LogInformation($"User found: {userSession.User.Email} (Role: {userSession.User.Role})");
                    return userSession.User;
                }
                else
                {
                    _logger.LogWarning("No valid session found for token");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user from session");
                return null;
            }
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return null;

            // Don't use .Include(e => e.Department) since we ignored the navigation property
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            return employee;
        }

        private async Task<string?> GetDepartmentNameAsync(int? departmentId)
        {
            if (!departmentId.HasValue) return null;

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == departmentId.Value);

            return department?.Name;
        }

        // =============================================================================
        // MENU ENDPOINTS - SESSION-BASED AUTHENTICATION
        // =============================================================================

        /// <summary>
        /// Get user's menu items based on their role and permissions
        /// </summary>
        [HttpGet("user-menus")]
        public async Task<IActionResult> GetUserMenus()
        {

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Authentication required. Please provide a valid session token." });
            }

            var employee = await GetCurrentEmployeeAsync();
            var departmentName = await GetDepartmentNameAsync(employee?.DepartmentId);

            _logger.LogInformation($"Loading menus for user {user.Email} with role {user.Role}");

            // Get role-based menus using the helper method
            var menuItems = await _context.GetUserMenuItemsAsync(user.Role, employee?.DepartmentId);

            // Build hierarchical menu structure
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
                    menus = hierarchicalMenus,
                    totalMenus = menuItems.Count,
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

                // Use the database helper method to check permissions
                var canView = await _context.CheckUserMenuPermissionAsync(user.Role, menuName, "VIEW");
                var canEdit = await _context.CheckUserMenuPermissionAsync(user.Role, menuName, "EDIT");
                var canDelete = await _context.CheckUserMenuPermissionAsync(user.Role, menuName, "DELETE");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        menuName = menuName,
                        userRole = user.Role,
                        permissions = new
                        {
                            canView = canView,
                            canEdit = canEdit,
                            canDelete = canDelete
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permissions for menu: {menuName}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error checking menu permissions",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get navigation breadcrumbs for current menu
        /// </summary>
        [HttpGet("breadcrumbs")]
        public async Task<IActionResult> GetBreadcrumbs([FromQuery] string? currentRoute)
        {
            try
            {
                if (string.IsNullOrEmpty(currentRoute))
                {
                    return Ok(new { success = true, data = new List<object>() });
                }

                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                // Find the current menu item and build breadcrumb trail
                var menuItem = await _context.MenuItems
                    .Include(m => m.Parent)
                    .Include(m => m.RolePermissions)
                    .FirstOrDefaultAsync(m => m.Route == currentRoute && m.IsActive);

                if (menuItem == null)
                {
                    return NotFound(new { success = false, message = "Menu item not found" });
                }

                // Check if user has permission to view this menu
                var hasPermission = menuItem.RolePermissions
                    .Any(rp => rp.Role == user.Role && rp.CanView);

                if (!hasPermission)
                {
                    return StatusCode(403, new { success = false, message = "Insufficient permissions" });
                }

                var breadcrumbs = BuildBreadcrumbTrail(menuItem);

                return Ok(new
                {
                    success = true,
                    data = breadcrumbs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error building breadcrumbs for route: {currentRoute}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error building breadcrumbs",
                    error = ex.Message
                });
            }
        }

        // =============================================================================
        // MENU MANAGEMENT ENDPOINTS (Admin/SuperAdmin Only)
        // =============================================================================

        /// <summary>
        /// Get all menu items for management (Admin/SuperAdmin only)
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllMenuItems()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                // Only Admin and SuperAdmin can view all menus
                if (!user.Role.Contains("Admin"))
                {
                    return StatusCode(403, new { success = false, message = "Admin privileges required" });
                }

                var allMenus = await _context.MenuItems
                    .Include(m => m.Parent)
                    .Include(m => m.Children)
                    .Include(m => m.RolePermissions)
                    .OrderBy(m => m.SortOrder)
                    .ThenBy(m => m.Name)
                    .ToListAsync();

                var hierarchicalMenus = BuildCompleteMenuHierarchy(allMenus);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        menus = hierarchicalMenus,
                        totalMenus = allMenus.Count,
                        roles = new[] { "SuperAdmin", "Admin", "HRAdmin", "ProgramDirector", "ProgramCoordinator", "Employee" }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all menu items");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error loading menu items",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get role permissions for all menus (SuperAdmin only)
        /// </summary>
        [HttpGet("role-permissions")]
        public async Task<IActionResult> GetRolePermissions()
        {
            //try
            //{
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                // Only SuperAdmin can view all role permissions
                if (user.Role != "SuperAdmin")
                {
                    return StatusCode(403, new { success = false, message = "SuperAdmin privileges required" });
                }

                var rolePermissions = await _context.RoleMenuPermissions
                    .Include(rmp => rmp.MenuItem)
                    .GroupBy(rmp => rmp.Role)
                    .Select(g => new
                    {
                        role = g.Key,
                        permissions = g.Select(rmp => new
                        {
                            menuId = rmp.MenuItemId,
                            menuName = rmp.MenuItem.Name,
                            menuRoute = rmp.MenuItem.Route,
                            canView = rmp.CanView,
                            canEdit = rmp.CanEdit,
                            canDelete = rmp.CanDelete
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = rolePermissions
                });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading role permissions");
        //        return StatusCode(500, new
        //        {
        //            success = false,
        //            message = "Error loading role permissions",
        //            error = ex.Message
        //        });
        //    }
        }

        /// <summary>
        /// Health check endpoint (no authentication required)
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                success = true,
                message = "Menu Management API is healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                authType = "Session-based"
            });
        }

        /// <summary>
        /// Debug current user session
        /// </summary>
        [HttpGet("debug-session")]
        public async Task<IActionResult> DebugSession()
        {
            try
            {
                var token = GetTokenFromHeader();
                var user = await GetCurrentUserAsync();
                var employee = await GetCurrentEmployeeAsync();
                var departmentName = await GetDepartmentNameAsync(employee?.DepartmentId);

                return Ok(new
                {
                    success = true,
                    debug = new
                    {
                        hasAuthHeader = !string.IsNullOrEmpty(Request.Headers["Authorization"].FirstOrDefault()),
                        tokenPreview = token?.Substring(0, Math.Min(20, token?.Length ?? 0)) + "...",
                        userFound = user != null,
                        user = user != null ? new { user.Id, user.Email, user.Role, user.IsActive } : null,
                        employee = employee != null ? new { employee.Id, employee.FirstName, employee.LastName, employee.DepartmentId, departmentName } : null,
                        sessionCount = await _context.UserSessions.CountAsync(s => s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                    }
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = ex.Message,
                    debug = new
                    {
                        hasAuthHeader = !string.IsNullOrEmpty(Request.Headers["Authorization"].FirstOrDefault())
                    }
                });
            }
        }

        // =============================================================================
        // HELPER METHODS
        // =============================================================================

        private List<object> BuildMenuHierarchy(List<UserMenuItemResult> menuItems)
        {
            var parentMenus = menuItems
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.SortOrder)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    route = m.Route,
                    icon = m.Icon,
                    permissions = new
                    {
                        canView = m.CanView,
                        canEdit = m.CanEdit,
                        canDelete = m.CanDelete
                    },
                    children = BuildChildMenus(menuItems, m.Id)
                })
                .Cast<object>()
                .ToList();

            return parentMenus;
        }

        private List<object> BuildChildMenus(List<UserMenuItemResult> allMenus, int parentId)
        {
            return allMenus
                .Where(m => m.ParentId == parentId)
                .OrderBy(m => m.SortOrder)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    route = m.Route,
                    icon = m.Icon,
                    permissions = new
                    {
                        canView = m.CanView,
                        canEdit = m.CanEdit,
                        canDelete = m.CanDelete
                    },
                    children = BuildChildMenus(allMenus, m.Id)
                })
                .Cast<object>()
                .ToList();
        }

        private List<object> BuildCompleteMenuHierarchy(List<MenuItem> allMenus)
        {
            var parentMenus = allMenus
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.SortOrder)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    route = m.Route,
                    icon = m.Icon,
                    isActive = m.IsActive,
                    requiredPermission = m.RequiredPermission,
                    rolePermissions = m.RolePermissions.Select(rp => new
                    {
                        role = rp.Role,
                        canView = rp.CanView,
                        canEdit = rp.CanEdit,
                        canDelete = rp.CanDelete
                    }).ToList(),
                    children = BuildCompleteChildMenus(allMenus, m.Id)
                })
                .Cast<object>()
                .ToList();

            return parentMenus;
        }

        private List<object> BuildCompleteChildMenus(List<MenuItem> allMenus, int parentId)
        {
            return allMenus
                .Where(m => m.ParentId == parentId)
                .OrderBy(m => m.SortOrder)
                .Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    route = m.Route,
                    icon = m.Icon,
                    isActive = m.IsActive,
                    requiredPermission = m.RequiredPermission,
                    rolePermissions = m.RolePermissions.Select(rp => new
                    {
                        role = rp.Role,
                        canView = rp.CanView,
                        canEdit = rp.CanEdit,
                        canDelete = rp.CanDelete
                    }).ToList(),
                    children = BuildCompleteChildMenus(allMenus, m.Id)
                })
                .Cast<object>()
                .ToList();
        }

        private List<object> BuildBreadcrumbTrail(MenuItem menuItem)
        {
            var breadcrumbs = new List<object>();
            var current = menuItem;

            // Build breadcrumb trail from current item back to root
            while (current != null)
            {
                breadcrumbs.Insert(0, new
                {
                    id = current.Id,
                    name = current.Name,
                    route = current.Route,
                    icon = current.Icon
                });
                current = current.Parent;
            }

            return breadcrumbs;
        }
    }

    // =============================================================================
    // REQUEST MODELS
    // =============================================================================

    /// <summary>
    /// Request models for menu operations
    /// </summary>
    public class MenuPermissionRequest
    {
        [Required]
        public string MenuName { get; set; } = string.Empty;

        [Required]
        public string PermissionType { get; set; } = string.Empty; // VIEW, EDIT, DELETE
    }
}