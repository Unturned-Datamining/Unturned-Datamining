using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

internal static class ServerMessageHandler_GetWorkshopFiles
{
    private static readonly NetLength MAX_FILES = new NetLength(255u);

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
        int hashCode = transportConnection.GetHashCode();
        float realtimeSinceStartup = Time.realtimeSinceStartup;
        bool flag = false;
        for (int num = Provider.workshopRequests.Count - 1; num >= 0; num--)
        {
            Provider.WorkshopRequestLog value2 = Provider.workshopRequests[num];
            bool flag2 = realtimeSinceStartup - value2.realTime < 30f;
            if (value2.sender == hashCode)
            {
                value2.realTime = realtimeSinceStartup;
                Provider.workshopRequests[num] = value2;
                if (flag2)
                {
                    return;
                }
                flag = true;
                break;
            }
            if (!flag2)
            {
                Provider.workshopRequests.RemoveAtFast(num);
            }
        }
        if (!flag)
        {
            Provider.WorkshopRequestLog item = default(Provider.WorkshopRequestLog);
            item.sender = hashCode;
            item.realTime = realtimeSinceStartup;
            Provider.workshopRequests.Add(item);
        }
        NetMessages.SendMessageToClient(EClientMessage.DownloadWorkshopFiles, ENetReliability.Reliable, transportConnection, delegate(NetPakWriter writer)
        {
            writer.WriteEnum(Provider.authorityHoliday);
            writer.WriteList(Provider.serverRequiredWorkshopFiles, WriteServerRequiredWorkshopFile, MAX_FILES);
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
