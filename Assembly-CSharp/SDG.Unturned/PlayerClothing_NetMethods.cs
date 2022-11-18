using System;
using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerClothing))]
public static class PlayerClothing_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveShirtQuality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveShirtQuality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadUInt8(out var value2);
                playerClothing.ReceiveShirtQuality(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveShirtQuality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveShirtQuality_Write(NetPakWriter writer, byte quality)
    {
        writer.WriteUInt8(quality);
    }

    [NetInvokableGeneratedMethod("ReceivePantsQuality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceivePantsQuality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadUInt8(out var value2);
                playerClothing.ReceivePantsQuality(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceivePantsQuality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceivePantsQuality_Write(NetPakWriter writer, byte quality)
    {
        writer.WriteUInt8(quality);
    }

    [NetInvokableGeneratedMethod("ReceiveHatQuality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveHatQuality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadUInt8(out var value2);
                playerClothing.ReceiveHatQuality(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveHatQuality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveHatQuality_Write(NetPakWriter writer, byte quality)
    {
        writer.WriteUInt8(quality);
    }

    [NetInvokableGeneratedMethod("ReceiveBackpackQuality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBackpackQuality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadUInt8(out var value2);
                playerClothing.ReceiveBackpackQuality(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBackpackQuality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBackpackQuality_Write(NetPakWriter writer, byte quality)
    {
        writer.WriteUInt8(quality);
    }

    [NetInvokableGeneratedMethod("ReceiveVestQuality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVestQuality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadUInt8(out var value2);
                playerClothing.ReceiveVestQuality(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveVestQuality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVestQuality_Write(NetPakWriter writer, byte quality)
    {
        writer.WriteUInt8(quality);
    }

    [NetInvokableGeneratedMethod("ReceiveMaskQuality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveMaskQuality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadUInt8(out var value2);
                playerClothing.ReceiveMaskQuality(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveMaskQuality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveMaskQuality_Write(NetPakWriter writer, byte quality)
    {
        writer.WriteUInt8(quality);
    }

    [NetInvokableGeneratedMethod("ReceiveGlassesQuality", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveGlassesQuality_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadUInt8(out var value2);
                playerClothing.ReceiveGlassesQuality(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveGlassesQuality", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveGlassesQuality_Write(NetPakWriter writer, byte quality)
    {
        writer.WriteUInt8(quality);
    }

    [NetInvokableGeneratedMethod("ReceiveWearShirt", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWearShirt_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadGuid(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                reader.ReadBit(out var value5);
                playerClothing.ReceiveWearShirt(value2, value3, array, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWearShirt", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWearShirt_Write(NetPakWriter writer, Guid id, byte quality, byte[] state, bool playEffect)
    {
        writer.WriteGuid(id);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteBit(playEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapShirtRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapShirtRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerClothing.ReceiveSwapShirtRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapShirtRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapShirtRequest_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveWearPants", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWearPants_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadGuid(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                reader.ReadBit(out var value5);
                playerClothing.ReceiveWearPants(value2, value3, array, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWearPants", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWearPants_Write(NetPakWriter writer, Guid id, byte quality, byte[] state, bool playEffect)
    {
        writer.WriteGuid(id);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteBit(playEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapPantsRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapPantsRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerClothing.ReceiveSwapPantsRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapPantsRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapPantsRequest_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveWearHat", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWearHat_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadGuid(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                reader.ReadBit(out var value5);
                playerClothing.ReceiveWearHat(value2, value3, array, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWearHat", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWearHat_Write(NetPakWriter writer, Guid id, byte quality, byte[] state, bool playEffect)
    {
        writer.WriteGuid(id);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteBit(playEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapHatRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapHatRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerClothing.ReceiveSwapHatRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapHatRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapHatRequest_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveWearBackpack", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWearBackpack_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadGuid(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                reader.ReadBit(out var value5);
                playerClothing.ReceiveWearBackpack(value2, value3, array, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWearBackpack", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWearBackpack_Write(NetPakWriter writer, Guid id, byte quality, byte[] state, bool playEffect)
    {
        writer.WriteGuid(id);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteBit(playEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapBackpackRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapBackpackRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerClothing.ReceiveSwapBackpackRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapBackpackRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapBackpackRequest_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveVisualToggleState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVisualToggleState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadEnum(out var value2);
                reader.ReadBit(out var value3);
                playerClothing.ReceiveVisualToggleState(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveVisualToggleState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVisualToggleState_Write(NetPakWriter writer, EVisualToggleType type, bool toggle)
    {
        writer.WriteEnum(type);
        writer.WriteBit(toggle);
    }

    [NetInvokableGeneratedMethod("ReceiveVisualToggleRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveVisualToggleRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadEnum(out var value2);
            playerClothing.ReceiveVisualToggleRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveVisualToggleRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveVisualToggleRequest_Write(NetPakWriter writer, EVisualToggleType type)
    {
        writer.WriteEnum(type);
    }

    [NetInvokableGeneratedMethod("ReceiveWearVest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWearVest_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadGuid(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                reader.ReadBit(out var value5);
                playerClothing.ReceiveWearVest(value2, value3, array, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWearVest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWearVest_Write(NetPakWriter writer, Guid id, byte quality, byte[] state, bool playEffect)
    {
        writer.WriteGuid(id);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteBit(playEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapVestRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapVestRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerClothing.ReceiveSwapVestRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapVestRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapVestRequest_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveWearMask", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWearMask_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadGuid(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                reader.ReadBit(out var value5);
                playerClothing.ReceiveWearMask(value2, value3, array, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWearMask", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWearMask_Write(NetPakWriter writer, Guid id, byte quality, byte[] state, bool playEffect)
    {
        writer.WriteGuid(id);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteBit(playEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapMaskRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapMaskRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerClothing.ReceiveSwapMaskRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapMaskRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapMaskRequest_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveWearGlasses", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveWearGlasses_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadGuid(out var value2);
                reader.ReadUInt8(out var value3);
                reader.ReadUInt8(out var value4);
                byte[] array = new byte[value4];
                reader.ReadBytes(array);
                reader.ReadBit(out var value5);
                playerClothing.ReceiveWearGlasses(value2, value3, array, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveWearGlasses", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveWearGlasses_Write(NetPakWriter writer, Guid id, byte quality, byte[] state, bool playEffect)
    {
        writer.WriteGuid(id);
        writer.WriteUInt8(quality);
        byte b = (byte)state.Length;
        writer.WriteUInt8(b);
        writer.WriteBytes(state, b);
        writer.WriteBit(playEffect);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapGlassesRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapGlassesRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerClothing.ReceiveSwapGlassesRequest(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapGlassesRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapGlassesRequest_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveClothingState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveClothingState_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                playerClothing.ReceiveClothingState(in context);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFaceState", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveFaceState_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerClothing playerClothing = obj as PlayerClothing;
            if (!(playerClothing == null))
            {
                reader.ReadUInt8(out var value2);
                playerClothing.ReceiveFaceState(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveFaceState", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveFaceState_Write(NetPakWriter writer, byte index)
    {
        writer.WriteUInt8(index);
    }

    [NetInvokableGeneratedMethod("ReceiveSwapFaceRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSwapFaceRequest_Read(in ServerInvocationContext context)
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
        PlayerClothing playerClothing = obj as PlayerClothing;
        if (!(playerClothing == null))
        {
            if (!context.IsOwnerOf(playerClothing.channel))
            {
                context.Kick($"not owner of {playerClothing}");
                return;
            }
            reader.ReadUInt8(out var value2);
            playerClothing.ReceiveSwapFaceRequest(value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSwapFaceRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSwapFaceRequest_Write(NetPakWriter writer, byte index)
    {
        writer.WriteUInt8(index);
    }
}
