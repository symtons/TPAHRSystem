using TPAHRSystem.Core.Models;

namespace TPAHRSystem.API.Services
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string email, string password, string ipAddress);
        Task<AuthResult> RegisterAsync(string email, string password, string name, string role);
        Task<bool> LogoutAsync(string sessionToken);
        Task<string?> RefreshSessionAsync(int userId, string sessionToken);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public Employee? Employee { get; set; }
        public string? Token { get; set; }
        public string? SessionToken { get; set; }
    }
}