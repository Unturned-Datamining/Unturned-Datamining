using System;
using Steamworks;

namespace SDG.Unturned;

public class WorkshopDownloadRestrictions
{
    public static readonly string IP_RESTRICTIONS_KVTAG = "allowed_ips";

    private static readonly char[] IP_SEPARATOR = new char[1] { ',' };

    public static string getAllowedIpsTagValue(UGCQueryHandle_t queryHandle, uint resultIndex)
    {
        WorkshopUtils.findQueryUGCKeyValue(queryHandle, resultIndex, IP_RESTRICTIONS_KVTAG, out var value);
        return value;
    }

    public static EWorkshopDownloadRestrictionResult getRestrictionResult(UGCQueryHandle_t queryHandle, uint resultIndex, uint ip)
    {
        if (WorkshopUtils.getQueryUGCResult(queryHandle, resultIndex, out var details))
        {
            if (details.m_bBanned)
            {
                return EWorkshopDownloadRestrictionResult.Banned;
            }
            if (details.m_eVisibility == ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate || details.m_eResult == EResult.k_EResultAccessDenied)
            {
                return EWorkshopDownloadRestrictionResult.PrivateVisibility;
            }
        }
        string allowedIpsTagValue = getAllowedIpsTagValue(queryHandle, resultIndex);
        if (string.IsNullOrEmpty(allowedIpsTagValue))
        {
            return EWorkshopDownloadRestrictionResult.NoRestrictions;
        }
        return getRestrictionResult(allowedIpsTagValue, ip);
    }

    public static EWorkshopDownloadRestrictionResult getRestrictionResult(string filter, uint ip)
    {
        parseAllowedIPs(filter, out var whitelist, out var blacklist);
        if (whitelist != null && whitelist.Length != 0)
        {
            if (isAddressInList(ip, whitelist))
            {
                return EWorkshopDownloadRestrictionResult.Allowed;
            }
            return EWorkshopDownloadRestrictionResult.NotWhitelisted;
        }
        if (blacklist != null && blacklist.Length != 0)
        {
            if (isAddressInList(ip, blacklist))
            {
                return EWorkshopDownloadRestrictionResult.Blacklisted;
            }
            return EWorkshopDownloadRestrictionResult.Allowed;
        }
        return EWorkshopDownloadRestrictionResult.NoRestrictions;
    }

    public static void splitAllowedIPs(string allowedIPs, out string[] whitelistIps, out string[] blacklistIps)
    {
        whitelistIps = null;
        blacklistIps = null;
        int num = allowedIPs.IndexOf('-');
        if (num >= 0 && num < allowedIPs.Length - 1)
        {
            string text = allowedIPs.Substring(0, num);
            string text2 = allowedIPs.Substring(num + 1);
            whitelistIps = text.Split(IP_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            blacklistIps = text2.Split(IP_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            whitelistIps = allowedIPs.Split(',');
        }
    }

    public static void parseAllowedIPs(string allowedIPs, out uint[] whitelist, out uint[] blacklist)
    {
        splitAllowedIPs(allowedIPs, out var whitelistIps, out var blacklistIps);
        if (whitelistIps == null || whitelistIps.Length < 1)
        {
            whitelist = null;
        }
        else
        {
            parseStringIps(whitelistIps, out whitelist);
        }
        if (blacklistIps == null || blacklistIps.Length < 1)
        {
            blacklist = null;
        }
        else
        {
            parseStringIps(blacklistIps, out blacklist);
        }
    }

    public static void parseStringIps(string[] strings, out uint[] integers)
    {
        int num = strings.Length;
        integers = new uint[num];
        for (int i = 0; i < num; i++)
        {
            integers[i] = Parser.getUInt32FromIP(strings[i]);
        }
    }

    public static bool isAddressInList(uint ip, uint[] list)
    {
        foreach (uint num in list)
        {
            if (ip == num)
            {
                return true;
            }
        }
        return false;
    }
}
