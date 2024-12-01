using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FileRenamerProject.Services
{
    public static class SecureConfigurationProvider
    {
        /// <summary>
        /// Securely retrieve a configuration value with optional encryption
        /// </summary>
        public static string GetSecureValue(
            this IConfiguration configuration, 
            string key, 
            bool decrypt = false)
        {
            var value = configuration[key];
            
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Optional decryption logic (placeholder)
            if (decrypt)
            {
                try 
                {
                    // Implement your decryption logic here
                    // This is a simplified example and should be replaced with 
                    // a more robust encryption mechanism
                    value = DecryptString(value);
                }
                catch
                {
                    // Log decryption error
                    return null;
                }
            }

            return value;
        }

        /// <summary>
        /// Encrypt a sensitive value
        /// </summary>
        public static string EncryptValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try 
            {
                // Implement your encryption logic here
                // This is a simplified example and should be replaced with 
                // a more robust encryption mechanism
                return EncryptString(value);
            }
            catch
            {
                return null;
            }
        }

        private static string EncryptString(string plainText)
        {
            // Placeholder encryption - replace with a secure method
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            using (var deriveBytes = new Rfc2898DeriveBytes(plainText, salt, 1000, HashAlgorithmName.SHA256))
            {
                var key = deriveBytes.GetBytes(32);
                return Convert.ToBase64String(key);
            }
        }

        private static string DecryptString(string encryptedText)
        {
            // Placeholder decryption - replace with a secure method
            return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedText));
        }
    }
}
