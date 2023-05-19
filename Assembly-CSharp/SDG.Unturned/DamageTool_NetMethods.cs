using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(DamageTool))]
public static class DamageTool_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveSpawnBulletImpact", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSpawnBulletImpact_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadClampedVector3(out var value);
        reader.ReadNormalVector3(out var value2);
        reader.ReadString(out var value3);
        reader.ReadTransform(out var value4);
        reader.ReadNetId(out var value5);
        DamageTool.ReceiveSpawnBulletImpact(value, value2, value3, value4, value5);
    }

    [NetInvokableGeneratedMethod("ReceiveSpawnBulletImpact", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSpawnBulletImpact_Write(NetPakWriter writer, Vector3 position, Vector3 normal, string materialName, Transform colliderTransform, NetId instigatorNetId)
    {
        writer.WriteClampedVector3(position);
        writer.WriteNormalVector3(normal);
        writer.WriteString(materialName);
        writer.WriteTransform(colliderTransform);
        writer.WriteNetId(instigatorNetId);
    }

    [NetInvokableGeneratedMethod("ReceiveSpawnLegacyImpact", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSpawnLegacyImpact_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadClampedVector3(out var value);
        reader.ReadNormalVector3(out var value2);
        reader.ReadString(out var value3);
        reader.ReadTransform(out var value4);
        DamageTool.ReceiveSpawnLegacyImpact(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveSpawnLegacyImpact", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSpawnLegacyImpact_Write(NetPakWriter writer, Vector3 position, Vector3 normal, string materialName, Transform colliderTransform)
    {
        writer.WriteClampedVector3(position);
        writer.WriteNormalVector3(normal);
        writer.WriteString(materialName);
        writer.WriteTransform(colliderTransform);
    }
}
