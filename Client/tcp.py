import socket
import threading
import packets_pb2 as pack
from google.protobuf import message as pb

class Client:
    def __init__(self, hostname: str, port: int):
        self.address = hostname, port
        self.running: bool = False
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.read_thread = threading.Thread(target=self.read)
        self.write_thread = threading.Thread(target=self.write)
        self.username: str = input("Please enter your name: ")

    def start(self):
        self.sock.connect(self.address)
        self.running = True
        self.read_thread.start()
        self.write_thread.start()

    def read(self):
        while self.running:
            try:
                data: bytes = self.sock.recv(1500)
                packet = pack.Packet.FromString(data)

                if packet.HasField("chat"):
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
        login_packet = pack.Packet()
        login_packet.login.username = self.username
        login_packet.login.password = "password123"
        login_message = login_packet.SerializeToString()
        self.sock.send(login_message)
        
        while self.running:
            try:
                packet = pack.Packet()
                packet.chat.name = self.username
                packet.chat.message = input()
                message: bytes = packet.SerializeToString()
                self.sock.send(message)
            except Exception as e:
                print("Error receiving data")
                print(e)
                self.stop()

    def stop(self):
        self.running = False
        self.sock.close()
        print("TCP client is stopping...")
        self.write_thread.join()
        self.read_thread.join()
