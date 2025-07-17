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
            //try
            //{
                Console.WriteLine($"🔍 Login attempt for: {email}");

                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                Console.WriteLine($"👤 User found: {user != null}");

                if (user == null)
                {
                    Console.WriteLine("❌ User not found");
                    return (false, "Invalid email or password", null, null);
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    Console.WriteLine("❌ User account is not active");
                    return (false, "Account is not active", null, null);
                }

                // Check if account is locked due to failed attempts
                if (user.FailedLoginAttempts >= 5)
                {
                    Console.WriteLine("🔒 Account locked due to failed attempts");
                    return (false, "Account is locked due to too many failed attempts", null, null);
                }

                // Debug password verification
                Console.WriteLine($"🔐 Stored hash: {user.PasswordHash}");
                Console.WriteLine($"🧂 Stored salt: {user.Salt}");
                Console.WriteLine($"📝 Input password: {password}");

                // Generate hash for the input password
                var computedHash = GeneratePasswordHash(password, user.Salt);
                Console.WriteLine($"🔢 Computed hash: {computedHash}");
                Console.WriteLine($"✅ Hashes match: {computedHash == user.PasswordHash}");

                // Verify password
                if (computedHash != user.PasswordHash)
                {
                    Console.WriteLine("❌ Password verification failed");
                    return (false, "Invalid email or password", null, null);
                }

                // Generate session token (but don't save to database)
                var sessionToken = GenerateSessionToken();

                Console.WriteLine("🎉 Login successful!");
                return (true, "Login successful", user, sessionToken);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"💥 Login error: {ex.Message}");
            //    return (false, "An error occurred during login", null, null);
            //}
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