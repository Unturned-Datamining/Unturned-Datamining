using Unturned.SystemEx;

namespace SDG.NetTransport;

public class ClientTransport_Null : IClientTransport
{
    public void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback)
    {
    }

    public bool Receive(byte[] buffer, out long size)
    {
        size = 0L;
        return false;
    }

    public void Send(byte[] buffer, long size, ENetReliability reliability)
    {
    }

    public void TearDown()
    {
    }

    public bool TryGetIPv4Address(out IPv4Address address)
    {
        address = IPv4Address.Zero;
        return false;
    }

    public bool TryGetConnectionPort(out ushort connectionPort)
    {
        connectionPort = 0;
        return false;
    }

    public bool TryGetQueryPort(out ushort queryPort)
    {
        queryPort = 0;
        return false;
    }
}
