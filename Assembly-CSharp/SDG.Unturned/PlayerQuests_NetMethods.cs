using System;
using SDG.NetPak;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerQuests))]
public static class PlayerQuests_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveMarkerState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveMarkerState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadBit(out var value2);
                reader.ReadClampedVector3(out var value3);
                reader.ReadString(out var value4);
                playerQuests.ReceiveMarkerState(value2, value3, value4);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveMarkerState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveMarkerState_Write(NetPakWriter writer, bool newIsMarkerPlaced, Vector3 newMarkerPosition, string newMarkerTextOverride)
    {
        writer.WriteBit(newIsMarkerPlaced);
        writer.WriteClampedVector3(newMarkerPosition);
        writer.WriteString(newMarkerTextOverride);
    }

    [NetInvokableGeneratedMethod("ReceiveSetMarkerRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSetMarkerRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadBit(out var value2);
            reader.ReadClampedVector3(out var value3);
            playerQuests.ReceiveSetMarkerRequest(value2, value3);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSetMarkerRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSetMarkerRequest_Write(NetPakWriter writer, bool newIsMarkerPlaced, Vector3 newMarkerPosition)
    {
        writer.WriteBit(newIsMarkerPlaced);
        writer.WriteClampedVector3(newMarkerPosition);
    }

    [NetInvokableGeneratedMethod("ReceiveRadioFrequencyState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRadioFrequencyState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadUInt32(out var value2);
                playerQuests.ReceiveRadioFrequencyState(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRadioFrequencyState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRadioFrequencyState_Write(NetPakWriter writer, uint newRadioFrequency)
    {
        writer.WriteUInt32(newRadioFrequency);
    }

    [NetInvokableGeneratedMethod("ReceiveSetRadioFrequencyRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSetRadioFrequencyRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadUInt32(out var value2);
            playerQuests.ReceiveSetRadioFrequencyRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSetRadioFrequencyRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSetRadioFrequencyRequest_Write(NetPakWriter writer, uint newRadioFrequency)
    {
        writer.WriteUInt32(newRadioFrequency);
    }

    [NetInvokableGeneratedMethod("ReceiveGroupState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveGroupState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadSteamID(out CSteamID value2);
                reader.ReadEnum(out var value3);
                playerQuests.ReceiveGroupState(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveGroupState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveGroupState_Write(NetPakWriter writer, CSteamID newGroupID, EPlayerGroupRank newGroupRank)
    {
        writer.WriteSteamID(newGroupID);
        writer.WriteEnum(newGroupRank);
    }

    [NetInvokableGeneratedMethod("ReceiveAcceptGroupInvitationRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAcceptGroupInvitationRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadSteamID(out CSteamID value2);
            playerQuests.ReceiveAcceptGroupInvitationRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAcceptGroupInvitationRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAcceptGroupInvitationRequest_Write(NetPakWriter writer, CSteamID newGroupID)
    {
        writer.WriteSteamID(newGroupID);
    }

    [NetInvokableGeneratedMethod("ReceiveDeclineGroupInvitationRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDeclineGroupInvitationRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadSteamID(out CSteamID value2);
            playerQuests.ReceiveDeclineGroupInvitationRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDeclineGroupInvitationRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDeclineGroupInvitationRequest_Write(NetPakWriter writer, CSteamID newGroupID)
    {
        writer.WriteSteamID(newGroupID);
    }

    [NetInvokableGeneratedMethod("ReceiveLeaveGroupRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLeaveGroupRequest_Read(in ServerInvocationContext context)
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
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
            }
            else
            {
                playerQuests.ReceiveLeaveGroupRequest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveLeaveGroupRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLeaveGroupRequest_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveDeleteGroupRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDeleteGroupRequest_Read(in ServerInvocationContext context)
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
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
            }
            else
            {
                playerQuests.ReceiveDeleteGroupRequest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDeleteGroupRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDeleteGroupRequest_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveCreateGroupRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveCreateGroupRequest_Read(in ServerInvocationContext context)
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
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
            }
            else
            {
                playerQuests.ReceiveCreateGroupRequest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveCreateGroupRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveCreateGroupRequest_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveAddGroupInviteClient", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAddGroupInviteClient_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadSteamID(out CSteamID value2);
                playerQuests.ReceiveAddGroupInviteClient(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAddGroupInviteClient", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAddGroupInviteClient_Write(NetPakWriter writer, CSteamID newGroupID)
    {
        writer.WriteSteamID(newGroupID);
    }

    [NetInvokableGeneratedMethod("ReceiveRemoveGroupInviteClient", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRemoveGroupInviteClient_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadSteamID(out CSteamID value2);
                playerQuests.ReceiveRemoveGroupInviteClient(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRemoveGroupInviteClient", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRemoveGroupInviteClient_Write(NetPakWriter writer, CSteamID newGroupID)
    {
        writer.WriteSteamID(newGroupID);
    }

    [NetInvokableGeneratedMethod("ReceiveAddGroupInviteRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAddGroupInviteRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadSteamID(out CSteamID value2);
            playerQuests.ReceiveAddGroupInviteRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAddGroupInviteRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAddGroupInviteRequest_Write(NetPakWriter writer, CSteamID targetID)
    {
        writer.WriteSteamID(targetID);
    }

    [NetInvokableGeneratedMethod("ReceivePromoteRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePromoteRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadSteamID(out CSteamID value2);
            playerQuests.ReceivePromoteRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePromoteRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePromoteRequest_Write(NetPakWriter writer, CSteamID targetID)
    {
        writer.WriteSteamID(targetID);
    }

    [NetInvokableGeneratedMethod("ReceiveDemoteRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDemoteRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadSteamID(out CSteamID value2);
            playerQuests.ReceiveDemoteRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDemoteRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDemoteRequest_Write(NetPakWriter writer, CSteamID targetID)
    {
        writer.WriteSteamID(targetID);
    }

    [NetInvokableGeneratedMethod("ReceiveKickFromGroup", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveKickFromGroup_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadSteamID(out CSteamID value2);
            playerQuests.ReceiveKickFromGroup(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveKickFromGroup", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveKickFromGroup_Write(NetPakWriter writer, CSteamID targetID)
    {
        writer.WriteSteamID(targetID);
    }

    [NetInvokableGeneratedMethod("ReceiveRenameGroupRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRenameGroupRequest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadString(out var value2);
            playerQuests.ReceiveRenameGroupRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRenameGroupRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRenameGroupRequest_Write(NetPakWriter writer, string newName)
    {
        writer.WriteString(newName);
    }

    [NetInvokableGeneratedMethod("ReceiveSellToVendor", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSellToVendor_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadGuid(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadBit(out var value4);
            playerQuests.ReceiveSellToVendor(in context, value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSellToVendor", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSellToVendor_Write(NetPakWriter writer, Guid assetGuid, byte index, bool asManyAsPossible)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteUInt8(index);
        writer.WriteBit(asManyAsPossible);
    }

    [NetInvokableGeneratedMethod("ReceiveBuyFromVendor", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBuyFromVendor_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadGuid(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadBit(out var value4);
            playerQuests.ReceiveBuyFromVendor(in context, value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBuyFromVendor", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBuyFromVendor_Write(NetPakWriter writer, Guid assetGuid, byte index, bool asManyAsPossible)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteUInt8(index);
        writer.WriteBit(asManyAsPossible);
    }

    [NetInvokableGeneratedMethod("ReceiveSetFlag", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSetFlag_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadUInt16(out var value2);
                reader.ReadInt16(out var value3);
                playerQuests.ReceiveSetFlag(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSetFlag", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSetFlag_Write(NetPakWriter writer, ushort id, short value)
    {
        writer.WriteUInt16(id);
        writer.WriteInt16(value);
    }

    [NetInvokableGeneratedMethod("ReceiveRemoveFlag", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRemoveFlag_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadUInt16(out var value2);
                playerQuests.ReceiveRemoveFlag(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRemoveFlag", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRemoveFlag_Write(NetPakWriter writer, ushort id)
    {
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveAddQuest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAddQuest_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadUInt16(out var value2);
                playerQuests.ReceiveAddQuest(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAddQuest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAddQuest_Write(NetPakWriter writer, ushort id)
    {
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveRemoveQuest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRemoveQuest_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                reader.ReadUInt16(out var value2);
                playerQuests.ReceiveRemoveQuest(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRemoveQuest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRemoveQuest_Write(NetPakWriter writer, ushort id)
    {
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveTrackQuest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTrackQuest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadUInt16(out var value2);
            playerQuests.ReceiveTrackQuest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTrackQuest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTrackQuest_Write(NetPakWriter writer, ushort id)
    {
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveAbandonQuest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAbandonQuest_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadUInt16(out var value2);
            playerQuests.ReceiveAbandonQuest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAbandonQuest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAbandonQuest_Write(NetPakWriter writer, ushort id)
    {
        writer.WriteUInt16(id);
    }

    [NetInvokableGeneratedMethod("ReceiveRegisterMessage", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRegisterMessage_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadGuid(out var value2);
            playerQuests.ReceiveRegisterMessage(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRegisterMessage", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRegisterMessage_Write(NetPakWriter writer, Guid assetGuid)
    {
        writer.WriteGuid(assetGuid);
    }

    [NetInvokableGeneratedMethod("ReceiveRegisterResponse", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRegisterResponse_Read(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj == null)
        {
            return;
        }
        PlayerQuests playerQuests = obj as PlayerQuests;
        if (!(playerQuests == null))
        {
            if (!context.IsOwnerOf(playerQuests.channel))
            {
                context.Kick($"not owner of {playerQuests}");
                return;
            }
            reader.ReadGuid(out var value2);
            reader.ReadUInt8(out var value3);
            playerQuests.ReceiveRegisterResponse(value2, value3);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRegisterResponse", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRegisterResponse_Write(NetPakWriter writer, Guid assetGuid, byte index)
    {
        writer.WriteGuid(assetGuid);
        writer.WriteUInt8(index);
    }

    [NetInvokableGeneratedMethod("ReceiveQuests", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveQuests_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerQuests playerQuests = obj as PlayerQuests;
            if (!(playerQuests == null))
            {
                playerQuests.ReceiveQuests(in context);
            }
        }
    }
}
