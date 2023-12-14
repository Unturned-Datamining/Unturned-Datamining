using Unturned.SystemEx;

namespace SDG.NetTransport;

public interface IClientTransport
{
    void Initialize(ClientTransportReady callback, ClientTransportFailure failureCallback);

    void TearDown();

    bool Receive(byte[] buffer, out long size);

    void Send(byte[] buffer, long size, ENetReliability reliability);

    bool TryGetIPv4Address(out IPv4Address address);

    bool TryGetConnectionPort(out ushort connectionPort);

    bool TryGetQueryPort(out ushort queryPort);
}
