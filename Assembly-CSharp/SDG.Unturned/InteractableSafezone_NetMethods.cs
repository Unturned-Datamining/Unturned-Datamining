using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableSafezone))]
public static class InteractableSafezone_NetMethods
{
    private static void ReceivePowered_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableSafezone interactableSafezone = voidNetObj as InteractableSafezone;
        if (!(interactableSafezone == null))
        {
            context.reader.ReadBit(out var value);
            interactableSafezone.ReceivePowered(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePowered", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePowered_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceivePowered_DeferredRead);
            return;
        }
        InteractableSafezone interactableSafezone = obj as InteractableSafezone;
        if (!(interactableSafezone == null))
        {
            reader.ReadBit(out var value2);
            interactableSafezone.ReceivePowered(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePowered", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePowered_Write(NetPakWriter writer, bool newPowered)
    {
        writer.WriteBit(newPowered);
    }

    [NetInvokableGeneratedMethod("ReceiveToggleRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveToggleRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableSafezone interactableSafezone = obj as InteractableSafezone;
            if (!(interactableSafezone == null))
            {
                reader.ReadBit(out var value2);
                interactableSafezone.ReceiveToggleRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToggleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleRequest_Write(NetPakWriter writer, bool desiredPowered)
    {
        writer.WriteBit(desiredPowered);
    }
}
