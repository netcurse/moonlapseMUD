from Client.Packets import packets_pb2 as pack

class PacketConfig:
    
    flags_cache: dict = {}  # Memoization cache for the `has_flag` method

    def __init__(self, rsa_encrypted: bool = False, aes_encrypted: bool = False, reserved1: bool = False, reserved2: bool = False, reserved3: bool = False, reserved4: bool = False, reserved5: bool = False, reserved6: bool = False):
        self.rsa_encrypted = rsa_encrypted
        self.aes_encrypted = aes_encrypted
        self.reserved1 = reserved1
        self.reserved2 = reserved2
        self.reserved3 = reserved3
        self.reserved4 = reserved4
        self.reserved5 = reserved5
        self.reserved6 = reserved6

    def to_byte(self) -> int:
        return self.rsa_encrypted << 7 | self.aes_encrypted << 6 | self.reserved1 << 5 | self.reserved2 << 4 | self.reserved3 << 3 | self.reserved4 << 2 | self.reserved5 << 1 | self.reserved6

    @staticmethod
    def from_byte(byte: int) -> 'PacketConfig':
        config = PacketConfig()
        config.rsa_encrypted = (byte & 0b10000000) != 0
        config.aes_encrypted = (byte & 0b01000000) != 0
        config.reserved1 = (byte & 0b00100000) != 0
        config.reserved2 = (byte & 0b00010000) != 0
        config.reserved3 = (byte & 0b00001000) != 0
        config.reserved4 = (byte & 0b00000100) != 0
        config.reserved5 = (byte & 0b00000010) != 0
        config.reserved6 = (byte & 0b00000001) != 0
        
        return config

    @staticmethod
    def has_flag(packet: pack.Packet, flag) -> bool:
        """
        Returns true only if the given packet has the given flag set to true. 
        Example usage (where `pack` is the `packets_pb2` module):
        ```python
        if PacketConfig.has_flag(packet, pack.flag_name): ...
        ```
        
        The flag to check is accessible with `pack.flag_name`.
        """
        packet_type_name: str = packet.WhichOneof('type')

        # Look this packet up in the cache and check if the flag is already set
        if packet_type_name in PacketConfig.flags_cache:
            cachedFlags: dict = PacketConfig.flags_cache[packet_type_name]
            if flag in cachedFlags:
                return cachedFlags[flag]
        else:
            PacketConfig.flags_cache[packet_type_name] = {}
        
        # If the flag is not cached, manually look it up in the packet descriptor and update the cache
        flag_value: bool = False
        packet_types = pack.Packet.DESCRIPTOR.oneofs_by_name['type'].fields
        for packet_type in packet_types:
            if packet_type.name == packet_type_name:
                flag_value = packet_type.GetOptions().Extensions[flag]
                break
        
        PacketConfig.flags_cache[packet_type_name][flag] = flag_value
        return flag_value