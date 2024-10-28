using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UserMicroService.Helpers
{
    public static class EncryptionHelper
    {
        private static string _encryptionKey;

        // Sætter krypteringsnøglen med validering af længde
        public static void SetEncryptionKey(string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey) || (encryptionKey.Length != 16 && encryptionKey.Length != 24 && encryptionKey.Length != 32))
            {
                throw new ArgumentException("Encryption key must be 16, 24, or 32 characters long.");
            }
    
            Console.WriteLine($"Encryption key set successfully: {encryptionKey}");
            _encryptionKey = encryptionKey;
        }


        // Krypterer en streng
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) 
                return plainText; // Returner plainText, hvis det er null eller tomt.

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                aes.GenerateIV();  // Genererer en tilfældig IV

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Først gem IV'en i begyndelsen af streamen
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }


        // Dekrypterer en streng
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText; // Returnér original cipherText, hvis tom eller null

            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);

                // Ekstraher IV fra den første del af den krypterede streng
                byte[] iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(fullCipher, 16, fullCipher.Length - 16))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
