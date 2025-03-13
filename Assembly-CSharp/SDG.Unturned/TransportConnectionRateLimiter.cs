using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Counts hits per-IPv4 address (if available) and per-SteamID (if available).
/// Connection is blocked if more than "threshold" hits occur within category (IPv4/SteamID).
/// Hit count resets when "window" seconds have passed since last hit.
/// </summary>
internal class TransportConnectionRateLimiter
{
    private struct AddressRateLimitingEntry
    {
        public uint address;

        public int counter;

        public double realtime;
    }

    private struct SteamIdRateLimitingEntry
    {
        public CSteamID steamId;

        public int counter;

        public double realtime;
    }

    private enum ERateLimitingResult
    {
        NOT_IN_LIST,
        HIT_RATE_LIMIT,
        WITHIN_RATE_LIMIT
    }

    /// <summary>
    /// If hit is within this many seconds of previous hit, it counts. Otherwise, counter is reset.
    /// </summary>
    public float window = 40f;

    /// <summary>
    /// If more than this many hits occur the limit is reached.
    /// </summary>
    public int threshold = 2;

    private List<AddressRateLimitingEntry> addressRateLimitingLog = new List<AddressRateLimitingEntry>();

    private List<SteamIdRateLimitingEntry> steamIdRateLimitingLog = new List<SteamIdRateLimitingEntry>();

    public bool IsBlocked(ITransportConnection transportConnection)
    {
        bool flag = false;
        if (transportConnection.TryGetSteamId(out var steamId))
        {
            flag |= IsBlockedBySteamIdRateLimiting(new CSteamID(steamId));
        }
        if (!Provider.configData.Server.Use_FakeIP && transportConnection.TryGetIPv4Address(out var address))
        {
            flag |= IsBlockedByAddressRateLimiting(address);
        }
        return flag;
    }

    public bool IsBlockedByAddressRateLimiting(uint connectionAddress)
    {
        double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
        ERateLimitingResult eRateLimitingResult = ERateLimitingResult.NOT_IN_LIST;
        for (int num = addressRateLimitingLog.Count - 1; num >= 0; num--)
        {
            AddressRateLimitingEntry value = addressRateLimitingLog[num];
            if (realtimeSinceStartupAsDouble - value.realtime > (double)window)
            {
                addressRateLimitingLog.RemoveAt(num);
            }
            else if (eRateLimitingResult == ERateLimitingResult.NOT_IN_LIST && value.address == connectionAddress)
            {
                value.counter++;
                value.realtime = realtimeSinceStartupAsDouble;
                addressRateLimitingLog[num] = value;
                eRateLimitingResult = ((value.counter > threshold) ? ERateLimitingResult.HIT_RATE_LIMIT : ERateLimitingResult.WITHIN_RATE_LIMIT);
            }
        }
        if (eRateLimitingResult != 0)
        {
            return eRateLimitingResult == ERateLimitingResult.HIT_RATE_LIMIT;
        }
        AddressRateLimitingEntry item = default(AddressRateLimitingEntry);
        item.address = connectionAddress;
        item.counter = 1;
        item.realtime = realtimeSinceStartupAsDouble;
        addressRateLimitingLog.Add(item);
        return false;
    }

    public bool IsBlockedBySteamIdRateLimiting(CSteamID connectionSteamId)
    {
        double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
        ERateLimitingResult eRateLimitingResult = ERateLimitingResult.NOT_IN_LIST;
        for (int num = steamIdRateLimitingLog.Count - 1; num >= 0; num--)
        {
            SteamIdRateLimitingEntry value = steamIdRateLimitingLog[num];
            if (realtimeSinceStartupAsDouble - value.realtime > (double)window)
            {
                steamIdRateLimitingLog.RemoveAt(num);
            }
            else if (eRateLimitingResult == ERateLimitingResult.NOT_IN_LIST && value.steamId == connectionSteamId)
            {
                value.counter++;
                value.realtime = realtimeSinceStartupAsDouble;
                steamIdRateLimitingLog[num] = value;
                eRateLimitingResult = ((value.counter > threshold) ? ERateLimitingResult.HIT_RATE_LIMIT : ERateLimitingResult.WITHIN_RATE_LIMIT);
            }
        }
        if (eRateLimitingResult != 0)
        {
            return eRateLimitingResult == ERateLimitingResult.HIT_RATE_LIMIT;
        }
        SteamIdRateLimitingEntry item = default(SteamIdRateLimitingEntry);
        item.steamId = connectionSteamId;
        item.counter = 1;
        item.realtime = realtimeSinceStartupAsDouble;
        steamIdRateLimitingLog.Add(item);
        return false;
    }
}
