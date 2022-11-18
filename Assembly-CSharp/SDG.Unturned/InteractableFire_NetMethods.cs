using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableFire))]
public static class InteractableFire_NetMethods
{
    private static void ReceiveLit_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableFire interactableFire = voidNetObj as InteractableFire;
        if (!(interactableFire == null))
        {
            context.reader.ReadBit(out var value);
            interactableFire.ReceiveLit(value);
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
        InteractableFire interactableFire = obj as InteractableFire;
        if (!(interactableFire == null))
        {
            reader.ReadBit(out var value2);
            interactableFire.ReceiveLit(value2);
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
            InteractableFire interactableFire = obj as InteractableFire;
            if (!(interactableFire == null))
            {
                reader.ReadBit(out var value2);
                interactableFire.ReceiveToggleRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToggleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleRequest_Write(NetPakWriter writer, bool desiredLit)
    {
        writer.WriteBit(desiredLit);
    }
}
