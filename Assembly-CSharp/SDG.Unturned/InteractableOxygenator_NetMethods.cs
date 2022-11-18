using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableOxygenator))]
public static class InteractableOxygenator_NetMethods
{
    private static void ReceivePowered_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableOxygenator interactableOxygenator = voidNetObj as InteractableOxygenator;
        if (!(interactableOxygenator == null))
        {
            context.reader.ReadBit(out var value);
            interactableOxygenator.ReceivePowered(value);
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
        InteractableOxygenator interactableOxygenator = obj as InteractableOxygenator;
        if (!(interactableOxygenator == null))
        {
            reader.ReadBit(out var value2);
            interactableOxygenator.ReceivePowered(value2);
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
            InteractableOxygenator interactableOxygenator = obj as InteractableOxygenator;
            if (!(interactableOxygenator == null))
            {
                reader.ReadBit(out var value2);
                interactableOxygenator.ReceiveToggleRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToggleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleRequest_Write(NetPakWriter writer, bool desiredPowered)
    {
        writer.WriteBit(desiredPowered);
    }
}
