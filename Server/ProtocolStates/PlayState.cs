using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.ProtocolStates {
    public class PlayState : ProtocolState {

        public PlayState(Protocol protocol) : base(protocol) {
            ChatPacketEvent += PlayState_ChatPacketEvent;
        }

        void PlayState_ChatPacketEvent(object _, PacketEventArgs args) {
            var sender = args.Sender;
            var packet = args.Packet;

            if (sender == protocol) {
                // If this came from our own client, broadcast to everyone else
                protocol.Broadcast(packet);
            }
            else {
                // If this came from someone else, send it just to our client
                protocol.QueueOutboundPacket(protocol, packet);
            }
        }
    }
}
