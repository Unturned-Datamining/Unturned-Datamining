using System;
using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

internal static class ServerMessageHandler_GetWorkshopFiles
{
    private static readonly NetLength MAX_FILES = new NetLength(255u);

    /// <summary>
    /// Nelson 2025-05-13: replacing the "workshop request log" which used transport connection hash code with this
    /// more recent IP address and Steam ID rate limiter.
    /// </summary>
    private static TransportConnectionRateLimiter rateLimiter = new TransportConnectionRateLimiter
    {
        window = 30f,
        threshold = 1
    };

    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        if (!reader.ReadBytesPtr(960, out var _, out var _))
        {
            Provider.refuseGarbageConnection(transportConnection, "missing empty buffer");
            return;
        }
        if (!reader.ReadString(out var value))
        {
            Provider.refuseGarbageConnection(transportConnection, "failed to read header string");
            return;
        }
        if (!string.Equals(value, "Hello!", StringComparison.Ordinal))
        {
            Provider.refuseGarbageConnection(transportConnection, "invalid header string");
            return;
        }
        if (rateLimiter.IsBlocked(transportConnection))
        {
            if ((bool)NetMessages.shouldLogBadMessages)
            {
                UnturnedLog.info($"Ignoring GetWorkshopFiles message from {transportConnection} because they requested recently");
            }
            Provider.IncrementBadPacketsFromConnection(transportConnection);
            return;
        }
        NetMessages.SendMessageToClient(EClientMessage.DownloadWorkshopFiles, ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteEnum(Provider.authorityHoliday);
            writer.WriteList(Provider.serverRequiredWorkshopFiles, WriteServerRequiredWorkshopFile, MAX_FILES);
            writer.WriteString(Provider.serverName);
            writer.WriteString(Provider.map);
            writer.WriteBit(Provider.isPvP);
            writer.WriteBit(Provider.hasCheats);
            writer.WriteBit(Provider.isVacActive);
            writer.WriteBit(Provider.isThirdpartyAntiCheatActive);
            writer.WriteBit(Provider.isGold);
            writer.WriteEnum(Provider.mode);
            writer.WriteEnum(Provider.cameraMode);
            writer.WriteUInt8(Provider.maxPlayers);
            writer.WriteString(Provider.configData.Browser.BookmarkHost);
            writer.WriteString(Provider.configData.Browser.Thumbnail);
            writer.WriteString(Provider.configData.Browser.Desc_Server_List);
        });
    }

    private static bool WriteServerRequiredWorkshopFile(NetPakWriter writer, Provider.ServerRequiredWorkshopFile item)
    {
        if (writer.WriteUInt64(item.fileId))
        {
            return writer.WriteDateTime(item.timestamp);
        }
        return false;
    }
}
