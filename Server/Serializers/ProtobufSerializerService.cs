using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Moonlapse.Server.Packets;
using Serilog;

namespace Moonlapse.Server.Serializers {
    public class ProtobufSerializerService : ISerializerService {
        public Packet Deserialize(byte[] data) {
            return Packet.Parser.ParseFrom(data);
        }

        public byte[] Serialize(Packet packet) {
            return packet.ToByteArray();
        }
    }
}
