// TPAHRSystem.Core/Models/UserSession.cs
namespace TPAHRSystem.Core.Models
{
    public class UserSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SessionToken { get; set; } = string.Empty;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual User User { get; set; } = null!;
    }
}