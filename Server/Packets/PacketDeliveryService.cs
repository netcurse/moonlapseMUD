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

        public async Task<Packet> ReceivePacketAsync(NetworkStream stream) {
            var maxBufferSize = 1500;
            var data = new byte[maxBufferSize];
            int bytesRead;
            try {
                bytesRead = await stream.ReadAsync(data);
            } catch (IOException) {
                throw new SocketClosedException();
            }
            if (bytesRead == 0) {
                throw new SocketClosedException();
            }

            data = data[0..bytesRead];  // strip trailing empty bytes

            var packet = serializerService.Deserialize(data);
            return packet;
        }

        public async Task SendPacketAsync(NetworkStream stream, Packet packet) {
            var data = serializerService.Serialize(packet);
            await stream.WriteAsync(data);
            await stream.FlushAsync();
        }
    }
}
