using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableDetonator))]
public static class UseableDetonator_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayPlunge", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayPlunge_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableDetonator useableDetonator = obj as UseableDetonator;
            if (!(useableDetonator == null))
            {
                useableDetonator.ReceivePlayPlunge();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayPlunge", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayPlunge_Write(NetPakWriter writer)
    {
    }
}
