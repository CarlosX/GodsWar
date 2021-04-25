

using System;
using System.Security.Cryptography;

namespace Framework.Cryptography
{
    public sealed class LoginCrypt : IDisposable
    {
        public void Initialize()
        {
            if (IsInitialized)
                throw new InvalidOperationException("PacketCrypt already initialized!");

            _serverEncrypt = new HashCrypto();
            _clientDecrypt = new HashCrypto();

            IsInitialized = true;
        }

        public bool Encrypt(ref byte[] data)
        {
            try
            {
                
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public bool Decrypt(ref byte[] data)
        {
            try
            {
                data = _clientDecrypt.Crypt(data);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public void Dispose()
        {
            IsInitialized = false;
        }

        public bool IsInitialized { get; set; }

        HashCrypto _serverEncrypt;
        HashCrypto _clientDecrypt;
        ulong _clientCounter;
        ulong _serverCounter;
    }
}
