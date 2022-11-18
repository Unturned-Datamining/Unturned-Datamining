using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(ZombieManager))]
public static class ZombieManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveBeacon", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBeacon_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadBit(out var value2);
        ZombieManager.ReceiveBeacon(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveBeacon", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBeacon_Write(NetPakWriter writer, byte reference, bool hasBeacon)
    {
        writer.WriteUInt8(reference);
        writer.WriteBit(hasBeacon);
    }

    [NetInvokableGeneratedMethod("ReceiveWave", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWave_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadBit(out var value);
        reader.ReadInt32(out var value2);
        ZombieManager.ReceiveWave(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveWave", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWave_Write(NetPakWriter writer, bool newWaveReady, int newWave)
    {
        writer.WriteBit(newWaveReady);
        writer.WriteInt32(newWave);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieAlive", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieAlive_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadUInt8(out var value3);
        reader.ReadUInt8(out var value4);
        reader.ReadUInt8(out var value5);
        reader.ReadUInt8(out var value6);
        reader.ReadUInt8(out var value7);
        reader.ReadUInt8(out var value8);
        reader.ReadClampedVector3(out var value9);
        reader.ReadUInt8(out var value10);
        ZombieManager.ReceiveZombieAlive(value, value2, value3, value4, value5, value6, value7, value8, value9, value10);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieAlive", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieAlive_Write(NetPakWriter writer, byte reference, ushort id, byte newType, byte newSpeciality, byte newShirt, byte newPants, byte newHat, byte newGear, Vector3 newPosition, byte newAngle)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteUInt8(newType);
        writer.WriteUInt8(newSpeciality);
        writer.WriteUInt8(newShirt);
        writer.WriteUInt8(newPants);
        writer.WriteUInt8(newHat);
        writer.WriteUInt8(newGear);
        writer.WriteClampedVector3(newPosition);
        writer.WriteUInt8(newAngle);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieDead", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieDead_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadClampedVector3(out var value3);
        reader.ReadEnum(out var value4);
        ZombieManager.ReceiveZombieDead(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieDead", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieDead_Write(NetPakWriter writer, byte reference, ushort id, Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteClampedVector3(newRagdoll);
        writer.WriteEnum(newRagdollEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieSpeciality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieSpeciality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadEnum(out var value3);
        ZombieManager.ReceiveZombieSpeciality(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieSpeciality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieSpeciality_Write(NetPakWriter writer, byte reference, ushort id, EZombieSpeciality speciality)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteEnum(speciality);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieThrow", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieThrow_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        ZombieManager.ReceiveZombieThrow(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieThrow", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieThrow_Write(NetPakWriter writer, byte reference, ushort id)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieBoulder", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieBoulder_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadClampedVector3(out var value3);
        reader.ReadClampedVector3(out var value4);
        ZombieManager.ReceiveZombieBoulder(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieBoulder", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieBoulder_Write(NetPakWriter writer, byte reference, ushort id, Vector3 origin, Vector3 direction)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteClampedVector3(origin);
        writer.WriteClampedVector3(direction);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieSpit", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieSpit_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        ZombieManager.ReceiveZombieSpit(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieSpit", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieSpit_Write(NetPakWriter writer, byte reference, ushort id)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieCharge", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieCharge_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        ZombieManager.ReceiveZombieCharge(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieCharge", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieCharge_Write(NetPakWriter writer, byte reference, ushort id)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieStomp", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieStomp_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        ZombieManager.ReceiveZombieStomp(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieStomp", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieStomp_Write(NetPakWriter writer, byte reference, ushort id)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieBreath", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieBreath_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        ZombieManager.ReceiveZombieBreath(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieBreath", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieBreath_Write(NetPakWriter writer, byte reference, ushort id)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieAcid", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieAcid_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadClampedVector3(out var value3);
        reader.ReadClampedVector3(out var value4);
        ZombieManager.ReceiveZombieAcid(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieAcid", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieAcid_Write(NetPakWriter writer, byte reference, ushort id, Vector3 origin, Vector3 direction)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteClampedVector3(origin);
        writer.WriteClampedVector3(direction);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieSpark", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieSpark_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadClampedVector3(out var value3);
        ZombieManager.ReceiveZombieSpark(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieSpark", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieSpark_Write(NetPakWriter writer, byte reference, ushort id, Vector3 target)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteClampedVector3(target);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieAttack", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieAttack_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadUInt8(out var value3);
        ZombieManager.ReceiveZombieAttack(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieAttack", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieAttack_Write(NetPakWriter writer, byte reference, ushort id, byte attack)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteUInt8(attack);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieStartle", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieStartle_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadUInt8(out var value3);
        ZombieManager.ReceiveZombieStartle(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieStartle", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieStartle_Write(NetPakWriter writer, byte reference, ushort id, byte startle)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteUInt8(startle);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieStun", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveZombieStun_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt16(out var value2);
        reader.ReadUInt8(out var value3);
        ZombieManager.ReceiveZombieStun(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveZombieStun", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveZombieStun_Write(NetPakWriter writer, byte reference, ushort id, byte stun)
    {
        writer.WriteUInt8(reference);
        writer.WriteUInt16(id);
        writer.WriteUInt8(stun);
    }
}
