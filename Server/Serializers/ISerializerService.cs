using Moonlapse.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Serializers {
    public interface ISerializerService {
        byte[] Serialize(Packet packet);
        Packet Deserialize(byte[] data);
    }
}
