using System.Net;

namespace M9Studio.SecureStream
{
    public interface ISecureTransportAdapter<TAddress>
    {
        event Action<TAddress> OnConnected;
        event Action<TAddress> OnDisconnected;

        bool SendTo(byte[] buffer, TAddress address);
        byte[] ReceiveFrom(TAddress address);
    }


}