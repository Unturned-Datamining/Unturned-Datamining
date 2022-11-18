using System;
using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(Assets))]
public static class Assets_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveKickForInvalidGuid", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveKickForInvalidGuid_Read(in ClientInvocationContext context)
    {
        context.reader.ReadGuid(out var value);
        Assets.ReceiveKickForInvalidGuid(value);
    }

    [NetInvokableGeneratedMethod("ReceiveKickForInvalidGuid", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveKickForInvalidGuid_Write(NetPakWriter writer, Guid guid)
    {
        writer.WriteGuid(guid);
    }

    [NetInvokableGeneratedMethod("ReceiveKickForHashMismatch", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveKickForHashMismatch_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadGuid(out var value);
        reader.ReadString(out var value2);
        reader.ReadString(out var value3);
        reader.ReadUInt8(out var value4);
        byte[] array = new byte[value4];
        reader.ReadBytes(array);
        reader.ReadString(out var value5);
        reader.ReadString(out var value6);
        Assets.ReceiveKickForHashMismatch(value, value2, value3, array, value5, value6);
    }

    [NetInvokableGeneratedMethod("ReceiveKickForHashMismatch", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveKickForHashMismatch_Write(NetPakWriter writer, Guid guid, string serverName, string serverFriendlyName, byte[] serverHash, string serverAssetBundleNameWithoutExtension, string serverAssetOrigin)
    {
        writer.WriteGuid(guid);
        writer.WriteString(serverName);
        writer.WriteString(serverFriendlyName);
        byte b = (byte)serverHash.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(serverHash, b);
        writer.WriteString(serverAssetBundleNameWithoutExtension);
        writer.WriteString(serverAssetOrigin);
    }
}
