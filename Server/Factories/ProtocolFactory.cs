using Moonlapse.Server.Packets;
using System.Net.Sockets;

namespace Moonlapse.Server; 
public class ProtocolFactory {
    private readonly IPacketDeliveryService _packetDeliveryService;
    private readonly ICryptoContextService _cryptoContextService;

    public ProtocolFactory(
        IPacketDeliveryService packetDeliveryService,
        ICryptoContextService cryptoContextService) {
        _packetDeliveryService = packetDeliveryService;
        _cryptoContextService = cryptoContextService;
    }

    public Protocol Create(TcpClient client, Server server) {
        return new Protocol(client, server, _packetDeliveryService, _cryptoContextService);
    }
}