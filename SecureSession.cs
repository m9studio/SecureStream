using System.Net;

namespace M9Studio.ShieldSocket
{
    internal class SecureSession
    {
        public EndPoint RemoteEndPoint { get; }

        public SecureSession(EndPoint remoteEP)
        {
            RemoteEndPoint = remoteEP;
        }

        public byte[] Encrypt(byte[] data)
        {
            // TODO: шифрование
            return data;
        }

        public byte[] Decrypt(byte[] data)
        {
            // TODO: расшифровка
            return data;
        }
    }
}
