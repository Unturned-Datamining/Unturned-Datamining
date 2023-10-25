using System;
using SDG.Provider;

namespace SDG.Unturned;

public class WorkshopTool
{
    public static bool checkMapMeta(string path, bool usePath)
    {
        return ReadWrite.fileExists(path + "/Map.meta", useCloud: false, usePath);
    }

    public static bool checkMapValid(string path, bool usePath)
    {
        string[] folders = ReadWrite.getFolders(path, usePath);
        if (folders.Length != 1)
        {
            return false;
        }
        return ReadWrite.fileExists(folders[0] + "/Level.dat", useCloud: false, usePath);
    }

    private static bool findMapNestedPath(string basePath, string searchPath, out string path)
    {
        string[] folders = ReadWrite.getFolders(basePath, usePath: false);
        for (int i = 0; i < folders.Length; i++)
        {
            string text = folders[i] + searchPath;
            if (ReadWrite.folderExists(text, usePath: false))
            {
                path = text;
                return true;
            }
        }
        path = null;
        return false;
    }

    /// <summary>
    /// Given path to a workshop map, try to find its /Bundles folder.
    /// </summary>
    public static bool findMapBundlesPath(string path, out string bundlesPath)
    {
        return findMapNestedPath(path, "/Bundles", out bundlesPath);
    }

    /// <summary>
    /// Given path to a workshop map, try to find its /Content folder.
    /// </summary>
    public static bool findMapContentPath(string path, out string contentPath)
    {
        return findMapNestedPath(path, "/Content", out contentPath);
    }

    [Obsolete]
    public static void loadMapBundlesAndContent(string workshopItemPath)
    {
        loadMapBundlesAndContent(workshopItemPath, 0uL);
    }

    /// <summary>
    /// Maps on the workshop are a root folder named after the published file id, containing
    /// the map folder itself with the level name. In order to load the map's bundles and content
    /// properly we need to find the nested Bundles and Content folders.
    /// </summary>
    public static void loadMapBundlesAndContent(string workshopItemPath, ulong workshopFileId)
    {
        if (findMapBundlesPath(workshopItemPath, out var bundlesPath))
        {
            Assets.RequestAddSearchLocation(bundlesPath, TempSteamworksWorkshop.FindOrAddOrigin(workshopFileId));
        }
    }

    public static bool checkLocalizationMeta(string path, bool usePath)
    {
        return ReadWrite.fileExists(path + "/Localization.meta", useCloud: false, usePath);
    }

    public static bool checkLocalizationValid(string path, bool usePath)
    {
        return ReadWrite.getFolders(path, usePath).Length != 0;
    }

    public static bool checkObjectMeta(string path, bool usePath)
    {
        return ReadWrite.fileExists(path + "/Object.meta", useCloud: false, usePath);
    }

    public static bool checkItemMeta(string path, bool usePath)
    {
        return ReadWrite.fileExists(path + "/Item.meta", useCloud: false, usePath);
    }

    public static bool checkVehicleMeta(string path, bool usePath)
    {
        return ReadWrite.fileExists(path + "/Vehicle.meta", useCloud: false, usePath);
    }

    public static bool checkSkinMeta(string path, bool usePath)
    {
        return ReadWrite.fileExists(path + "/Skin.meta", useCloud: false, usePath);
    }

    public static bool checkBundleValid(string path, bool usePath)
    {
        return ReadWrite.getFolders(path, usePath).Length != 0;
    }

    public static bool detectUGCMetaType(string path, bool usePath, out ESteamUGCType outType)
    {
        if (checkMapMeta(path, usePath))
        {
            outType = ESteamUGCType.MAP;
        }
        else if (checkLocalizationMeta(path, usePath))
        {
            outType = ESteamUGCType.LOCALIZATION;
        }
        else if (checkObjectMeta(path, usePath))
        {
            outType = ESteamUGCType.OBJECT;
        }
        else if (checkItemMeta(path, usePath: false))
        {
            outType = ESteamUGCType.ITEM;
        }
        else
        {
            if (!checkVehicleMeta(path, usePath: false))
            {
                outType = ESteamUGCType.ITEM;
                return false;
            }
            outType = ESteamUGCType.VEHICLE;
        }
        return true;
    }
}
