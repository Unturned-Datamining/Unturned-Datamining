namespace SDG.NetTransport;

public interface IClientTransport
{
    void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback);

    void TearDown();

    bool Receive(byte[] buffer, out long size);

    void Send(byte[] buffer, long size, ENetReliability reliability);
}
