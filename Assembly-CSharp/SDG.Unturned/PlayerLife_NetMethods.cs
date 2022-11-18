using SDG.NetPak;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerLife))]
public static class PlayerLife_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveDeath", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDeath_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadEnum(out var value2);
                reader.ReadEnum(out var value3);
                reader.ReadSteamID(out CSteamID value4);
                playerLife.ReceiveDeath(value2, value3, value4);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDeath", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDeath_Write(NetPakWriter writer, EDeathCause newCause, ELimb newLimb, CSteamID newKiller)
    {
        writer.WriteEnum(newCause);
        writer.WriteEnum(newLimb);
        writer.WriteSteamID(newKiller);
    }

    [NetInvokableGeneratedMethod("ReceiveDead", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDead_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadClampedVector3(out var value2);
                reader.ReadEnum(out var value3);
                playerLife.ReceiveDead(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDead", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDead_Write(NetPakWriter writer, Vector3 newRagdoll, ERagdollEffect newRagdollEffect)
    {
        writer.WriteClampedVector3(newRagdoll);
        writer.WriteEnum(newRagdollEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveRevive", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRevive_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadClampedVector3(out var value2);
                reader.ReadUInt8(out var value3);
                playerLife.ReceiveRevive(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRevive", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRevive_Write(NetPakWriter writer, Vector3 position, byte angle)
    {
        writer.WriteClampedVector3(position);
        writer.WriteUInt8(angle);
    }

    [NetInvokableGeneratedMethod("ReceiveLifeStats", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveLifeStats_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                reader.ReadUInt8(out var value5);
                reader.ReadUInt8(out var value6);
                reader.ReadBit(out var value7);
                reader.ReadBit(out var value8);
                playerLife.ReceiveLifeStats(value2, value3, value4, value5, value6, value7, value8);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveLifeStats", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveLifeStats_Write(NetPakWriter writer, byte newHealth, byte newFood, byte newWater, byte newVirus, byte newOxygen, bool newBleeding, bool newBroken)
    {
        writer.WriteUInt8(newHealth);
        writer.WriteUInt8(newFood);
        writer.WriteUInt8(newWater);
        writer.WriteUInt8(newVirus);
        writer.WriteUInt8(newOxygen);
        writer.WriteBit(newBleeding);
        writer.WriteBit(newBroken);
    }

    [NetInvokableGeneratedMethod("ReceiveHealth", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveHealth_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadUInt8(out var value2);
                playerLife.ReceiveHealth(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveHealth", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveHealth_Write(NetPakWriter writer, byte newHealth)
    {
        writer.WriteUInt8(newHealth);
    }

    [NetInvokableGeneratedMethod("ReceiveDamagedEvent", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveDamagedEvent_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadClampedVector3(out var value3);
                playerLife.ReceiveDamagedEvent(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveDamagedEvent", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveDamagedEvent_Write(NetPakWriter writer, byte damageAmount, Vector3 damageDirection)
    {
        writer.WriteUInt8(damageAmount);
        writer.WriteClampedVector3(damageDirection);
    }

    [NetInvokableGeneratedMethod("ReceiveFood", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveFood_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadUInt8(out var value2);
                playerLife.ReceiveFood(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFood", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveFood_Write(NetPakWriter writer, byte newFood)
    {
        writer.WriteUInt8(newFood);
    }

    [NetInvokableGeneratedMethod("ReceiveWater", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWater_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadUInt8(out var value2);
                playerLife.ReceiveWater(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWater", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWater_Write(NetPakWriter writer, byte newWater)
    {
        writer.WriteUInt8(newWater);
    }

    [NetInvokableGeneratedMethod("ReceiveVirus", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVirus_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadUInt8(out var value2);
                playerLife.ReceiveVirus(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveVirus", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVirus_Write(NetPakWriter writer, byte newVirus)
    {
        writer.WriteUInt8(newVirus);
    }

    [NetInvokableGeneratedMethod("ReceiveBleeding", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBleeding_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadBit(out var value2);
                playerLife.ReceiveBleeding(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBleeding", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBleeding_Write(NetPakWriter writer, bool newBleeding)
    {
        writer.WriteBit(newBleeding);
    }

    [NetInvokableGeneratedMethod("ReceiveBroken", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBroken_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadBit(out var value2);
                playerLife.ReceiveBroken(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBroken", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBroken_Write(NetPakWriter writer, bool newBroken)
    {
        writer.WriteBit(newBroken);
    }

    [NetInvokableGeneratedMethod("ReceiveModifyStamina", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveModifyStamina_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadInt16(out var value2);
                playerLife.ReceiveModifyStamina(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveModifyStamina", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveModifyStamina_Write(NetPakWriter writer, short delta)
    {
        writer.WriteInt16(delta);
    }

    [NetInvokableGeneratedMethod("ReceiveModifyHallucination", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveModifyHallucination_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadInt16(out var value2);
                playerLife.ReceiveModifyHallucination(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveModifyHallucination", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveModifyHallucination_Write(NetPakWriter writer, short delta)
    {
        writer.WriteInt16(delta);
    }

    [NetInvokableGeneratedMethod("ReceiveModifyWarmth", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveModifyWarmth_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerLife playerLife = obj as PlayerLife;
            if (!(playerLife == null))
            {
                reader.ReadInt16(out var value2);
                playerLife.ReceiveModifyWarmth(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveModifyWarmth", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveModifyWarmth_Write(NetPakWriter writer, short delta)
    {
        writer.WriteInt16(delta);
    }

    [NetInvokableGeneratedMethod("ReceiveRespawnRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRespawnRequest_Read(in ServerInvocationContext context)
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
        PlayerLife playerLife = obj as PlayerLife;
        if (!(playerLife == null))
        {
            if (!context.IsOwnerOf(playerLife.channel))
            {
                context.Kick($"not owner of {playerLife}");
                return;
            }
            reader.ReadBit(out var value2);
            playerLife.ReceiveRespawnRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRespawnRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRespawnRequest_Write(NetPakWriter writer, bool atHome)
    {
        writer.WriteBit(atHome);
    }

    [NetInvokableGeneratedMethod("ReceiveSuicideRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSuicideRequest_Read(in ServerInvocationContext context)
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
        PlayerLife playerLife = obj as PlayerLife;
        if (!(playerLife == null))
        {
            if (!context.IsOwnerOf(playerLife.channel))
            {
                context.Kick($"not owner of {playerLife}");
            }
            else
            {
                playerLife.ReceiveSuicideRequest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSuicideRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSuicideRequest_Write(NetPakWriter writer)
    {
    }
}
