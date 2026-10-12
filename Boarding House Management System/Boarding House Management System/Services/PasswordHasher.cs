using System.Security.Cryptography;
using System.Text;

namespace Boarding_House_Management_System.Services
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(password))
                return false;

            // Check if stored as salt.hash format
            var parts = storedHash.Split('.');
            if (parts.Length != 2)
            {
                // Fallback comparison for plain text legacy/development data
                return password == storedHash;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] originalHash = Convert.FromBase64String(parts[1]);

                byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    KeySize
                );

                return CryptographicOperations.FixedTimeEquals(originalHash, computedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
