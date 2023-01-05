using Moonlapse.Server.Serializers;
using Moonlapse.Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Packets {
    public class PacketDeliveryService : IPacketDeliveryService {
        readonly ISerializerService serializerService;

        public PacketDeliveryService(ISerializerService serializerService) {
            this.serializerService = serializerService;
        }

        public async Task<Packet> ReceivePacketAsync(NetworkStream stream, CryptoContext cryptoContext) {
            var maxBufferSize = 1500;
            var header = new byte[1];
            int headerBytesRead;
            var data = new byte[maxBufferSize - 1];
            int dataBytesRead;
            try {
                headerBytesRead = await stream.ReadAsync(header, 0, 1);
                dataBytesRead = await stream.ReadAsync(data, 0, maxBufferSize - 1);
            } catch (IOException) {
                throw new SocketClosedException();
            }
            if (headerBytesRead == 0 || dataBytesRead == 0) {
                throw new SocketClosedException();
            }
            
            var packetConfig = PacketConfig.FromByte(header[0]);
            data = data[0..dataBytesRead];  // strip trailing empty bytes

            if (packetConfig.RSAEncrypted) {
                data = cryptoContext.RSADecrypt(data);
            }
            if (packetConfig.AESEncrypted) {
                data = cryptoContext.AESDecrypt(data);
            }

            var packet = serializerService.Deserialize(data);
            return packet;
        }

        public async Task SendPacketAsync(NetworkStream stream, Packet packet, CryptoContext cryptoContext, PacketConfig? config = default) {
            config ??= new PacketConfig();

            // Ensure the AESEncrypted flag is set on the header if the packet type demands encryption
            if (PacketConfig.HasFlag(packet, PacketsExtensions.Encrypted)) {
                config.AESEncrypted = true;
                config.RSAEncrypted = false;
            }

            var header = config.ToByte();
            
            var data = serializerService.Serialize(packet);
            if (config.AESEncrypted) {
                data = cryptoContext.AESEncrypt(data);
            }

            await stream.WriteAsync(new[] { header });
            await stream.WriteAsync(data);
            await stream.FlushAsync();
        }
    }
}
