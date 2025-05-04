using System.Net;

namespace M9Studio.SecureStream
{
    public class SecureSession<TAddress>
    {
        private readonly ISecureTransportAdapter<TAddress> _adapter;
        private readonly TAddress _address;

        public SecureSession(ISecureTransportAdapter<TAddress> adapter, TAddress address)
        {
            _adapter = adapter;
            _address = address;
        }

        public bool Send(byte[] data)
        {
            var encrypted = Encrypt(data);
            return _adapter.SendTo(encrypted, _address);
        }

        public byte[] Receive()
        {
            var encrypted = _adapter.ReceiveFrom(_address);
            return Decrypt(encrypted);
        }

        private byte[] Encrypt(byte[] data) => data;
        private byte[] Decrypt(byte[] data) => data;
    }


}
