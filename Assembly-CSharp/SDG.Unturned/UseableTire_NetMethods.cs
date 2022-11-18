using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableTire))]
public static class UseableTire_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayAttach", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayAttach_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableTire useableTire = obj as UseableTire;
            if (!(useableTire == null))
            {
                useableTire.ReceivePlayAttach();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayAttach", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayAttach_Write(NetPakWriter writer)
    {
    }
}
