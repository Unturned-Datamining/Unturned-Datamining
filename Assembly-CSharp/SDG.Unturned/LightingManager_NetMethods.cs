using System;
using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(LightingManager))]
public static class LightingManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveInitialLightingState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveInitialLightingState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt32(out var value);
        reader.ReadUInt32(out var value2);
        reader.ReadUInt32(out var value3);
        reader.ReadUInt8(out var value4);
        reader.ReadUInt8(out var value5);
        reader.ReadGuid(out var value6);
        reader.ReadFloat(out var value7);
        reader.ReadNetId(out var value8);
        reader.ReadInt32(out var value9);
        LightingManager.ReceiveInitialLightingState(value, value2, value3, value4, value5, value6, value7, value8, value9);
    }

    [NetInvokableGeneratedMethod("ReceiveInitialLightingState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveInitialLightingState_Write(NetPakWriter writer, uint serverTime, uint newCycle, uint newOffset, byte moon, byte wind, Guid activeWeatherGuid, float activeWeatherBlendAlpha, NetId activeWeatherNetId, int newDateCounter)
    {
        writer.WriteUInt32(serverTime);
        writer.WriteUInt32(newCycle);
        writer.WriteUInt32(newOffset);
        writer.WriteUInt8(moon);
        writer.WriteUInt8(wind);
        writer.WriteGuid(activeWeatherGuid);
        writer.WriteFloat(activeWeatherBlendAlpha);
        writer.WriteNetId(activeWeatherNetId);
        writer.WriteInt32(newDateCounter);
    }

    [NetInvokableGeneratedMethod("ReceiveLightingCycle", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLightingCycle_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt32(out var value);
        LightingManager.ReceiveLightingCycle(value);
    }

    [NetInvokableGeneratedMethod("ReceiveLightingCycle", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLightingCycle_Write(NetPakWriter writer, uint newScale)
    {
        writer.WriteUInt32(newScale);
    }

    [NetInvokableGeneratedMethod("ReceiveLightingOffset", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLightingOffset_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt32(out var value);
        LightingManager.ReceiveLightingOffset(value);
    }

    [NetInvokableGeneratedMethod("ReceiveLightingOffset", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLightingOffset_Write(NetPakWriter writer, uint newOffset)
    {
        writer.WriteUInt32(newOffset);
    }

    [NetInvokableGeneratedMethod("ReceiveLightingWind", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLightingWind_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt8(out var value);
        LightingManager.ReceiveLightingWind(value);
    }

    [NetInvokableGeneratedMethod("ReceiveLightingWind", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLightingWind_Write(NetPakWriter writer, byte newWind)
    {
        writer.WriteUInt8(newWind);
    }

    [NetInvokableGeneratedMethod("ReceiveLightingActiveWeather", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLightingActiveWeather_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadFloat(out var value2);
        reader.ReadNetId(out var value3);
        LightingManager.ReceiveLightingActiveWeather(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveLightingActiveWeather", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLightingActiveWeather_Write(NetPakWriter writer, Guid assetGuid, float blendAlpha, NetId netId)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteFloat(blendAlpha);
        writer.WriteNetId(netId);
    }
}
