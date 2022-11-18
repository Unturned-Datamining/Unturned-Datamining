using System.Collections.Generic;
using SDG.NetPak;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

internal static class ClientMessageHandler_DownloadWorkshopFiles
{
    private static List<PublishedFileId_t> fileIds = new List<PublishedFileId_t>();

    private static readonly NetLength MAX_FILES = new NetLength(255u);

    internal static void ReadMessage(NetPakReader reader)
    {
        Provider.isWaitingForWorkshopResponse = false;
        reader.ReadEnum(out var value);
        fileIds.Clear();
        reader.ReadList(fileIds, (SystemNetPakReaderEx.ReadListItem<PublishedFileId_t>)reader.ReadSteamID, MAX_FILES);
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
        cachedWorkshopResponse.publishedFileIds = fileIds;
        cachedWorkshopResponse.realTime = Time.realtimeSinceStartup;
        Provider.receiveWorkshopResponse(cachedWorkshopResponse);
    }
}
