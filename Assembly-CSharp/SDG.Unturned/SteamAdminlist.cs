using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned;

public class SteamAdminlist
{
    public static readonly byte SAVEDATA_VERSION = 2;

    private static List<SteamAdminID> _list;

    public static CSteamID ownerID;

    public static List<SteamAdminID> list => _list;

    public static void admin(CSteamID playerID, CSteamID judgeID)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].playerID == playerID)
            {
                list[i].judgeID = judgeID;
                return;
            }
        }
        list.Add(new SteamAdminID(playerID, judgeID));
        SteamPlayer client = PlayerTool.getSteamPlayer(playerID);
        if (client != null)
        {
            client.isAdmin = true;
            NetMessages.SendMessageToClients(EClientMessage.Admined, ENetReliability.Reliable, Provider.GatherRemoteClientConnectionsMatchingPredicate((SteamPlayer potentialRecipient) => potentialRecipient == client || !Provider.hideAdmins), delegate(NetPakWriter writer)
            {
                writer.WriteUInt8((byte)client.channel);
            });
        }
    }

    public static void unadmin(CSteamID playerID)
    {
        SteamPlayer client = PlayerTool.getSteamPlayer(playerID);
        if (client != null && client.isAdmin)
        {
            client.isAdmin = false;
            NetMessages.SendMessageToClients(EClientMessage.Unadmined, ENetReliability.Reliable, Provider.GatherRemoteClientConnectionsMatchingPredicate((SteamPlayer potentialRecipient) => potentialRecipient == client || !Provider.hideAdmins), delegate(NetPakWriter writer)
            {
                writer.WriteUInt8((byte)client.channel);
            });
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].playerID == playerID)
            {
                list.RemoveAt(i);
                break;
            }
        }
    }

    public static bool checkAC(CSteamID playerID)
    {
        UnturnedLog.info(playerID);
        byte[] array = Hash.SHA1(playerID);
        string text = "";
        for (int i = 0; i < array.Length; i++)
        {
            if (i > 0)
            {
                text += ", ";
            }
            text += array[i];
        }
        UnturnedLog.info(text);
        return false;
    }

    public static bool checkAdmin(CSteamID playerID)
    {
        if (playerID == ownerID)
        {
            return true;
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].playerID == playerID)
            {
                return true;
            }
        }
        return false;
    }

    public static void load()
    {
        _list = new List<SteamAdminID>();
        ownerID = CSteamID.Nil;
        if (!ServerSavedata.fileExists("/Server/Adminlist.dat"))
        {
            return;
        }
        River river = ServerSavedata.openRiver("/Server/Adminlist.dat", isReading: true);
        if (river.readByte() > 1)
        {
            ushort num = river.readUInt16();
            for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
            {
                CSteamID newPlayerID = river.readSteamID();
                CSteamID newJudgeID = river.readSteamID();
                SteamAdminID item = new SteamAdminID(newPlayerID, newJudgeID);
                list.Add(item);
            }
        }
        river.closeRiver();
    }

    public static void save()
    {
        River river = ServerSavedata.openRiver("/Server/Adminlist.dat", isReading: false);
        river.writeByte(SAVEDATA_VERSION);
        river.writeUInt16((ushort)list.Count);
        for (ushort num = 0; num < list.Count; num = (ushort)(num + 1))
        {
            SteamAdminID steamAdminID = list[num];
            river.writeSteamID(steamAdminID.playerID);
            river.writeSteamID(steamAdminID.judgeID);
        }
        river.closeRiver();
    }
}
