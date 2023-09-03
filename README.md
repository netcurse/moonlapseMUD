# MoonlapseMUD
Welcome to MoonlapseMUD: an open-source, multi-user dungeon designed to play directly in any terminal.

For more information, see the [website](https://moonlapse.net).

## Quick start for developers
### Clone the repo
```bash
git clone https://github.com/netcurse/moonlapseMUD
cd moonlapseMUD
```

### Client requirements installation
To install the client's requirements, run the following commands from the project's root directory:
```bash
python -m venv Client/.venv # (may need to use "python3" or "py" instead of "python")
source Client/.venv/bin/activate # (or for Windows: Client\.venv\Scripts\activate)
pip install -r Client/requirements.txt
```

### Run the server
To start the server, run this command from the project's root directory:
```bash
dotnet run --project Server
```

### Run the client
To start the client, run this command from the project's root directory:
```bash
source Client/.venv/bin/activate # (or for Windows: Client\.venv\Scripts\activate)
python Client
```

### Making changes to the packets
This project uses `protobuf` to define the packets that are sent between the client and server. The packets are defined in the root directory's `packets.proto`.

Note that **some** packets can be marked as encrypted. This is done by adding the `[(encrypted) = true]` option to the field of the base `Packet` definition. For example, here is a barebones definition for the base packet, noting encrypted Login, Registration, and AES Key packets, but all others are unencrypted: 
```proto
message Packet {
    oneof type {
        LoginPacket login = 1 [(encrypted) = true];
        RegisterPacket register = 2 [(encrypted) = true];
        ChatPacket chat = 3 [(encrypted) = false];
        PublicRSAKeyPacket public_rsa_key = 4 [(encrypted) = false];
        AESKeyPacket aes_key = 5 [(encrypted) = true];
    }
}
```
The `AESKeyPacket` is marked as encrypted, although this is understood to be a special case where the client uses the server's RSA public key to send its own AES private key. **Do not change the encryption status of this packet**.

Every time you make a change to one of the packets in `packets.proto`, you need to run `protoc` to generate the C# and Python code.
To get this set up initially, make sure you have the following pip packages installed (you should if you ran `pip install -r requirements.txt` as per above):
```
mypy-protobuf
pylint-protobuf
```

Also, download the `protoc` compiler from [here](https://github.com/protocolbuffers/protobuf/releases) and add it to your PATH. If you don't want to add it to your PATH, you can put it inside the `Client/.venv/bin` directory (or `Client\.venv\Scripts` for Windows) and it will be found automatically when you run the following command (if you have activated the virtual environment as per above).

Now to actually (re-)generate the C# and Python code defining the packets, run the following command from the project's root directory, ensuring that you have activated the virtual environment as per above:
```bash
protoc -I="Shared" --python_out="Client" --mypy_out="Client" --csharp_out="Server/Packets" "packets.proto"
```

You should see the following files are updated:
```
Server/Packets/Packets.cs
Client/packets_pb2.py
Client/packets_pb2.pyi
```

## Notes for developers
### The tick loop
The server has a tick loop that runs according to the `tickRate` variable in `Server/Server.cs`. This variable represents how many times per second the server will tick. The server ticks by calling the `TickAsync` method in each protocol (these are all run at the same time). The server tick should run at a constant rate, but if the server is under heavy load, the tick rate will drop.

The protocol's `TickAsync` function should **only** ever be called from the server's tick loop. It should never be called from anywhere else. The `TickAsync` function dequeues packets from each protocol's outbound queue, and gets the recipients to process according to their [state](#state-machine).

### State machine
The server's protocol uses a state pattern, so the `ProtocolState` member variable is a reference to an object that can process packets on each server tick.

To set the state of a protocol called `protocol`, to a state called `ExampleState`, simply write
```csharp
protocol.ChangeState<ExampleState>();
``` 
If there is a corresponding class located in `ProtocolStates/ExampleState.cs` which inherits from `ProtocolState`, then the protocol's state will be changed to an instance of that class.

These state classes should also subscribe to events defined in the parent `ProtocolState` class's `DispatchPacket` function. Once subscribed, the handler will be called whenever a packet is received by the protocol.

### Sending packets to other protocols
To send a packet `packet` to another protocol `other` for processing, simply write 
```csharp
QueueOutboundPacket(other, packet);
```

This will schedule your packet to be sent to the receiving protocol in a future tick (depending on how many other packets you've sent to this protocol recently).

When the receiving protocol gets this packet, it will be processed straight away according to its state in the `packetReceived` method.

To send a packet to all protocols, use the `Broadcast` method, and consider using the optional `includeSelf` parameter which is `false` by default.

### Sending packets directly to the client
To send a packet `packet` directly to the client of a protocol `proto`, simply use:
```csharp
proto.QueueOutboundPacket(proto, packet);
```

### A note on packet transmission
Packets are sent with a corresponding `PacketConfig` object. The `PacketConfig` object contains a one-byte (8-bit) header that is sent in front of the packet, and communicates flags such as whether the packet is encrypted or not.

When you send a packet, you can construct a `PacketConfig` to use (this may be useful if you want to communicate other properties of the packet), but ultimately, the server will set the appropriate encryption bits in the header for you, depending on the `encrypted` flag of the packet's definition in `packets.proto`.

### A note on encryption
The server's RSA key is generated on startup (unless it already exists), and is stored in the server's memory. It is also kept in a `Server/bin/Keys` directory as `public.pem` and `private.pem` for debugging purposes. 
> ⚠️ Note that `private.pem` should **never** be shared.

Each client generates its own unique AES private key on startup, and sends it to the server using the `AESKeyPacket` (which is encrypted using the server's RSA public key). The client's server protocol then stores the received and decrypted AES private key in the `CryptoContextService`. 
You don't have to worry about this, as the `CryptoContextService` is injected into the server's protocol, and is used to encrypt and decrypt packets.