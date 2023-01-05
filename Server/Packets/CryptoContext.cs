using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;
using Serilog;

namespace Moonlapse.Server.Packets {
    public class CryptoContext {
        private byte[]? clientAESPrivateKey;
        private RSACryptoServiceProvider serverRSA;

        public CryptoContext() {
            serverRSA = new RSACryptoServiceProvider(2048);

            string publicKey;
            string privateKey;
            try {
                publicKey = File.ReadAllText(Path.Join("Keys", "public.pem"));
                privateKey = File.ReadAllText(Path.Join("Keys", "private.pem"));
            }
            catch (IOException e) {
                Log.Error("Error while reading keys: " + e.Message);
                Log.Error("Did you generate the RSA KeyPair first (use CryptoContext.GenerateRSAKeyPair())?");
                throw e;
            }
            serverRSA.ImportFromPem(publicKey);
            serverRSA.ImportFromPem(privateKey);
        }

        public void SetClientAESPrivateKey(byte[] key) {
            clientAESPrivateKey = key;
        }

        public string GetServerRSAPublicKey() {
            return serverRSA.ExportRSAPublicKeyPem();
        }

        public static void GenerateRSAKeyPair() {
            using var rsa = new RSACryptoServiceProvider(2048);
            var publicKey = rsa.ExportRSAPublicKeyPem();
            var privateKey = rsa.ExportRSAPrivateKeyPem();

            // Write the pem files to disk
            var keysDir = Directory.CreateDirectory("Keys");
            File.WriteAllText(Path.Join("Keys", "public.pem"), publicKey);
            File.WriteAllText(Path.Join("Keys", "private.pem"), privateKey);
        }

        public byte[] AESEncrypt(byte[] plainText) {
            if (clientAESPrivateKey is null) {
                throw new Exception("Client AES Private Key not set");
            }
            
            using (var aes = Aes.Create()) {
                aes.Key = clientAESPrivateKey;
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

        public byte[] RSADecrypt(byte[] data) {
            return serverRSA.Decrypt(data, true);
        }

        public byte[] AESDecrypt(byte[] data) {
            if (clientAESPrivateKey is null) {
                throw new Exception("Client AES Private Key not set");
            }
            
            using (var aes = Aes.Create()) {
                aes.Key = clientAESPrivateKey;

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
