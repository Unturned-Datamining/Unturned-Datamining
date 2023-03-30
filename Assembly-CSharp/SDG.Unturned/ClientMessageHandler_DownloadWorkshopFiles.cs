using System.Collections.Generic;
using SDG.NetPak;
using UnityEngine;

namespace SDG.Unturned;

internal static class ClientMessageHandler_DownloadWorkshopFiles
{
    private static List<Provider.ServerRequiredWorkshopFile> requiredFiles = new List<Provider.ServerRequiredWorkshopFile>();

    private static readonly NetLength MAX_FILES = new NetLength(255u);

    internal static void ReadMessage(NetPakReader reader)
    {
        Provider.isWaitingForWorkshopResponse = false;
        reader.ReadEnum(out var value);
        requiredFiles.Clear();
        reader.ReadList(requiredFiles, ReadRequiredWorkshopFile, MAX_FILES);
        uint ip = Provider.currentServerInfo.ip;
        if (ip == 0)
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
        cachedWorkshopResponse.ip = ip;
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
