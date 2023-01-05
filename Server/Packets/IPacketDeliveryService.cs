﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Packets {
    public interface IPacketDeliveryService {
        public Task SendPacketAsync(NetworkStream stream, Packet packet, byte[] aesPrivateKey, PacketConfig? config = default);
        public Task<Packet> ReceivePacketAsync(NetworkStream stream, byte[] aesPrivateKey);
    }
}
