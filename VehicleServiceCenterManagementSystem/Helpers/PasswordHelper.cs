using System;
using System.Security.Cryptography;
namespace VehicleServiceCenterManagementSystem.Helpers
{
    public static class PasswordHelper
    {
        private const int Iterations=600000;
        public static string HashPassword(string password)
        {
            byte[] salt=RandomNumberGenerator.GetBytes(16);
            byte[] hash=Rfc2898DeriveBytes.Pbkdf2(password,salt,Iterations,HashAlgorithmName.SHA256,32);
            return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }
        public static bool VerifyPassword(string password,string stored)
        {
            try
            {
                byte[] salt,expected; int iterations;
                if(stored.StartsWith("PBKDF2$"))
                {
                    var fields=stored.Split('$'); if(fields.Length!=4 || !int.TryParse(fields[1],out iterations) || iterations<10000 || iterations>2000000) return false;
                    salt=Convert.FromBase64String(fields[2]); expected=Convert.FromBase64String(fields[3]);
                }
                else
                {
                    byte[] legacy=Convert.FromBase64String(stored); if(legacy.Length!=48) return false;
                    salt=legacy[..16]; expected=legacy[16..]; iterations=10000;
                }
                return salt.Length==16 && expected.Length==32 && CryptographicOperations.FixedTimeEquals(expected,Rfc2898DeriveBytes.Pbkdf2(password,salt,iterations,HashAlgorithmName.SHA256,32));
            }
            catch(FormatException) { return false; }
            catch(ArgumentException) { return false; }
        }
        public static bool IsHash(string value) => value.StartsWith("PBKDF2$") || value.Length==64;
        public static bool Valid(string password) => !string.IsNullOrWhiteSpace(password) && password.Length>=4 && password.Length<=128;
        public static string RecoveryCode() => Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
    }
}
