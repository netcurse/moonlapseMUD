"""
@generated by mypy-protobuf.  Do not edit manually!
isort:skip_file
Run the following command in the Shared directory to re-generate the .py and .cs file
protoc -I="." --python_out="../Client" --mypy_out="../Client" --csharp_out="../Server/Packets" "./packets.proto"
"""
import builtins
import google.protobuf.descriptor
import google.protobuf.descriptor_pb2
import google.protobuf.internal.extension_dict
import google.protobuf.message
import sys

if sys.version_info >= (3, 8):
    import typing as typing_extensions
else:
    import typing_extensions

DESCRIPTOR: google.protobuf.descriptor.FileDescriptor

@typing_extensions.final
class LoginPacket(google.protobuf.message.Message):
    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    USERNAME_FIELD_NUMBER: builtins.int
    PASSWORD_FIELD_NUMBER: builtins.int
    username: builtins.str
    password: builtins.str
    def __init__(
        self,
        *,
        username: builtins.str = ...,
        password: builtins.str = ...,
    ) -> None: ...
    def ClearField(self, field_name: typing_extensions.Literal["password", b"password", "username", b"username"]) -> None: ...

global___LoginPacket = LoginPacket

@typing_extensions.final
class RegisterPacket(google.protobuf.message.Message):
    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    USERNAME_FIELD_NUMBER: builtins.int
    PASSWORD_FIELD_NUMBER: builtins.int
    username: builtins.str
    password: builtins.str
    def __init__(
        self,
        *,
        username: builtins.str = ...,
        password: builtins.str = ...,
    ) -> None: ...
    def ClearField(self, field_name: typing_extensions.Literal["password", b"password", "username", b"username"]) -> None: ...

global___RegisterPacket = RegisterPacket

@typing_extensions.final
class ChatPacket(google.protobuf.message.Message):
    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    NAME_FIELD_NUMBER: builtins.int
    MESSAGE_FIELD_NUMBER: builtins.int
    name: builtins.str
    message: builtins.str
    def __init__(
        self,
        *,
        name: builtins.str = ...,
        message: builtins.str = ...,
    ) -> None: ...
    def ClearField(self, field_name: typing_extensions.Literal["message", b"message", "name", b"name"]) -> None: ...

global___ChatPacket = ChatPacket

@typing_extensions.final
class PublicRSAKeyPacket(google.protobuf.message.Message):
    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    KEY_FIELD_NUMBER: builtins.int
    key: builtins.bytes
    def __init__(
        self,
        *,
        key: builtins.bytes = ...,
    ) -> None: ...
    def ClearField(self, field_name: typing_extensions.Literal["key", b"key"]) -> None: ...

global___PublicRSAKeyPacket = PublicRSAKeyPacket

@typing_extensions.final
class AESKeyPacket(google.protobuf.message.Message):
    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    KEY_FIELD_NUMBER: builtins.int
    key: builtins.bytes
    def __init__(
        self,
        *,
        key: builtins.bytes = ...,
    ) -> None: ...
    def ClearField(self, field_name: typing_extensions.Literal["key", b"key"]) -> None: ...

global___AESKeyPacket = AESKeyPacket

@typing_extensions.final
class Packet(google.protobuf.message.Message):
    DESCRIPTOR: google.protobuf.descriptor.Descriptor

    LOGIN_FIELD_NUMBER: builtins.int
    REGISTER_FIELD_NUMBER: builtins.int
    CHAT_FIELD_NUMBER: builtins.int
    PUBLIC_RSA_KEY_FIELD_NUMBER: builtins.int
    AES_KEY_FIELD_NUMBER: builtins.int
    @property
    def login(self) -> global___LoginPacket: ...
    @property
    def register(self) -> global___RegisterPacket: ...
    @property
    def chat(self) -> global___ChatPacket: ...
    @property
    def public_rsa_key(self) -> global___PublicRSAKeyPacket: ...
    @property
    def aes_key(self) -> global___AESKeyPacket: ...
    def __init__(
        self,
        *,
        login: global___LoginPacket | None = ...,
        register: global___RegisterPacket | None = ...,
        chat: global___ChatPacket | None = ...,
        public_rsa_key: global___PublicRSAKeyPacket | None = ...,
        aes_key: global___AESKeyPacket | None = ...,
    ) -> None: ...
    def HasField(self, field_name: typing_extensions.Literal["aes_key", b"aes_key", "chat", b"chat", "login", b"login", "public_rsa_key", b"public_rsa_key", "register", b"register", "type", b"type"]) -> builtins.bool: ...
    def ClearField(self, field_name: typing_extensions.Literal["aes_key", b"aes_key", "chat", b"chat", "login", b"login", "public_rsa_key", b"public_rsa_key", "register", b"register", "type", b"type"]) -> None: ...
    def WhichOneof(self, oneof_group: typing_extensions.Literal["type", b"type"]) -> typing_extensions.Literal["login", "register", "chat", "public_rsa_key", "aes_key"] | None: ...

global___Packet = Packet

ENCRYPTED_FIELD_NUMBER: builtins.int
encrypted: google.protobuf.internal.extension_dict._ExtensionFieldDescriptor[google.protobuf.descriptor_pb2.FieldOptions, builtins.bool]
