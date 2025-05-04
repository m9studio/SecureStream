using System.Net;

namespace M9Studio.ShieldSocket
{
    public interface ISecureTransportAdapter
    {
        event Action<EndPoint> OnConnected;
        event Action<EndPoint> OnDisconnected;

        bool SendTo(byte[] buffer, EndPoint remoteEP);
        byte[] ReceiveFrom(EndPoint remoteEP);
    }
}
