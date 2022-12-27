using Microsoft.Extensions.DependencyInjection;
using Moonlapse.Server.Packets;
using Moonlapse.Server.Serializers;
using Moonlapse.Server.Utils;
using Serilog;
using System.Collections.Generic;

namespace Moonlapse.Server {
    public class Program {
        static async Task Main(string[] args) {
            Container.ConfigureServices();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();
            Log.Information("Logger has been initialized.");

            var server = new Server();
            await server.StartAsync();
        }
    }
}