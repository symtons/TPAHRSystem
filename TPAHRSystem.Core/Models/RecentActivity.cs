// =============================================================================
// UPDATED RecentActivity Model - Make Employee navigation truly optional
// File: TPAHRSystem.Core/Models/RecentActivity.cs (Replace existing)
// =============================================================================

using System.ComponentModel.DataAnnotations.Schema;

namespace TPAHRSystem.Core.Models
{
    public class RecentActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? EmployeeId { get; set; } // Nullable - not all activities need Employee
        public int ActivityTypeId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual User User { get; set; } = null!;

        // Make Employee navigation truly optional
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        public virtual ActivityType ActivityType { get; set; } = null!;

        // Helper method to get display name
        [NotMapped]
        public string DisplayName
        {
            get
            {
                if (Employee != null && !string.IsNullOrEmpty(Employee.FirstName))
                {
                    return $"{Employee.FirstName} {Employee.LastName}";
                }

                // Fallback to user email-based name
                if (!string.IsNullOrEmpty(User?.Email))
                {
                    var namePart = User.Email.Split('@')[0];
                    if (namePart.Contains('.'))
                    {
                        var parts = namePart.Split('.');
                        if (parts.Length >= 2)
                        {
                            return $"{CapitalizeFirst(parts[0])} {CapitalizeFirst(parts[1])}";
                        }
                    }
                    return CapitalizeFirst(namePart.Replace(".", " ").Replace("_", " "));
                }

                return "Unknown User";
            }
        }

        [NotMapped]
        public string Avatar
        {
            get
            {
                if (Employee != null && !string.IsNullOrEmpty(Employee.FirstName))
                {
                    return Employee.FirstName.Substring(0, 1).ToUpper();
                }

                if (!string.IsNullOrEmpty(User?.Email))
                {
                    return User.Email.Substring(0, 1).ToUpper();
                }

                return "U";
            }
        }

        private static string CapitalizeFirst(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}