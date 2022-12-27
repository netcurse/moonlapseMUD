import tcp

if __name__ == '__main__':
    client = tcp.Client('localhost', 42523)
    client.start()