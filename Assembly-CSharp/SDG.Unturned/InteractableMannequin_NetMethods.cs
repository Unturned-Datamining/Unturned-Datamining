using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableMannequin))]
public static class InteractableMannequin_NetMethods
{
    private static void ReceivePose_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableMannequin interactableMannequin = voidNetObj as InteractableMannequin;
        if (!(interactableMannequin == null))
        {
            context.reader.ReadUInt8(out var value);
            interactableMannequin.ReceivePose(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePose", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePose_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceivePose_DeferredRead);
            return;
        }
        InteractableMannequin interactableMannequin = obj as InteractableMannequin;
        if (!(interactableMannequin == null))
        {
            reader.ReadUInt8(out var value2);
            interactableMannequin.ReceivePose(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePose", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePose_Write(NetPakWriter writer, byte poseComp)
    {
        writer.WriteUInt8(poseComp);
    }

    [NetInvokableGeneratedMethod("ReceivePoseRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePoseRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableMannequin interactableMannequin = obj as InteractableMannequin;
            if (!(interactableMannequin == null))
            {
                reader.ReadUInt8(out var value2);
                interactableMannequin.ReceivePoseRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePoseRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePoseRequest_Write(NetPakWriter writer, byte poseComp)
    {
        writer.WriteUInt8(poseComp);
    }

    private static void ReceiveUpdate_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableMannequin interactableMannequin = voidNetObj as InteractableMannequin;
        if (!(interactableMannequin == null))
        {
            NetPakReader reader = context.reader;
            reader.ReadUInt8(out var value);
            byte[] array = new byte[value];
            reader.ReadBytes(array);
            interactableMannequin.ReceiveUpdate(array);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpdate", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUpdate_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveUpdate_DeferredRead);
            return;
        }
        InteractableMannequin interactableMannequin = obj as InteractableMannequin;
        if (!(interactableMannequin == null))
        {
            reader.ReadUInt8(out var value2);
            byte[] array = new byte[value2];
            reader.ReadBytes(array);
            interactableMannequin.ReceiveUpdate(array);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpdate", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUpdate_Write(NetPakWriter writer, byte[] state)
    {
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUpdateRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableMannequin interactableMannequin = obj as InteractableMannequin;
            if (!(interactableMannequin == null))
            {
                reader.ReadEnum(out var value2);
                interactableMannequin.ReceiveUpdateRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUpdateRequest_Write(NetPakWriter writer, EMannequinUpdateMode updateMode)
    {
        writer.WriteEnum(updateMode);
    }
}
