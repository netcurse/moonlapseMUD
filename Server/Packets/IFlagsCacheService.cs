using Google.Protobuf.Reflection;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Packets {
    public interface IFlagsCacheService {
        void UpdateFlag(string packetName, Extension<FieldOptions, bool> flag, bool hasFlagSet);
        bool GetFlag(string packetName, Extension<FieldOptions, bool> flag, out bool cachedFlag);
    }
}
