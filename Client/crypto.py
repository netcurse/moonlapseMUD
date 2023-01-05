from Crypto.Cipher import AES, PKCS1_OAEP
from Crypto.PublicKey import RSA
from Crypto.Util.Padding import pad, unpad
from Crypto.Random import get_random_bytes
from typing import Tuple

class CryptoContext:
    def __init__(self):
        self._client_aes_private_key = get_random_bytes(16)
        self._server_rsa_public_key = None

    def aes_encrypt(self, plaintext: bytes) -> Tuple[bytes]:
        cipher = AES.new(self._client_aes_private_key, AES.MODE_CBC)
        ciphertext = cipher.encrypt(pad(plaintext, AES.block_size))
        return cipher.iv + ciphertext

    def aes_decrypt(self, data: bytes) -> bytes:
        iv, ciphertext = data[:16], data[16:]
        cipher = AES.new(self._client_aes_private_key, AES.MODE_CBC, iv)
        plaintext = unpad(cipher.decrypt(ciphertext), AES.block_size)
        return plaintext

    def set_server_rsa_public_key(self, key: bytes):
        self._server_rsa_public_key = key

    def rsa_encrypt(self, data: bytes):
        if not self._server_rsa_public_key:
            raise Exception("Server public key not set")

        pubkey = RSA.import_key(self._server_rsa_public_key)
        cipher = PKCS1_OAEP.new(pubkey)
        return cipher.encrypt(data)

    def get_client_aes_private_key(self) -> bytes:
        """Warning: Only access this function if you know what you're doing!"""
        return self._client_aes_private_key

