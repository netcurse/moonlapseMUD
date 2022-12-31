using Google.Protobuf.Reflection;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public PacketConfig(bool rsaEncrypted = false, bool aesEncrypted = false, bool reserved1 = false, bool reserved2 = false, bool reserved3 = false, bool reserved4 = false, bool reserved5 = false, bool reserved6 = false) {
            RSAEncrypted = rsaEncrypted;
            AESEncrypted = aesEncrypted;
            Reserved1 = reserved1;
            Reserved2 = reserved2;
            Reserved3 = reserved3;
            Reserved4 = reserved4;
            Reserved5 = reserved5;
            Reserved6 = reserved6;
        }

        public static PacketConfig FromByte(byte b) {
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

        /// <summary>
        /// Returns true only if the given packet has the given flag set to true. 
        /// Example usage:
        /// <code>
        /// if (PacketConfig.HasFlag(packet, PacketsExtensions.FlagName)) { ...
        /// </code>
        /// </summary>
        /// <param name="packet">The packet to check</param>
        /// <param name="flag">The flag to check (accessible with <c>PacketsExtensions.FlagName</c></param>
        public static bool HasFlag(Packet packet, Extension<FieldOptions, bool> flag) {
            var packetTypes = Packet.Descriptor.Oneofs[0].Fields;
            foreach (var packetType in packetTypes) {
                if (packetType.PropertyName == packet.TypeCase.ToString()) {
                    return packetType.GetOptions().GetExtension(flag);
                }
            }
            return false;
        }
    }
};