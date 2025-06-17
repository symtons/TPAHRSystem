// TPAHRSystem.Infrastructure/Data/DataSeeder.cs
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace TPAHRSystem.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TPADbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new Department { Name = "Information Technology", Description = "IT Department" },
                    new Department { Name = "Human Resources", Description = "HR Department" },
                    new Department { Name = "Finance", Description = "Finance Department" },
                    new Department { Name = "Operations", Description = "Field Operations" },
                    new Department { Name = "Administration", Description = "Administrative Services" }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            // Seed Activity Types
            if (!await context.ActivityTypes.AnyAsync())
            {
                var activityTypes = new List<ActivityType>
                {
                    new ActivityType { Name = "Employee Added", IconName = "PersonAdd", Color = "#4caf50" },
                    new ActivityType { Name = "Payroll Updated", IconName = "AccountBalance", Color = "#2196f3" },
                    new ActivityType { Name = "Leave Approved", IconName = "CheckCircle", Color = "#ff9800" },
                    new ActivityType { Name = "Shift Created", IconName = "Schedule", Color = "#1976d2" },
                    new ActivityType { Name = "Task Completed", IconName = "Assignment", Color = "#9c27b0" },
                    new ActivityType { Name = "Document Updated", IconName = "Edit", Color = "#795548" }
                };

                await context.ActivityTypes.AddRangeAsync(activityTypes);
                await context.SaveChangesAsync();
            }

            // Seed Menu Items
            if (!await context.MenuItems.AnyAsync())
            {
                var menuItems = new List<MenuItem>
                {
                    new MenuItem { Name = "Dashboard", Route = "/dashboard", Icon = "Dashboard", SortOrder = 1 },
                    new MenuItem { Name = "Employees", Route = "/employees", Icon = "People", SortOrder = 2, RequiredPermission = "employees.view" },
                    new MenuItem { Name = "Time & Attendance", Route = "/time-attendance", Icon = "Schedule", SortOrder = 3, RequiredPermission = "time.view" },
                    new MenuItem { Name = "Leave Management", Route = "/leave-management", Icon = "RequestPage", SortOrder = 4, RequiredPermission = "leave.view" },
                    new MenuItem { Name = "Onboarding", Route = "/onboarding", Icon = "Assignment", SortOrder = 5, RequiredPermission = "onboarding.view" },
                    new MenuItem { Name = "Settings", Route = "/settings", Icon = "Settings", SortOrder = 6, RequiredPermission = "settings.view" }
                };

                await context.MenuItems.AddRangeAsync(menuItems);
                await context.SaveChangesAsync();
            }

            // Seed Dashboard Stats
            if (!await context.DashboardStats.AnyAsync())
            {
                var dashboardStats = new List<DashboardStat>
                {
                    new DashboardStat { StatKey = "total_employees", StatName = "Total Employees", StatValue = "142", StatColor = "primary", IconName = "People", Subtitle = "Active staff members", ApplicableRoles = "Admin,HR Manager", SortOrder = 1 },
                    new DashboardStat { StatKey = "pending_requests", StatName = "Pending Requests", StatValue = "24", StatColor = "warning", IconName = "Warning", Subtitle = "Awaiting approval", ApplicableRoles = "Admin,HR Manager", SortOrder = 2 },
                    new DashboardStat { StatKey = "active_shifts", StatName = "Active Shifts", StatValue = "8", StatColor = "info", IconName = "Schedule", Subtitle = "Currently running", ApplicableRoles = "Admin,HR Manager", SortOrder = 3 },
                    new DashboardStat { StatKey = "system_status", StatName = "System Status", StatValue = "Online", StatColor = "success", IconName = "Analytics", Subtitle = "All systems operational", ApplicableRoles = "Admin", SortOrder = 4 },
                    new DashboardStat { StatKey = "pto_balance", StatName = "PTO Balance", StatValue = "15 days", StatColor = "success", IconName = "Schedule", Subtitle = "Available this year", ApplicableRoles = "Employee (Admin Staff)", SortOrder = 1 },
                    new DashboardStat { StatKey = "hours_week", StatName = "Hours This Week", StatValue = "32", StatColor = "info", IconName = "AccessTime", Subtitle = "Out of 40 hours", ApplicableRoles = "Employee (Admin Staff)", SortOrder = 2 }
                };

                await context.DashboardStats.AddRangeAsync(dashboardStats);
                await context.SaveChangesAsync();
            }

            // Seed Quick Actions
            if (!await context.QuickActions.AnyAsync())
            {
                var quickActions = new List<QuickAction>
                {
                    new QuickAction { Title = "MANAGE EMPLOYEES", ActionKey = "employees", IconName = "People", Route = "/employees", ApplicableRoles = "Admin", SortOrder = 1 },
                    new QuickAction { Title = "SCHEDULE SHIFTS", ActionKey = "schedule", IconName = "Schedule", Route = "/schedule", ApplicableRoles = "Admin", SortOrder = 2 },
                    new QuickAction { Title = "VIEW REPORTS", ActionKey = "reports", IconName = "Assessment", Route = "/reports", ApplicableRoles = "Admin", SortOrder = 3 },
                    new QuickAction { Title = "SYSTEM SETTINGS", ActionKey = "settings", IconName = "Settings", Route = "/settings", ApplicableRoles = "Admin", SortOrder = 4 },
                    new QuickAction { Title = "ADD NEW EMPLOYEE", ActionKey = "add-employee", IconName = "PersonAdd", Route = "/employees/add", ApplicableRoles = "HR Manager", SortOrder = 1 },
                    new QuickAction { Title = "REVIEW LEAVE REQUESTS", ActionKey = "leave-requests", IconName = "RequestPage", Route = "/leave-requests", ApplicableRoles = "HR Manager", SortOrder = 2 },
                    new QuickAction { Title = "CLOCK IN/OUT", ActionKey = "time-tracking", IconName = "AccessTime", Route = "/time-tracking", ApplicableRoles = "Employee (Admin Staff),Employee (Field Staff)", SortOrder = 1 },
                    new QuickAction { Title = "REQUEST LEAVE", ActionKey = "request-leave", IconName = "RequestPage", Route = "/leave/request", ApplicableRoles = "Employee (Admin Staff)", SortOrder = 2 }
                };

                await context.QuickActions.AddRangeAsync(quickActions);
                await context.SaveChangesAsync();
            }

            // Seed Demo Users
            if (!await context.Users.AnyAsync())
            {
                var salt = GenerateSalt();
                var users = new List<User>
                {
                    new User { Email = "admin@tpa.com", PasswordHash = HashPassword("admin123", salt), Salt = salt, Role = "Admin" },
                    new User { Email = "hr@tpa.com", PasswordHash = HashPassword("hr123", salt), Salt = salt, Role = "HR Manager" },
                    new User { Email = "staff@tpa.com", PasswordHash = HashPassword("staff123", salt), Salt = salt, Role = "Employee (Admin Staff)" },
                    new User { Email = "field@tpa.com", PasswordHash = HashPassword("field123", salt), Salt = salt, Role = "Employee (Field Staff)" }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }

        private static string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}