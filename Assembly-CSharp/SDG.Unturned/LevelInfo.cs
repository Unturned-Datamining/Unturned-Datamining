using System.IO;

namespace SDG.Unturned;

public class LevelInfo
{
    private string _name;

    private ELevelSize _size;

    private ELevelType _type;

    private bool _isEditable;

    private LevelAsset cachedAsset;

    private bool didResolveAsset;

    private Local cachedLocalization;

    /// <summary>
    /// Absolute path to the map folder.
    /// </summary>
    public string path { get; protected set; }

    public string name => _name;

    /// <summary>
    /// Whether unity analytics should track this map's name. Don't want to burn all the analysis points!
    /// </summary>
    public bool canAnalyticsTrack => isSpecial;

    /// <summary>
    /// Maps included with the game only, separate from category because arena maps are misc.
    /// Category is set as part of the config file. This is only mainly used to enable unity analytics tracking for map name.
    /// </summary>
    public bool isSpecial
    {
        get
        {
            if (!(name == "Alpha Valley") && !(name == "Monolith") && !(name == "Paintball_Arena_0") && !(name == "PEI") && !(name == "PEI Arena") && !(name == "Tutorial") && !(name == "Washington") && !(name == "Washington Arena") && !(name == "Yukon") && !(name == "Russia") && !(name == "Hawaii"))
            {
                return name == "Germany";
            }
            return true;
        }
    }

    /// <summary>
    /// Only used for play menu categories at the moment.
    /// </summary>
    public bool isFromWorkshop { get; protected set; }

    public ulong publishedFileId { get; protected set; }

    /// <summary>
    /// Test whether this map's workshop file ID is in the curated maps list.
    /// </summary>
    public bool isCurated
    {
        get
        {
            if (isFromWorkshop)
            {
                foreach (CuratedMapLink curated_Map_Link in Provider.statusData.Maps.Curated_Map_Links)
                {
                    if (curated_Map_Link.Workshop_File_Id == publishedFileId)
                    {
                        return true;
                    }
                }
                return false;
            }
            if (!(name == "France"))
            {
                return name == "Canyon Arena";
            }
            return true;
        }
    }

    /// <summary>
    /// Web URL to map feedback discussions.
    /// </summary>
    public string feedbackUrl
    {
        get
        {
            if (configData != null && !string.IsNullOrEmpty(configData.Feedback))
            {
                return configData.Feedback;
            }
            if (isFromWorkshop)
            {
                return "https://steamcommunity.com/sharedfiles/filedetails/discussions/" + publishedFileId;
            }
            return null;
        }
    }

    public ELevelSize size => _size;

    public ELevelType type => _type;

    public bool isEditable => _isEditable;

    public LevelInfoConfigData configData { get; private set; }

    /// <summary>
    /// Cache level's asset, if any.
    /// </summary>
    public LevelAsset resolveAsset()
    {
        if (cachedAsset == null && !didResolveAsset)
        {
            didResolveAsset = true;
            if (configData != null && configData.Asset.isValid)
            {
                cachedAsset = Assets.find(configData.Asset);
                if (cachedAsset == null)
                {
                    UnturnedLog.warn("Unable to find level asset {0} for {1}", configData.Asset, name);
                }
            }
            if (cachedAsset == null)
            {
                cachedAsset = Assets.find(LevelAsset.defaultLevel);
                if (cachedAsset == null)
                {
                    UnturnedLog.warn("Unable to find default level asset for {0}", name);
                }
            }
        }
        return cachedAsset;
    }

    public Local getLocalization()
    {
        if (cachedLocalization == null)
        {
            string text = path + "/" + Provider.language + ".dat";
            if (ReadWrite.fileExists(text, useCloud: false, usePath: false))
            {
                cachedLocalization = new Local(ReadWrite.ReadDataWithoutHash(text));
            }
            else
            {
                string text2 = Provider.localizationRoot + "/Maps/" + name + ".dat";
                if (ReadWrite.fileExists(text2, useCloud: false, usePath: false))
                {
                    cachedLocalization = new Local(ReadWrite.ReadDataWithoutHash(text2));
                }
                else
                {
                    string text3 = Provider.localizationRoot + "/Maps/" + name.Replace(' ', '_') + ".dat";
                    if (ReadWrite.fileExists(text3, useCloud: false, usePath: false))
                    {
                        cachedLocalization = new Local(ReadWrite.ReadDataWithoutHash(text3));
                    }
                }
            }
            if (cachedLocalization == null)
            {
                string text4 = path + "/English.dat";
                if (ReadWrite.fileExists(text4, useCloud: false, usePath: false))
                {
                    cachedLocalization = new Local(ReadWrite.ReadDataWithoutHash(text4));
                }
                else
                {
                    cachedLocalization = new Local();
                }
            }
        }
        return cachedLocalization;
    }

    public string getLocalizedName()
    {
        Local localization = getLocalization();
        if (localization != null && localization.has("Name"))
        {
            return localization.format("Name");
        }
        return name;
    }

    /// <summary>
    /// Preview.png should be 320x180
    /// </summary>
    public string GetPreviewImageFilePath()
    {
        string result = Path.Combine(path, "Preview.png");
        if (File.Exists(result))
        {
            return result;
        }
        return GetLoadingScreenImagePath();
    }

    /// <summary>
    /// Get a random file path in the /Screenshots folder, or fallback to Level.png if it exists.
    /// </summary>
    public string GetLoadingScreenImagePath()
    {
        string randomScreenshotPath = GetRandomScreenshotPath();
        if (!string.IsNullOrEmpty(randomScreenshotPath))
        {
            return randomScreenshotPath;
        }
        string result = Path.Combine(path, "Level.png");
        if (File.Exists(result))
        {
            return result;
        }
        return null;
    }

    /// <summary>
    /// Get a random file path in the /Screenshots folder
    /// </summary>
    internal string GetRandomScreenshotPath()
    {
        string text = Path.Combine(path, "Screenshots");
        if (!Directory.Exists(text))
        {
            return null;
        }
        return LoadingUI.GetRandomImagePathInDirectory(text, onlyWithoutHud: false);
    }

    public LevelInfo(string newPath, string newName, ELevelSize newSize, ELevelType newType, bool newEditable, LevelInfoConfigData newConfigData, ulong publishedFileId)
    {
        path = newPath;
        _name = newName;
        _size = newSize;
        _type = newType;
        _isEditable = newEditable;
        configData = newConfigData;
        isFromWorkshop = publishedFileId != 0;
        this.publishedFileId = publishedFileId;
    }
}
