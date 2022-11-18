using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableOven))]
public static class InteractableOven_NetMethods
{
    private static void ReceiveLit_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableOven interactableOven = voidNetObj as InteractableOven;
        if (!(interactableOven == null))
        {
            context.reader.ReadBit(out var value);
            interactableOven.ReceiveLit(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveLit", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLit_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveLit_DeferredRead);
            return;
        }
        InteractableOven interactableOven = obj as InteractableOven;
        if (!(interactableOven == null))
        {
            reader.ReadBit(out var value2);
            interactableOven.ReceiveLit(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveLit", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLit_Write(NetPakWriter writer, bool newLit)
    {
        writer.WriteBit(newLit);
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
            InteractableOven interactableOven = obj as InteractableOven;
            if (!(interactableOven == null))
            {
                reader.ReadBit(out var value2);
                interactableOven.ReceiveToggleRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToggleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleRequest_Write(NetPakWriter writer, bool desiredLit)
    {
        writer.WriteBit(desiredLit);
    }
}
