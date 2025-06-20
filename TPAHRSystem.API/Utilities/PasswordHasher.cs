// TPAHRSystem.API/Utilities/PasswordHasher.cs
// Utility to generate password hashes for SQL scripts

using System.Security.Cryptography;
using System.Text;

namespace TPAHRSystem.API.Utilities
{
    public static class PasswordHasher
    {
        public static (string hash, string salt) HashPassword(string password)
        {
            // Generate salt
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            // Hash password with salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

            return (hash, salt);
        }

        public static bool VerifyPassword(string password, string hash, string salt)
        {
            var (computedHash, _) = HashPassword(password, salt);
            return computedHash == hash;
        }

        private static (string hash, string salt) HashPassword(string password, string salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));
            return (hash, salt);
        }

        // Method to generate hashes for known passwords
        public static void GenerateKnownHashes()
        {
            var passwords = new[] { "admin123", "hr123", "staff123", "field123", "demo123" };

            Console.WriteLine("-- Password Hashes for SQL Scripts --");
            foreach (var pwd in passwords)
            {
                var (hash, salt) = HashPassword(pwd);
                Console.WriteLine($"Password: {pwd}");
                Console.WriteLine($"Hash: {hash}");
                Console.WriteLine($"Salt: {salt}");
                Console.WriteLine();
            }
        }
    }
}