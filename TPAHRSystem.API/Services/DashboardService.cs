// =============================================================================
// ALTERNATIVE DASHBOARD SERVICE - SIMPLIFIED SYNTAX
// File: TPAHRSystem.API/Services/DashboardService.cs (Replace existing)
// =============================================================================

using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.API.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly TPADbContext _context;

        public DashboardService(TPADbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DashboardStat>> GetDashboardStatsAsync(string role)
        {
            return await _context.DashboardStats
                .Where(s => s.IsActive &&
                           (s.ApplicableRoles == null ||
                            s.ApplicableRoles.Contains(role)))
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuickAction>> GetQuickActionsAsync(string role)
        {
            return await _context.QuickActions
                .Where(qa => qa.IsActive &&
                            (qa.ApplicableRoles == null ||
                             qa.ApplicableRoles.Contains(role)))
                .OrderBy(qa => qa.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetRecentActivitiesAsync(int userId, string role)
        {
            var query = _context.RecentActivities
                .Include(ra => ra.User)
                .Include(ra => ra.Employee)
                .Include(ra => ra.ActivityType)
                .AsQueryable();

            // Filter based on role
            switch (role)
            {
                case "Admin":
                    // Admins see all activities
                    break;
                case "HR Manager":
                    // HR sees HR-related activities
                    var hrActivityTypes = new[] { "USER_CREATED", "LEAVE_APPROVED", "ONBOARDING_COMPLETED", "PAYROLL_PROCESSED" };
                    query = query.Where(ra => hrActivityTypes.Contains(ra.ActivityType.Name));
                    break;
                case "Employee (Admin Staff)":
                case "Employee (Field Staff)":
                    // Employees see their own activities and general announcements
                    query = query.Where(ra => ra.UserId == userId ||
                                             ra.ActivityType.Name.Contains("SYSTEM"));
                    break;
            }

            // First get the data, then transform it
            var rawActivities = await query
                .OrderByDescending(ra => ra.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Transform the data with proper null checking
            var activities = rawActivities.Select(ra => new
            {
                id = ra.Id,
                user = GetUserName(ra),
                action = ra.Action,
                details = ra.Details,
                time = ra.CreatedAt,
                avatar = GetUserAvatar(ra),
                color = ra.ActivityType.Color,
                type = ra.ActivityType.Name,
                isNew = ra.CreatedAt > DateTime.UtcNow.AddHours(-1)
            }).ToList();

            return activities;
        }

        private string GetUserName(RecentActivity ra)
        {
            if (ra.Employee != null)
            {
                return ra.Employee.FullName;
            }
            else
            {
                return ra.User.Email.Split('@')[0];
            }
        }

        private string GetUserAvatar(RecentActivity ra)
        {
            if (ra.Employee != null)
            {
                return ra.Employee.FirstName.Substring(0, 1);
            }
            else
            {
                return ra.User.Email.Substring(0, 1);
            }
        }

        public async Task<object> GetDashboardSummaryAsync(int userId, string role)
        {
            var stats = await GetDashboardStatsAsync(role);
            var actions = await GetQuickActionsAsync(role);
            var activities = await GetRecentActivitiesAsync(userId, role);

            return new
            {
                stats = stats.Select(s => new
                {
                    title = s.StatName,
                    value = s.StatValue,
                    subtitle = s.Subtitle,
                    icon = s.IconName,
                    color = s.StatColor
                }),
                quickActions = actions.Select(qa => new
                {
                    key = qa.ActionKey,
                    label = qa.Title,
                    icon = qa.IconName,
                    color = qa.Color,
                    route = qa.Route
                }),
                recentActivities = activities
            };
        }
    }
}