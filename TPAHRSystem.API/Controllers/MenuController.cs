// =============================================================================
// MENU CONTROLLER - FINAL CLEAN VERSION
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

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return null;

            try
            {
                return await _context.Employees
                    .Where(e => e.UserId == user.Id)
                    .Select(e => new Employee
                    {
                        Id = e.Id,
                        UserId = e.UserId,
                        FirstName = e.FirstName ?? "",
                        LastName = e.LastName ?? "",
                        DepartmentId = e.DepartmentId,
                        IsActive = e.IsActive ,
                        CreatedAt = e.CreatedAt
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching employee data for user {user.Id}");
                return null;
            }
        }

        private async Task<string?> GetDepartmentNameAsync(int? departmentId)
        {
            if (!departmentId.HasValue) return null;

            try
            {
                var department = await _context.Departments
                    .Where(d => d.Id == departmentId.Value)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync();

                return department;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching department name for ID {departmentId}");
                return null;
            }
        }

        // =============================================================================
        // MAIN MENU ENDPOINTS
        // =============================================================================

        /// <summary>
        /// Get user's accessible menu items based on their role and department
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserMenus()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Please provide a valid session token." });
                }

                var employee = await GetCurrentEmployeeAsync();
                var departmentName = await GetDepartmentNameAsync(employee?.DepartmentId);

                _logger.LogInformation($"Loading menus for user {user.Email} with role {user.Role}");

                // Get all active menu items with role permissions
                var allMenus = await _context.MenuItems
                    .Include(m => m.Parent)
                    .Include(m => m.Children)
                    .Include(m => m.RolePermissions)
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.SortOrder)
                    .ThenBy(m => m.Name)
                    .ToListAsync();

                // Filter menus based on user role
                var accessibleMenus = allMenus.Where(menu => HasMenuAccess(menu, user.Role)).ToList();

                // Build hierarchical structure for user display
                var hierarchicalMenus = BuildUserMenuTree(accessibleMenus);

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
                        totalMenus = accessibleMenus.Count,
                        lastUpdated = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user menus");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error loading menu items",
                    error = ex.Message
                });
            }
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

                // Find the menu and check permissions
                var menuItem = await _context.MenuItems
                    .Include(m => m.RolePermissions)
                    .FirstOrDefaultAsync(m => m.Name.ToLower() == menuName.ToLower() && m.IsActive);

                if (menuItem == null)
                {
                    return NotFound(new { success = false, message = "Menu item not found" });
                }

                var rolePermission = menuItem.RolePermissions
                    .FirstOrDefault(rp => rp.Role == user.Role);

                var permissions = new
                {
                    canView = rolePermission?.CanView ?? false,
                    canEdit = rolePermission?.CanEdit ?? false,
                    canDelete = rolePermission?.CanDelete ?? false
                };

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        menuName = menuName,
                        userRole = user.Role,
                        permissions = permissions
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
        // MENU CRUD OPERATIONS (SuperAdmin Only)
        // =============================================================================

        /// <summary>
        /// Create a new menu item (SuperAdmin only)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (user.Role != "SuperAdmin")
                {
                    return StatusCode(403, new { success = false, message = "SuperAdmin privileges required" });
                }

                // Validate parent exists if specified
                if (request.ParentId.HasValue)
                {
                    var parentExists = await _context.MenuItems.AnyAsync(m => m.Id == request.ParentId);
                    if (!parentExists)
                    {
                        return BadRequest(new { success = false, message = "Parent menu item not found" });
                    }
                }

                var menuItem = new MenuItem
                {
                    Name = request.Name,
                    Route = request.Route,
                    Icon = request.Icon,
                    ParentId = request.ParentId,
                    SortOrder = request.SortOrder,
                    IsActive = true,
                    RequiredPermission = request.RequiredPermission,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

                // Create role permissions if specified
                if (request.RolePermissions != null && request.RolePermissions.Any())
                {
                    foreach (var rolePermission in request.RolePermissions)
                    {
                        var permission = new RoleMenuPermission
                        {
                            MenuItemId = menuItem.Id,
                            Role = rolePermission.Role,
                            CanView = rolePermission.CanView,
                            CanEdit = rolePermission.CanEdit,
                            CanDelete = rolePermission.CanDelete
                        };
                        _context.RoleMenuPermissions.Add(permission);
                    }
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Menu item '{menuItem.Name}' created by {user.Email}");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = menuItem.Id,
                        name = menuItem.Name,
                        route = menuItem.Route,
                        message = "Menu item created successfully"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu item");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error creating menu item",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a specific menu item for editing (SuperAdmin only)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (user.Role != "SuperAdmin")
                {
                    return StatusCode(403, new { success = false, message = "SuperAdmin privileges required" });
                }

                var menuItem = await _context.MenuItems
                    .Include(m => m.Parent)
                    .Include(m => m.Children)
                    .Include(m => m.RolePermissions)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (menuItem == null)
                {
                    return NotFound(new { success = false, message = "Menu item not found" });
                }

                return Ok(new
                {
                    success = true,
                    menuItem = new
                    {
                        id = menuItem.Id,
                        name = menuItem.Name,
                        route = menuItem.Route,
                        icon = menuItem.Icon,
                        parentId = menuItem.ParentId,
                        parentName = menuItem.Parent?.Name,
                        sortOrder = menuItem.SortOrder,
                        isActive = menuItem.IsActive,
                        requiredPermission = menuItem.RequiredPermission,
                        createdAt = menuItem.CreatedAt,
                        childrenCount = menuItem.Children.Count,
                        rolePermissions = menuItem.RolePermissions.Select(rp => new
                        {
                            role = rp.Role,
                            canView = rp.CanView,
                            canEdit = rp.CanEdit,
                            canDelete = rp.CanDelete
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting menu item {id}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error retrieving menu item",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing menu item (SuperAdmin only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] UpdateMenuRequest request)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (user.Role != "SuperAdmin")
                {
                    return StatusCode(403, new { success = false, message = "SuperAdmin privileges required" });
                }

                var menuItem = await _context.MenuItems.FindAsync(id);
                if (menuItem == null)
                {
                    return NotFound(new { success = false, message = "Menu item not found" });
                }

                // Update properties only if provided
                if (!string.IsNullOrEmpty(request.Name))
                    menuItem.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Route))
                    menuItem.Route = request.Route;

                if (request.Icon != null)
                    menuItem.Icon = request.Icon;

                if (request.SortOrder.HasValue)
                    menuItem.SortOrder = request.SortOrder.Value;

                if (request.IsActive.HasValue)
                    menuItem.IsActive = request.IsActive.Value;

                if (request.RequiredPermission != null)
                    menuItem.RequiredPermission = request.RequiredPermission;

                // Handle parent change
                if (request.ParentId.HasValue)
                {
                    if (request.ParentId.Value == 0)
                    {
                        menuItem.ParentId = null;
                    }
                    else
                    {
                        var parentExists = await _context.MenuItems.AnyAsync(m => m.Id == request.ParentId.Value);
                        if (!parentExists)
                        {
                            return BadRequest(new { success = false, message = "Parent menu item not found" });
                        }

                        // Prevent circular reference
                        if (request.ParentId.Value == id)
                        {
                            return BadRequest(new { success = false, message = "Menu item cannot be its own parent" });
                        }

                        menuItem.ParentId = request.ParentId.Value;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Menu item '{menuItem.Name}' updated by {user.Email}");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = menuItem.Id,
                        name = menuItem.Name,
                        route = menuItem.Route,
                        message = "Menu item updated successfully"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating menu item {id}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating menu item",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a menu item (SuperAdmin only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (user.Role != "SuperAdmin")
                {
                    return StatusCode(403, new { success = false, message = "SuperAdmin privileges required" });
                }

                var menuItem = await _context.MenuItems
                    .Include(m => m.Children)
                    .Include(m => m.RolePermissions)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (menuItem == null)
                {
                    return NotFound(new { success = false, message = "Menu item not found" });
                }

                // Check if menu has children
                if (menuItem.Children.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cannot delete menu item with child items. Delete children first or move them to another parent.",
                        childCount = menuItem.Children.Count
                    });
                }

                var menuName = menuItem.Name;

                // Remove role permissions first
                _context.RoleMenuPermissions.RemoveRange(menuItem.RolePermissions);

                // Remove menu item
                _context.MenuItems.Remove(menuItem);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Menu item '{menuName}' deleted by {user.Email}");

                return Ok(new
                {
                    success = true,
                    message = $"Menu item '{menuName}' deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting menu item {id}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error deleting menu item",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Update role permissions for a menu item (SuperAdmin only)
        /// </summary>
        [HttpPost("{menuId}/permissions")]
        public async Task<IActionResult> UpdateMenuPermissions(int menuId, [FromBody] List<RolePermissionRequest> permissions)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "Authentication required" });
                }

                if (user.Role != "SuperAdmin")
                {
                    return StatusCode(403, new { success = false, message = "SuperAdmin privileges required" });
                }

                var menuItem = await _context.MenuItems.FindAsync(menuId);
                if (menuItem == null)
                {
                    return NotFound(new { success = false, message = "Menu item not found" });
                }

                // Remove existing permissions for this menu
                var existingPermissions = await _context.RoleMenuPermissions
                    .Where(rmp => rmp.MenuItemId == menuId)
                    .ToListAsync();

                _context.RoleMenuPermissions.RemoveRange(existingPermissions);

                // Add new permissions
                foreach (var permission in permissions)
                {
                    var rolePermission = new RoleMenuPermission
                    {
                        MenuItemId = menuId,
                        Role = permission.Role,
                        CanView = permission.CanView,
                        CanEdit = permission.CanEdit,
                        CanDelete = permission.CanDelete
                    };
                    _context.RoleMenuPermissions.Add(rolePermission);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Permissions updated for menu '{menuItem.Name}' by {user.Email}");

                return Ok(new
                {
                    success = true,
                    message = "Menu permissions updated successfully",
                    data = new
                    {
                        menuId = menuId,
                        menuName = menuItem.Name,
                        permissionsCount = permissions.Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permissions for menu {menuId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error updating menu permissions",
                    error = ex.Message
                });
            }
        }

        // =============================================================================
        // ADMIN ENDPOINTS
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

                var hierarchicalMenus = BuildAdminMenuTree(allMenus);

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

        private bool HasMenuAccess(MenuItem menu, string userRole)
        {
            // If no role permissions defined, assume accessible to all
            if (!menu.RolePermissions.Any())
            {
                return true;
            }

            // Check if user's role has view permission
            var rolePermission = menu.RolePermissions
                .FirstOrDefault(rp => rp.Role == userRole);

            return rolePermission?.CanView == true;
        }

        private List<object> BuildUserMenuTree(List<MenuItem> allMenus)
        {
            return allMenus
                .Where(m => m.ParentId == null)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Name)
                .Select(menu => new
                {
                    id = menu.Id,
                    name = menu.Name,
                    route = menu.Route,
                    icon = menu.Icon,
                    permissions = new
                    {
                        canView = true, // Since we already filtered by access
                        canEdit = false,
                        canDelete = false
                    },
                    children = BuildUserMenuChildren(allMenus, menu.Id)
                })
                .Cast<object>()
                .ToList();
        }

        private List<object> BuildUserMenuChildren(List<MenuItem> allMenus, int parentId)
        {
            return allMenus
                .Where(m => m.ParentId == parentId)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Name)
                .Select(menu => new
                {
                    id = menu.Id,
                    name = menu.Name,
                    route = menu.Route,
                    icon = menu.Icon,
                    permissions = new
                    {
                        canView = true,
                        canEdit = false,
                        canDelete = false
                    },
                    children = BuildUserMenuChildren(allMenus, menu.Id)
                })
                .Cast<object>()
                .ToList();
        }

        private List<object> BuildAdminMenuTree(List<MenuItem> allMenus)
        {
            return allMenus
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
                    children = BuildAdminMenuChildren(allMenus, m.Id)
                })
                .Cast<object>()
                .ToList();
        }

        private List<object> BuildAdminMenuChildren(List<MenuItem> allMenus, int parentId)
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
                    children = BuildAdminMenuChildren(allMenus, m.Id)
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
    // REQUEST/RESPONSE MODELS
    // =============================================================================

    /// <summary>
    /// Request model for creating a new menu item
    /// </summary>
    public class CreateMenuRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Route { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Icon { get; set; }

        public int? ParentId { get; set; }

        public int SortOrder { get; set; } = 0;

        [StringLength(100)]
        public string? RequiredPermission { get; set; }

        public List<RolePermissionRequest>? RolePermissions { get; set; }
    }

    /// <summary>
    /// Request model for updating an existing menu item
    /// </summary>
    public class UpdateMenuRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(200)]
        public string? Route { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public int? ParentId { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(100)]
        public string? RequiredPermission { get; set; }
    }

    /// <summary>
    /// Request model for role permissions
    /// </summary>
    public class RolePermissionRequest
    {
        [Required]
        public string Role { get; set; } = string.Empty;
        public bool CanView { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
    }

    /// <summary>
    /// Request models for menu operations (kept for compatibility)
    /// </summary>
    public class MenuPermissionRequest
    {
        [Required]
        public string MenuName { get; set; } = string.Empty;

        [Required]
        public string PermissionType { get; set; } = string.Empty; // VIEW, EDIT, DELETE
    }
}