using Moonlapse.Server.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.ProtocolStates {
    public abstract class ProtocolState : IProtocolState {
        protected delegate void PacketEventHandler(object sender, PacketEventArgs args);
        protected event PacketEventHandler? LoginPacketEvent, ChatPacketEvent, RegisterPacketEvent;

        protected readonly Protocol protocol;

        protected ProtocolState(Protocol protocol) {
            this.protocol = protocol;
        }

        public void DispatchPacket(Protocol sender, Packet packet) {
            Dictionary<Packet.TypeOneofCase, PacketEventHandler?> map = new() {
                { Packet.TypeOneofCase.Login, LoginPacketEvent },
                { Packet.TypeOneofCase.Register, RegisterPacketEvent },
                { Packet.TypeOneofCase.Chat, ChatPacketEvent },
            };
            var type = packet.TypeCase;
            try {
                var handler = map[type];
                handler!.Invoke(this, new PacketEventArgs(sender, packet));
            }
            catch (KeyNotFoundException) {
                throw new Exception($"Packet type ({type}) doesn't exist");
            }
            catch (ArgumentNullException) {
                throw new Exception($"Packet type ({type}) doesn't exist");
            }
            catch (NullReferenceException) {
                throw new PacketEventNotSubscribedException(type);
            }
        }
    }

    public class PacketEventArgs : EventArgs {
        public readonly Packet Packet;
        public readonly Protocol Sender;

        public PacketEventArgs(Protocol sender, Packet packet) {
            Sender = sender;
            Packet = packet;
        }
    }

    public class PacketEventNotSubscribedException : Exception {
        public PacketEventNotSubscribedException(Packet.TypeOneofCase packetType) : base($"{packetType} not registered in this ProtocolState.") {
        }
    }
}
