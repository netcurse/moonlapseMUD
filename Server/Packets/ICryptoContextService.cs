using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Packets {
    public interface ICryptoContextService {
        void SetClientAESPrivateKey(byte[] key);
        string GetServerRSAPublicKey();
        byte[] AESEncrypt(byte[] plainText);
        byte[] RSADecrypt(byte[] data);
        byte[] AESDecrypt(byte[] data);
        void GenerateRSAKeyPair();
    }
}
