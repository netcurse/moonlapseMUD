using Moonlapse.Server.Packets;
using Moonlapse.Server.Serializers;
using Moonlapse.Server.Utils;

namespace Moonlapse.Server.Tests.Unit.Packets;

public class TestPacketDeliveryService {

    readonly Packet mockLoginPacket;

    readonly IPacketDeliveryService packetDeliveryService;
    readonly ISerializerService serializerService;

    public TestPacketDeliveryService() {
        // Making the mock packets
        mockLoginPacket = new() {
            Login = new() {
                Username = "john",
                Password = "123"
            }
        };

        // creating the container and injecting services
        // todo: it's probably better to have tests for each implementation of the abstract services, rather than testing the injected implementation
        Container.ConfigureServices();
        packetDeliveryService = Container.ResolveRequired<IPacketDeliveryService>();
        serializerService = Container.ResolveRequired<ISerializerService>();
    }

    [Fact]
    public void TestSinglePacketReceive() {
        // write login packet to stream
        var stream = new MemoryStream();
        var bytes = serializerService.Serialize(mockLoginPacket);
        stream.Write(bytes);
        stream.Position = 0;

        // read
        var task = packetDeliveryService.ReceivePacketAsync(stream);
        task.Wait();

        // assume successful wait
        var packet = task.Result;

        Assert.Equal(mockLoginPacket, packet);
    }

    [Fact(Skip ="In current state (no message delimiter), service will read an entire byte stream and only parse the first LoginPacket that comes through")]
    public void TestMultiPacketReceive() {
        // write two login packet to stream immediately
        var stream = new MemoryStream();
        var bytes = serializerService.Serialize(mockLoginPacket);
        stream.Write(bytes);
        stream.Write(bytes);
        stream.Position = 0;

        // read
        var task = packetDeliveryService.ReceivePacketAsync(stream);
        task.Wait();

        // assume successful wait
        var packet = task.Result;
        Assert.Equal(mockLoginPacket, packet);

        // read second packet
        task = packetDeliveryService.ReceivePacketAsync(stream);
        task.Wait();

        // assume second successful wait
        packet = task.Result;
        Assert.Equal(mockLoginPacket, packet);
    }

    [Fact]
    public void TestPacketSend() {
        // send the packet to mock stream
        var stream = new MemoryStream();
        packetDeliveryService.SendPacketAsync(stream, mockLoginPacket).Wait();

        // test bytes sent can be deserialized into original packet
        var data = stream.ToArray();
        var packet = serializerService.Deserialize(data);

        Assert.Equal(mockLoginPacket, packet);
    }

    /// <summary>
    /// Checks if sending and receiving work together
    /// </summary>
    [Fact]
    public void TestPacketLoop() {
        // send the packet to mock stream
        var stream = new MemoryStream();
        packetDeliveryService.SendPacketAsync(stream, mockLoginPacket).Wait();

        // resest stream position
        stream.Position = 0;

        // read stream
        var packet = Task.Run(() => packetDeliveryService.ReceivePacketAsync(stream)).Result;

        // check equality
        Assert.Equal(mockLoginPacket, packet);
    }
}
