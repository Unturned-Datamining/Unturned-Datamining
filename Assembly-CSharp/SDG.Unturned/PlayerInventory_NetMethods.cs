using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerInventory))]
public static class PlayerInventory_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveDragItem", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDragItem_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerInventory playerInventory = obj as PlayerInventory;
        if (!(playerInventory == null))
        {
            if (!context.IsOwnerOf(playerInventory.channel))
            {
                context.Kick($"not owner of {playerInventory}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            reader.ReadUInt8(out var value6);
            reader.ReadUInt8(out var value7);
            reader.ReadUInt8(out var value8);
            playerInventory.ReceiveDragItem(value2, value3, value4, value5, value6, value7, value8);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDragItem", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDragItem_Write(NetPakWriter writer, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1)
    {
        writer.WriteUInt8(page_0);
        writer.WriteUInt8(x_0);
        writer.WriteUInt8(y_0);
        writer.WriteUInt8(page_1);
        writer.WriteUInt8(x_1);
        writer.WriteUInt8(y_1);
        writer.WriteUInt8(rot_1);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapItem", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapItem_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerInventory playerInventory = obj as PlayerInventory;
        if (!(playerInventory == null))
        {
            if (!context.IsOwnerOf(playerInventory.channel))
            {
                context.Kick($"not owner of {playerInventory}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            reader.ReadUInt8(out var value6);
            reader.ReadUInt8(out var value7);
            reader.ReadUInt8(out var value8);
            reader.ReadUInt8(out var value9);
            playerInventory.ReceiveSwapItem(value2, value3, value4, value5, value6, value7, value8, value9);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapItem", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapItem_Write(NetPakWriter writer, byte page_0, byte x_0, byte y_0, byte rot_0, byte page_1, byte x_1, byte y_1, byte rot_1)
    {
        writer.WriteUInt8(page_0);
        writer.WriteUInt8(x_0);
        writer.WriteUInt8(y_0);
        writer.WriteUInt8(rot_0);
        writer.WriteUInt8(page_1);
        writer.WriteUInt8(x_1);
        writer.WriteUInt8(y_1);
        writer.WriteUInt8(rot_1);
    }

    [NetInvokableGeneratedMethod("ReceiveDropItem", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDropItem_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerInventory playerInventory = obj as PlayerInventory;
        if (!(playerInventory == null))
        {
            if (!context.IsOwnerOf(playerInventory.channel))
            {
                context.Kick($"not owner of {playerInventory}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerInventory.ReceiveDropItem(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDropItem", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDropItem_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateAmount", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUpdateAmount_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInventory playerInventory = obj as PlayerInventory;
            if (!(playerInventory == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                playerInventory.ReceiveUpdateAmount(value2, value3, value4);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateAmount", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUpdateAmount_Write(NetPakWriter writer, byte page, byte index, byte amount)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(index);
        writer.WriteUInt8(amount);
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateQuality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUpdateQuality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInventory playerInventory = obj as PlayerInventory;
            if (!(playerInventory == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                playerInventory.ReceiveUpdateQuality(value2, value3, value4);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateQuality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUpdateQuality_Write(NetPakWriter writer, byte page, byte index, byte quality)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(index);
        writer.WriteUInt8(quality);
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateInvState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUpdateInvState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInventory playerInventory = obj as PlayerInventory;
            if (!(playerInventory == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                playerInventory.ReceiveUpdateInvState(value2, value3, array);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateInvState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUpdateInvState_Write(NetPakWriter writer, byte page, byte index, byte[] state)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(index);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
    }

    [NetInvokableGeneratedMethod("ReceiveItemAdd", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveItemAdd_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInventory playerInventory = obj as PlayerInventory;
            if (!(playerInventory == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                reader.ReadUInt8(out var value5);
                reader.ReadUInt16(out var value6);
                reader.ReadUInt8(out var value7);
                reader.ReadUInt8(out var value8);
                reader.ReadUInt8(out var value9);
                byte[] array = new byte[value9];
                reader.ReadBytes(array);
                playerInventory.ReceiveItemAdd(value2, value3, value4, value5, value6, value7, value8, array);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveItemAdd", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveItemAdd_Write(NetPakWriter writer, byte page, byte x, byte y, byte rot, ushort id, byte amount, byte quality, byte[] state)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt8(rot);
        writer.WriteUInt16(id);
        writer.WriteUInt8(amount);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
    }

    [NetInvokableGeneratedMethod("ReceiveItemRemove", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveItemRemove_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInventory playerInventory = obj as PlayerInventory;
            if (!(playerInventory == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                playerInventory.ReceiveItemRemove(value2, value3, value4);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveItemRemove", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveItemRemove_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveSize", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSize_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInventory playerInventory = obj as PlayerInventory;
            if (!(playerInventory == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                playerInventory.ReceiveSize(value2, value3, value4);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSize", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSize_Write(NetPakWriter writer, byte page, byte newWidth, byte newHeight)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(newWidth);
        writer.WriteUInt8(newHeight);
    }

    [NetInvokableGeneratedMethod("ReceiveStoraging", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveStoraging_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInventory playerInventory = obj as PlayerInventory;
            if (!(playerInventory == null))
            {
                playerInventory.ReceiveStoraging(in context);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveInventory", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveInventory_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerInventory playerInventory = obj as PlayerInventory;
            if (!(playerInventory == null))
            {
                playerInventory.ReceiveInventory(in context);
            }
        }
    }
}
