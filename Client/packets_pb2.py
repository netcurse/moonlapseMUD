# -*- coding: utf-8 -*-
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: packets.proto
"""Generated protocol buffer code."""
from google.protobuf import descriptor as _descriptor
from google.protobuf import descriptor_pool as _descriptor_pool
from google.protobuf import symbol_database as _symbol_database
from google.protobuf.internal import builder as _builder
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()


from google.protobuf import descriptor_pb2 as google_dot_protobuf_dot_descriptor__pb2


DESCRIPTOR = _descriptor_pool.Default().AddSerializedFile(b'\n\rpackets.proto\x12\x07packets\x1a google/protobuf/descriptor.proto\"\x1b\n\x08OkPacket\x12\x0f\n\x07message\x18\x01 \x01(\t\"\x1c\n\nDenyPacket\x12\x0e\n\x06reason\x18\x01 \x01(\t\"1\n\x0bLoginPacket\x12\x10\n\x08username\x18\x01 \x01(\t\x12\x10\n\x08password\x18\x02 \x01(\t\"4\n\x0eRegisterPacket\x12\x10\n\x08username\x18\x01 \x01(\t\x12\x10\n\x08password\x18\x02 \x01(\t\"+\n\nChatPacket\x12\x0c\n\x04name\x18\x01 \x01(\t\x12\x0f\n\x07message\x18\x02 \x01(\t\"!\n\x12PublicRSAKeyPacket\x12\x0b\n\x03key\x18\x01 \x01(\x0c\"\x1b\n\x0c\x41\x45SKeyPacket\x12\x0b\n\x03key\x18\x01 \x01(\x0c\"\xda\x02\n\x06Packet\x12%\n\x02ok\x18\x01 \x01(\x0b\x32\x11.packets.OkPacketB\x04\xf8\xf0\x04\x00H\x00\x12)\n\x04\x64\x65ny\x18\x02 \x01(\x0b\x32\x13.packets.DenyPacketB\x04\xf8\xf0\x04\x00H\x00\x12+\n\x05login\x18\x03 \x01(\x0b\x32\x14.packets.LoginPacketB\x04\xf8\xf0\x04\x01H\x00\x12\x31\n\x08register\x18\x04 \x01(\x0b\x32\x17.packets.RegisterPacketB\x04\xf8\xf0\x04\x01H\x00\x12)\n\x04\x63hat\x18\x05 \x01(\x0b\x32\x13.packets.ChatPacketB\x04\xf8\xf0\x04\x00H\x00\x12;\n\x0epublic_rsa_key\x18\x06 \x01(\x0b\x32\x1b.packets.PublicRSAKeyPacketB\x04\xf8\xf0\x04\x00H\x00\x12.\n\x07\x61\x65s_key\x18\x07 \x01(\x0b\x32\x15.packets.AESKeyPacketB\x04\xf8\xf0\x04\x01H\x00\x42\x06\n\x04type:1\n\tencrypted\x12\x1d.google.protobuf.FieldOptions\x18\x8fN \x01(\x08\x42\x1b\xaa\x02\x18Moonlapse.Server.Packetsb\x06proto3')

_globals = globals()
_builder.BuildMessageAndEnumDescriptors(DESCRIPTOR, _globals)
_builder.BuildTopDescriptorsAndMessages(DESCRIPTOR, 'packets_pb2', _globals)
if _descriptor._USE_C_DESCRIPTORS == False:
  google_dot_protobuf_dot_descriptor__pb2.FieldOptions.RegisterExtension(encrypted)

  DESCRIPTOR._options = None
  DESCRIPTOR._serialized_options = b'\252\002\030Moonlapse.Server.Packets'
  _PACKET.fields_by_name['ok']._options = None
  _PACKET.fields_by_name['ok']._serialized_options = b'\370\360\004\000'
  _PACKET.fields_by_name['deny']._options = None
  _PACKET.fields_by_name['deny']._serialized_options = b'\370\360\004\000'
  _PACKET.fields_by_name['login']._options = None
  _PACKET.fields_by_name['login']._serialized_options = b'\370\360\004\001'
  _PACKET.fields_by_name['register']._options = None
  _PACKET.fields_by_name['register']._serialized_options = b'\370\360\004\001'
  _PACKET.fields_by_name['chat']._options = None
  _PACKET.fields_by_name['chat']._serialized_options = b'\370\360\004\000'
  _PACKET.fields_by_name['public_rsa_key']._options = None
  _PACKET.fields_by_name['public_rsa_key']._serialized_options = b'\370\360\004\000'
  _PACKET.fields_by_name['aes_key']._options = None
  _PACKET.fields_by_name['aes_key']._serialized_options = b'\370\360\004\001'
  _globals['_OKPACKET']._serialized_start=60
  _globals['_OKPACKET']._serialized_end=87
  _globals['_DENYPACKET']._serialized_start=89
  _globals['_DENYPACKET']._serialized_end=117
  _globals['_LOGINPACKET']._serialized_start=119
  _globals['_LOGINPACKET']._serialized_end=168
  _globals['_REGISTERPACKET']._serialized_start=170
  _globals['_REGISTERPACKET']._serialized_end=222
  _globals['_CHATPACKET']._serialized_start=224
  _globals['_CHATPACKET']._serialized_end=267
  _globals['_PUBLICRSAKEYPACKET']._serialized_start=269
  _globals['_PUBLICRSAKEYPACKET']._serialized_end=302
  _globals['_AESKEYPACKET']._serialized_start=304
  _globals['_AESKEYPACKET']._serialized_end=331
  _globals['_PACKET']._serialized_start=334
  _globals['_PACKET']._serialized_end=680
# @@protoc_insertion_point(module_scope)
