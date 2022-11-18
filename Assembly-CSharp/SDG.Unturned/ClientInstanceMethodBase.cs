using SDG.NetPak;

namespace SDG.Unturned;

public abstract class ClientInstanceMethodBase : ClientMethodHandle
{
    protected NetPakWriter GetWriterWithInstanceHeader(NetId netId)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        writerWithStaticHeader.WriteNetId(netId);
        return writerWithStaticHeader;
    }

    protected ClientInstanceMethodBase(ClientMethodInfo clientMethodInfo)
        : base(clientMethodInfo)
    {
    }
}
