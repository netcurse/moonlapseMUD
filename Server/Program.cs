using Microsoft.Extensions.DependencyInjection;
using Moonlapse.Server.Packets;
using Moonlapse.Server.Serializers;
using Moonlapse.Server.Utils;
using Serilog;
using System;
using System.Collections.Generic;

namespace Moonlapse.Server {
    public class Program {
        static ServiceProvider? serviceProvider;

        static async Task Main(string[] args) {
            ConfigureServices();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();
            Log.Information("Logger has been initialized.");

            var server = serviceProvider!.GetRequiredService<Server>();
            await server.StartAsync();
        }

        public static void ConfigureServices() {
            var services = new ServiceCollection()
                .AddSingleton<ISerializerService, ProtobufSerializerService>()
                .AddSingleton<IPacketDeliveryService, PacketDeliveryService>()
                .AddSingleton<ICryptoContextService, CryptoContextService>()
                .AddSingleton<IFlagsCacheService, FlagsCacheService>()
                .AddSingleton<IPacketConfigService, PacketConfigService>()
                .AddSingleton<ProtocolFactory>()
                .AddSingleton<Server>()
                ;

            serviceProvider = services.BuildServiceProvider();
        }
    }
}