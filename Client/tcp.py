import time
import socket
import packets_pb2 as pack
from crypto import CryptoContext
from typing import Optional
from google.protobuf import message as pb
from packet_config import PacketConfig
from queue import Queue
import threading

class SharedNetworkReceiver:
    def __init__(self, tcp_client):
        self.tcp_client = tcp_client
        self.packet_queue = Queue()
        self.running = False
        self.thread = threading.Thread(target=self.listen_for_packets)

    def listen_for_packets(self):
        while self.running:
            packet = self.tcp_client.receive_packet()
            self.packet_queue.put(packet)

    def start(self):
        self.running = True
        self.thread.start()

    def stop(self):
        self.running = False
        self.thread.join()

    def get_packet(self) -> Optional[pack.Packet]:
        if not self.packet_queue.empty():
            return self.packet_queue.get()
        return None

class Client:
    def __init__(self, hostname: str, port: int):
        self.address = hostname, port        
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.crypto_context: CryptoContext = CryptoContext()
        self.running = False

    def start(self, max_attempts: int = 20):
        try:
            self.sock.connect(self.address)
        except socket.error:
            raise socket.error("Connection refused by server. Is the server running?")

        # Receive the server's RSA public key
        num_tries: int = 0
        while num_tries < max_attempts:
            time.sleep(0.1)
            packet = self.receive_packet()
            if packet is None:
                num_tries += 1
                continue
            
            if packet.HasField("public_rsa_key"):
                print("Received public RSA key")
                self.crypto_context.set_server_rsa_public_key(packet.public_rsa_key.key)

                aes_key_packet = pack.Packet()
                aes_key_packet.aes_key.key = self.crypto_context.get_client_aes_private_key()
                self.send_packet(aes_key_packet)

                # Wait for the server to send us an OK packet to confirm the AES key was received
                packet = self.receive_packet()
                if packet is None:
                    num_tries += 1
                    continue
                if packet.HasField("ok"):
                    print("AES key exchange successful")
                    self.running = True
                    return
        
        raise Exception("Failed to exchange AES key with server (max attempts exceeded)")
        
    def receive_packet(self) -> Optional[pack.Packet]:
        try:
            # Read the packet length (assuming it's a 4-byte integer)
            packet_length_data = self.sock.recv(4)
            packet_length = int.from_bytes(packet_length_data, byteorder='big')

            # Now read the packet data
            header = self.sock.recv(1)
            data = self.sock.recv(packet_length)
        except socket.error:
            if not self.running:
                return None
            else:
                raise

        if len(header) == 0 or len(data) == 0:
            return

        packet_config = PacketConfig.from_byte(header[0])
        if packet_config.aes_encrypted:
            data = self.crypto_context.aes_decrypt(data)

        try:
            packet = pack.Packet.FromString(data)
        except pb.DecodeError:
            print(f"Failed to decode packet. Length: {packet_length}, Header: {header}, Data: {data}")
            return None
        return packet


    def send_packet(self, packet: pack.Packet, config: Optional[PacketConfig] = None):
        if config is None:
            config = PacketConfig()

        # Ensure the AESEncrypted flag is set on the header if the packet type demands encryption
        # (Unless the packet is an AESKey packet, in which case it needs to be RSA)
        if PacketConfig.has_flag(packet, pack.encrypted) and not packet.HasField("aes_key"):
            config.aes_encrypted = True
            config.rsa_encrypted = False
        elif packet.HasField("aes_key"):
            config.aes_encrypted = False
            config.rsa_encrypted = True
        
        header: int = config.to_byte()

        data: bytes = packet.SerializeToString()
        if config.rsa_encrypted:
            data = self.crypto_context.rsa_encrypt(data)
        elif config.aes_encrypted:
            data = self.crypto_context.aes_encrypt(data)
        
        # Send the packet length (assuming it's a 4-byte integer)
        self.sock.send(len(data).to_bytes(4, byteorder='big'))
        self.sock.send(header.to_bytes(1, byteorder='big'))
        self.sock.send(data)

    def stop(self):        
        self.running = False
        self.sock.close()
        print("TCP client is stopping...")