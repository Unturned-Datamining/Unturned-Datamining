using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableArrestEnd))]
public static class UseableArrestEnd_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayArrest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayArrest_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableArrestEnd useableArrestEnd = obj as UseableArrestEnd;
            if (!(useableArrestEnd == null))
            {
                useableArrestEnd.ReceivePlayArrest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayArrest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayArrest_Write(NetPakWriter writer)
    {
    }
}
