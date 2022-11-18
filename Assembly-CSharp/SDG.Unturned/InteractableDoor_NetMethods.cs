using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableDoor))]
public static class InteractableDoor_NetMethods
{
    private static void ReceiveOpen_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableDoor interactableDoor = voidNetObj as InteractableDoor;
        if (!(interactableDoor == null))
        {
            context.reader.ReadBit(out var value);
            interactableDoor.ReceiveOpen(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveOpen", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveOpen_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveOpen_DeferredRead);
            return;
        }
        InteractableDoor interactableDoor = obj as InteractableDoor;
        if (!(interactableDoor == null))
        {
            reader.ReadBit(out var value2);
            interactableDoor.ReceiveOpen(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveOpen", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveOpen_Write(NetPakWriter writer, bool newOpen)
    {
        writer.WriteBit(newOpen);
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
            InteractableDoor interactableDoor = obj as InteractableDoor;
            if (!(interactableDoor == null))
            {
                reader.ReadBit(out var value2);
                interactableDoor.ReceiveToggleRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToggleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleRequest_Write(NetPakWriter writer, bool desiredOpen)
    {
        writer.WriteBit(desiredOpen);
    }
}
