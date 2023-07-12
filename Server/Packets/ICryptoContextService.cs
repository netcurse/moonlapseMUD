using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Packets {
    public interface ICryptoContextService {
        void SetClientAESPrivateKey(int protocolId, byte[] key);
        string GetServerRSAPublicKey();
        byte[] AESEncrypt(int protocolId, byte[] plainText);
        byte[] RSADecrypt(byte[] data);
        byte[] AESDecrypt(int protocolId, byte[] data);
        void GenerateRSAKeyPair();
    }
}
