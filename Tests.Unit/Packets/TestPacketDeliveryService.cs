using Moonlapse.Server.Packets;
using Moonlapse.Server.Serializers;
using Moonlapse.Server.Utils;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;

namespace Moonlapse.Server.Tests.Unit.Packets;

public class TestPacketDeliveryService {

    readonly Packet mockLoginPacket;
    readonly byte[] clientAESPrivateKey;
    readonly Packet mockAesKeyPacket;
    readonly Packet mockServerRsaPublicKeyPacket;
    readonly Packet mockChatPacket;

    readonly ServiceProvider serviceProvider;
    readonly IPacketDeliveryService packetDeliveryService;
    readonly ISerializerService serializerService;
    readonly ICryptoContextService cryptoContextService;
    readonly IPacketConfigService packetConfigService;
    public TestPacketDeliveryService() {
        // creating the container and injecting services
        // todo: it's probably better to have tests for each implementation of the abstract services, rather than testing the injected implementation
        
        var services = new ServiceCollection()
            .AddSingleton<ISerializerService, ProtobufSerializerService>()
            .AddSingleton<IPacketDeliveryService, PacketDeliveryService>()
            .AddSingleton<ICryptoContextService, CryptoContextService>()
            .AddSingleton<IFlagsCacheService, FlagsCacheService>()
            .AddSingleton<IPacketConfigService, PacketConfigService>()
            ;
        serviceProvider = services.BuildServiceProvider();

        packetDeliveryService = serviceProvider.GetRequiredService<IPacketDeliveryService>();
        serializerService = serviceProvider.GetRequiredService<ISerializerService>();
        cryptoContextService = serviceProvider.GetRequiredService<ICryptoContextService>();
        packetConfigService = serviceProvider.GetRequiredService<IPacketConfigService>();

        // Mock login packet
        mockLoginPacket = new() {
            Login = new() {
                Username = "john",
                Password = "123"
            }
        };

        // Mock chat packet
        mockChatPacket = new() {
            Chat = new() {
                Name = "John",
                Message = "Hello World!"
            }
        };

        // Mock AES key packet
        var clientAes = Aes.Create();
        clientAESPrivateKey = clientAes.Key;
        cryptoContextService.SetClientAESPrivateKey(0, clientAESPrivateKey);

        mockAesKeyPacket = new() {
            AesKey = new() {
                Key = ByteString.CopyFrom(clientAes.Key)
            }
        };

        // Mock server RSA keys
        mockServerRsaPublicKeyPacket = new() {
            PublicRsaKey = new() {
                Key = ByteString.CopyFrom(cryptoContextService.GetServerRSAPublicKey(), Encoding.UTF8)
            }
        };
    }

    /// <summary>
    /// Encrypts the given plaintext with the server's public RSA key. This method is intentionally omitted from the <c>CryptoContextService</c> 
    /// because it is only used for testing. Including this in the service could only lead to confusion or bad practice.
    /// </summary>
    /// <param name="plaintext">The plaintext bytes to encrypt with RSA</param>
    /// <returns>The encrypted bytes</returns>
    byte[] RsaEncrypt(byte[] plaintext) {
        using var rsa = new RSACryptoServiceProvider(2048);
        rsa.ImportFromPem(cryptoContextService.GetServerRSAPublicKey());
        return rsa.Encrypt(plaintext, true);
    }

    /// <summary>
    /// Uses a given configuration and packet to construct a header byte, and writes a the header and packet to a stream. If no configuration is provided, 
    /// a default configuration is used. If the packet type is not an AESKeyPacket, but demands encryption, the AESEncrypted flag is set on the header. 
    /// Otherwise, if the packet has an AES key, the RSAEncrypted flag is set on the header. No other flags are automatically set on the header.
    /// </summary>
    /// <param name="stream">The stream to write the packet to</param>
    /// <param name="packet">The packet to write to the stream</param>
    /// <param name="config">The configuration to use when writing the packet to the stream. If not provided, a default configuration is used.</param>
    void WritePacketToStream(Stream stream, Packet packet, PacketConfig? config = default) {
        config ??= new PacketConfig();

        // Ensure the AESEncrypted flag is set on the header if the packet type demands encryption
        if (packetConfigService.HasFlag(config, packet, PacketsExtensions.Encrypted) && packet.AesKey == null) {
            config.AESEncrypted = true;
            config.RSAEncrypted = false;
        } else if (packet.AesKey != null) {
            config.AESEncrypted = false;
            config.RSAEncrypted = true;
        }

        var header = config.ToByte();

        var data = serializerService.Serialize(packet);
        if (config.RSAEncrypted) {
            data = RsaEncrypt(data);
        } else if (config.AESEncrypted) {
            data = cryptoContextService.AESEncrypt(0, data);
        }

        stream.WriteByte(header);
        stream.Write(data);
        stream.Position = 0;
    }

    [Fact]
    public async void TestSendAndReceiveAESKeyPacket() {
        // send the packet to mock stream
        var stream = new MemoryStream();
        WritePacketToStream(stream, mockAesKeyPacket);

        // read and reconstruct packet
        var packet = await packetDeliveryService.ReceivePacketAsync(0, stream);

        // tests
        Assert.Equal(mockAesKeyPacket, packet);
        Assert.Equal(clientAESPrivateKey, packet.AesKey.Key.ToByteArray());
    }

    [Fact]
    public async void TestSendAndReceiveLoginPacket() {
        // send the packet to the mock stream
        var stream = new MemoryStream();
        WritePacketToStream(stream, mockLoginPacket);

        // read and reconstruct packet
        var packet = await packetDeliveryService.ReceivePacketAsync(0, stream);

        // tests
        Assert.Equal(mockLoginPacket, packet);
        Assert.Equal("john", packet.Login.Username);
        Assert.Equal("123", packet.Login.Password);
    }

    [Fact(Skip = "In current state (no message delimiter), service will read an entire byte stream and only parse the first LoginPacket that comes through")]
    public async void TestMultiPacketReceive() {
        // write two login packet to stream immediately
        var stream = new MemoryStream();
        var bytes = serializerService.Serialize(mockChatPacket);
        stream.Write(bytes);
        stream.Write(bytes);
        stream.Position = 0;

        // read first packet
        var packet = await packetDeliveryService.ReceivePacketAsync(0, stream);
        Assert.Equal(mockChatPacket, packet);

        // read second packet
        packet = await packetDeliveryService.ReceivePacketAsync(0, stream);
        Assert.Equal(mockChatPacket, packet);
    }

    [Fact]
    public void TestPacketSend() {
        // send the packet to mock stream
        var stream = new MemoryStream();
        packetDeliveryService.SendPacketAsync(0, stream, mockChatPacket).Wait();

        // test bytes sent can be deserialized into original packet
        var data = stream.ToArray();
        var packet = serializerService.Deserialize(data[1..]);

        Assert.Equal(mockChatPacket, packet);
    }

    /// <summary>
    /// Checks if sending and receiving work together
    /// </summary>
    [Fact]
    public void TestPacketLoop() {
        // send the packet to mock stream
        var stream = new MemoryStream();
        packetDeliveryService.SendPacketAsync(0, stream, mockChatPacket).Wait();

        // resest stream position
        stream.Position = 0;

        // read stream
        var packet = Task.Run(() => packetDeliveryService.ReceivePacketAsync(0, stream)).Result;

        // check equality
        Assert.Equal(mockChatPacket, packet);
    }
}
