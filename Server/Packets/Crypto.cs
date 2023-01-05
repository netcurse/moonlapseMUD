using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

// TODO: Implement this file

namespace Moonlapse.Server.Packets {
    public class Crypto {
        public static void GenerateRSAKeyPair() {
            using var rsa = new RSACryptoServiceProvider(2048);
            var publicKey = rsa.ExportRSAPublicKeyPem();
            var privateKey = rsa.ExportRSAPrivateKeyPem();

            // Write the pem files to disk
            var keysDir = Directory.CreateDirectory("Keys");
            File.WriteAllText(Path.Join("Keys", "public.pem"), publicKey);
            File.WriteAllText(Path.Join("Keys", "private.pem"), privateKey);
        }

        public static byte[] GetRSAPublicKey() {
            var publicKey = File.ReadAllText(Path.Join("Keys", "public.pem"));
            return Encoding.UTF8.GetBytes(publicKey);
        }

        public static byte[] RSAEncrypt(byte[] data) {
            using var rsa = new RSACryptoServiceProvider(2048);
            var publicKey = File.ReadAllText(Path.Join("Keys", "public.pem"));
            rsa.ImportFromPem(publicKey);
            return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        public static byte[] AESEncrypt(byte[] plainText, byte[] key) {
            using (var aes = Aes.Create()) {
                aes.Key = key;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var memStream = new MemoryStream()) {
                    memStream.Write(aes.IV);
                    using (var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write)) {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    return memStream.ToArray();
                }
            }
        }

        public static byte[] RSADecrypt(byte[] data) {
            using (var rsa = new RSACryptoServiceProvider(2048)) {
                var privateKey = File.ReadAllText(Path.Join("Keys", "private.pem"));
                rsa.ImportFromPem(privateKey);
                return rsa.Decrypt(data, true);
            }
        }

        public static byte[] AESDecrypt(byte[] data, byte[] key) {
            using (var aes = Aes.Create()) {
                aes.Key = key;

                // The first part of the data is the initialisation vector (IV)
                var iv = new byte[aes.IV.Length];
                Array.Copy(data, iv, iv.Length);
                aes.IV = iv;

                // The rest is the ciphertext
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var memStream = new MemoryStream()) {
                    using (var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write)) {
                        cryptoStream.Write(data, aes.IV.Length, data.Length - aes.IV.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    return memStream.ToArray();
                }
            }
        }
    }
}
