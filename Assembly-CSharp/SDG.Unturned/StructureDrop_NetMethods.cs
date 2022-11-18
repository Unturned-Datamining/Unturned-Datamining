using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(StructureDrop))]
public static class StructureDrop_NetMethods
{
    private static void ReceiveHealth_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        if (voidNetObj is StructureDrop structureDrop)
        {
            context.reader.ReadUInt8(out var value);
            structureDrop.ReceiveHealth(value);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveHealth", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveHealth_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (reader.ReadNetId(out var value))
        {
            object obj = NetIdRegistry.Get(value);
            if (obj == null)
            {
                NetInvocationDeferralRegistry.Defer(value, in context, ReceiveHealth_DeferredRead);
            }
            else if (obj is StructureDrop structureDrop)
            {
                reader.ReadUInt8(out var value2);
                structureDrop.ReceiveHealth(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveHealth", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveHealth_Write(NetPakWriter writer, byte hp)
    {
        writer.WriteUInt8(hp);
    }

    private static void ReceiveTransform_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        if (voidNetObj is StructureDrop structureDrop)
        {
            NetPakReader reader = context.reader;
            reader.ReadUInt8(out var value);
            reader.ReadUInt8(out var value2);
            reader.ReadClampedVector3(out var value3, 13, 11);
            reader.ReadUInt8(out var value4);
            reader.ReadUInt8(out var value5);
            reader.ReadUInt8(out var value6);
            structureDrop.ReceiveTransform(in context, value, value2, value3, value4, value5, value6);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTransform", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTransform_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (reader.ReadNetId(out var value))
        {
            object obj = NetIdRegistry.Get(value);
            if (obj == null)
            {
                NetInvocationDeferralRegistry.Defer(value, in context, ReceiveTransform_DeferredRead);
            }
            else if (obj is StructureDrop structureDrop)
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadClampedVector3(out var value4, 13, 11);
                reader.ReadUInt8(out var value5);
                reader.ReadUInt8(out var value6);
                reader.ReadUInt8(out var value7);
                structureDrop.ReceiveTransform(in context, value2, value3, value4, value5, value6, value7);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTransform", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTransform_Write(NetPakWriter writer, byte old_x, byte old_y, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        writer.WriteUInt8(old_x);
        writer.WriteUInt8(old_y);
        writer.WriteClampedVector3(point, 13, 11);
        writer.WriteUInt8(angle_x);
        writer.WriteUInt8(angle_y);
        writer.WriteUInt8(angle_z);
    }

    [NetInvokableGeneratedMethod("ReceiveTransformRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTransformRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (reader.ReadNetId(out var value))
        {
            object obj = NetIdRegistry.Get(value);
            if (obj != null && obj is StructureDrop structureDrop)
            {
                reader.ReadClampedVector3(out var value2, 13, 11);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                reader.ReadUInt8(out var value5);
                structureDrop.ReceiveTransformRequest(in context, value2, value3, value4, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTransformRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTransformRequest_Write(NetPakWriter writer, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        writer.WriteClampedVector3(point, 13, 11);
        writer.WriteUInt8(angle_x);
        writer.WriteUInt8(angle_y);
        writer.WriteUInt8(angle_z);
    }

    private static void ReceiveOwnerAndGroup_DeferredRead(object voidNetObj, in ClientInvocationContext context)
    {
        if (voidNetObj is StructureDrop structureDrop)
        {
            NetPakReader reader = context.reader;
            reader.ReadUInt64(out var value);
            reader.ReadUInt64(out var value2);
            structureDrop.ReceiveOwnerAndGroup(value, value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveOwnerAndGroup", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveOwnerAndGroup_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (reader.ReadNetId(out var value))
        {
            object obj = NetIdRegistry.Get(value);
            if (obj == null)
            {
                NetInvocationDeferralRegistry.Defer(value, in context, ReceiveOwnerAndGroup_DeferredRead);
            }
            else if (obj is StructureDrop structureDrop)
            {
                reader.ReadUInt64(out var value2);
                reader.ReadUInt64(out var value3);
                structureDrop.ReceiveOwnerAndGroup(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveOwnerAndGroup", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveOwnerAndGroup_Write(NetPakWriter writer, ulong newOwner, ulong newGroup)
    {
        writer.WriteUInt64(newOwner);
        writer.WriteUInt64(newGroup);
    }

    [NetInvokableGeneratedMethod("ReceiveSalvageRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSalvageRequest_Read(in ServerInvocationContext context)
    {
        if (context.reader.ReadNetId(out var value))
        {
            object obj = NetIdRegistry.Get(value);
            if (obj != null && obj is StructureDrop structureDrop)
            {
                structureDrop.ReceiveSalvageRequest(in context);
            }
        }
    }
}
