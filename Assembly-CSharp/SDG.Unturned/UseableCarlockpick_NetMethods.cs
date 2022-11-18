using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableCarlockpick))]
public static class UseableCarlockpick_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayJimmy", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayJimmy_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableCarlockpick useableCarlockpick = obj as UseableCarlockpick;
            if (!(useableCarlockpick == null))
            {
                useableCarlockpick.ReceivePlayJimmy();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayJimmy", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayJimmy_Write(NetPakWriter writer)
    {
    }
}
