using System.Collections.Concurrent;
using System.Net;

namespace M9Studio.ShieldSocket
{
    public class SecureChannelManager
    {
        private readonly ISecureTransportAdapter _adapter;
        private readonly ConcurrentDictionary<EndPoint, SecureSession> _sessions = new();
        private readonly TimeSpan _handshakeTimeout = TimeSpan.FromSeconds(10);

        public SecureChannelManager(ISecureTransportAdapter adapter)
        {
            _adapter = adapter;
            _adapter.OnConnected += StartHandshake;
            _adapter.OnDisconnected += Disconnect;
        }

        private void StartHandshake(EndPoint remoteEP)
        {
            Task.Run(() =>
            {
                var cts = new CancellationTokenSource(_handshakeTimeout);
                try
                {
                    // ожидание первого рукопожатного пакета
                    byte[] handshakeMessage = _adapter.ReceiveFrom(remoteEP);
                    if (handshakeMessage == null || handshakeMessage.Length == 0)
                    {
                        Console.WriteLine($"[{remoteEP}] Empty handshake packet.");
                        return;
                    }

                    // TODO: обработка handshakeMessage и создание SecureSession
                    var session = new SecureSession(remoteEP); // Заглушка
                    _sessions[remoteEP] = session;
                    Console.WriteLine($"[{remoteEP}] Secure session established.");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"[{remoteEP}] Handshake timed out.");
                    // Прекращаем соединение
                    Disconnect(remoteEP);
                }
            });
        }

        private void Disconnect(EndPoint remoteEP)
        {
            _sessions.TryRemove(remoteEP, out _);
            Console.WriteLine($"[{remoteEP}] Disconnected.");
        }

        public void Send(byte[] data, EndPoint remoteEP)
        {
            if (_sessions.TryGetValue(remoteEP, out var session))
            {
                byte[] encrypted = session.Encrypt(data);
                _adapter.SendTo(encrypted, remoteEP);
            }
            else
            {
                Console.WriteLine($"[{remoteEP}] No session found for sending.");
            }
        }

        public byte[] Receive(EndPoint remoteEP)
        {
            byte[] encrypted = _adapter.ReceiveFrom(remoteEP);
            if (_sessions.TryGetValue(remoteEP, out var session))
            {
                return session.Decrypt(encrypted);
            }

            Console.WriteLine($"[{remoteEP}] No session found for receiving.");
            return null;
        }
    }
}
