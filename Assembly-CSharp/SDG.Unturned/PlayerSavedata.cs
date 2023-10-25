namespace SDG.Unturned;

public class PlayerSavedata
{
    public static bool hasSync;

    public static void writeData(SteamPlayerID playerID, string path, Data data)
    {
        if (hasSync)
        {
            ReadWrite.writeData("/Sync/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, useCloud: false, data);
        }
        else
        {
            ServerSavedata.writeData("/Players/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, data);
        }
    }

    public static Data readData(SteamPlayerID playerID, string path)
    {
        if (hasSync)
        {
            return ReadWrite.readData("/Sync/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, useCloud: false);
        }
        return ServerSavedata.readData("/Players/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path);
    }

    public static void writeBlock(SteamPlayerID playerID, string path, Block block)
    {
        if (hasSync)
        {
            ReadWrite.writeBlock("/Sync/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, useCloud: false, block);
        }
        else
        {
            ServerSavedata.writeBlock("/Players/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, block);
        }
    }

    public static Block readBlock(SteamPlayerID playerID, string path, byte prefix)
    {
        if (hasSync)
        {
            return ReadWrite.readBlock("/Sync/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, useCloud: false, prefix);
        }
        return ServerSavedata.readBlock("/Players/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, prefix);
    }

    public static River openRiver(SteamPlayerID playerID, string path, bool isReading)
    {
        if (hasSync)
        {
            return new River("/Sync/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, usePath: true, useCloud: false, isReading);
        }
        return ServerSavedata.openRiver("/Players/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, isReading);
    }

    public static void deleteFile(SteamPlayerID playerID, string path)
    {
        if (hasSync)
        {
            ReadWrite.deleteFile("/Sync/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, useCloud: false);
        }
        else
        {
            ServerSavedata.deleteFile("/Players/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path);
        }
    }

    public static bool fileExists(SteamPlayerID playerID, string path)
    {
        if (hasSync)
        {
            return ReadWrite.fileExists("/Sync/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path, useCloud: false);
        }
        return ServerSavedata.fileExists("/Players/" + playerID.steamID.ToString() + "_" + playerID.characterID + "/" + Level.info.name + path);
    }

    /// <summary>
    /// Delete all savedata folders for player's characters.
    /// </summary>
    public static void deleteFolder(SteamPlayerID playerID)
    {
        int num = Customization.FREE_CHARACTERS + Customization.PRO_CHARACTERS;
        if (hasSync)
        {
            for (int i = 0; i < num; i++)
            {
                string path = "/Sync/" + playerID.steamID.ToString() + "_" + i;
                if (ReadWrite.folderExists(path, usePath: false))
                {
                    ReadWrite.deleteFolder(path, usePath: false);
                }
            }
            return;
        }
        for (int j = 0; j < num; j++)
        {
            string path2 = "/Players/" + playerID.steamID.ToString() + "_" + j;
            if (ServerSavedata.folderExists(path2))
            {
                ServerSavedata.deleteFolder(path2);
            }
        }
    }
}
