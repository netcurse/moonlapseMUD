import view
import sys
import widgets
import packets_pb2 as pack
import tcp
import threading
from blessed import Terminal
from typing import List, Callable, Optional
import services

def compare_input(user_input, key):
    if user_input.is_sequence:
        return user_input.code == key
    return user_input == key

class Controller:
    def __init__(self):
        self.term = Terminal()
        self.view = view.View(self)
        self.running = False
        self.shared_network_receiver = services.ServiceLocator.get('shared_network_receiver')
        self.tcp_client = services.ServiceLocator.get('tcp_client')

    def get_input(self):
        with self.term.cbreak():
            return self.term.inkey()

    def handle_input(self, user_input):
        pass

    def handle_packet(self, packet: pack.Packet):
        pass
        
    def run(self):
        self.running = True

        self.view.draw()

        # Main loop
        while self.running:
            packet = self.shared_network_receiver.get_packet()
            if packet is not None:
                self.handle_packet(packet)
            self.handle_input(self.get_input())
            self.view.draw()


class MenuController(Controller):
    def __init__(self):
        super().__init__()
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
        

class MainMenuController(MenuController):
    def __init__(self):
        super().__init__()
        self.view = view.MainMenuView(self)
        self.widgets.append(widgets.Button(self, 'Login', self.login))
        self.widgets.append(widgets.Button(self, 'Register', self.register))
        self.widgets.append(widgets.Button(self, 'Exit', sys.exit))

    def login(self):
        LoginMenuController().run()

    def register(self):
        RegisterMenuController().run()

class UserMenuController(MenuController):
    def __init__(self):
        super().__init__()
        self.view = view.UserMenuView(self)
        self.username_widget = widgets.TextField(self, 'Username')
        self.password_widget = widgets.TextField(self, 'Password', censored=True)
        self.widgets.append(self.username_widget)
        self.widgets.append(self.password_widget)

class LoginMenuController(UserMenuController):
    def __init__(self):
        super().__init__()
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
            ChatroomController().run()
        elif packet.HasField("deny"):
            print(packet.deny.reason)
            self.password_widget.clear()

    def go_back(self):
        MainMenuController().run()


class RegisterMenuController(UserMenuController):
    def __init__(self):
        super().__init__()
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
        MainMenuController().run()


class ChatroomController(MenuController):
    def __init__(self):
        super().__init__()
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

    def send_message(self):
        chat_packet = pack.Packet()
        chat_packet.chat.message = self.chat_entry_widget.text
        self.tcp_client.send_packet(chat_packet)
        self.chat_entry_widget.clear()

    def exit_chatroom(self):
        MainMenuController().run()
