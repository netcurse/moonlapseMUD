﻿using Microsoft.Extensions.DependencyInjection;
using Moonlapse.Server.Packets;
using Moonlapse.Server.ProtocolStates;
using Moonlapse.Server.Serializers;
using Moonlapse.Server.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server {
    public class Protocol {
        public bool Connected { get; private set; }
        public IProtocolState ProtocolState { get; private set; }

        /// <summary>
        /// A collection of queues sent from this protocol, organised in a dictionary whose keys are the receiving protocols.
        /// </summary>
        readonly IDictionary<Protocol, CircularQueue<Packet>> outboundPacketQueues;
        readonly IPacketDeliveryService packetDeliveryService;
        readonly TcpClient client;
        readonly Server server;

        public Protocol(TcpClient client, Server server) {
            this.client = client;
            this.server = server;
            packetDeliveryService = Container.ResolveRequired<IPacketDeliveryService>();
            ChangeState<EntryState>();
            outboundPacketQueues = new Dictionary<Protocol, CircularQueue<Packet>>();
        }

        public async Task StartAsync() {
            Connected = true;
            await ListenLoopAsync();
        }

        public void ChangeState<T>() where T : notnull, ProtocolState {
            ProtocolState = (T)Activator.CreateInstance(typeof(T), this)!;
        }

        /// <summary>
        /// Gets the oldest packet from each outbound queue, and gets the respective recipient to process it.
        /// This function should only be called from the parent server, in its <c>tickLoopAsync</c> function.
        /// </summary>
        public async Task TickAsync() {
            foreach (var pair in outboundPacketQueues) {
                var recipient = pair.Key;
                var packetsToSend = pair.Value;

                if (packetsToSend.Count > 0) {
                    var packetToSend = packetsToSend.Dequeue();

                    if (recipient == this) {
                        await SendClientAsync(packetToSend);
                    } else {
                        recipient.ProtocolState.DispatchPacket(this, packetToSend);
                    }

                    // Remove the recipient from the outboundPacketQueues dictionary if the queue has become empty
                    if (packetsToSend.Count <= 0) {
                        outboundPacketQueues.Remove(recipient);
                    }
                }
            }
        }

        /// <summary>
        /// Broadcasts a packet to all protocols connected to the server except yourself 
        /// (unless you set the optional <c>includeSender</c> parameter to <c>true</c>).
        /// </summary>
        public void Broadcast(Packet packet, bool includeSender = false) {
            foreach (var proto in server.ConnectedProtocols) {
                if (proto != this || includeSender) {
                    QueueOutboundPacket(proto, packet);
                }
            }
        }

        async Task SendClientAsync(Packet packet) {
            try {
                var stream = client.GetStream();
                await packetDeliveryService.SendPacketAsync(client.GetStream(), packet);
            } catch (InvalidOperationException) {
                throw new SocketClosedException();
            }
        }

        /// <summary>
        /// Gets this protocol to add a packet to its outbound queue. Use this function to send a packet <c>packet</c> to another protocol <c>other</c>:
        /// This will schedule the packet for dispatch in a future tick, which in turn, will call the receiving protocol's packetReceived method for processing.
        /// </summary>
        public void QueueOutboundPacket(Protocol recipient, Packet packet) {
            if (outboundPacketQueues.TryGetValue(recipient, out CircularQueue<Packet>? value)) {
                value!.Enqueue(packet);
            }
            else {
                outboundPacketQueues.Add(recipient, new CircularQueue<Packet>(10)); // Each queue has a max size of 10 (rolls over) to avoid spamming
                outboundPacketQueues[recipient].Enqueue(packet);
            }
            Log.Debug($"{client.Client.Handle} queued a {packet.TypeCase} packet for protocol {recipient.client.Client.Handle} while in the {ProtocolState.GetType()} state");
        }

        async Task ListenLoopAsync() {
            while (Connected) {
                try {
                    var nextPacket = await ReadNextPacketAsync();
                    ProtocolState.DispatchPacket(this, nextPacket);
                } catch (SocketClosedException) {
                    End();
                } catch (Exception ex) {
                    Log.Error($"Protocol {client.Client.Handle} received exception in {ProtocolState.GetType()} state: {ex}");
                    continue;
                }
            }
        }

        async Task<Packet> ReadNextPacketAsync() {
            try {
                var stream = client.GetStream();
                return await packetDeliveryService.ReceivePacketAsync(client.GetStream());
            }
            catch (InvalidOperationException) {
                throw new SocketClosedException();
            }
        }

        void End() {
            Log.Information($"Client {client.Client.Handle} has disconnected");
            client.Close();
            Connected = false;
            server.ConnectedProtocols.Remove(this);
        }
    }
}
