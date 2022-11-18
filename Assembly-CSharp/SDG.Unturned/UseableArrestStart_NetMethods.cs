using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableArrestStart))]
public static class UseableArrestStart_NetMethods
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
            UseableArrestStart useableArrestStart = obj as UseableArrestStart;
            if (!(useableArrestStart == null))
            {
                useableArrestStart.ReceivePlayArrest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayArrest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayArrest_Write(NetPakWriter writer)
    {
    }
}
