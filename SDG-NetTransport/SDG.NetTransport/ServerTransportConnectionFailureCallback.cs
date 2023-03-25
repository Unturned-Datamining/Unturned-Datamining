namespace SDG.NetTransport;

public delegate void ServerTransportConnectionFailureCallback(ITransportConnection transportConnection, string debugString, bool isError);
