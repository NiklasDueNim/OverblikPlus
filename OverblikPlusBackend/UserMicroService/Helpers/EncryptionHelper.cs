using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UserMicroService.Helpers
{
    public static class EncryptionHelper
    {
        private static string _encryptionKey;
        private static readonly byte[] DefaultIV = Encoding.UTF8.GetBytes("TestIV1234567890");
        
        public static void SetEncryptionKey(string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey) || (encryptionKey.Length != 16 && encryptionKey.Length != 24 && encryptionKey.Length != 32))
            {
                throw new ArgumentException("Encryption key must be 16, 24, or 32 characters long.");
            }

            _encryptionKey = encryptionKey;
        }

        public static string Encrypt(string plainText, bool useFixedIV = false)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                
                aes.IV = useFixedIV ? DefaultIV : aes.IV ?? aes.IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    if (!useFixedIV)
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

        public static string Decrypt(string cipherText, bool useFixedIV = false)
        {
            if (string.IsNullOrEmpty(cipherText)) 
                return cipherText;

            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);

                byte[] iv = new byte[16];
                if (useFixedIV)
                {
                    iv = DefaultIV;
                }
                else
                {
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                }

                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(fullCipher, useFixedIV ? 0 : 16, fullCipher.Length - (useFixedIV ? 0 : 16)))
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
