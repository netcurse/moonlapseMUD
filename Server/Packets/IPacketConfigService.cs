using Google.Protobuf.Reflection;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Packets {
    public interface IPacketConfigService {
        bool HasFlag(PacketConfig config, Packet packet, Extension<FieldOptions, bool> flag);
        PacketConfig FromByte(byte b);
    }
}
