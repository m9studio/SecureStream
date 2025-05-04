using System.Collections.Concurrent;

namespace M9Studio.SecureStream
{
    public class SecureChannelManager<TAddress>
    {
        private readonly ISecureTransportAdapter<TAddress> _adapter;
        private readonly ConcurrentDictionary<TAddress, SecureSession<TAddress>> _sessions = new();

        public event Action<SecureSession<TAddress>>? OnSecureSessionEstablished;

        public SecureChannelManager(ISecureTransportAdapter<TAddress> adapter)
        {
            _adapter = adapter;
            _adapter.OnConnected += HandleConnection;
            _adapter.OnDisconnected += address => _sessions.TryRemove(address, out _);
        }

        private void HandleConnection(TAddress address)
        {
            if (_sessions.ContainsKey(address))
            {
                Console.WriteLine($"[SecureChannelManager] Session with {address} already exists. Ignoring duplicate.");
                return;
            }
            Task.Run(() =>
            {
                try
                {
                    byte[] firstPacket = _adapter.ReceiveFrom(address);

                    if (IsX25519PublicKey(firstPacket))
                    {
                        var session = new SecureSession<TAddress>(_adapter, address);
                        _sessions[address] = session;

                        session.PerformHandshakeAsServer(firstPacket);

                        OnSecureSessionEstablished?.Invoke(session);
                    }
                    else
                    {
                        // Лог: некорректный формат handshake
                    }
                }
                catch (Exception ex)
                {
                    // Лог ошибок (опционально)
                }
            });
        }

        public SecureSession<TAddress> Connect(TAddress address)
        {
            Console.WriteLine($"[SecureChannelManager] Connecting to {address}...");

            var session = new SecureSession<TAddress>(_adapter, address);
            _sessions[address] = session;

            session.PerformHandshakeAsClient();

            Console.WriteLine($"[SecureChannelManager] Handshake complete with {address}");

            OnSecureSessionEstablished?.Invoke(session);

            return session;
        }

        private bool IsX25519PublicKey(byte[] data)
        {
            return data != null && data.Length == 32; // X25519 pubkey = 32 bytes
        }
    }
}
