using System;
using Steamworks;

namespace SDG.Unturned;

/// <summary>
/// Utilities for calling workshop functions without worrying about client/server.
/// This could be nicely refactored into a client and server interface, but not enough time for that right now.
/// </summary>
public class WorkshopUtils
{
    /// <summary>
    /// Client/server safe version of GetQueryUGCNumKeyValueTags.
    /// </summary>
    public static uint getQueryUGCNumKeyValueTags(UGCQueryHandle_t queryHandle, uint resultIndex)
    {
        if (Dedicator.IsDedicatedServer)
        {
            return SteamGameServerUGC.GetQueryUGCNumKeyValueTags(queryHandle, resultIndex);
        }
        return SteamUGC.GetQueryUGCNumKeyValueTags(queryHandle, resultIndex);
    }

    /// <summary>
    /// Client/server safe version of GetQueryUGCKeyValueTag.
    /// </summary>
    public static bool getQueryUGCKeyValueTag(UGCQueryHandle_t queryHandle, uint resultIndex, uint tagIndex, out string key, out string value)
    {
        if (Dedicator.IsDedicatedServer)
        {
            return SteamGameServerUGC.GetQueryUGCKeyValueTag(queryHandle, resultIndex, tagIndex, out key, 255u, out value, 255u);
        }
        return SteamUGC.GetQueryUGCKeyValueTag(queryHandle, resultIndex, tagIndex, out key, 255u, out value, 255u);
    }

    /// <summary>
    /// Search for the value associated with a given key.
    /// </summary>
    public static bool findQueryUGCKeyValue(UGCQueryHandle_t queryHandle, uint resultIndex, string key, out string value)
    {
        uint queryUGCNumKeyValueTags = getQueryUGCNumKeyValueTags(queryHandle, resultIndex);
        for (uint num = 0u; num < queryUGCNumKeyValueTags; num++)
        {
            if (getQueryUGCKeyValueTag(queryHandle, resultIndex, num, out var key2, out var value2) && key2.Equals(key, StringComparison.InvariantCultureIgnoreCase))
            {
                value = value2;
                return true;
            }
        }
        value = null;
        return false;
    }

    /// <summary>
    ///             Client/server safe version of GetQueryUGCResult.
    /// </summary>
    public static bool getQueryUGCResult(UGCQueryHandle_t queryHandle, uint resultIndex, out SteamUGCDetails_t details)
    {
        if (Dedicator.IsDedicatedServer)
        {
            return SteamGameServerUGC.GetQueryUGCResult(queryHandle, resultIndex, out details);
        }
        return SteamUGC.GetQueryUGCResult(queryHandle, resultIndex, out details);
    }

    /// <summary>
    /// Is file banned?
    /// </summary>
    public static bool getQueryUGCBanned(UGCQueryHandle_t queryHandle, uint resultIndex)
    {
        if (getQueryUGCResult(queryHandle, resultIndex, out var details))
        {
            return details.m_bBanned;
        }
        return false;
    }
}
