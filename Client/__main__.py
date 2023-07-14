import controller
import tcp
import services

if __name__ == '__main__':
    tcp_client = tcp.Client('localhost', 42523)
    shared_network_receiver = tcp.SharedNetworkReceiver(tcp_client)
    services.ServiceLocator.register('tcp_client', tcp_client)
    services.ServiceLocator.register('shared_network_receiver', shared_network_receiver)

    tcp_client.start()
    shared_network_receiver = tcp.SharedNetworkReceiver(tcp_client)
    shared_network_receiver.start()
    controller.MainMenuController().run()
