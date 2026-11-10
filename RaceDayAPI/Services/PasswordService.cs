using System.Security.Cryptography;
namespace RaceDayAPI.Services
{
   
        public class PasswordService
        {
            public string HashPassword(string password)
            {
                using var rng = RandomNumberGenerator.Create();

                byte[] salt = new byte[16];
                rng.GetBytes(salt);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    100000,
                    HashAlgorithmName.SHA256);

                byte[] hash = pbkdf2.GetBytes(32);

                return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            }

            public bool VerifyPassword(string password, string storedHash)
            {
                string[] parts = storedHash.Split(':');

                if (parts.Length != 2)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedPasswordHash = Convert.FromBase64String(parts[1]);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    100000,
                    HashAlgorithmName.SHA256);

                byte[] passwordHash = pbkdf2.GetBytes(32);

                return CryptographicOperations.FixedTimeEquals(
                    passwordHash,
                    storedPasswordHash);
            }
        }
    }
