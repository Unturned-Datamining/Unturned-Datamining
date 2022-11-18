using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(ObjectManager))]
public static class ObjectManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveObjectRubble", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveObjectRubble_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        reader.ReadUInt8(out var value4);
        reader.ReadBit(out var value5);
        reader.ReadClampedVector3(out var value6);
        ObjectManager.ReceiveObjectRubble(value, value2, value3, value4, value5, value6);
    }

    [NetInvokableGeneratedMethod("ReceiveObjectRubble", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveObjectRubble_Write(NetPakWriter writer, byte x, byte y, ushort index, byte section, bool isAlive, Vector3 ragdoll)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
        writer.WriteUInt8(section);
        writer.WriteBit(isAlive);
        writer.WriteClampedVector3(ragdoll);
    }

    [NetInvokableGeneratedMethod("ReceiveUseObjectNPC", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUseObjectNPC_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        ObjectManager.ReceiveUseObjectNPC(in context, value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveUseObjectNPC", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUseObjectNPC_Write(NetPakWriter writer, byte x, byte y, ushort index)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
    }

    [NetInvokableGeneratedMethod("ReceiveUseObjectQuest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUseObjectQuest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        ObjectManager.ReceiveUseObjectQuest(in context, value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveUseObjectQuest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUseObjectQuest_Write(NetPakWriter writer, byte x, byte y, ushort index)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
    }

    [NetInvokableGeneratedMethod("ReceiveUseObjectDropper", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUseObjectDropper_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        ObjectManager.ReceiveUseObjectDropper(in context, value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveUseObjectDropper", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUseObjectDropper_Write(NetPakWriter writer, byte x, byte y, ushort index)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
    }

    [NetInvokableGeneratedMethod("ReceiveObjectResourceState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveObjectResourceState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        reader.ReadUInt16(out var value4);
        ObjectManager.ReceiveObjectResourceState(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveObjectResourceState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveObjectResourceState_Write(NetPakWriter writer, byte x, byte y, ushort index, ushort amount)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
        writer.WriteUInt16(amount);
    }

    [NetInvokableGeneratedMethod("ReceiveObjectBinaryState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveObjectBinaryState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        reader.ReadBit(out var value4);
        ObjectManager.ReceiveObjectBinaryState(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveObjectBinaryState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveObjectBinaryState_Write(NetPakWriter writer, byte x, byte y, ushort index, bool isUsed)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
        writer.WriteBit(isUsed);
    }

    [NetInvokableGeneratedMethod("ReceiveToggleObjectBinaryStateRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveToggleObjectBinaryStateRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        reader.ReadBit(out var value4);
        ObjectManager.ReceiveToggleObjectBinaryStateRequest(in context, value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveToggleObjectBinaryStateRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleObjectBinaryStateRequest_Write(NetPakWriter writer, byte x, byte y, ushort index, bool isUsed)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
        writer.WriteBit(isUsed);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionObjects", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClearRegionObjects_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        ObjectManager.ReceiveClearRegionObjects(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionObjects", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveClearRegionObjects_Write(NetPakWriter writer, byte x, byte y)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }
}
