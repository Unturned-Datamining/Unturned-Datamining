using SDG.NetPak;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(ChatManager))]
public static class ChatManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveVoteStart", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVoteStart_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadSteamID(out CSteamID value);
        reader.ReadSteamID(out CSteamID value2);
        reader.ReadUInt8(out var value3);
        ChatManager.ReceiveVoteStart(value, value2, value3);
    }

    [NetInvokableGeneratedMethod("ReceiveVoteStart", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVoteStart_Write(NetPakWriter writer, CSteamID origin, CSteamID target, byte votesNeeded)
    {
        writer.WriteSteamID(origin);
        writer.WriteSteamID(target);
        writer.WriteUInt8(votesNeeded);
    }

    [NetInvokableGeneratedMethod("ReceiveVoteUpdate", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVoteUpdate_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadUInt8(out var value2);
        ChatManager.ReceiveVoteUpdate(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveVoteUpdate", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVoteUpdate_Write(NetPakWriter writer, byte voteYes, byte voteNo)
    {
        writer.WriteUInt8(voteYes);
        writer.WriteUInt8(voteNo);
    }

    [NetInvokableGeneratedMethod("ReceiveVoteStop", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVoteStop_Read(in ClientInvocationContext context)
    {
        context.reader.ReadEnum(out var value);
        ChatManager.ReceiveVoteStop(value);
    }

    [NetInvokableGeneratedMethod("ReceiveVoteStop", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVoteStop_Write(NetPakWriter writer, EVotingMessage message)
    {
        writer.WriteEnum(message);
    }

    [NetInvokableGeneratedMethod("ReceiveVoteMessage", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVoteMessage_Read(in ClientInvocationContext context)
    {
        context.reader.ReadEnum(out var value);
        ChatManager.ReceiveVoteMessage(value);
    }

    [NetInvokableGeneratedMethod("ReceiveVoteMessage", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVoteMessage_Write(NetPakWriter writer, EVotingMessage message)
    {
        writer.WriteEnum(message);
    }

    [NetInvokableGeneratedMethod("ReceiveSubmitVoteRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSubmitVoteRequest_Read(in ServerInvocationContext context)
    {
        context.reader.ReadBit(out var value);
        ChatManager.ReceiveSubmitVoteRequest(in context, value);
    }

    [NetInvokableGeneratedMethod("ReceiveSubmitVoteRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSubmitVoteRequest_Write(NetPakWriter writer, bool vote)
    {
        writer.WriteBit(vote);
    }

    [NetInvokableGeneratedMethod("ReceiveCallVoteRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveCallVoteRequest_Read(in ServerInvocationContext context)
    {
        context.reader.ReadSteamID(out CSteamID value);
        ChatManager.ReceiveCallVoteRequest(in context, value);
    }

    [NetInvokableGeneratedMethod("ReceiveCallVoteRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveCallVoteRequest_Write(NetPakWriter writer, CSteamID target)
    {
        writer.WriteSteamID(target);
    }

    [NetInvokableGeneratedMethod("ReceiveChatEntry", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveChatEntry_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadSteamID(out CSteamID value);
        reader.ReadString(out var value2);
        reader.ReadEnum(out var value3);
        reader.ReadColor32RGB(out Color value4);
        reader.ReadBit(out var value5);
        reader.ReadString(out var value6);
        ChatManager.ReceiveChatEntry(value, value2, value3, value4, value5, value6);
    }

    [NetInvokableGeneratedMethod("ReceiveChatEntry", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveChatEntry_Write(NetPakWriter writer, CSteamID owner, string iconURL, EChatMode mode, Color color, bool rich, string text)
    {
        writer.WriteSteamID(owner);
        writer.WriteString(iconURL);
        writer.WriteEnum(mode);
        writer.WriteColor32RGB(color);
        writer.WriteBit(rich);
        writer.WriteString(text);
    }

    [NetInvokableGeneratedMethod("ReceiveChatRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveChatRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadString(out var value2);
        ChatManager.ReceiveChatRequest(in context, value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveChatRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveChatRequest_Write(NetPakWriter writer, byte flags, string text)
    {
        writer.WriteUInt8(flags);
        writer.WriteString(text);
    }
}
