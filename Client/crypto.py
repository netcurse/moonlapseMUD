from Crypto.Cipher import AES, PKCS1_OAEP
from Crypto.PublicKey import RSA
from Crypto.Util.Padding import pad, unpad
from Crypto.Random import get_random_bytes
from typing import Tuple

def gen_aes_key() -> bytes:
    return get_random_bytes(16)

def aes_encrypt(plaintext: bytes, key: bytes) -> Tuple[bytes]:
    cipher = AES.new(key, AES.MODE_CBC)
    ciphertext = cipher.encrypt(pad(plaintext, AES.block_size))
    return cipher.iv + ciphertext

def aes_decrypt(data: bytes, key: bytes) -> bytes:
    iv, ciphertext = data[:16], data[16:]
    cipher = AES.new(key, AES.MODE_CBC, iv)
    plaintext = unpad(cipher.decrypt(ciphertext), AES.block_size)
    return plaintext

def rsa_encrypt(data: bytes, public_key: bytes):
    pubkey = RSA.import_key(public_key)
    cipher = PKCS1_OAEP.new(pubkey)
    return cipher.encrypt(data)

