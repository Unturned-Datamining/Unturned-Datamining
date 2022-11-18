using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableRefill))]
public static class UseableRefill_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayUse", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayUse_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableRefill useableRefill = obj as UseableRefill;
            if (!(useableRefill == null))
            {
                useableRefill.ReceivePlayUse();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayUse", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayUse_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceivePlayRefill", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayRefill_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableRefill useableRefill = obj as UseableRefill;
            if (!(useableRefill == null))
            {
                useableRefill.ReceivePlayRefill();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayRefill", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayRefill_Write(NetPakWriter writer)
    {
    }
}
