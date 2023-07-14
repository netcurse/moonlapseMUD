using Moonlapse.Server.Serializers;
using Moonlapse.Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Moonlapse.Server.Packets {
    public class PacketDeliveryService : IPacketDeliveryService {
        readonly ISerializerService serializerService;
        readonly ICryptoContextService cryptoContextService;
        readonly IPacketConfigService packetConfigService;


        public PacketDeliveryService(ISerializerService serializerService, ICryptoContextService cryptoContext, IPacketConfigService packetConfigService) {
            this.serializerService = serializerService;
            this.cryptoContextService = cryptoContext;
            this.packetConfigService = packetConfigService;
        }

        public async Task<Packet> ReceivePacketAsync(Stream stream) {
            // Read the first 4 bytes to get the length of the packet
            var dataLengthBytes = new byte[4]; // 4 bytes for 32-bit unsigned int
            int dataLengthBytesRead;
            try {
                dataLengthBytesRead = await stream.ReadAsync(dataLengthBytes, 0, 4);
            }
            catch (IOException) {
                throw new SocketClosedException();
            }
            if (dataLengthBytesRead == 0) {
                throw new SocketClosedException();
            }
            
            int dataLengthNetworkOrder = BitConverter.ToInt32(dataLengthBytes);
            int dataLengthInt = IPAddress.NetworkToHostOrder(dataLengthNetworkOrder);

            if (dataLengthInt == 0) {
                throw new SocketClosedException();
            }

            // Read the next byte to get the header, and the rest of the packet (dataLengthInt bytes)
            var header = new byte[1];
            int headerBytesRead;
            var data = new byte[dataLengthInt];
            try {
                headerBytesRead = await stream.ReadAsync(header, 0, 1);
                await stream.ReadAsync(data, 0, dataLengthInt);
            }
            catch (IOException) {
                throw new SocketClosedException();
            }
            if (headerBytesRead == 0) {
                throw new SocketClosedException();
            }

            var packetConfig = packetConfigService.FromByte(header[0]);
            if (packetConfig.RSAEncrypted) {
                data = cryptoContextService.RSADecrypt(data);
            }
            else if (packetConfig.AESEncrypted) {
                data = cryptoContextService.AESDecrypt(data);
            }

            var packet = serializerService.Deserialize(data);
            return packet;
        }

        public async Task SendPacketAsync(Stream stream, Packet packet, PacketConfig? config = default) {
            config ??= new PacketConfig();

            // Ensure the AESEncrypted flag is set on the header if the packet type demands encryption
            if (packetConfigService.HasFlag(config, packet, PacketsExtensions.Encrypted)) {
                config.AESEncrypted = true;
                config.RSAEncrypted = false;
            }

            var header = config.ToByte();

            var data = serializerService.Serialize(packet);
            if (config.AESEncrypted) {
                data = cryptoContextService.AESEncrypt(data);
            }

            int dataLengthNetworkOrder = IPAddress.HostToNetworkOrder(data.Length);
            byte[] dataLength = BitConverter.GetBytes(dataLengthNetworkOrder);
            
            await stream.WriteAsync(dataLength);
            await stream.WriteAsync(new[] { header });
            await stream.WriteAsync(data);
            await stream.FlushAsync();

            System.Console.WriteLine($"Sent packet. Length: {data.Length}, Header: {header}, Data: {System.Text.Encoding.UTF8.GetString(data)}");
        }
    }
}
