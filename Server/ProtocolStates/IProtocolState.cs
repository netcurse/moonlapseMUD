using Moonlapse.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.ProtocolStates {
    public interface IProtocolState {
        public void DispatchPacket(Protocol sender, Packet packet);
    }
}
