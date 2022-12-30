# MoonlapseMUD
Welcome to MoonlapseMUD: an open-source, multi-user dungeon designed to play directly in any terminal.

For more information, see the [website](https://moonlapse.net).

## Quick start for developers
### Clone the repo
```bash
git clone https://github.com/netcurse/moonlapseMUD
cd moonlapseMUD
```

### Setup the client virtual environment
```bash
cd Client/
python -m venv ./.venv
source ./.venv/bin/activate # (or for Windows: .\.venv\Scripts\activate)
pip install -r requirements.txt
```

### Setting up VS Code for editing the client code
Firstly, you should get the ms-python.python extension for VS Code. Without it, life will suck.

You might get an annoying error when editing client python files in VS Code if you have Visual Studio installed. It will say something like `__main__.py" is overriding the stdlib module "__main__"`. To fix this, simply add the following to your `settings.json` file in VS Code (this will already be there if you kept the `settings.json` from the cloned repository):
```
"python.languageServer": "Pylance",
"python.analysis.diagnosticSeverityOverrides": {
    "reportShadowedImports": "none"
}
```

### Making changes to the packets
This project uses `protobuf` to define the packets that are sent between the client and server. The packets are defined in the root directory's `packets.proto`.

Every time you make a change to one of the packets in `packets.proto`, you need to run `protoc` to generate the C# and Python code. 
To get this set up initially, make sure you have the following pip packages installed (you should if you ran `pip install -r requirements.txt` as per above):
```
mypy-protobuf
pylint-protobuf
```

You should also make sure the following setting is defined in your `settings.json` file (again, this will already be there if you kept the `settings.json` from the cloned repository):
```json
"python.linting.pylintArgs": [
    "--load-plugins",
    "pylint_protobuf"
]
```

Now to actually (re-)generate the C# and Python code defining the packets, run the following command from the project's root directory:
```bash
protoc -I="." --python_out="./Client" --mypy_out="./Client" --csharp_out="./Server" "./packets.proto"
```

You should see the following files are updated:
```
Server/Packets.cs
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