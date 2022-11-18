using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(AnimalManager))]
public static class AnimalManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveAnimalAlive", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAnimalAlive_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadUInt8(out var value3);
        AnimalManager.ReceiveAnimalAlive(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalAlive", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAnimalAlive_Write(NetPakWriter writer, ushort index, Vector3 newPosition, byte newAngle)
    {
        writer.WriteUInt16(index);
        writer.WriteClampedVector3(newPosition);
        writer.WriteUInt8(newAngle);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalDead", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAnimalDead_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadEnum(out var value3);
        AnimalManager.ReceiveAnimalDead(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalDead", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAnimalDead_Write(NetPakWriter writer, ushort index, Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
    {
        writer.WriteUInt16(index);
        writer.WriteClampedVector3(newRagdoll);
        writer.WriteEnum(newRagdollEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalStartle", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAnimalStartle_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt16(out var value);
        AnimalManager.ReceiveAnimalStartle(value);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalStartle", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAnimalStartle_Write(NetPakWriter writer, ushort index)
    {
        writer.WriteUInt16(index);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalAttack", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAnimalAttack_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt16(out var value);
        AnimalManager.ReceiveAnimalAttack(value);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalAttack", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAnimalAttack_Write(NetPakWriter writer, ushort index)
    {
        writer.WriteUInt16(index);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalPanic", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAnimalPanic_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt16(out var value);
        AnimalManager.ReceiveAnimalPanic(value);
    }

    [NetInvokableGeneratedMethod("ReceiveAnimalPanic", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAnimalPanic_Write(NetPakWriter writer, ushort index)
    {
        writer.WriteUInt16(index);
    }
}
