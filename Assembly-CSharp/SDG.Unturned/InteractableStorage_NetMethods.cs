using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(InteractableStorage))]
public static class InteractableStorage_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveInteractRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveInteractRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableStorage interactableStorage = obj as InteractableStorage;
            if (!(interactableStorage == null))
            {
                reader.ReadBit(out var value2);
                interactableStorage.ReceiveInteractRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveInteractRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveInteractRequest_Write(NetPakWriter writer, bool quickGrab)
    {
        writer.WriteBit(quickGrab);
    }

    private static void ReceiveDisplay_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableStorage interactableStorage = voidNetObj as InteractableStorage;
        if (!(interactableStorage == null))
        {
            NetPakReader reader = context.reader;
            reader.ReadUInt16(out var value);
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            byte[] array = new byte[value3];
            reader.ReadBytes(array);
            reader.ReadUInt16(out var value4);
            reader.ReadUInt16(out var value5);
            reader.ReadString(out var value6);
            reader.ReadString(out var value7);
            interactableStorage.ReceiveDisplay(value, value2, array, value4, value5, value6, value7);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDisplay", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDisplay_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveDisplay_DeferredRead);
            return;
        }
        InteractableStorage interactableStorage = obj as InteractableStorage;
        if (!(interactableStorage == null))
        {
            reader.ReadUInt16(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            byte[] array = new byte[value4];
            reader.ReadBytes(array);
            reader.ReadUInt16(out var value5);
            reader.ReadUInt16(out var value6);
            reader.ReadString(out var value7);
            reader.ReadString(out var value8);
            interactableStorage.ReceiveDisplay(value2, value3, array, value5, value6, value7, value8);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDisplay", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDisplay_Write(NetPakWriter writer, ushort id, byte quality, byte[] state, ushort skin, ushort mythic, string tags, string dynamicProps)
    {
        writer.WriteUInt16(id);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteUInt16(skin);
        writer.WriteUInt16(mythic);
        writer.WriteString(tags);
        writer.WriteString(dynamicProps);
    }

    private static void ReceiveRotDisplay_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        InteractableStorage interactableStorage = voidNetObj as InteractableStorage;
        if (!(interactableStorage == null))
        {
            context.reader.ReadUInt8(out var value);
            interactableStorage.ReceiveRotDisplay(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRotDisplay", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRotDisplay_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            NetInvocationDeferralRegistry.Defer(value, in context, ReceiveRotDisplay_DeferredRead);
            return;
        }
        InteractableStorage interactableStorage = obj as InteractableStorage;
        if (!(interactableStorage == null))
        {
            reader.ReadUInt8(out var value2);
            interactableStorage.ReceiveRotDisplay(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRotDisplay", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRotDisplay_Write(NetPakWriter writer, byte rotComp)
    {
        writer.WriteUInt8(rotComp);
    }

    [NetInvokableGeneratedMethod("ReceiveRotDisplayRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRotDisplayRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            InteractableStorage interactableStorage = obj as InteractableStorage;
            if (!(interactableStorage == null))
            {
                reader.ReadUInt8(out var value2);
                interactableStorage.ReceiveRotDisplayRequest(in context, value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRotDisplayRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRotDisplayRequest_Write(NetPakWriter writer, byte rotComp)
    {
        writer.WriteUInt8(rotComp);
    }
}
