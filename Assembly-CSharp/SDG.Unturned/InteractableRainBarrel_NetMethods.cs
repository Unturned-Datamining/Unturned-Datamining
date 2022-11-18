using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableRainBarrel))]
public static class InteractableRainBarrel_NetMethods
{
    private static void ReceiveFull_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableRainBarrel interactableRainBarrel = voidNetObj as InteractableRainBarrel;
        if (!(interactableRainBarrel == null))
        {
            context.reader.ReadBit(out var value);
            interactableRainBarrel.ReceiveFull(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFull", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveFull_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveFull_DeferredRead);
            return;
        }
        InteractableRainBarrel interactableRainBarrel = obj as InteractableRainBarrel;
        if (!(interactableRainBarrel == null))
        {
            reader.ReadBit(out var value2);
            interactableRainBarrel.ReceiveFull(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFull", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveFull_Write(NetPakWriter writer, bool newFull)
    {
        writer.WriteBit(newFull);
    }
}
