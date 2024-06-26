using System;
using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(BarricadeManager))]
public static class BarricadeManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveDestroyBarricade", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDestroyBarricade_Read(in ClientInvocationContext context)
    {
        context.reader.ReadNetId(out var value);
        BarricadeManager.ReceiveDestroyBarricade(in context, value);
    }

    [NetInvokableGeneratedMethod("ReceiveDestroyBarricade", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDestroyBarricade_Write(NetPakWriter writer, NetId netId)
    {
        writer.WriteNetId(netId);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionBarricades", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClearRegionBarricades_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        BarricadeManager.ReceiveClearRegionBarricades(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionBarricades", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveClearRegionBarricades_Write(NetPakWriter writer, byte x, byte y)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveSingleBarricade", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSingleBarricade_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadNetId(out var value);
        reader.ReadGuid(out var value2);
        reader.ReadUInt8(out var value3);
        byte[] array = new byte[value3];
        reader.ReadBytes(array);
        reader.ReadClampedVector3(out var value4, 13, 11);
        reader.ReadQuaternion(out var value5);
        reader.ReadUInt64(out var value6);
        reader.ReadUInt64(out var value7);
        reader.ReadNetId(out var value8);
        BarricadeManager.ReceiveSingleBarricade(in context, value, value2, array, value4, value5, value6, value7, value8);
    }

    [NetInvokableGeneratedMethod("ReceiveSingleBarricade", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSingleBarricade_Write(NetPakWriter writer, NetId parentNetId, Guid assetId, byte[] state, Vector3 point, Quaternion rotation, ulong owner, ulong group, NetId netId)
    {
        writer.WriteNetId(parentNetId);
        writer.WriteGuid(assetId);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteClampedVector3(point, 13, 11);
        writer.WriteQuaternion(rotation);
        writer.WriteUInt64(owner);
        writer.WriteUInt64(group);
        writer.WriteNetId(netId);
    }
}
