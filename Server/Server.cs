#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

using Moonlapse.Server.Utils;
using Moonlapse.Server.Packets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server {
    public class Server {
        public const int Port = 42523;

        public ISet<Protocol> ConnectedProtocols;

        const int tickRate = 5; // Hertz
        const long timeBetweenTicks = 1000 / tickRate; // Milliseconds

        readonly TcpListener listener;

        public Server() {
            listener = new(IPAddress.Any, Port);
            ConnectedProtocols = new HashSet<Protocol>();
        }

        public async Task StartAsync() {
            CryptoContextService.GenerateRSAKeyPair();
            listener.Start();
            Log.Information($"Started listening on port {Port}");

            Task.Run(TickLoopAsync);

            await ListenLoopAsync();
        }

        async Task ListenLoopAsync() {
            while (true) {
                var client = await listener.AcceptTcpClientAsync();
                Log.Information($"New connection! {client.Client.Handle}");

                var protocol = new Protocol(client, this);
                ConnectedProtocols.Add(protocol);
                Task.Run(protocol.StartAsync);
            }
        }

        async Task TickLoopAsync() {
            var sw = new Stopwatch();
            sw.Start();
            while (true) {
                sw.Restart();
                // Wait for each protocol's tick to complete
                var tickTasks = from proto in ConnectedProtocols
                                select proto.TickAsync();
                await Task.WhenAll(tickTasks);

                sw.Stop();

                // Get delta time and wait the remaining time available
                var deltaTime = sw.ElapsedMilliseconds;

                var msToWait = timeBetweenTicks - deltaTime;

                if (msToWait > 0) {
                    await Task.Delay((int)msToWait);
                }
                else if (msToWait < 0) {
                    Log.Warning($"Server time budget exceeded by {-msToWait}ms this tick");
                }

                sw.Start();
            }
        }
    }
}
