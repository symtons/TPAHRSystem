// =============================================================================

// File: TPAHRSystem.API/Services/IDashboardService.cs
// =============================================================================

using TPAHRSystem.Core.Models;

namespace TPAHRSystem.API.Services
{
    public interface IDashboardService
    {
        Task<IEnumerable<DashboardStat>> GetDashboardStatsAsync(string role);
        Task<IEnumerable<QuickAction>> GetQuickActionsAsync(string role);
        Task<IEnumerable<object>> GetRecentActivitiesAsync(int userId, string role);
        Task<object> GetDashboardSummaryAsync(int userId, string role);
    }
}