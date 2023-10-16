using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

public class SteamWhitelist
{
    public static readonly byte SAVEDATA_VERSION = 2;

    private static List<SteamWhitelistID> _list;

    public static List<SteamWhitelistID> list => _list;

    public static void whitelist(CSteamID steamID, string tag, CSteamID judgeID)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].steamID == steamID)
            {
                list[i].tag = tag;
                list[i].judgeID = judgeID;
                return;
            }
        }
        list.Add(new SteamWhitelistID(steamID, tag, judgeID));
    }

    public static bool unwhitelist(CSteamID steamID)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].steamID == steamID)
            {
                if (Provider.isWhitelisted)
                {
                    Provider.kick(steamID, "Removed from whitelist.");
                }
                list.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public static bool checkWhitelisted(CSteamID steamID)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].steamID == steamID)
            {
                return true;
            }
        }
        return false;
    }

    public static void load()
    {
        _list = new List<SteamWhitelistID>();
        if (!ServerSavedata.fileExists("/Server/Whitelist.dat"))
        {
            return;
        }
        River river = ServerSavedata.openRiver("/Server/Whitelist.dat", isReading: true);
        if (river.readByte() > 1)
        {
            ushort num = river.readUInt16();
            for (ushort num2 = 0; num2 < num; num2++)
            {
                CSteamID newSteamID = river.readSteamID();
                string newTag = river.readString();
                CSteamID newJudgeID = river.readSteamID();
                SteamWhitelistID item = new SteamWhitelistID(newSteamID, newTag, newJudgeID);
                list.Add(item);
            }
        }
        river.closeRiver();
    }

    public static void save()
    {
        River river = ServerSavedata.openRiver("/Server/Whitelist.dat", isReading: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeUInt16((ushort)list.Count);
        for (ushort num = 0; num < list.Count; num++)
        {
            SteamWhitelistID steamWhitelistID = list[num];
            river.writeSteamID(steamWhitelistID.steamID);
            river.writeString(steamWhitelistID.tag);
            river.writeSteamID(steamWhitelistID.judgeID);
        }
        river.closeRiver();
    }
}
