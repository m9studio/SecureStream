using System.Collections.Concurrent;
using System.Net;

namespace M9Studio.SecureStream
{
    public class SecureChannelManager<TAddress>
    {
        private readonly ISecureTransportAdapter<TAddress> _adapter;
        private readonly ConcurrentDictionary<TAddress, SecureSession<TAddress>> _sessions = new();

        public event Action<SecureSession<TAddress>> OnSecureSessionEstablished;

        public SecureChannelManager(ISecureTransportAdapter<TAddress> adapter)
        {
            _adapter = adapter;
            _adapter.OnConnected += HandleConnection;
            _adapter.OnDisconnected += address => _sessions.TryRemove(address, out _);
        }

        private void HandleConnection(TAddress address)
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] handshakeData = _adapter.ReceiveFrom(address);

                    // TODO: обработка рукопожатия
                    var session = new SecureSession<TAddress>(_adapter, address);
                    _sessions[address] = session;
                    OnSecureSessionEstablished?.Invoke(session);
                }
                catch
                {
                    Console.WriteLine($"[!] Handshake failed with {address}");
                }
            });
        }

        public SecureSession<TAddress> Connect(TAddress address)
        {
            // TODO: отправка handshake
            var session = new SecureSession<TAddress>(_adapter, address);
            _sessions[address] = session;
            return session;
        }
    }
}
