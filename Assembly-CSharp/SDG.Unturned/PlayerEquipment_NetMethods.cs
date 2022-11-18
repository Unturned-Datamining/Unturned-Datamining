using System;
using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerEquipment))]
public static class PlayerEquipment_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveItemHotkeySuggeston", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveItemHotkeySuggeston_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerEquipment playerEquipment = obj as PlayerEquipment;
            if (!(playerEquipment == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadGuid(out var value3);
                reader.ReadUInt8(out var value4);
                reader.ReadUInt8(out var value5);
                reader.ReadUInt8(out var value6);
                playerEquipment.ReceiveItemHotkeySuggeston(in context, value2, value3, value4, value5, value6);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveItemHotkeySuggeston", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveItemHotkeySuggeston_Write(NetPakWriter writer, byte hotkeyIndex, Guid expectedAssetGuid, byte page, byte x, byte y)
    {
        writer.WriteUInt8(hotkeyIndex);
        writer.WriteGuid(expectedAssetGuid);
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveToggleVisionRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveToggleVisionRequest_Read(in ServerInvocationContext context)
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
        PlayerEquipment playerEquipment = obj as PlayerEquipment;
        if (!(playerEquipment == null))
        {
            if (!context.IsOwnerOf(playerEquipment.channel))
            {
                context.Kick($"not owner of {playerEquipment}");
            }
            else
            {
                playerEquipment.ReceiveToggleVisionRequest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToggleVisionRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleVisionRequest_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveToggleVision", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveToggleVision_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerEquipment playerEquipment = obj as PlayerEquipment;
            if (!(playerEquipment == null))
            {
                playerEquipment.ReceiveToggleVision();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveToggleVision", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveToggleVision_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveSlot", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSlot_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerEquipment playerEquipment = obj as PlayerEquipment;
            if (!(playerEquipment == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt16(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                playerEquipment.ReceiveSlot(value2, value3, array);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSlot", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSlot_Write(NetPakWriter writer, byte slot, ushort id, byte[] state)
    {
        writer.WriteUInt8(slot);
        writer.WriteUInt16(id);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateStateTemp", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUpdateStateTemp_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerEquipment playerEquipment = obj as PlayerEquipment;
            if (!(playerEquipment == null))
            {
                reader.ReadUInt8(out var value2);
                byte[] array = new byte[value2];
                reader.ReadBytes(array);
                playerEquipment.ReceiveUpdateStateTemp(array);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateStateTemp", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUpdateStateTemp_Write(NetPakWriter writer, byte[] newState)
    {
        byte b = (byte)newState.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(newState, b);
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUpdateState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerEquipment playerEquipment = obj as PlayerEquipment;
            if (!(playerEquipment == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                playerEquipment.ReceiveUpdateState(value2, value3, array);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUpdateState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUpdateState_Write(NetPakWriter writer, byte page, byte index, byte[] newState)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(index);
        byte b = (byte)newState.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(newState, b);
    }

    [NetInvokableGeneratedMethod("ReceiveEquip", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEquip_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerEquipment playerEquipment = obj as PlayerEquipment;
            if (!(playerEquipment == null))
            {
                reader.ReadUInt8(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                reader.ReadGuid(out var value5);
                reader.ReadUInt8(out var value6);
                reader.ReadUInt8(out var value7);
                byte[] array = new byte[value7];
                reader.ReadBytes(array);
                reader.ReadNetId(out var value8);
                playerEquipment.ReceiveEquip(value2, value3, value4, value5, value6, array, value8);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveEquip", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEquip_Write(NetPakWriter writer, byte page, byte x, byte y, Guid newAssetGuid, byte newQuality, byte[] newState, NetId useableNetId)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
        writer.WriteGuid(newAssetGuid);
        writer.WriteUInt8(newQuality);
        byte b = (byte)newState.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(newState, b);
        writer.WriteNetId(useableNetId);
    }

    [NetInvokableGeneratedMethod("ReceiveEquipRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveEquipRequest_Read(in ServerInvocationContext context)
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
        PlayerEquipment playerEquipment = obj as PlayerEquipment;
        if (!(playerEquipment == null))
        {
            if (!context.IsOwnerOf(playerEquipment.channel))
            {
                context.Kick($"not owner of {playerEquipment}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerEquipment.ReceiveEquipRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveEquipRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveEquipRequest_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }
}
