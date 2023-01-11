using Moonlapse.Server.Packets;
using Moonlapse.Server.Serializers;
using Moonlapse.Server.Extensions;
using Moonlapse.Server.Utils;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Moonlapse.Server.Tests.Unit.Packets;

public class TestPacketDeliveryService : IDisposable {

    const int PORT = 42720;
    readonly TimeSpan timeout = TimeSpan.FromMilliseconds(5000);

    NetworkStream ClientStream => clientConnection.GetStream();
    NetworkStream ServerStream => serverConnection.GetStream();

    readonly Packet mockLoginPacket;

    readonly IPacketDeliveryService packetDeliveryService;
    readonly ISerializerService serializerService;

    readonly TcpListener listener;
    readonly TcpClient clientConnection;      // acts as the client's connection to the server
    TcpClient serverConnection;               // acts as the server's connection to the client


    public TestPacketDeliveryService() {
        // Making the mock packets
        mockLoginPacket = new() {
            Login = new() {
                Username = "john",
                Password = "123"
            }
        };

        // create server socket
        listener = new TcpListener(IPAddress.Loopback, PORT);

        // create mock client socket
        clientConnection = new TcpClient();

        // start connection for each test
        StartSocketConnection();

        // creating the container and injecting services
        // todo: it's probably better to have tests for each implementation of the abstract services, rather than testing the injected implementation
        Container.ConfigureServices();
        packetDeliveryService = Container.ResolveRequired<IPacketDeliveryService>();
        serializerService = Container.ResolveRequired<ISerializerService>();
    }

    /// <summary>
    /// Tests a single packet sent from client to server
    /// </summary>
    [Fact]
    public void TestSinglePacketLoop() {
        // write login packet from client
        var bytes = serializerService.Serialize(mockLoginPacket);
        ClientStream.Write(bytes);

        // server read
        var task = packetDeliveryService
            .ReceivePacketAsync(ServerStream)
            .TimeoutAfter(timeout)
            .AwaitResult();

        var packet = task.Result;
        Assert.Equal(mockLoginPacket, packet);
    }

    /// <summary>
    /// Tests multiple packets being sent immediately from client to server
    /// </summary>
    [Fact(Skip = "In current state (no message delimiter), service will read an entire byte stream and only parse the first LoginPacket that comes through")]
    public void TestMultiPacketReceive() {
        // write two login packet to stream immediately
        var bytes = serializerService.Serialize(mockLoginPacket);
        ClientStream.Write(bytes);
        ClientStream.Write(bytes);

        // read
        var task = packetDeliveryService
            .ReceivePacketAsync(ServerStream)
            .TimeoutAfter(timeout)
            .AwaitResult();

        var packet = task.Result;
        Assert.Equal(mockLoginPacket, packet);

        // read second packet
        task = packetDeliveryService
            .ReceivePacketAsync(ServerStream)
            .TimeoutAfter(timeout)
            .AwaitResult();

        packet = task.Result;
        Assert.Equal(mockLoginPacket, packet);
    }

    void StartSocketConnection() {
        listener.Start();

        var serverTask = listener.AcceptTcpClientAsync();
        var clientTask = clientConnection.ConnectAsync(IPAddress.Loopback, PORT);

        Task.WaitAll(new Task[] { clientTask.TimeoutAfter(timeout), serverTask.TimeoutAfter(timeout) }, timeout);

        serverConnection = serverTask.Result;
    }

    public void Dispose() {
        clientConnection.Close();
        serverConnection.Close();
        listener.Stop();
    }
}
