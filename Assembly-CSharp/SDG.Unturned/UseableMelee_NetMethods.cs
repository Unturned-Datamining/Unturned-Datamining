using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(UseableMelee))]
public static class UseableMelee_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveSpawnMeleeImpact", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSpawnMeleeImpact_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableMelee useableMelee = obj as UseableMelee;
            if (!(useableMelee == null))
            {
                reader.ReadClampedVector3(out var value2);
                reader.ReadClampedVector3(out var value3);
                reader.ReadString(out var value4);
                reader.ReadTransform(out var value5);
                useableMelee.ReceiveSpawnMeleeImpact(value2, value3, value4, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSpawnMeleeImpact", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSpawnMeleeImpact_Write(NetPakWriter writer, Vector3 position, Vector3 normal, string materialName, Transform colliderTransform)
    {
        writer.WriteClampedVector3(position);
        writer.WriteClampedVector3(normal);
        writer.WriteString(materialName);
        writer.WriteTransform(colliderTransform);
    }

    [NetInvokableGeneratedMethod("ReceiveInteractMelee", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveInteractMelee_Read(in ServerInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        UseableMelee useableMelee = obj as UseableMelee;
        if (!(useableMelee == null))
        {
            if (!context.IsOwnerOf(useableMelee.channel))
            {
                context.Kick($"not owner of {useableMelee}");
            }
            else
            {
                useableMelee.ReceiveInteractMelee();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveInteractMelee", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveInteractMelee_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceivePlaySwingStart", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlaySwingStart_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableMelee useableMelee = obj as UseableMelee;
            if (!(useableMelee == null))
            {
                useableMelee.ReceivePlaySwingStart();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlaySwingStart", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlaySwingStart_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceivePlaySwingStop", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlaySwingStop_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableMelee useableMelee = obj as UseableMelee;
            if (!(useableMelee == null))
            {
                useableMelee.ReceivePlaySwingStop();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlaySwingStop", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlaySwingStop_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceivePlaySwing", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePlaySwing_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            UseableMelee useableMelee = obj as UseableMelee;
            if (!(useableMelee == null))
            {
                reader.ReadEnum(out var value2);
                useableMelee.ReceivePlaySwing(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePlaySwing", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePlaySwing_Write(NetPakWriter writer, ESwingMode mode)
    {
        writer.WriteEnum(mode);
    }
}
