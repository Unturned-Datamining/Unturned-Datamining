using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(PlayerCrafting))]
public static class PlayerCrafting_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveStripAttachments", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveStripAttachments_Read(in ServerInvocationContext context)
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
        PlayerCrafting playerCrafting = obj as PlayerCrafting;
        if (!(playerCrafting == null))
        {
            if (!context.IsOwnerOf(playerCrafting.channel))
            {
                context.Kick($"not owner of {playerCrafting}");
                return;
            }
            reader.ReadUInt8(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadUInt8(out var value4);
            playerCrafting.ReceiveStripAttachments(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveStripAttachments", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveStripAttachments_Write(NetPakWriter writer, byte page, byte x, byte y)
    {
        writer.WriteUInt8(page);
        writer.WriteUInt8(x);
        writer.WriteUInt8(y);
    }

    [NetInvokableGeneratedMethod("ReceiveRefreshCrafting", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRefreshCrafting_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            PlayerCrafting playerCrafting = obj as PlayerCrafting;
            if (!(playerCrafting == null))
            {
                playerCrafting.ReceiveRefreshCrafting();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRefreshCrafting", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRefreshCrafting_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveCraft", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveCraft_Read(in ServerInvocationContext context)
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
        PlayerCrafting playerCrafting = obj as PlayerCrafting;
        if (!(playerCrafting == null))
        {
            if (!context.IsOwnerOf(playerCrafting.channel))
            {
                context.Kick($"not owner of {playerCrafting}");
                return;
            }
            reader.ReadUInt16(out var value2);
            reader.ReadUInt8(out var value3);
            reader.ReadBit(out var value4);
            playerCrafting.ReceiveCraft(value2, value3, value4);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveCraft", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveCraft_Write(NetPakWriter writer, ushort id, byte index, bool force)
    {
        writer.WriteUInt16(id);
        writer.WriteUInt8(index);
        writer.WriteBit(force);
    }
}
