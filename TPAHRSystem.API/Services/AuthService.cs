using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TPAHRSystem.Core.Models;
using TPAHRSystem.Infrastructure.Data;

namespace TPAHRSystem.Application.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User, string? Token)> LoginAsync(string email, string password);
        Task<bool> LogoutAsync(string token);
        Task<User?> GetUserByTokenAsync(string token);
        string GeneratePasswordHash(string password, string salt);
        string GenerateSalt();
    }

    public class AuthService : IAuthService
    {
        private readonly TPADbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(TPADbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, User? User, string? Token)> LoginAsync(string email, string password)
        {
            try
            {
                Console.WriteLine($"🔍 Login attempt for: {email}");

                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

                Console.WriteLine($"👤 User found: {user != null}");

                if (user == null)
                {
                    Console.WriteLine("❌ User not found or inactive");
                    return (false, "Invalid email or password", null, null);
                }

                // Check if account is locked
                if (user.FailedLoginAttempts >= 5)
                {
                    Console.WriteLine("🔒 Account locked");
                    return (false, "Account is locked due to too many failed attempts", null, null);
                }

                // Debug password verification
                Console.WriteLine($"🔐 Stored hash: {user.PasswordHash}");
                Console.WriteLine($"🧂 Stored salt: {user.Salt}");
                Console.WriteLine($"📝 Input password: {password}");

                var computedHash = GeneratePasswordHash(password, user.Salt);
                Console.WriteLine($"🔢 Computed hash: {computedHash}");
                Console.WriteLine($"✅ Hashes match: {computedHash == user.PasswordHash}");

                if (computedHash != user.PasswordHash)
                {
                    // Increment failed login attempts
                    user.FailedLoginAttempts++;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    Console.WriteLine("❌ Password verification failed");
                    return (false, "Invalid email or password", null, null);
                }

                // Reset failed login attempts on successful login
                user.FailedLoginAttempts = 0;
                user.LastLogin = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                // Generate session token
                var sessionToken = GenerateSessionToken();

                // Create session record
                var session = new UserSession
                {
                    UserId = user.Id,
                    SessionToken = sessionToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(8), // 8 hour session
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserSessions.Add(session);
                await _context.SaveChangesAsync();

                Console.WriteLine("🎉 Login successful!");
                return (true, "Login successful", user, sessionToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Login error: {ex.Message}");
                Console.WriteLine($"📚 Stack trace: {ex.StackTrace}");
                return (false, "An error occurred during login", null, null);
            }
        }

        public string GeneratePasswordHash(string password, string salt)
        {
            // Use the EXACT same algorithm 
            var combined = password + salt;
            var bytes = Encoding.UTF8.GetBytes(combined);

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public string GenerateSalt()
        {
            var saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private string GenerateSessionToken()
        {
            var tokenBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes);
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                var session = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.SessionToken == token && s.IsActive);

                if (session != null)
                {
                    session.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> GetUserByTokenAsync(string token)
        {
            try
            {
                var session = await _context.UserSessions
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.SessionToken == token &&
                                           s.IsActive &&
                                           s.ExpiresAt > DateTime.UtcNow);

                return session?.User;
            }
            catch
            {
                return null;
            }
        }
    }
}