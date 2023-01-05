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
        }

        void EntryProtocolState_AesKeyPacketEvent(object sender, PacketEventArgs args) {
            var packet = args.Packet;
            Log.Debug($"Received AES key from the client");
            protocol.SetAESPrivateKey(packet.AesKey.Key.ToByteArray());
        }
    }
}
