using System;
using System.Net;

namespace SDG.NetTransport;

public interface ITransportConnection : IEquatable<ITransportConnection>
{
    bool TryGetIPv4Address(out uint address);

    bool TryGetPort(out ushort port);

    IPAddress GetAddress();

    string GetAddressString(bool withPort);

    void CloseConnection();

    void Send(byte[] buffer, long size, ENetReliability reliability);
}
