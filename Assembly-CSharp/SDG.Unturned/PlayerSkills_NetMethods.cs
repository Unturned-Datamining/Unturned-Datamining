using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerSkills))]
public static class PlayerSkills_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveExperience", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveExperience_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerSkills playerSkills = obj as PlayerSkills;
            if (!(playerSkills == null))
            {
                reader.ReadUInt32(out var value2);
                playerSkills.ReceiveExperience(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveExperience", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveExperience_Write(NetPakWriter writer, uint newExperience)
    {
        writer.WriteUInt32(newExperience);
    }

    [NetInvokableGeneratedMethod("ReceiveReputation", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveReputation_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerSkills playerSkills = obj as PlayerSkills;
            if (!(playerSkills == null))
            {
                reader.ReadInt32(out var value2);
                playerSkills.ReceiveReputation(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveReputation", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveReputation_Write(NetPakWriter writer, int newReputation)
    {
        writer.WriteInt32(newReputation);
    }

    [NetInvokableGeneratedMethod("ReceiveBoost", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBoost_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerSkills playerSkills = obj as PlayerSkills;
            if (!(playerSkills == null))
            {
                reader.ReadEnum(out var value2);
                playerSkills.ReceiveBoost(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBoost", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBoost_Write(NetPakWriter writer, EPlayerBoost newBoost)
    {
        writer.WriteEnum(newBoost);
    }

    [NetInvokableGeneratedMethod("ReceiveSingleSkillLevel", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSingleSkillLevel_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerSkills playerSkills = obj as PlayerSkills;
            if (!(playerSkills == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                playerSkills.ReceiveSingleSkillLevel(value2, value3, value4);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSingleSkillLevel", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSingleSkillLevel_Write(NetPakWriter writer, byte speciality, byte index, byte level)
    {
        writer.WriteUInt8(speciality);
        writer.WriteUInt8(index);
        writer.WriteUInt8(level);
    }

    [NetInvokableGeneratedMethod("ReceiveUpgradeRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUpgradeRequest_Read(in ServerInvocationContext context)
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
        PlayerSkills playerSkills = obj as PlayerSkills;
        if (!(playerSkills == null))
        {
            if (!context.IsOwnerOf(playerSkills.channel))
            {
                context.Kick($"not owner of {playerSkills}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadBit(out var value4);
            playerSkills.ReceiveUpgradeRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpgradeRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUpgradeRequest_Write(NetPakWriter writer, byte speciality, byte index, bool force)
    {
        writer.WriteUInt8(speciality);
        writer.WriteUInt8(index);
        writer.WriteBit(force);
    }

    [NetInvokableGeneratedMethod("ReceiveBoostRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBoostRequest_Read(in ServerInvocationContext context)
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
        PlayerSkills playerSkills = obj as PlayerSkills;
        if (!(playerSkills == null))
        {
            if (!context.IsOwnerOf(playerSkills.channel))
            {
                context.Kick($"not owner of {playerSkills}");
            }
            else
            {
                playerSkills.ReceiveBoostRequest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBoostRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBoostRequest_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceivePurchaseRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePurchaseRequest_Read(in ServerInvocationContext context)
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
        PlayerSkills playerSkills = obj as PlayerSkills;
        if (!(playerSkills == null))
        {
            if (!context.IsOwnerOf(playerSkills.channel))
            {
                context.Kick($"not owner of {playerSkills}");
                return;
            }
            reader.ReadNetId(out var value2);
            playerSkills.ReceivePurchaseRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceivePurchaseRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePurchaseRequest_Write(NetPakWriter writer, NetId volumeNetId)
    {
        writer.WriteNetId(volumeNetId);
    }

    [NetInvokableGeneratedMethod("ReceiveMultipleSkillLevels", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveMultipleSkillLevels_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerSkills playerSkills = obj as PlayerSkills;
            if (!(playerSkills == null))
            {
                playerSkills.ReceiveMultipleSkillLevels(in context);
            }
        }
    }
}
