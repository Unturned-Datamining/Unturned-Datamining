using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(ItemManager))]
public static class ItemManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveDestroyItem", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDestroyItem_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt32(out var value3);
        reader.ReadBit(out var value4);
        ItemManager.ReceiveDestroyItem(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveDestroyItem", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDestroyItem_Write(NetPakWriter writer, byte x, byte y, uint instanceID, bool shouldPlayEffect)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt32(instanceID);
        writer.WriteBit(shouldPlayEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveTakeItemRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTakeItemRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt32(out var value3);
        reader.ReadUInt8(out var value4);
        reader.ReadUInt8(out var value5);
        reader.ReadUInt8(out var value6);
        reader.ReadUInt8(out var value7);
        ItemManager.ReceiveTakeItemRequest(in context, value, value2, value3, value4, value5, value6, value7);
    }

    [NetInvokableGeneratedMethod("ReceiveTakeItemRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTakeItemRequest_Write(NetPakWriter writer, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt32(instanceID);
        writer.WriteUInt8(to_x);
        writer.WriteUInt8(to_y);
        writer.WriteUInt8(to_rot);
        writer.WriteUInt8(to_page);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionItems", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClearRegionItems_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        ItemManager.ReceiveClearRegionItems(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionItems", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveClearRegionItems_Write(NetPakWriter writer, byte x, byte y)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveItem", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveItem_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        reader.ReadUInt8(out var value4);
        reader.ReadUInt8(out var value5);
        reader.ReadUInt8(out var value6);
        byte[] array = new byte[value6];
        reader.ReadBytes(array);
        reader.ReadClampedVector3(out var value7);
        reader.ReadUInt32(out var value8);
        reader.ReadBit(out var value9);
        ItemManager.ReceiveItem(value, value2, value3, value4, value5, array, value7, value8, value9);
    }

    [NetInvokableGeneratedMethod("ReceiveItem", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveItem_Write(NetPakWriter writer, byte x, byte y, ushort id, byte amount, byte quality, byte[] state, Vector3 point, uint instanceID, bool shouldPlayEffect)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(id);
        writer.WriteUInt8(amount);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteClampedVector3(point);
        writer.WriteUInt32(instanceID);
        writer.WriteBit(shouldPlayEffect);
    }
}
