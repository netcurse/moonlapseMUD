import view
import sys
import widgets
import packets_pb2 as pack
import tcp
import threading
from blessed import Terminal
from typing import List, Callable, Optional
from queue import Queue

def compare_input(user_input, key):
    if user_input.is_sequence:
        return user_input.code == key
    return user_input == key

class Controller:
    def __init__(self, tcp_client: tcp.Client):
        self.term = Terminal()
        self.view = view.View(self)
        self.tcp_client = tcp_client
        self.running = False
        self._network_receive_stop_event = threading.Event()
        self._network_receive_thread = threading.Thread(target=self.get_network_data, daemon=True)
        self.inbound_packets: Queue[pack.Packet] = Queue()

    def get_input(self):
        with self.term.cbreak():
            return self.term.inkey(timeout=0.1)
        
    def get_network_data(self):
        while not self._network_receive_stop_event.is_set():
            if self.tcp_client.data_available():  # You'd need to implement this method
                p = self.tcp_client.receive_packet()
                if p is not None:
                    self.inbound_packets.put(p)
            self._network_receive_stop_event.wait(timeout=0.1)  # Adjust the timeout as needed

    def handle_input(self, user_input):
        pass

    def handle_packet(self, packet: pack.Packet):
        pass
        
    def run(self):
        with self.term.cbreak(), self.term.hidden_cursor():
            self.running = True
            self._network_receive_thread.start()
            self.view.draw()
            # Main loop
            while self.running:
                if not self.inbound_packets.empty():
                    self.handle_packet(self.inbound_packets.get())
                self.handle_input(self.get_input())
                self.view.redraw_if_dirty()
        
            self._network_receive_thread.join()

    def stop(self):
        self.running = False
        self._network_receive_stop_event.set()
        self._network_receive_thread.join()

    def switch_controller(self, controller):
        self.stop()
        controller.run()


class MenuController(Controller):
    def __init__(self, tcp_client):
        super().__init__(tcp_client)
        self.view = view.MenuView(self)
        self.widgets: List[widgets.Widget] = []
        self.cursor_idx = 0

    def handle_input(self, user_input):
        super().handle_input(user_input)
        for widget in self.widgets:
            if widget.selected:
                widget.handle_input(user_input)


        # Down or Tab to move the cursor down
        if compare_input(user_input, self.term.KEY_DOWN) or compare_input(user_input, self.term.KEY_TAB):
            self.cursor_idx = (self.cursor_idx + 1) % len(self.widgets)
        
        
        # Up or Shift+Tab to move the cursor up
        elif compare_input(user_input, self.term.KEY_UP) or compare_input(user_input, self.term.KEY_BTAB):
            self.cursor_idx = (self.cursor_idx - 1) % len(self.widgets)

        # Enter to activate the selected widget
        elif compare_input(user_input, self.term.KEY_ENTER):
            self.widgets[self.cursor_idx].activate()

        # Ensure only one widget is selected at a time
        for i, widget in enumerate(self.widgets):
            widget.selected = i == self.cursor_idx

        if user_input:
            self.view.dirty = True
        

class MainMenuController(MenuController):
    def __init__(self, tcp_client):
        super().__init__(tcp_client)
        self.view = view.MainMenuView(self)
        self.widgets.append(widgets.Button(self, 'Login', self.login))
        self.widgets.append(widgets.Button(self, 'Register', self.register))
        self.widgets.append(widgets.Button(self, 'Exit', sys.exit))

    def login(self):
        self.switch_controller(LoginMenuController(self.tcp_client))

    def register(self):
        self.switch_controller(RegisterMenuController(self.tcp_client))

class UserMenuController(MenuController):
    def __init__(self, tcp_client):
        super().__init__(tcp_client)
        self.view = view.UserMenuView(self)
        self.username_widget = widgets.TextField(self, 'Username')
        self.password_widget = widgets.TextField(self, 'Password', censored=True)
        self.widgets.append(self.username_widget)
        self.widgets.append(self.password_widget)

class LoginMenuController(UserMenuController):
    def __init__(self, tcp_client):
        super().__init__(tcp_client)
        self.view = view.LoginMenuView(self)
        self.widgets.append(widgets.CheckBox(self, 'Remember me'))
        self.widgets.append(widgets.Button(self, 'Login', self.login))
        self.widgets.append(widgets.Button(self, 'Go back', self.go_back))
    
    def login(self):
        username = self.username_widget.text
        password = self.password_widget.text
        login_packet = pack.Packet()
        login_packet.login.username = username
        login_packet.login.password = password
        self.tcp_client.send_packet(login_packet)


    def handle_packet(self, packet: pack.Packet):
        super().handle_packet(packet)
        if packet.HasField("ok"):
            print(packet.ok.message)
            self.switch_controller(ChatroomController(self.tcp_client))
        elif packet.HasField("deny"):
            print(packet.deny.reason)
            self.password_widget.clear()
        self.view.dirty = True

    def go_back(self):
        self.switch_controller(MainMenuController(self.tcp_client))


class RegisterMenuController(UserMenuController):
    def __init__(self, tcp_client):
        super().__init__(tcp_client)
        self.view = view.RegisterMenuView(self)
        self.widgets.append(widgets.Button(self, 'Register', self.register))
        self.widgets.append(widgets.Button(self, 'Go back', self.go_back))
    
    def register(self):
        username = self.username_widget.text
        password = self.password_widget.text
        register_packet = pack.Packet()
        register_packet.register.username = username
        register_packet.register.password = password
        self.tcp_client.send_packet(register_packet)


    def go_back(self):
        self.switch_controller(MainMenuController(self.tcp_client))


class ChatroomController(MenuController):
    def __init__(self, tcp_client):
        super().__init__(tcp_client)
        self.view = view.ChatroomView(self)
        self.chat_log = []
        self.chat_entry_widget = widgets.TextField(self, 'Type your message here')
        self.widgets.append(self.chat_entry_widget)
        self.widgets.append(widgets.Button(self, 'Send', self.send_message))
        self.widgets.append(widgets.Button(self, 'Exit', self.exit_chatroom))

    def handle_packet(self, packet: pack.Packet):
        super().handle_packet(packet)
        if packet.HasField("chat"):
            self.chat_log.append(packet.chat.message)
        self.view.dirty = True

    def send_message(self):
        chat_packet = pack.Packet()
        chat_packet.chat.message = self.chat_entry_widget.text
        self.tcp_client.send_packet(chat_packet)
        self.chat_entry_widget.clear()

    def exit_chatroom(self):
        self.switch_controller(MainMenuController(self.tcp_client))
