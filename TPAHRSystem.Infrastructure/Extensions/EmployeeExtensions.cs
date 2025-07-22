// =============================================================================
// MISSING EXTENSION METHODS AND SERVICES
// File: TPAHRSystem.Infrastructure/Extensions/EmployeeExtensions.cs (NEW FILE)
// =============================================================================

using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.Infrastructure.Extensions
{
    public static class EmployeeExtensions
    {
        public static bool IsOnboardingLocked(this Employee employee)
        {
            return employee.IsOnboardingLocked ?? true;
        }

        public static bool HasRoutePermissionAsync(this Employee employee, string route)
        {
            // Basic implementation - can be enhanced based on your business logic
            return !string.IsNullOrEmpty(route);
        }

        public static bool CheckUserMenuPermissionAsync(this Employee employee, string permission)
        {
            // Basic implementation - can be enhanced based on your business logic
            return !string.IsNullOrEmpty(permission);
        }
    }
}

