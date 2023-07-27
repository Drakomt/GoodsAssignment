using GoodsAssignment.Models;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace GoodsAssignment.Services
{
    public class Auth
    {
        private readonly string encryptionKey;
        public Auth(string key)
        {
            encryptionKey = key;
        }


        public string EncryptUser(User user)
        {
            string jsonData = JsonSerializer.Serialize(user);
            byte[] encryptedBytes = EncryptStringToBytes(jsonData, encryptionKey);
            return Convert.ToBase64String(encryptedBytes);
        }

        public User DecryptUser(string encryptedData)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            string decryptedData = DecryptStringFromBytes(encryptedBytes, encryptionKey);
            return JsonSerializer.Deserialize<User>(decryptedData);
        }

        private byte[] EncryptStringToBytes(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] keyBytes = new byte[32];
                byte[] ivBytes = new byte[16];

                byte[] keyBytes128 = Encoding.UTF8.GetBytes(key);
                Array.Copy(keyBytes128, keyBytes, Math.Min(keyBytes.Length, keyBytes128.Length));

                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        private string DecryptStringFromBytes(byte[] cipherText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] keyBytes = new byte[32];
                byte[] ivBytes = new byte[16];

                byte[] keyBytes128 = Encoding.UTF8.GetBytes(key);
                Array.Copy(keyBytes128, keyBytes, Math.Min(keyBytes.Length, keyBytes128.Length));

                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
