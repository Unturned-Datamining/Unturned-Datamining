using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(ResourceManager))]
public static class ResourceManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveClearRegionResources", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClearRegionResources_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        ResourceManager.ReceiveClearRegionResources(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveClearRegionResources", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveClearRegionResources_Write(NetPakWriter writer, byte x, byte y)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveForageRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveForageRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        ResourceManager.ReceiveForageRequest(in context, value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveForageRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveForageRequest_Write(NetPakWriter writer, byte x, byte y, ushort index)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
    }

    [NetInvokableGeneratedMethod("ReceiveResourceDead", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveResourceDead_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        reader.ReadClampedVector3(out var value4);
        ResourceManager.ReceiveResourceDead(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveResourceDead", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveResourceDead_Write(NetPakWriter writer, byte x, byte y, ushort index, Vector3 ragdoll)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
        writer.WriteClampedVector3(ragdoll);
    }

    [NetInvokableGeneratedMethod("ReceiveResourceAlive", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveResourceAlive_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        reader.ReadUInt16(out var value3);
        ResourceManager.ReceiveResourceAlive(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveResourceAlive", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveResourceAlive_Write(NetPakWriter writer, byte x, byte y, ushort index)
    {
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteUInt16(index);
    }
}
