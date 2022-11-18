using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableTank))]
public static class InteractableTank_NetMethods
{
    private static void ReceiveAmount_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableTank interactableTank = voidNetObj as InteractableTank;
        if (!(interactableTank == null))
        {
            context.reader.ReadUInt16(out var value);
            interactableTank.ReceiveAmount(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAmount", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAmount_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveAmount_DeferredRead);
            return;
        }
        InteractableTank interactableTank = obj as InteractableTank;
        if (!(interactableTank == null))
        {
            reader.ReadUInt16(out var value2);
            interactableTank.ReceiveAmount(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAmount", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAmount_Write(NetPakWriter writer, ushort newAmount)
    {
        writer.WriteUInt16(newAmount);
    }
}
