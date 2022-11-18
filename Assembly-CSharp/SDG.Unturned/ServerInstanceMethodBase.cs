using SDG.NetPak;

namespace SDG.Unturned;

public abstract class ServerInstanceMethodBase : ServerMethodHandle
{
    protected NetPakWriter GetWriterWithInstanceHeader(NetId netId)
    {
        NetPakWriter writerWithStaticHeader = GetWriterWithStaticHeader();
        writerWithStaticHeader.WriteNetId(netId);
        return writerWithStaticHeader;
    }

    protected ServerInstanceMethodBase(ServerMethodInfo serverMethodInfo)
        : base(serverMethodInfo)
    {
    }
}
