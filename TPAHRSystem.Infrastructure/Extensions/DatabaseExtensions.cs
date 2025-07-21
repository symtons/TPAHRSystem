// =============================================================================
// DATABASE EXTENSION METHODS FOR ONBOARDING
// File: TPAHRSystem.Infrastructure/Extensions/DatabaseExtensions.cs
// =============================================================================

using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.Infrastructure.Extensions
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Get user menu items based on role and department
        /// </summary>
        public static async Task<List<MenuItem>> GetUserMenuItemsAsync(
            this TPADbContext context,
            string userRole,
            int? departmentId = null)
        {
            try
            {
                // This would typically call a stored procedure or complex query
                // For now, we'll simulate the menu structure
                var menuItems = new List<MenuItem>();

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
                return new List<MenuItem>();
            }
        }

        /// <summary>
        /// Check if user has permission for a specific menu action
        /// </summary>
        public static async Task<bool> CheckUserMenuPermissionAsync(
            this TPADbContext context,
            string userRole,
            string menuName,
            string action)
        {
            try
            {
                // This would typically query the RoleMenuPermissions table
                // For now, we'll simulate the permission check

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

        /// <summary>
        /// Get full menu structure for admin/HR users
        /// </summary>
        private static List<MenuItem> GetFullMenuStructure()
        {
            return new List<MenuItem>
            {
                // Dashboard
                new MenuItem { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "BarChart3", ParentId = null, SortOrder = 1, IsActive = true },
                
                // Employee Management
                new MenuItem { Id = 2, Name = "Employee Management", Route = "/employees", Icon = "Users", ParentId = null, SortOrder = 2, IsActive = true },
                new MenuItem { Id = 21, Name = "Employee List", Route = "/employees/list", Icon = "List", ParentId = 2, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 22, Name = "Add Employee", Route = "/employees/add", Icon = "UserPlus", ParentId = 2, SortOrder = 2, IsActive = true },
                new MenuItem { Id = 23, Name = "Employee Reports", Route = "/employees/reports", Icon = "FileText", ParentId = 2, SortOrder = 3, IsActive = true },
                
                // Onboarding
                new MenuItem { Id = 3, Name = "Onboarding", Route = "/onboarding", Icon = "UserCheck", ParentId = null, SortOrder = 3, IsActive = true },
                new MenuItem { Id = 31, Name = "Onboarding Overview", Route = "/onboarding/overview", Icon = "Eye", ParentId = 3, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 32, Name = "Task Management", Route = "/onboarding/tasks", Icon = "CheckSquare", ParentId = 3, SortOrder = 2, IsActive = true },
                new MenuItem { Id = 33, Name = "Progress Tracking", Route = "/onboarding/progress", Icon = "TrendingUp", ParentId = 3, SortOrder = 3, IsActive = true },
                
                // Time & Attendance
                new MenuItem { Id = 4, Name = "Time & Attendance", Route = "/time", Icon = "Clock", ParentId = null, SortOrder = 4, IsActive = true },
                new MenuItem { Id = 41, Name = "Time Entry", Route = "/time/entry", Icon = "Plus", ParentId = 4, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 42, Name = "Timesheets", Route = "/time/sheets", Icon = "Calendar", ParentId = 4, SortOrder = 2, IsActive = true },
                new MenuItem { Id = 43, Name = "Time Reports", Route = "/time/reports", Icon = "BarChart", ParentId = 4, SortOrder = 3, IsActive = true },
                
                // Administration
                new MenuItem { Id = 5, Name = "Administration", Route = "/admin", Icon = "Settings", ParentId = null, SortOrder = 5, IsActive = true },
                new MenuItem { Id = 51, Name = "User Management", Route = "/admin/users", Icon = "UserCog", ParentId = 5, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 52, Name = "Department Setup", Route = "/admin/departments", Icon = "Building", ParentId = 5, SortOrder = 2, IsActive = true },
                new MenuItem { Id = 53, Name = "System Settings", Route = "/admin/settings", Icon = "Cog", ParentId = 5, SortOrder = 3, IsActive = true },
                
                // Reports
                new MenuItem { Id = 6, Name = "Reports", Route = "/reports", Icon = "FileText", ParentId = null, SortOrder = 6, IsActive = true },
                new MenuItem { Id = 61, Name = "Employee Reports", Route = "/reports/employees", Icon = "Users", ParentId = 6, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 62, Name = "Time Reports", Route = "/reports/time", Icon = "Clock", ParentId = 6, SortOrder = 2, IsActive = true },
                new MenuItem { Id = 63, Name = "Onboarding Reports", Route = "/reports/onboarding", Icon = "UserCheck", ParentId = 6, SortOrder = 3, IsActive = true },
                
                // Profile
                new MenuItem { Id = 7, Name = "Profile", Route = "/profile", Icon = "User", ParentId = null, SortOrder = 7, IsActive = true },
                new MenuItem { Id = 71, Name = "Personal Information", Route = "/profile/personal", Icon = "Info", ParentId = 7, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 72, Name = "Account Settings", Route = "/profile/settings", Icon = "Settings", ParentId = 7, SortOrder = 2, IsActive = true },
                
                // Help
                new MenuItem { Id = 8, Name = "Help", Route = "/help", Icon = "HelpCircle", ParentId = null, SortOrder = 8, IsActive = true },
                new MenuItem { Id = 81, Name = "Documentation", Route = "/help/docs", Icon = "Book", ParentId = 8, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 82, Name = "Support", Route = "/help/support", Icon = "MessageCircle", ParentId = 8, SortOrder = 2, IsActive = true }
            };
        }

        /// <summary>
        /// Get limited menu structure for regular employees
        /// </summary>
        private static List<MenuItem> GetEmployeeMenuStructure(int? departmentId)
        {
            return new List<MenuItem>
            {
                // Dashboard
                new MenuItem { Id = 1, Name = "Dashboard", Route = "/dashboard", Icon = "BarChart3", ParentId = null, SortOrder = 1, IsActive = true },
                
                // Time & Attendance
                new MenuItem { Id = 4, Name = "Time & Attendance", Route = "/time", Icon = "Clock", ParentId = null, SortOrder = 2, IsActive = true },
                new MenuItem { Id = 41, Name = "Time Entry", Route = "/time/entry", Icon = "Plus", ParentId = 4, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 42, Name = "My Timesheets", Route = "/time/my-sheets", Icon = "Calendar", ParentId = 4, SortOrder = 2, IsActive = true },
                
                // Profile
                new MenuItem { Id = 7, Name = "Profile", Route = "/profile", Icon = "User", ParentId = null, SortOrder = 3, IsActive = true },
                new MenuItem { Id = 71, Name = "Personal Information", Route = "/profile/personal", Icon = "Info", ParentId = 7, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 72, Name = "Account Settings", Route = "/profile/settings", Icon = "Settings", ParentId = 7, SortOrder = 2, IsActive = true },
                
                // Help
                new MenuItem { Id = 8, Name = "Help", Route = "/help", Icon = "HelpCircle", ParentId = null, SortOrder = 4, IsActive = true },
                new MenuItem { Id = 81, Name = "Documentation", Route = "/help/docs", Icon = "Book", ParentId = 8, SortOrder = 1, IsActive = true },
                new MenuItem { Id = 82, Name = "Support", Route = "/help/support", Icon = "MessageCircle", ParentId = 8, SortOrder = 2, IsActive = true }
            };
        }
    }

    /// <summary>
    /// Menu item class for database extensions
    /// </summary>
    public class MenuItem
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

// =============================================================================
// STORED PROCEDURE RESULT CLASSES
// File: TPAHRSystem.Core/Models/StoredProcedureResults.cs
// =============================================================================

namespace TPAHRSystem.Core.Models
{
    // These classes map to the results returned by the stored procedures

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