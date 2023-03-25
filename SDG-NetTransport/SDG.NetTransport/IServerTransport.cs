namespace SDG.NetTransport;

public interface IServerTransport
{
    void Initialize(ServerTransportConnectionFailureCallback connectionFailureCallback);

    void TearDown();

    bool Receive(byte[] buffer, out long size, out ITransportConnection transportConnection);
}
