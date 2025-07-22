// =============================================================================
// MISSING TPADBCONTEXT EXTENSIONS
// File: TPAHRSystem.Infrastructure/Extensions/TPADbContextExtensions.cs (NEW FILE)  
// =============================================================================

using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.Infrastructure.Extensions
{
    public static class TPADbContextExtensions
    {
        public static async Task<bool> HasRoutePermissionAsync(this TPADbContext context, int userId, string route)
        {
            // Basic implementation - enhance based on your permission logic
            var user = await context.Users.FindAsync(userId);
            return user != null && user.IsActive;
        }

        public static async Task<bool> CheckUserMenuPermissionAsync(this TPADbContext context, int userId, string permission)
        {
            // Basic implementation - enhance based on your permission logic
            var user = await context.Users.FindAsync(userId);
            return user != null && user.IsActive;
        }
    }
}