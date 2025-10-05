using System.Security.Cryptography;
using System.Text;
using Serilog.Context;

namespace CreditCalculatorApi.Security
{
    //AES key üretme, encrypt/decrypt işlemleri için kullanılacak servis
    public class AesService
    {
        private static readonly byte[] MasterKey = Convert.FromBase64String("Xb5AEZJ2LTTvJG8ckW3ZQby6KSeRuwg0vAD2ng5ur5M=");


        /*  public static string EncryptWithMasterKey(string plainText)
          {
              Console.WriteLine(" Şifrelenmeden gelen TC: " + plainText); //  Burası önemli!

              byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

              using var aes = Aes.Create();
              aes.Key = MasterKey;
              aes.GenerateIV();
              aes.Mode = CipherMode.CBC;
              aes.Padding = PaddingMode.PKCS7;

              using var encryptor = aes.CreateEncryptor();
              byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

              // Şifreli veri = IV + gerçek şifreli veri
              byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];
              Array.Copy(aes.IV, 0, combined, 0, aes.IV.Length);
              Array.Copy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);
              Console.WriteLine(" Şifreli veri uzunluğu: " + combined.Length);
              return Convert.ToBase64String(combined);

          }*/
        public static string EncryptWithMasterKey(string plainText,string logContext="Genel")
        {
            Console.WriteLine($"🔐 [{logContext}] Şifrelemeden gelen veri: {plainText}");

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var aes = Aes.Create();
            aes.Key = MasterKey;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];
            Array.Copy(aes.IV, 0, combined, 0, aes.IV.Length);
            Array.Copy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

            Console.WriteLine(" Şifreli veri uzunluğu: " + combined.Length);

            return Convert.ToBase64String(combined);
        }


        public static (string encryptedKey, string encryptedIV) GenerateEncryptedAesKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();

            var encryptedKey = EncryptWithMasterKey(Convert.ToBase64String(aes.Key));
            var encryptedIV = EncryptWithMasterKey(Convert.ToBase64String(aes.IV));

            return (encryptedKey, encryptedIV);
        }
        public static string DecryptWithMasterKey(string encryptedBase64)
        {
            byte[] combined = Convert.FromBase64String(encryptedBase64);

            using var aes = Aes.Create();
            aes.Key = MasterKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // IV ilk 16 byte
            byte[] iv = new byte[16];
            Array.Copy(combined, 0, iv, 0, iv.Length);
            aes.IV = iv;

            // Geri kalanı şifreli veri
            byte[] cipherBytes = new byte[combined.Length - iv.Length];
            Array.Copy(combined, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

    }
}
