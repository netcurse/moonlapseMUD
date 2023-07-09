import socket
import threading
import time
import packets_pb2 as pack
from crypto import CryptoContext
from typing import Optional
from google.protobuf import message as pb
from packet_config import PacketConfig


class Client:
    def __init__(self, hostname: str, port: int):
        self.address = hostname, port
        self.running: bool = False
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.read_thread = threading.Thread(target=self.read)
        self.write_thread = threading.Thread(target=self.write)
        self.username: str = input("Please enter your name: ")
        self.crypto_context: CryptoContext = CryptoContext()

    def start(self):
        self.sock.connect(self.address)
        self.running = True
        self.read_thread.start()
        self.write_thread.start()

    def receive_packet(self) -> pack.Packet:
        max_buffer_size: int = 1500
        header: bytes = self.sock.recv(1)
        data: bytes = self.sock.recv(max_buffer_size - 1)

        if len(header) == 0 or len(data) == 0:
            raise socket.error("Socket connection broken")

        packet_config = PacketConfig.from_byte(header[0])
        if packet_config.aes_encrypted:
            data = self.crypto_context.aes_decrypt(data)

        packet = pack.Packet.FromString(data)
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
            
        self.sock.send(header.to_bytes(1, "big") + data)

    def read(self):
        while self.running:
            try:
                packet = self.receive_packet()

                if packet.HasField("public_rsa_key"):
                    print("Received public RSA key")
                    self.crypto_context.set_server_rsa_public_key(packet.public_rsa_key.key)

                    aes_key_packet = pack.Packet()
                    aes_key_packet.aes_key.key = self.crypto_context.get_client_aes_private_key()
                    self.send_packet(aes_key_packet)
                    time.sleep(1) # Required to ensure the server has received the AES key before we try to login
                    self.login()

                elif packet.HasField("chat"):
                    print(f"{packet.chat.name}: {packet.chat.message}")
                elif packet.HasField("login"):
                    print("Received a login packet")
                elif packet.HasField("register"):
                    print("Received a register packet")
                else:
                    print("Got unknown data?")
            except Exception as e:
                print("Error receiving data")
                print(e)
                self.stop()
        
    def write(self):
        while self.running:
            try:
                packet = pack.Packet()
                packet.chat.name = self.username
                packet.chat.message = input()
                self.send_packet(packet)
            except Exception as e:
                print("Error receiving data")
                print(e)
                self.stop()

    def login(self):
        login_packet = pack.Packet()
        login_packet.login.username = self.username
        login_packet.login.password = "password123"
        self.send_packet(login_packet)

    def stop(self):
        self.running = False
        self.sock.close()
        print("TCP client is stopping...")
        self.write_thread.join()
        self.read_thread.join()
