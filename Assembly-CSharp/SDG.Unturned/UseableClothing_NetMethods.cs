using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableClothing))]
public static class UseableClothing_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayWear", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayWear_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableClothing useableClothing = obj as UseableClothing;
            if (!(useableClothing == null))
            {
                useableClothing.ReceivePlayWear();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayWear", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayWear_Write(NetPakWriter writer)
    {
    }
}
