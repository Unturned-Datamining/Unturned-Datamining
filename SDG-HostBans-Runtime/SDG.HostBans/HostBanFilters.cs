using System.Collections.Generic;
using System.Text.RegularExpressions;
using SDG.NetPak;

namespace SDG.HostBans;

public class HostBanFilters
{
    public List<HostBanIPv4Filter> addresses;

    public List<HostBanRegexFilter> nameRegexes;

    public List<HostBanRegexFilter> descriptionRegexes;

    public List<HostBanRegexFilter> thumbnailRegexes;

    public List<HostBanSteamIdFilter> steamIds;

    public EHostBanFlags IsAddressMatch(uint ip, ushort port)
    {
        foreach (HostBanIPv4Filter address in addresses)
        {
            if (address.ip == ip && port >= address.minPort && port <= address.maxPort)
            {
                return address.flags;
            }
        }
        return EHostBanFlags.None;
    }

    public EHostBanFlags IsNameMatch(string name)
    {
        return GetRegexResult(nameRegexes, name);
    }

    public EHostBanFlags IsDescriptionMatch(string description)
    {
        return GetRegexResult(descriptionRegexes, description);
    }

    public EHostBanFlags IsThumbnailMatch(string thumbnail)
    {
        return GetRegexResult(thumbnailRegexes, thumbnail);
    }

    public EHostBanFlags IsSteamIdMatch(ulong steamId)
    {
        foreach (HostBanSteamIdFilter steamId2 in steamIds)
        {
            if (steamId2.steamId == steamId)
            {
                return steamId2.flags;
            }
        }
        return EHostBanFlags.None;
    }

    public void ReadConfiguration(NetPakReader reader)
    {
        if (!reader.ReadUInt8(out var value) || value > 4)
        {
            addresses = new List<HostBanIPv4Filter>();
            nameRegexes = new List<HostBanRegexFilter>();
            descriptionRegexes = new List<HostBanRegexFilter>();
            thumbnailRegexes = new List<HostBanRegexFilter>();
            steamIds = new List<HostBanSteamIdFilter>();
            return;
        }
        if (reader.ReadInt32(out var value2) && value2 > 0)
        {
            addresses = new List<HostBanIPv4Filter>(value2);
            for (int i = 0; i < value2; i++)
            {
                HostBanIPv4Filter item = default(HostBanIPv4Filter);
                reader.ReadUInt32(out item.ip);
                reader.ReadUInt16(out item.minPort);
                reader.ReadUInt16(out item.maxPort);
                reader.ReadUInt32(out var value3);
                item.flags = (EHostBanFlags)value3;
                addresses.Add(item);
            }
        }
        else
        {
            addresses = new List<HostBanIPv4Filter>();
        }
        nameRegexes = ReadRegexes(reader);
        if (value > 1)
        {
            descriptionRegexes = ReadRegexes(reader);
            thumbnailRegexes = ReadRegexes(reader);
        }
        else
        {
            descriptionRegexes = new List<HostBanRegexFilter>();
            thumbnailRegexes = new List<HostBanRegexFilter>();
        }
        if (value > 3)
        {
            if (reader.ReadInt32(out var value4) && value4 > 0)
            {
                steamIds = new List<HostBanSteamIdFilter>(value4);
                for (int j = 0; j < value4; j++)
                {
                    HostBanSteamIdFilter item2 = default(HostBanSteamIdFilter);
                    reader.ReadUInt64(out item2.steamId);
                    reader.ReadUInt32(out var value5);
                    item2.flags = (EHostBanFlags)value5;
                    steamIds.Add(item2);
                }
            }
            else
            {
                steamIds = new List<HostBanSteamIdFilter>();
            }
        }
        else
        {
            steamIds = new List<HostBanSteamIdFilter>();
        }
    }

    private EHostBanFlags GetRegexResult(List<HostBanRegexFilter> regexes, string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            foreach (HostBanRegexFilter regex in regexes)
            {
                if (regex.regex.IsMatch(text))
                {
                    return regex.flags;
                }
            }
        }
        return EHostBanFlags.None;
    }

    private List<HostBanRegexFilter> ReadRegexes(NetPakReader reader)
    {
        if (reader.ReadInt32(out var value) && value > 0)
        {
            List<HostBanRegexFilter> list = new List<HostBanRegexFilter>(value);
            for (int i = 0; i < value; i++)
            {
                HostBanRegexFilter item = default(HostBanRegexFilter);
                reader.ReadString(out var value2);
                item.regex = new Regex(value2);
                reader.ReadUInt32(out var value3);
                item.flags = (EHostBanFlags)value3;
                list.Add(item);
            }
            return list;
        }
        return new List<HostBanRegexFilter>();
    }
}
