using Microsoft.Extensions.DependencyInjection;
using Moonlapse.Server.Packets;
using Moonlapse.Server.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Utils {
    public static class Container {
        public static ServiceProvider ServiceProvider => serviceProvider ?? throw new Exception("Container was not configured before using.");
        static ServiceProvider? serviceProvider;

        public static void ConfigureServices() {
            var services = new ServiceCollection()
                .AddSingleton<ISerializerService, ProtobufSerializerService>()
                .AddSingleton<IPacketDeliveryService, PacketDeliveryService>();

            serviceProvider = services.BuildServiceProvider();
        }

        public static T? Resolve<T>() {
            return ServiceProvider.GetService<T>();
        }

        public static T ResolveRequired<T>() where T : notnull {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}
