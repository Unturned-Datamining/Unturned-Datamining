using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableCarjack))]
public static class UseableCarjack_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayPull", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayPull_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableCarjack useableCarjack = obj as UseableCarjack;
            if (!(useableCarjack == null))
            {
                useableCarjack.ReceivePlayPull();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayPull", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayPull_Write(NetPakWriter writer)
    {
    }
}
