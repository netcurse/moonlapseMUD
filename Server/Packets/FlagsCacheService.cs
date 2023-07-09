using Google.Protobuf.Reflection;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Packets {
    public class FlagsCacheService : IFlagsCacheService {
        private readonly Dictionary<string, Dictionary<Extension<FieldOptions, bool>, bool>> flagsCache = new();

        public void UpdateFlag(string packetName, Extension<FieldOptions, bool> flag, bool hasFlagSet) {
            if (!flagsCache.ContainsKey(packetName)) {
                flagsCache.Add(packetName, new Dictionary<Extension<FieldOptions, bool>, bool>());
            }

            var cachedPacketTypeFlags = flagsCache[packetName];
            if (!cachedPacketTypeFlags.ContainsKey(flag)) {
                cachedPacketTypeFlags.Add(flag, hasFlagSet);
            }
            else {
                cachedPacketTypeFlags[flag] = hasFlagSet;
            }
        }

        public bool GetFlag(string packetName, Extension<FieldOptions, bool> flag, out bool cachedFlag) {
            if (flagsCache.ContainsKey(packetName)) {
                var cachedPacketTypeFlags = flagsCache[packetName];
                if (cachedPacketTypeFlags.ContainsKey(flag)) {
                    cachedFlag = cachedPacketTypeFlags[flag];
                    return true;
                }
            }

            cachedFlag = false;
            return false;
        }
    }

}
