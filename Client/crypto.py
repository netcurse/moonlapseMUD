from Crypto.Cipher import AES, PKCS1_OAEP
from Crypto.PublicKey import RSA
from Crypto.Util.Padding import pad, unpad
from Crypto.Random import get_random_bytes
from typing import Tuple

class CryptoContext:
    _BLOCK_SIZE = 16

    def __init__(self):
        self._client_aes_private_key = get_random_bytes(self._BLOCK_SIZE)
        self._server_rsa_public_key = None

    def aes_encrypt(self, plaintext: bytes) -> bytes:
        """Encrypts plaintext using AES-CBC encryption and returns the ciphertext."""
        iv = get_random_bytes(self._BLOCK_SIZE)
        cipher = AES.new(self._client_aes_private_key, AES.MODE_CBC, iv)
        ciphertext = cipher.encrypt(pad(plaintext, self._BLOCK_SIZE))
        return iv + ciphertext

    def aes_decrypt(self, ciphertext: bytes) -> bytes:
        """Decrypts ciphertext using AES-CBC decryption and returns the plaintext."""
        iv, ciphertext = ciphertext[:self._BLOCK_SIZE], ciphertext[self._BLOCK_SIZE:]
        cipher = AES.new(self._client_aes_private_key, AES.MODE_CBC, iv)
        plaintext = unpad(cipher.decrypt(ciphertext), AES.block_size)
        return plaintext

    def set_server_rsa_public_key(self, key: bytes):
        """Sets the server's RSA public key."""
        self._server_rsa_public_key = key

    def rsa_encrypt(self, plaintext: bytes) -> bytes:
        """Encrypts plaintext using RSA-OAEP encryption and returns the ciphertext."""
        if not self._server_rsa_public_key:
            raise Exception("Server public key not set")

        pubkey = RSA.import_key(self._server_rsa_public_key)
        cipher = PKCS1_OAEP.new(pubkey)
        return cipher.encrypt(plaintext)

    def get_client_aes_private_key(self) -> bytes:
        """Returns the client's AES private key."""
        return self._client_aes_private_key
