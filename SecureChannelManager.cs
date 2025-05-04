using System.Collections.Concurrent;
using System.Text;

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
                    // Получаем первый пакет от удалённой стороны
                    byte[] firstPacket = _adapter.ReceiveFrom(address);

                    SecureSession<TAddress> session;

                    if (IsHandshakePacket(firstPacket))
                    {
                        // Это HELLO → обычное рукопожатие
                        session = new SecureSession<TAddress>(_adapter, address);
                    }
                    else
                    {
                        // Это уже рабочее сообщение → буферизуем
                        session = new SecureSession<TAddress>(_adapter, address, firstPacket);
                    }

                    _sessions[address] = session;

                    // Только теперь уведомляем внешнюю логику
                    OnSecureSessionEstablished?.Invoke(session);
                }
                catch (Exception)
                {
                    // Обработка ошибок подключения (опционально)
                }
            });
        }

        public SecureSession<TAddress> Connect(TAddress address)
        {
            // Отправляем явное handshake-сообщение
            byte[] handshakeInit = Encoding.UTF8.GetBytes("HELLO");
            _adapter.SendTo(handshakeInit, address);

            var session = new SecureSession<TAddress>(_adapter, address);
            _sessions[address] = session;

            return session;
        }

        private bool IsHandshakePacket(byte[] data)
        {
            return data != null &&
                   data.Length == 5 &&
                   Encoding.UTF8.GetString(data) == "HELLO";
        }
    }
}
