# -*- coding: utf-8 -*-
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: packets.proto
"""Generated protocol buffer code."""
from google.protobuf.internal import builder as _builder
from google.protobuf import descriptor as _descriptor
from google.protobuf import descriptor_pool as _descriptor_pool
from google.protobuf import symbol_database as _symbol_database
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()




DESCRIPTOR = _descriptor_pool.Default().AddSerializedFile(b'\n\rpackets.proto\x12\x07packets\"1\n\x0bLoginPacket\x12\x10\n\x08username\x18\x01 \x01(\t\x12\x10\n\x08password\x18\x02 \x01(\t\"4\n\x0eRegisterPacket\x12\x10\n\x08username\x18\x01 \x01(\t\x12\x10\n\x08password\x18\x02 \x01(\t\"+\n\nChatPacket\x12\x0c\n\x04name\x18\x01 \x01(\t\x12\x0f\n\x07message\x18\x02 \x01(\t\"\x89\x01\n\x06Packet\x12%\n\x05login\x18\x01 \x01(\x0b\x32\x14.packets.LoginPacketH\x00\x12+\n\x08register\x18\x02 \x01(\x0b\x32\x17.packets.RegisterPacketH\x00\x12#\n\x04\x63hat\x18\x03 \x01(\x0b\x32\x13.packets.ChatPacketH\x00\x42\x06\n\x04typeB\x1b\xaa\x02\x18Moonlapse.Server.Packetsb\x06proto3')

_builder.BuildMessageAndEnumDescriptors(DESCRIPTOR, globals())
_builder.BuildTopDescriptorsAndMessages(DESCRIPTOR, 'packets_pb2', globals())
if _descriptor._USE_C_DESCRIPTORS == False:

  DESCRIPTOR._options = None
  DESCRIPTOR._serialized_options = b'\252\002\030Moonlapse.Server.Packets'
  _LOGINPACKET._serialized_start=26
  _LOGINPACKET._serialized_end=75
  _REGISTERPACKET._serialized_start=77
  _REGISTERPACKET._serialized_end=129
  _CHATPACKET._serialized_start=131
  _CHATPACKET._serialized_end=174
  _PACKET._serialized_start=177
  _PACKET._serialized_end=314
# @@protoc_insertion_point(module_scope)
