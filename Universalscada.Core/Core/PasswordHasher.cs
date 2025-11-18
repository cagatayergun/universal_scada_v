// Universalscada.Core/Core/PasswordHasher.cs (Refactored)
using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Universalscada.core
{
    // Ciddi Güvenlik İyileştirmesi: Salt ve PBKDF2 kullanıldı.
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        /// <summary>
        /// Şifreyi güvenli bir şekilde hash'ler. Çıktı formatı: Base64 kodlanmış {Salt + Hash}.
        /// </summary>
        public static string HashPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

            // PBKDF2 Hash oluştur
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Salt ve Hash'i birleştir ve Base64 formatında döndür
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Kullanıcının girdiği şifreyi veritabanındaki hash ile doğrular.
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                // Salt ve Hash'i ayır
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                byte[] storedHash = new byte[HashSize];
                Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

                // Yeni bir hash oluşturmak için aynı Salt ve Iterasyonları kullan
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
                byte[] newHash = pbkdf2.GetBytes(HashSize);

                // İki hash'i güvenli bir şekilde karşılaştır
                return storedHash.SequenceEqual(newHash);
            }
            catch
            {
                return false;
            }
        }
    }
}