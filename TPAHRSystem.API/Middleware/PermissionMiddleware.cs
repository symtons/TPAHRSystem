// =============================================================================
// COMPLETE PERMISSION CHECKING MIDDLEWARE
// File: TPAHRSystem.API/Middleware/PermissionMiddleware.cs (NEW FILE)
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.API.Middleware
{
    /// <summary>
    /// Middleware to check user permissions for menu access
    /// </summary>
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionMiddleware> _logger;

        public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, TPADbContext dbContext)
        {
            // Skip permission check for certain paths
            if (ShouldSkipPermissionCheck(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Only check permissions for authenticated requests
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            try
            {
                var userRole = await GetUserRole(context, dbContext);
                var requestPath = context.Request.Path.Value?.TrimStart('/');

                if (!string.IsNullOrEmpty(userRole) && !string.IsNullOrEmpty(requestPath))
                {
                    var hasPermission = await CheckRoutePermission(dbContext, userRole, requestPath);

                    if (!hasPermission)
                    {
                        _logger.LogWarning($"Permission denied for user role {userRole} accessing {requestPath}");

                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                        {
                            success = false,
                            message = "Insufficient permissions to access this resource",
                            statusCode = 403
                        }));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in permission middleware");
                // Continue processing - don't block on middleware errors
            }

            await _next(context);
        }

        private bool ShouldSkipPermissionCheck(string path)
        {
            var skipPaths = new[]
            {
                "/api/auth",
                "/api/test",
                "/api/menu/user-menus", // Allow menu loading
                "/api/menu/health",
                "/health",
                "/swagger",
                "/favicon.ico",
                "/_blazor",
                "/css",
                "/js",
                "/images"
            };

            return skipPaths.Any(skipPath => path.StartsWith(skipPath, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<string?> GetUserRole(HttpContext context, TPADbContext dbContext)
        {
            // Try to get role from claims first
            var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(roleClaim))
            {
                return roleClaim;
            }

            // If no claims, try to get from token in Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    var userSession = await dbContext.UserSessions
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.SessionToken == token &&
                                                 s.ExpiresAt > DateTime.UtcNow &&
                                                 s.IsActive);

                    return userSession?.User?.Role;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting user role from token");
                    return null;
                }
            }

            return null;
        }

        private async Task<bool> CheckRoutePermission(TPADbContext dbContext, string userRole, string requestPath)
        {
            try
            {
                // Map API routes to menu items
                var menuRoute = MapApiRouteToMenuRoute(requestPath);

                if (string.IsNullOrEmpty(menuRoute))
                {
                    // If we can't map the route, allow access (for non-menu routes)
                    return true;
                }

                // Use the DbContext helper method
                var hasPermission = await dbContext.HasRoutePermissionAsync(userRole, menuRoute, "VIEW");
                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking route permission for {requestPath}");
                // On error, allow access to prevent blocking
                return true;
            }
        }

        private string? MapApiRouteToMenuRoute(string apiPath)
        {
            // Map API routes to frontend menu routes
            var routeMappings = new Dictionary<string, string>
            {
                // Dashboard
                { "api/dashboard", "/dashboard" },
                
                // Employee Management
                { "api/employee", "/employees" },
                { "api/employees", "/employees" },
                
                // Leave Management  
                { "api/leave", "/leave" },
                { "api/leave-requests", "/leave" },
                
                // Time & Attendance
                { "api/time", "/time-attendance" },
                { "api/timesheet", "/time-attendance" },
                { "api/timesheets", "/time-attendance" },
                { "api/schedule", "/time-attendance" },
                { "api/schedules", "/time-attendance" },
                
                // Onboarding
                { "api/onboarding", "/onboarding" },
                
                // Role Management (SuperAdmin)
                { "api/role", "/role-management" },
                { "api/user-role", "/role-management" },
                
                // Reports
                { "api/reports", "/reports" },
                
                // Settings
                { "api/settings", "/settings" },
                
                // Menu Management
                { "api/menu", "/menu-management" }
            };

            // Find the longest matching route prefix
            var matchingRoute = routeMappings
                .Where(mapping => apiPath.StartsWith(mapping.Key, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(mapping => mapping.Key.Length)
                .FirstOrDefault();

            return matchingRoute.Value;
        }
    }

    /// <summary>
    /// Extension method to register the permission middleware
    /// </summary>
    public static class PermissionMiddlewareExtensions
    {
        public static IApplicationBuilder UsePermissionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionMiddleware>();
        }
    }

    /// <summary>
    /// Custom authorization attribute for role-based permissions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireMenuPermissionAttribute : Attribute
    {
        public string MenuName { get; }
        public string PermissionType { get; }

        public RequireMenuPermissionAttribute(string menuName, string permissionType = "VIEW")
        {
            MenuName = menuName;
            PermissionType = permissionType;
        }
    }

    /// <summary>
    /// Authorization filter for menu permissions
    /// </summary>
    public class MenuPermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly TPADbContext _context;
        private readonly ILogger<MenuPermissionFilter> _logger;

        public MenuPermissionFilter(TPADbContext context, ILogger<MenuPermissionFilter> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get the RequireMenuPermission attribute
            var permissionAttribute = context.ActionDescriptor.EndpointMetadata
                .OfType<RequireMenuPermissionAttribute>()
                .FirstOrDefault();

            if (permissionAttribute == null)
            {
                // No permission attribute, allow access
                return;
            }

            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            try
            {
                // Get user role
                var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userRole))
                {
                    // Try to get from token
                    var token = GetTokenFromContext(context.HttpContext);
                    if (!string.IsNullOrEmpty(token))
                    {
                        var userSession = await _context.UserSessions
                            .Include(s => s.User)
                            .FirstOrDefaultAsync(s => s.SessionToken == token &&
                                                     s.ExpiresAt > DateTime.UtcNow &&
                                                     s.IsActive);
                        userRole = userSession?.User?.Role;
                    }
                }

                if (string.IsNullOrEmpty(userRole))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Check permission
                var hasPermission = await _context.CheckUserMenuPermissionAsync(
                    userRole,
                    permissionAttribute.MenuName,
                    permissionAttribute.PermissionType);

                if (!hasPermission)
                {
                    _logger.LogWarning($"Access denied for role {userRole} to menu {permissionAttribute.MenuName} with permission {permissionAttribute.PermissionType}");
                    context.Result = new ForbidResult();
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in menu permission filter");
                context.Result = new StatusCodeResult(500);
                return;
            }
        }

        private string? GetTokenFromContext(HttpContext httpContext)
        {
            return httpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();
        }
    }
}