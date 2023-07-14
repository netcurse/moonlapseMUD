class ServiceLocator:
    _services = {}

    @staticmethod
    def register(service_name, instance):
        ServiceLocator._services[service_name] = instance

    @staticmethod
    def get(service_name):
        return ServiceLocator._services[service_name]
