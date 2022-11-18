using System;
using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

public class SteamBlacklist
{
    public const byte SAVEDATA_VERSION_ADDED_HWID = 4;

    private const byte SAVEDATA_VERSION_NEWEST = 4;

    public static readonly byte SAVEDATA_VERSION = 4;

    public static readonly uint PERMANENT = 31536000u;

    public static readonly uint TEMPORARY = 180u;

    private static List<SteamBlacklistID> _list;

    public static List<SteamBlacklistID> list => _list;

    [Obsolete]
    public static void ban(CSteamID playerID, CSteamID judgeID, string reason, uint duration)
    {
        ban(playerID, 0u, judgeID, reason, duration);
    }

    [Obsolete("Now accepts list of HWIDs")]
    public static void ban(CSteamID playerID, uint ip, CSteamID judgeID, string reason, uint duration)
    {
        ban(playerID, ip, null, judgeID, reason, duration);
    }

    public static void ban(CSteamID playerID, uint ip, IEnumerable<byte[]> hwids, CSteamID judgeID, string reason, uint duration)
    {
        Provider.ban(playerID, reason, duration);
        for (int i = 0; i < SteamBlacklist.list.Count; i++)
        {
            if (SteamBlacklist.list[i].playerID == playerID)
            {
                SteamBlacklist.list[i].judgeID = judgeID;
                SteamBlacklist.list[i].reason = reason;
                SteamBlacklist.list[i].duration = duration;
                SteamBlacklist.list[i].banned = Provider.time;
                return;
            }
        }
        byte[][] newHwids;
        if (hwids != null)
        {
            List<byte[]> list = new List<byte[]>(8);
            foreach (byte[] hwid in hwids)
            {
                list.Add(hwid);
            }
            newHwids = list.ToArray();
        }
        else
        {
            newHwids = null;
        }
        SteamBlacklist.list.Add(new SteamBlacklistID(playerID, ip, judgeID, reason, duration, Provider.time, newHwids));
    }

    public static bool unban(CSteamID playerID)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].playerID == playerID)
            {
                list.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    [Obsolete]
    public static bool checkBanned(CSteamID playerID, out SteamBlacklistID blacklistID)
    {
        return checkBanned(playerID, 0u, out blacklistID);
    }

    [Obsolete("Now checks HWID")]
    public static bool checkBanned(CSteamID playerID, uint ip, out SteamBlacklistID blacklistID)
    {
        return checkBanned(playerID, ip, null, out blacklistID);
    }

    public static bool checkBanned(CSteamID playerID, uint ip, IEnumerable<byte[]> hwids, out SteamBlacklistID blacklistID)
    {
        blacklistID = null;
        for (int num = list.Count - 1; num >= 0; num--)
        {
            if (list[num].playerID == playerID || (list[num].ip == ip && ip != 0) || list[num].DoesAnyHwidMatch(hwids))
            {
                if (list[num].isExpired)
                {
                    list.RemoveAt(num);
                    return false;
                }
                blacklistID = list[num];
                return true;
            }
        }
        return false;
    }

    public static void load()
    {
        _list = new List<SteamBlacklistID>();
        if (!ServerSavedata.fileExists("/Server/Blacklist.dat"))
        {
            return;
        }
        River river = ServerSavedata.openRiver("/Server/Blacklist.dat", isReading: true);
        byte b = river.readByte();
        if (b > 1)
        {
            ushort num = river.readUInt16();
            for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
            {
                CSteamID newPlayerID = river.readSteamID();
                uint newIP = ((b > 2) ? river.readUInt32() : 0u);
                CSteamID newJudgeID = river.readSteamID();
                string newReason = river.readString();
                uint newDuration = river.readUInt32();
                uint newBanned = river.readUInt32();
                byte[][] array;
                if (b >= 4)
                {
                    int num3 = river.readInt32();
                    if (num3 > 0)
                    {
                        array = new byte[num3][];
                        for (int i = 0; i < num3; i++)
                        {
                            array[i] = river.readBytes();
                        }
                    }
                    else
                    {
                        array = null;
                    }
                }
                else
                {
                    array = null;
                }
                SteamBlacklistID steamBlacklistID = new SteamBlacklistID(newPlayerID, newIP, newJudgeID, newReason, newDuration, newBanned, array);
                if (!steamBlacklistID.isExpired)
                {
                    list.Add(steamBlacklistID);
                }
            }
        }
        river.closeRiver();
    }

    public static void save()
    {
        River river = ServerSavedata.openRiver("/Server/Blacklist.dat", isReading: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeUInt16((ushort)list.Count);
        for (ushort num = 0; num < list.Count; num = (ushort)(num + 1))
        {
            SteamBlacklistID steamBlacklistID = list[num];
            river.writeSteamID(steamBlacklistID.playerID);
            river.writeUInt32(steamBlacklistID.ip);
            river.writeSteamID(steamBlacklistID.judgeID);
            river.writeString(steamBlacklistID.reason);
            river.writeUInt32(steamBlacklistID.duration);
            river.writeUInt32(steamBlacklistID.banned);
            if (steamBlacklistID.hwids == null)
            {
                river.writeInt32(0);
            }
            else
            {
                river.writeInt32(steamBlacklistID.hwids.Length);
                byte[][] hwids = steamBlacklistID.hwids;
                foreach (byte[] values in hwids)
                {
                    river.writeBytes(values);
                }
            }
        }
        river.closeRiver();
    }
}
