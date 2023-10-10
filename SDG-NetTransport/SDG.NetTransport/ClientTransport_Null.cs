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
}
