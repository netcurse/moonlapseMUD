using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.ProtocolStates {
    public class EntryState : ProtocolState {
        public EntryState(Protocol protocol) : base(protocol) {
            LoginPacketEvent += EntryProtocolState_LoginPacketEvent;
            AesKeyPacketEvent += EntryProtocolState_AesKeyPacketEvent;
        }

        void EntryProtocolState_LoginPacketEvent(object sender, PacketEventArgs args) {
            var packet = args.Packet;
            Log.Debug($"Received login packet from {packet.Login.Username}");
            protocol.ChangeState<PlayState>();

            // Send an OK packet to let the client know the login was successful TODO: integrate logic here once we have a database
            var okPacket = new Packets.Packet();
            okPacket.Ok = new() { Message = "Login successful" };
            protocol.QueueOutboundPacket(protocol, okPacket);
        }

        void EntryProtocolState_AesKeyPacketEvent(object sender, PacketEventArgs args) {
            var packet = args.Packet;
            Log.Debug($"Received AES key from the client");
            protocol.SetAESPrivateKey(packet.AesKey.Key.ToByteArray());
            
            // Send an OK packet to let the client know we're ready to receive the login packet
            var okPacket = new Packets.Packet();
            okPacket.Ok = new() { Message = "AES key received" };
            protocol.QueueOutboundPacket(protocol, okPacket);
        }
    }
}
