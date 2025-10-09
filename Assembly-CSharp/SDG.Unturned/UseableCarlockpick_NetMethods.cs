using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableCarlockpick))]
public static class UseableCarlockpick_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayJimmy", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayJimmy_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableCarlockpick useableCarlockpick = obj as UseableCarlockpick;
            if (!(useableCarlockpick == null))
            {
                reader.ReadBit(out var value2);
                useableCarlockpick.ReceivePlayJimmy(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayJimmy", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayJimmy_Write(NetPakWriter writer, bool isFailure)
    {
        writer.WriteBit(isFailure);
    }
}
