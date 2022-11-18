using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableGenerator))]
public static class InteractableGenerator_NetMethods
{
    private static void ReceiveFuel_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableGenerator interactableGenerator = voidNetObj as InteractableGenerator;
        if (!(interactableGenerator == null))
        {
            context.reader.ReadUInt16(out var value);
            interactableGenerator.ReceiveFuel(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFuel", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveFuel_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveFuel_DeferredRead);
            return;
        }
        InteractableGenerator interactableGenerator = obj as InteractableGenerator;
        if (!(interactableGenerator == null))
        {
            reader.ReadUInt16(out var value2);
            interactableGenerator.ReceiveFuel(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFuel", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveFuel_Write(NetPakWriter writer, ushort newFuel)
    {
        writer.WriteUInt16(newFuel);
    }

    private static void ReceivePowered_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableGenerator interactableGenerator = voidNetObj as InteractableGenerator;
        if (!(interactableGenerator == null))
        {
            context.reader.ReadBit(out var value);
            interactableGenerator.ReceivePowered(value);
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
        InteractableGenerator interactableGenerator = obj as InteractableGenerator;
        if (!(interactableGenerator == null))
        {
            reader.ReadBit(out var value2);
            interactableGenerator.ReceivePowered(value2);
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
            InteractableGenerator interactableGenerator = obj as InteractableGenerator;
            if (!(interactableGenerator == null))
            {
                reader.ReadBit(out var value2);
                interactableGenerator.ReceiveToggleRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToggleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleRequest_Write(NetPakWriter writer, bool desiredPowered)
    {
        writer.WriteBit(desiredPowered);
    }
}
