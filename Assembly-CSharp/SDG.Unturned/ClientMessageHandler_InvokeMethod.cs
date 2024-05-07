using System;
using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_InvokeMethod
{
    internal static void ReadMessage(NetPakReader reader)
    {
        if (!reader.ReadBits(NetReflection.clientMethodsBitCount, out var value))
        {
            UnturnedLog.warn("unable to read method index");
            return;
        }
        if (value >= NetReflection.clientMethodsLength)
        {
            UnturnedLog.warn("out of bounds method index ({0}/{1})", value, NetReflection.clientMethodsLength);
            return;
        }
        ClientMethodInfo clientMethodInfo = NetReflection.clientMethods[(int)value];
        ClientInvocationContext context = new ClientInvocationContext(ClientInvocationContext.EOrigin.Remote, reader, clientMethodInfo);
        try
        {
            clientMethodInfo.readMethod(in context);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception invoking {0} from server:", clientMethodInfo);
        }
    }
}
