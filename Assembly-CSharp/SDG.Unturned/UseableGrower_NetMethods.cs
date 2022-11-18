using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableGrower))]
public static class UseableGrower_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayGrow", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayGrow_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableGrower useableGrower = obj as UseableGrower;
            if (!(useableGrower == null))
            {
                useableGrower.ReceivePlayGrow();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayGrow", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayGrow_Write(NetPakWriter writer)
    {
    }
}
