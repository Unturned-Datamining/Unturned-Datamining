using System;
using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(EffectManager))]
public static class EffectManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveEffectClearById", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectClearById_Read(in ClientInvocationContext context)
    {
        context.reader.ReadUInt16(out var value);
        EffectManager.ReceiveEffectClearById(value);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectClearById", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectClearById_Write(NetPakWriter writer, ushort id)
    {
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectClearByGuid", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectClearByGuid_Read(in ClientInvocationContext context)
    {
        context.reader.ReadGuid(out var value);
        EffectManager.ReceiveEffectClearByGuid(value);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectClearByGuid", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectClearByGuid_Write(NetPakWriter writer, Guid assetGuid)
    {
        writer.WriteGuid(assetGuid);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectClearAll", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectClearAll_Read(in ClientInvocationContext context)
    {
        EffectManager.ReceiveEffectClearAll();
    }

    [NetInvokableGeneratedMethod("ReceiveEffectClearAll", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectClearAll_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPointNormal_NonUniformScale", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectPointNormal_NonUniformScale_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadClampedVector3(out var value3);
        reader.ReadClampedVector3(out var value4);
        EffectManager.ReceiveEffectPointNormal_NonUniformScale(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPointNormal_NonUniformScale", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectPointNormal_NonUniformScale_Write(NetPakWriter writer, Guid assetGuid, Vector3 point, Vector3 normal, Vector3 scale)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteClampedVector3(point);
        writer.WriteClampedVector3(normal);
        writer.WriteClampedVector3(scale);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPointNormal_UniformScale", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectPointNormal_UniformScale_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadClampedVector3(out var value3);
        reader.ReadFloat(out var value4);
        EffectManager.ReceiveEffectPointNormal_UniformScale(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPointNormal_UniformScale", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectPointNormal_UniformScale_Write(NetPakWriter writer, Guid assetGuid, Vector3 point, Vector3 normal, float uniformScale)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteClampedVector3(point);
        writer.WriteClampedVector3(normal);
        writer.WriteFloat(uniformScale);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPointNormal", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectPointNormal_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadClampedVector3(out var value3);
        EffectManager.ReceiveEffectPointNormal(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPointNormal", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectPointNormal_Write(NetPakWriter writer, Guid assetGuid, Vector3 point, Vector3 normal)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteClampedVector3(point);
        writer.WriteClampedVector3(normal);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPoint_NonUniformScale", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectPoint_NonUniformScale_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadClampedVector3(out var value3);
        EffectManager.ReceiveEffectPoint_NonUniformScale(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPoint_NonUniformScale", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectPoint_NonUniformScale_Write(NetPakWriter writer, Guid assetGuid, Vector3 point, Vector3 scale)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteClampedVector3(point);
        writer.WriteClampedVector3(scale);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPoint_UniformScale", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectPoint_UniformScale_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadClampedVector3(out var value2);
        reader.ReadFloat(out var value3);
        EffectManager.ReceiveEffectPoint_UniformScale(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPoint_UniformScale", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectPoint_UniformScale_Write(NetPakWriter writer, Guid assetGuid, Vector3 point, float uniformScale)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteClampedVector3(point);
        writer.WriteFloat(uniformScale);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPoint", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectPoint_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadClampedVector3(out var value2);
        EffectManager.ReceiveEffectPoint(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectPoint", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectPoint_Write(NetPakWriter writer, Guid assetGuid, Vector3 point)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteClampedVector3(point);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIEffect_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        reader.ReadInt16(out var value2);
        EffectManager.ReceiveUIEffect(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIEffect_Write(NetPakWriter writer, ushort id, short key)
    {
        writer.WriteUInt16(id);
        writer.WriteInt16(key);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect1Arg", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIEffect1Arg_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        reader.ReadInt16(out var value2);
        reader.ReadString(out var value3);
        EffectManager.ReceiveUIEffect1Arg(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect1Arg", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIEffect1Arg_Write(NetPakWriter writer, ushort id, short key, string arg0)
    {
        writer.WriteUInt16(id);
        writer.WriteInt16(key);
        writer.WriteString(arg0);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect2Args", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIEffect2Args_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        reader.ReadInt16(out var value2);
        reader.ReadString(out var value3);
        reader.ReadString(out var value4);
        EffectManager.ReceiveUIEffect2Args(value, value2, value3, value4);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect2Args", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIEffect2Args_Write(NetPakWriter writer, ushort id, short key, string arg0, string arg1)
    {
        writer.WriteUInt16(id);
        writer.WriteInt16(key);
        writer.WriteString(arg0);
        writer.WriteString(arg1);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect3Args", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIEffect3Args_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        reader.ReadInt16(out var value2);
        reader.ReadString(out var value3);
        reader.ReadString(out var value4);
        reader.ReadString(out var value5);
        EffectManager.ReceiveUIEffect3Args(value, value2, value3, value4, value5);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect3Args", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIEffect3Args_Write(NetPakWriter writer, ushort id, short key, string arg0, string arg1, string arg2)
    {
        writer.WriteUInt16(id);
        writer.WriteInt16(key);
        writer.WriteString(arg0);
        writer.WriteString(arg1);
        writer.WriteString(arg2);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect4Args", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIEffect4Args_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt16(out var value);
        reader.ReadInt16(out var value2);
        reader.ReadString(out var value3);
        reader.ReadString(out var value4);
        reader.ReadString(out var value5);
        reader.ReadString(out var value6);
        EffectManager.ReceiveUIEffect4Args(value, value2, value3, value4, value5, value6);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffect4Args", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIEffect4Args_Write(NetPakWriter writer, ushort id, short key, string arg0, string arg1, string arg2, string arg3)
    {
        writer.WriteUInt16(id);
        writer.WriteInt16(key);
        writer.WriteString(arg0);
        writer.WriteString(arg1);
        writer.WriteString(arg2);
        writer.WriteString(arg3);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffectVisibility", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIEffectVisibility_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadInt16(out var value);
        reader.ReadString(out var value2);
        reader.ReadBit(out var value3);
        EffectManager.ReceiveUIEffectVisibility(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffectVisibility", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIEffectVisibility_Write(NetPakWriter writer, short key, string childName, bool visible)
    {
        writer.WriteInt16(key);
        writer.WriteString(childName);
        writer.WriteBit(visible);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffectText", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIEffectText_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadInt16(out var value);
        reader.ReadString(out var value2);
        reader.ReadString(out var value3);
        EffectManager.ReceiveUIEffectText(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffectText", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIEffectText_Write(NetPakWriter writer, short key, string childName, string text)
    {
        writer.WriteInt16(key);
        writer.WriteString(childName);
        writer.WriteString(text);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffectImageURL", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIEffectImageURL_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadInt16(out var value);
        reader.ReadString(out var value2);
        reader.ReadString(out var value3);
        reader.ReadBit(out var value4);
        reader.ReadBit(out var value5);
        EffectManager.ReceiveUIEffectImageURL(value, value2, value3, value4, value5);
    }

    [NetInvokableGeneratedMethod("ReceiveUIEffectImageURL", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIEffectImageURL_Write(NetPakWriter writer, short key, string childName, string url, bool shouldCache, bool forceRefresh)
    {
        writer.WriteInt16(key);
        writer.WriteString(childName);
        writer.WriteString(url);
        writer.WriteBit(shouldCache);
        writer.WriteBit(forceRefresh);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectClicked", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectClicked_Read(in ServerInvocationContext context)
    {
        context.reader.ReadString(out var value);
        EffectManager.ReceiveEffectClicked(in context, value);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectClicked", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectClicked_Write(NetPakWriter writer, string buttonName)
    {
        writer.WriteString(buttonName);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectTextCommitted", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEffectTextCommitted_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadString(out var value);
        reader.ReadString(out var value2);
        EffectManager.ReceiveEffectTextCommitted(in context, value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveEffectTextCommitted", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEffectTextCommitted_Write(NetPakWriter writer, string inputFieldName, string text)
    {
        writer.WriteString(inputFieldName);
        writer.WriteString(text);
    }
}
