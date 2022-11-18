using System;
using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(StructureManager))]
public static class StructureManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveDestroyStructure", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDestroyStructure_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadNetId(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadBit(out var value3);
        StructureManager.ReceiveDestroyStructure(in context, value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveDestroyStructure", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDestroyStructure_Write(NetPakWriter writer, NetId netId, Vector3 ragdoll, bool wasPickedUp)
    {
        writer.WriteNetId(netId);
        writer.WriteClampedVector3(ragdoll);
        writer.WriteBit(wasPickedUp);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionStructures", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClearRegionStructures_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        StructureManager.ReceiveClearRegionStructures(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionStructures", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveClearRegionStructures_Write(NetPakWriter writer, byte x, byte y)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveSingleStructure", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSingleStructure_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadGuid(out var value3);
        reader.ReadClampedVector3(out var value4, 13, 11);
        reader.ReadUInt8(out var value5);
        reader.ReadUInt8(out var value6);
        reader.ReadUInt8(out var value7);
        reader.ReadUInt64(out var value8);
        reader.ReadUInt64(out var value9);
        reader.ReadNetId(out var value10);
        StructureManager.ReceiveSingleStructure(value, value2, value3, value4, value5, value6, value7, value8, value9, value10);
    }

    [NetInvokableGeneratedMethod("ReceiveSingleStructure", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSingleStructure_Write(NetPakWriter writer, byte x, byte y, Guid id, Vector3 point, byte angle_x, byte angle_y, byte angle_z, ulong owner, ulong group, NetId netId)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteGuid(id);
        writer.WriteClampedVector3(point, 13, 11);
        writer.WriteUInt8(angle_x);
        writer.WriteUInt8(angle_y);
        writer.WriteUInt8(angle_z);
        writer.WriteUInt64(owner);
        writer.WriteUInt64(group);
        writer.WriteNetId(netId);
    }
}
