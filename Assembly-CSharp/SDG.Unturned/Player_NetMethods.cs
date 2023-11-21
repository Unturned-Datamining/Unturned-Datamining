using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(Player))]
public static class Player_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveScreenshotDestination", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveScreenshotDestination_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                player.ReceiveScreenshotDestination(in context);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveScreenshotRelay", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveScreenshotRelay_Read(in ServerInvocationContext context)
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
        Player player = obj as Player;
        if (!(player == null))
        {
            if (!context.IsOwnerOf(player.channel))
            {
                context.Kick($"not owner of {player}");
            }
            else
            {
                player.ReceiveScreenshotRelay(in context);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTakeScreenshot", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTakeScreenshot_Read(in ClientInvocationContext context)
    {
        if (!context.reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                player.ReceiveTakeScreenshot();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTakeScreenshot", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTakeScreenshot_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveBrowserRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBrowserRequest_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadString(out var value2);
                reader.ReadString(out var value3);
                player.ReceiveBrowserRequest(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBrowserRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBrowserRequest_Write(NetPakWriter writer, string msg, string url)
    {
        writer.WriteString(msg);
        writer.WriteString(url);
    }

    [NetInvokableGeneratedMethod("ReceiveHintMessage", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveHintMessage_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadString(out var value2);
                reader.ReadFloat(out var value3);
                player.ReceiveHintMessage(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveHintMessage", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveHintMessage_Write(NetPakWriter writer, string message, float durationSeconds)
    {
        writer.WriteString(message);
        writer.WriteFloat(durationSeconds);
    }

    [NetInvokableGeneratedMethod("ReceiveRelayToServer", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveRelayToServer_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadUInt32(out var value2);
                reader.ReadUInt16(out var value3);
                reader.ReadString(out var value4);
                reader.ReadBit(out var value5);
                player.ReceiveRelayToServer(value2, value3, value4, value5);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveRelayToServer", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveRelayToServer_Write(NetPakWriter writer, uint ip, ushort port, string password, bool shouldShowMenu)
    {
        writer.WriteUInt32(ip);
        writer.WriteUInt16(port);
        writer.WriteString(password);
        writer.WriteBit(shouldShowMenu);
    }

    [NetInvokableGeneratedMethod("ReceiveSetPluginWidgetFlags", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveSetPluginWidgetFlags_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadUInt32(out var value2);
                player.ReceiveSetPluginWidgetFlags(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveSetPluginWidgetFlags", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveSetPluginWidgetFlags_Write(NetPakWriter writer, uint newFlags)
    {
        writer.WriteUInt32(newFlags);
    }

    [NetInvokableGeneratedMethod("ReceiveAdminUsageFlags", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAdminUsageFlags_Read(in ServerInvocationContext context)
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
        Player player = obj as Player;
        if (!(player == null))
        {
            if (!context.IsOwnerOf(player.channel))
            {
                context.Kick($"not owner of {player}");
                return;
            }
            reader.ReadUInt32(out var value2);
            player.ReceiveAdminUsageFlags(in context, value2);
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAdminUsageFlags", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAdminUsageFlags_Write(NetPakWriter writer, uint newFlagsBitmask)
    {
        writer.WriteUInt32(newFlagsBitmask);
    }

    [NetInvokableGeneratedMethod("ReceiveBattlEyeLogsRequest", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBattlEyeLogsRequest_Read(in ServerInvocationContext context)
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
        Player player = obj as Player;
        if (!(player == null))
        {
            if (!context.IsOwnerOf(player.channel))
            {
                context.Kick($"not owner of {player}");
            }
            else
            {
                player.ReceiveBattlEyeLogsRequest();
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveBattlEyeLogsRequest", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBattlEyeLogsRequest_Write(NetPakWriter writer)
    {
    }

    [NetInvokableGeneratedMethod("ReceiveTerminalRelay", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTerminalRelay_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadString(out var value2);
                player.ReceiveTerminalRelay(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTerminalRelay", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTerminalRelay_Write(NetPakWriter writer, string internalMessage)
    {
        writer.WriteString(internalMessage);
    }

    [NetInvokableGeneratedMethod("ReceiveTeleport", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveTeleport_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadClampedVector3(out var value2);
                reader.ReadUInt8(out var value3);
                player.ReceiveTeleport(value2, value3);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveTeleport", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveTeleport_Write(NetPakWriter writer, Vector3 position, byte angle)
    {
        writer.WriteClampedVector3(position);
        writer.WriteUInt8(angle);
    }

    [NetInvokableGeneratedMethod("ReceiveStat", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveStat_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadEnum(out var value2);
                player.ReceiveStat(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveStat", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveStat_Write(NetPakWriter writer, EPlayerStat stat)
    {
        writer.WriteEnum(stat);
    }

    [NetInvokableGeneratedMethod("ReceiveAchievementUnlocked", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveAchievementUnlocked_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadString(out var value2);
                player.ReceiveAchievementUnlocked(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveAchievementUnlocked", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveAchievementUnlocked_Write(NetPakWriter writer, string id)
    {
        writer.WriteString(id);
    }

    [NetInvokableGeneratedMethod("ReceiveUIMessage", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveUIMessage_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (!reader.ReadNetId(out var value))
        {
            return;
        }
        object obj = NetIdRegistry.Get(value);
        if (obj != null)
        {
            Player player = obj as Player;
            if (!(player == null))
            {
                reader.ReadEnum(out var value2);
                player.ReceiveUIMessage(value2);
            }
        }
    }

    [NetInvokableGeneratedMethod("ReceiveUIMessage", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveUIMessage_Write(NetPakWriter writer, EPlayerMessage message)
    {
        writer.WriteEnum(message);
    }
}
