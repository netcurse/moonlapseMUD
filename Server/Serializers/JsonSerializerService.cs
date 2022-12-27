using Google.Protobuf;
using Moonlapse.Server.Packets;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Serializers {
    public class JsonSerializerService : ISerializerService {

        readonly JsonParser parser;
        readonly JsonFormatter formatter; 

        public JsonSerializerService() {
            parser = new(JsonParser.Settings.Default);
            formatter = new(JsonFormatter.Settings.Default);
        }

        public Packet Deserialize(byte[] data) {
            var s = Encoding.UTF8.GetString(data);
            return parser.Parse<Packet>(s);
        }

        public byte[] Serialize(Packet packet) {
            var s = formatter.Format(packet);
            return Encoding.UTF8.GetBytes(s);
        }
    }
}
