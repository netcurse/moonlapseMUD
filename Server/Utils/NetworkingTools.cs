using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Utils {
    public class NetworkingTools {
        /// <summary>
        /// Ensures the data is in big-endian order. This is a common convention for network data and allows us to always assume data should be read in 
        /// big-endian order on the client-side.
        /// </summary>
        /// <param name="data">The byte array to potentially reverse if need-be.</param>
        public static void EnsureBigEndian(byte[] data) {
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(data);
            }
        }
    }
}