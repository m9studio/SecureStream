using System.Collections.Concurrent;
using System.Net;

namespace M9Studio.ShieldSocket
{
    public class SecureChannelManager
    {
        private readonly ISecureTransportAdapter _adapter;
        private readonly ConcurrentDictionary<EndPoint, SecureSession> _sessions = new();

        public event Action<SecureSession> OnSecureSessionEstablished;

        public SecureChannelManager(ISecureTransportAdapter adapter)
        {
            _adapter = adapter;
            _adapter.OnConnected += HandleIncomingConnection;
            _adapter.OnDisconnected += remoteEP => _sessions.TryRemove(remoteEP, out _);
        }

        private void HandleIncomingConnection(EndPoint remoteEP)
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] handshakeMessage = _adapter.ReceiveFrom(remoteEP);

                    // TODO: обработка рукопожатия
                    var session = new SecureSession(_adapter, remoteEP);
                    _sessions[remoteEP] = session;

                    OnSecureSessionEstablished?.Invoke(session);
                }
                catch
                {
                    Console.WriteLine($"Handshake failed with {remoteEP}");
                }
            });
        }

        public SecureSession Connect(EndPoint remoteEP)
        {
            // TODO: отправка и получение handshake
            var session = new SecureSession(_adapter, remoteEP);
            _sessions[remoteEP] = session;

            return session;
        }
    }
}
