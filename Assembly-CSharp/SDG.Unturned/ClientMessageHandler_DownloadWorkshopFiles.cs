using System.Collections.Generic;
using SDG.NetPak;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal static class ClientMessageHandler_DownloadWorkshopFiles
{
    private static List<Provider.ServerRequiredWorkshopFile> requiredFiles = new List<Provider.ServerRequiredWorkshopFile>();

    private static readonly NetLength MAX_FILES = new NetLength(255u);

    internal static void ReadMessage(NetPakReader reader)
    {
        Provider.isWaitingForWorkshopResponse = false;
        reader.ReadEnum(out var value);
        UnturnedLog.info($"Server holiday: {value}");
        requiredFiles.Clear();
        reader.ReadList(requiredFiles, ReadRequiredWorkshopFile, MAX_FILES);
        reader.ReadString(out var value2);
        UnturnedLog.info("Server name: \"" + value2 + "\"");
        reader.ReadString(out var value3);
        UnturnedLog.info("Server level name: \"" + value3 + "\"");
        reader.ReadBit(out var value4);
        UnturnedLog.info($"Server is PvP: {value4}");
        reader.ReadBit(out var value5);
        UnturnedLog.info($"Server allows admin cheat codes: {value5}");
        reader.ReadBit(out var value6);
        UnturnedLog.info($"Server is VAC secure: {value6}");
        reader.ReadBit(out var value7);
        UnturnedLog.info($"Server is BattlEye secure: {value7}");
        reader.ReadBit(out var value8);
        UnturnedLog.info($"Server requires gold: {value8}");
        reader.ReadEnum(out var value9);
        UnturnedLog.info($"Server difficulty: {value9}");
        reader.ReadEnum(out var value10);
        UnturnedLog.info($"Server camera mode: {value10}");
        reader.ReadUInt8(out var value11);
        UnturnedLog.info($"Server max players: {value11}");
        reader.ReadString(out var value12);
        UnturnedLog.info("Server bookmark host: \"" + value12 + "\"");
        reader.ReadString(out var value13);
        UnturnedLog.info("Server thumbnail URL: \"" + value13 + "\"");
        reader.ReadString(out var value14);
        UnturnedLog.info("Server description: \"" + value14 + "\"");
        IPv4Address address;
        uint num = (Provider.clientTransport.TryGetIPv4Address(out address) ? address.value : 0u);
        if (num == 0)
        {
            UnturnedLog.warn("Unable to determine server IP for download restrictions");
        }
        Provider.CachedWorkshopResponse cachedWorkshopResponse = null;
        foreach (Provider.CachedWorkshopResponse cachedWorkshopResponse2 in Provider.cachedWorkshopResponses)
        {
            if (cachedWorkshopResponse2.server == Provider.server)
            {
                cachedWorkshopResponse = cachedWorkshopResponse2;
                break;
            }
        }
        if (cachedWorkshopResponse == null)
        {
            cachedWorkshopResponse = new Provider.CachedWorkshopResponse();
            cachedWorkshopResponse.server = Provider.server;
            Provider.cachedWorkshopResponses.Add(cachedWorkshopResponse);
        }
        cachedWorkshopResponse.holiday = value;
        cachedWorkshopResponse.serverName = value2;
        cachedWorkshopResponse.levelName = value3;
        cachedWorkshopResponse.isPvP = value4;
        cachedWorkshopResponse.allowAdminCheatCodes = value5;
        cachedWorkshopResponse.isVACSecure = value6;
        cachedWorkshopResponse.isBattlEyeSecure = value7;
        cachedWorkshopResponse.isGold = value8;
        cachedWorkshopResponse.gameMode = value9;
        cachedWorkshopResponse.cameraMode = value10;
        cachedWorkshopResponse.maxPlayers = value11;
        cachedWorkshopResponse.bookmarkHost = value12;
        cachedWorkshopResponse.thumbnailUrl = value13;
        cachedWorkshopResponse.serverDescription = value14;
        cachedWorkshopResponse.ip = num;
        cachedWorkshopResponse.requiredFiles = requiredFiles;
        cachedWorkshopResponse.realTime = Time.realtimeSinceStartup;
        Provider.receiveWorkshopResponse(cachedWorkshopResponse);
    }

    private static bool ReadRequiredWorkshopFile(NetPakReader reader, out Provider.ServerRequiredWorkshopFile requiredFile)
    {
        requiredFile = default(Provider.ServerRequiredWorkshopFile);
        if (reader.ReadUInt64(out requiredFile.fileId))
        {
            return reader.ReadDateTime(out requiredFile.timestamp);
        }
        return false;
    }
}
