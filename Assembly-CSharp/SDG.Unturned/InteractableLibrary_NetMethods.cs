using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableLibrary))]
public static class InteractableLibrary_NetMethods
{
    private static void ReceiveAmount_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableLibrary interactableLibrary = voidNetObj as InteractableLibrary;
        if (!(interactableLibrary == null))
        {
            context.reader.ReadUInt32(out var value);
            interactableLibrary.ReceiveAmount(value);
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
        InteractableLibrary interactableLibrary = obj as InteractableLibrary;
        if (!(interactableLibrary == null))
        {
            reader.ReadUInt32(out var value2);
            interactableLibrary.ReceiveAmount(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAmount", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAmount_Write(NetPakWriter writer, uint newAmount)
    {
        writer.WriteUInt32(newAmount);
    }

    [NetInvokableGeneratedMethod("ReceiveTransferLibraryRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTransferLibraryRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableLibrary interactableLibrary = obj as InteractableLibrary;
            if (!(interactableLibrary == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt32(out var value3);
                interactableLibrary.ReceiveTransferLibraryRequest(in context, value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTransferLibraryRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTransferLibraryRequest_Write(NetPakWriter writer, byte transaction, uint delta)
    {
        writer.WriteUInt8(transaction);
        writer.WriteUInt32(delta);
    }
}
