using System.Net;

namespace M9Studio.ShieldSocket
{
    public class SecureSession
    {
        private readonly ISecureTransportAdapter _adapter;
        private readonly EndPoint _remoteEP;

        public SecureSession(ISecureTransportAdapter adapter, EndPoint remoteEP)
        {
            _adapter = adapter;
            _remoteEP = remoteEP;
        }

        public bool Send(byte[] plainData)
        {
            byte[] encrypted = Encrypt(plainData);
            return _adapter.SendTo(encrypted, _remoteEP);
        }

        public byte[] Receive()
        {
            byte[] encrypted = _adapter.ReceiveFrom(_remoteEP);
            return Decrypt(encrypted);
        }

        private byte[] Encrypt(byte[] data) => data;  // Заглушка
        private byte[] Decrypt(byte[] data) => data;  // Заглушка
    }

}
