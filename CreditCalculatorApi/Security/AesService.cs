using System.Security.Cryptography;
using System.Text;

namespace CreditCalculatorApi.Security
{
    public class AesService
    {
        private readonly byte[] _masterKey;

        public AesService(IConfiguration configuration)
        {
            var base64Key = configuration["Encryption:MasterKey"];

            if (string.IsNullOrWhiteSpace(base64Key))
                throw new InvalidOperationException(
                    "Encryption:MasterKey configuration is missing.");

            _masterKey = Convert.FromBase64String(base64Key);

            if (_masterKey.Length != 32)
                throw new InvalidOperationException(
                    "Encryption:MasterKey must be a 256-bit Base64 encoded key.");
        }

        public string EncryptWithMasterKey(
            string plainText,
            string logContext = "Genel")
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var aes = Aes.Create();
            aes.Key = _masterKey;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();

            byte[] cipherBytes =
                encryptor.TransformFinalBlock(
                    plainBytes,
                    0,
                    plainBytes.Length);

            byte[] combined =
                new byte[aes.IV.Length + cipherBytes.Length];

            Array.Copy(
                aes.IV,
                0,
                combined,
                0,
                aes.IV.Length);

            Array.Copy(
                cipherBytes,
                0,
                combined,
                aes.IV.Length,
                cipherBytes.Length);

            return Convert.ToBase64String(combined);
        }

        public (string encryptedKey, string encryptedIV)
            GenerateEncryptedAesKey()
        {
            using var aes = Aes.Create();

            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();

            var encryptedKey =
                EncryptWithMasterKey(
                    Convert.ToBase64String(aes.Key));

            var encryptedIV =
                EncryptWithMasterKey(
                    Convert.ToBase64String(aes.IV));

            return (encryptedKey, encryptedIV);
        }

        public string DecryptWithMasterKey(string encryptedBase64)
        {
            byte[] combined =
                Convert.FromBase64String(encryptedBase64);

            using var aes = Aes.Create();

            aes.Key = _masterKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[16];

            Array.Copy(
                combined,
                0,
                iv,
                0,
                iv.Length);

            aes.IV = iv;

            byte[] cipherBytes =
                new byte[combined.Length - iv.Length];

            Array.Copy(
                combined,
                iv.Length,
                cipherBytes,
                0,
                cipherBytes.Length);

            using var decryptor = aes.CreateDecryptor();

            byte[] decryptedBytes =
                decryptor.TransformFinalBlock(
                    cipherBytes,
                    0,
                    cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}