using System.Security.Cryptography;
using System.Text;

namespace SimpleStore.Utilities
{
    public static class PasswordHasher
    {
        // Simple password hashing using HMACSHA256
        private static string HashPassword(string password, string salt)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }

        // Generate a random salt
        private static string GenerateSalt()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        // Hash password with a new salt
        public static (string hash, string salt) HashNewPassword(string password)
        {
            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);
            return (hash, salt);
        }

        // Verify password against stored hash and salt
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var computedHash = HashPassword(password, storedSalt);
            return computedHash == storedHash;
        }
    }
}