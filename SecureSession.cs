namespace M9Studio.SecureStream
{
    public class SecureSession<TAddress>
    {
        private readonly ISecureTransportAdapter<TAddress> _adapter;
        private readonly TAddress _address;
        private byte[]? _initialBuffer;

        public SecureSession(ISecureTransportAdapter<TAddress> adapter, TAddress address, byte[]? initialBuffer = null)
        {
            _adapter = adapter;
            _address = address;
            _initialBuffer = initialBuffer;
        }

        public bool Send(byte[] data)
        {
            var encrypted = Encrypt(data);
            return _adapter.SendTo(encrypted, _address);
        }

        public byte[] Receive()
        {
            if (_initialBuffer != null)
            {
                var data = _initialBuffer;
                _initialBuffer = null;
                return Decrypt(data);
            }

            var encrypted = _adapter.ReceiveFrom(_address);
            return Decrypt(encrypted);
        }

        private byte[] Encrypt(byte[] data) => data;
        private byte[] Decrypt(byte[] data) => data;
    }
}
