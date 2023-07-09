using Google.Protobuf.Reflection;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Moonlapse.Server.Packets {
    public class PacketConfig {
        public bool RSAEncrypted;
        public bool AESEncrypted;
        public bool Reserved1;
        public bool Reserved2;
        public bool Reserved3;
        public bool Reserved4;
        public bool Reserved5;
        public bool Reserved6;

        public byte ToByte() {
            return (byte)(
                (RSAEncrypted ? 1 : 0) << 7 |
                (AESEncrypted ? 1 : 0) << 6 |
                (Reserved1 ? 1 : 0) << 5 |
                (Reserved2 ? 1 : 0) << 4 |
                (Reserved3 ? 1 : 0) << 3 |
                (Reserved4 ? 1 : 0) << 2 |
                (Reserved5 ? 1 : 0) << 1 |
                (Reserved6 ? 1 : 0)
            );
        }
    }
};