using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableFuel))]
public static class UseableFuel_NetMethods
{
    [NetInvokableGeneratedMethod("ReceivePlayGlug", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlayGlug_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableFuel useableFuel = obj as UseableFuel;
            if (!(useableFuel == null))
            {
                useableFuel.ReceivePlayGlug();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlayGlug", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlayGlug_Write(NetPakWriter writer)
    {
    }
}
