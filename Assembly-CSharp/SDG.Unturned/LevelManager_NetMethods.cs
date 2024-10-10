using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(LevelManager))]
public static class LevelManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveArenaOrigin", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveArenaOrigin_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadClampedVector3(out var value);
        reader.ReadFloat(out var value2);
        reader.ReadClampedVector3(out var value3);
        reader.ReadFloat(out var value4);
        reader.ReadClampedVector3(out var value5);
        reader.ReadFloat(out var value6);
        reader.ReadFloat(out var value7);
        reader.ReadUInt8(out var value8);
        LevelManager.ReceiveArenaOrigin(value, value2, value3, value4, value5, value6, value7, value8);
    }

    [NetInvokableGeneratedMethod("ReceiveArenaOrigin", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveArenaOrigin_Write(NetPakWriter writer, Vector3 newArenaCurrentCenter, float newArenaCurrentRadius, Vector3 newArenaOriginCenter, float newArenaOriginRadius, Vector3 newArenaTargetCenter, float newArenaTargetRadius, float newArenaCompactorSpeed, byte delay)
    {
        writer.WriteClampedVector3(newArenaCurrentCenter);
        writer.WriteFloat(newArenaCurrentRadius);
        writer.WriteClampedVector3(newArenaOriginCenter);
        writer.WriteFloat(newArenaOriginRadius);
        writer.WriteClampedVector3(newArenaTargetCenter);
        writer.WriteFloat(newArenaTargetRadius);
        writer.WriteFloat(newArenaCompactorSpeed);
        writer.WriteUInt8(delay);
    }

    [NetInvokableGeneratedMethod("ReceiveArenaMessage", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveArenaMessage_Read(in ClientInvocationContext context)
    {
        context.reader.ReadEnum(out var value);
        LevelManager.ReceiveArenaMessage(value);
    }

    [NetInvokableGeneratedMethod("ReceiveArenaMessage", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveArenaMessage_Write(NetPakWriter writer, EArenaMessage newArenaMessage)
    {
        writer.WriteEnum(newArenaMessage);
    }

    [NetInvokableGeneratedMethod("ReceiveLevelNumber", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLevelNumber_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt8(out var value);
        LevelManager.ReceiveLevelNumber(value);
    }

    [NetInvokableGeneratedMethod("ReceiveLevelNumber", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLevelNumber_Write(NetPakWriter writer, byte newLevelNumber)
    {
        writer.WriteUInt8(newLevelNumber);
    }

    [NetInvokableGeneratedMethod("ReceiveLevelTimer", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLevelTimer_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt8(out var value);
        LevelManager.ReceiveLevelTimer(value);
    }

    [NetInvokableGeneratedMethod("ReceiveLevelTimer", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLevelTimer_Write(NetPakWriter writer, byte newTimerCount)
    {
        writer.WriteUInt8(newTimerCount);
    }

    [NetInvokableGeneratedMethod("ReceiveAirdropState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAirdropState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadNormalVector3(out var value3);
        reader.ReadFloat(out var value4);
        reader.ReadFloat(out var value5);
        reader.ReadFloat(out var value6);
        LevelManager.ReceiveAirdropState(value, value2, value3, value4, value5, value6);
    }

    [NetInvokableGeneratedMethod("ReceiveAirdropState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAirdropState_Write(NetPakWriter writer, ushort id, Vector3 state, Vector3 direction, float speed, float force, float delay)
    {
        writer.WriteUInt16(id);
        writer.WriteClampedVector3(state);
        writer.WriteNormalVector3(direction);
        writer.WriteFloat(speed);
        writer.WriteFloat(force);
        writer.WriteFloat(delay);
    }
}
