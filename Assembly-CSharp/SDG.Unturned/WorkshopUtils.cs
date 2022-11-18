using System;
using Steamworks;

namespace SDG.Unturned;

public class WorkshopUtils
{
    public static uint getQueryUGCNumKeyValueTags(UGCQueryHandle_t queryHandle, uint resultIndex)
    {
        return SteamGameServerUGC.GetQueryUGCNumKeyValueTags(queryHandle, resultIndex);
    }

    public static bool getQueryUGCKeyValueTag(UGCQueryHandle_t queryHandle, uint resultIndex, uint tagIndex, out string key, out string value)
    {
        return SteamGameServerUGC.GetQueryUGCKeyValueTag(queryHandle, resultIndex, tagIndex, out key, 255u, out value, 255u);
    }

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

    public static bool getQueryUGCResult(UGCQueryHandle_t queryHandle, uint resultIndex, out SteamUGCDetails_t details)
    {
        return SteamGameServerUGC.GetQueryUGCResult(queryHandle, resultIndex, out details);
    }

    public static bool getQueryUGCBanned(UGCQueryHandle_t queryHandle, uint resultIndex)
    {
        if (getQueryUGCResult(queryHandle, resultIndex, out var details))
        {
            return details.m_bBanned;
        }
        return false;
    }
}
