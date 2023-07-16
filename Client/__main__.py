import controller
import tcp

if __name__ == '__main__':
    tcp_client = tcp.Client('localhost', 42523)
    tcp_client.start()
    controller.MainMenuController(tcp_client).run()
