using Google.Protobuf.Reflection;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Packets {
    public class PacketConfigService : IPacketConfigService {
        readonly IFlagsCacheService flagsCacheService;

        public PacketConfigService(IFlagsCacheService flagsCacheService) {
            this.flagsCacheService = flagsCacheService;
        }

        public PacketConfig FromByte(byte b) {
            var config = new PacketConfig();
            config.RSAEncrypted = (b & 0b10000000) != 0;
            config.AESEncrypted = (b & 0b01000000) != 0;
            config.Reserved1 = (b & 0b00100000) != 0;
            config.Reserved2 = (b & 0b00010000) != 0;
            config.Reserved3 = (b & 0b00001000) != 0;
            config.Reserved4 = (b & 0b00000100) != 0;
            config.Reserved5 = (b & 0b00000010) != 0;
            config.Reserved6 = (b & 0b00000001) != 0;
            return config;
        }

        public bool HasFlag(PacketConfig config, Packet packet, Extension<FieldOptions, bool> flag) {
            string packetTypeName = packet.TypeCase.ToString();

            // Look this packet up in the cache and check if the flag is already set
            if (flagsCacheService.GetFlag(packetTypeName, flag, out bool cachedFlag)) {
                return cachedFlag;
            }

            // Manually check if the flag's set, and update the cache
            bool flagValue = false;
            var packetTypes = Packet.Descriptor.Oneofs[0].Fields;
            foreach (var packetType in packetTypes) {
                if (packetType.PropertyName == packetTypeName) {
                    flagValue = packetType.GetOptions().GetExtension(flag);
                    break;
                }
            }

            flagsCacheService.UpdateFlag(packetTypeName, flag, flagValue);

            return flagValue;
        }

    }
}
