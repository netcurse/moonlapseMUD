import unittest
from unittest.mock import Mock, patch
from Client import tcp
from Client.Packets import packets_pb2 as pack
from Client.Packets.packet_config import PacketConfig
from Client.Packets.crypto import CryptoContext
from google.protobuf import message as pb

class TestTCPClient(unittest.TestCase):
    def setUp(self):
        self.hostname = "localhost"
        self.port = 1234

        self.patcher_socket = patch('socket.socket')
        self.mock_socket = self.patcher_socket.start()
        self.mock_socket_instance = self.mock_socket.return_value

        self.client = tcp.Client(self.hostname, self.port)

    def tearDown(self):
        self.patcher_socket.stop()

    def test_init(self):
        self.assertEqual(self.client.address, (self.hostname, self.port))
        self.assertFalse(self.client.running)
        self.assertIsNotNone(self.client.sock)

    def test_receive_chat_packet_with_blank_header(self):
        mock_recv_pack = pack.Packet()
        mock_recv_pack.chat.name = "John"
        mock_recv_pack.chat.message = "Hello"
        mock_recv_data = mock_recv_pack.SerializeToString()
        mock_recv_header = PacketConfig().to_byte()

        self.mock_socket_instance.recv.side_effect = [
            mock_recv_header.to_bytes(1, byteorder='big'), 
            mock_recv_data
        ]

        packet = self.client.receive_packet()
        self.assertEqual(packet.chat.name, "John")
        self.assertEqual(packet.chat.message, "Hello")


if __name__ == '__main__':
    unittest.main()
