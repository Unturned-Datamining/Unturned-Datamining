using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class LoadingUI : MonoBehaviour
{
    private static readonly byte TIP_COUNT = 31;

    private static bool _isInitialized;

    public Camera placeholderCamera;

    public static SleekWindow window;

    private static Local localization;

    private static ISleekImage backgroundImage;

    private static ISleekLabel tipLabel;

    private static ISleekBox loadingBarBox;

    private static SleekLoadingScreenProgressBar loadingProgressBar;

    private static SleekLoadingScreenProgressBar assetBundleProgressBar;

    private static SleekLoadingScreenProgressBar downloadProgressBar;

    private static SleekLoadingScreenProgressBar searchProgressBar;

    private static SleekLoadingScreenProgressBar readProgressBar;

    private static ISleekButton cancelButton;

    private static ISleekLabel creditsLabel;

    private static int lastLoading;

    private static ELoadingTip tip;

    private static bool wasLoadingAssetBundles;

    private static int previousAssetBundlesLoaded;

    private static int previousAssetBundlesFound;

    private static bool wasSearching;

    private static int previousFilesFound;

    private static bool wasReading;

    private static int previousReadFilesRead;

    private static int previousReadFilesFound;

    private static int previousAssetLoadingFilesLoaded = -1;

    private static int previousAssetLoadingFilesFound = -1;

    private static float animMaxProgress;

    private static float animStart_X;

    private static float animStart_Y;

    private static float animEnd_X;

    private static float animEnd_Y;

    public static bool isInitialized => _isInitialized;

    public static GameObject loader { get; private set; }

    public static bool isBlocked => Time.frameCount <= lastLoading;

    public static void SetLoadingText(string key)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            if (loadingProgressBar != null)
            {
                loadingProgressBar.DescriptionText = localization.format(key);
                loadingProgressBar.ProgressPercentage = 1f;
            }
        }
        else
        {
            CommandWindow.Log(localization.format(key));
        }
    }

    public static void NotifyLevelLoadingProgress(float progress)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            if (loadingProgressBar != null)
            {
                loadingProgressBar.ProgressPercentage = progress;
                UpdateBackgroundAnim(progress);
            }
        }
        else
        {
            CommandWindow.Log(localization.format("Level_Load", (int)(progress * 100f)));
        }
    }

    private static void UpdateAssetBundleProgress(AssetLoadingStats loadingStats)
    {
        bool num = loadingStats.isLoadingAssetBundles || wasLoadingAssetBundles;
        if (loadingStats.isLoadingAssetBundles != wasLoadingAssetBundles)
        {
            if (!wasLoadingAssetBundles)
            {
                previousAssetBundlesLoaded = -1;
                previousAssetBundlesFound = -1;
            }
            wasLoadingAssetBundles = loadingStats.isLoadingAssetBundles;
        }
        if (num)
        {
            int assetBundlesLoaded = loadingStats.AssetBundlesLoaded;
            int assetBundlesFound = loadingStats.AssetBundlesFound;
            if (assetBundlesLoaded != previousAssetBundlesLoaded || assetBundlesFound != previousAssetBundlesFound)
            {
                previousAssetBundlesLoaded = assetBundlesLoaded;
                previousAssetBundlesFound = assetBundlesFound;
                string text = localization.format("Loading_Asset_Bundles", Assets.loadingStats.AssetBundlesLoaded, Assets.loadingStats.AssetBundlesFound);
                if (Dedicator.IsDedicatedServer)
                {
                    CommandWindow.Log(text);
                }
                else
                {
                    assetBundleProgressBar.DescriptionText = text;
                }
            }
            if (!Dedicator.IsDedicatedServer)
            {
                if (!assetBundleProgressBar.isVisible)
                {
                    assetBundleProgressBar.isVisible = true;
                    UpdateLoadingBarPositions();
                }
                assetBundleProgressBar.ProgressPercentage = loadingStats.EstimateAssetBundleProgressPercentage();
            }
        }
        else if (!Dedicator.IsDedicatedServer && assetBundleProgressBar.isVisible)
        {
            assetBundleProgressBar.isVisible = false;
            UpdateLoadingBarPositions();
        }
    }

    private static void UpdateSearchProgress(AssetLoadingStats loadingStats)
    {
        bool flag = loadingStats.SearchLocationsFinishedSearching < loadingStats.RegisteredSearchLocations;
        bool flag2 = flag || wasSearching;
        if (flag != wasSearching)
        {
            if (!wasSearching)
            {
                previousFilesFound = -1;
            }
            wasSearching = flag;
        }
        if (flag2)
        {
            int filesFound = loadingStats.FilesFound;
            if (filesFound != previousFilesFound)
            {
                previousFilesFound = filesFound;
                string text = localization.format("Loading_Search", Assets.loadingStats.SearchLocationsFinishedSearching, Assets.loadingStats.RegisteredSearchLocations, Assets.loadingStats.FilesFound);
                if (Dedicator.IsDedicatedServer)
                {
                    CommandWindow.Log(text);
                }
                else
                {
                    searchProgressBar.DescriptionText = text;
                }
            }
            if (!Dedicator.IsDedicatedServer)
            {
                if (!searchProgressBar.isVisible)
                {
                    searchProgressBar.isVisible = true;
                    UpdateLoadingBarPositions();
                }
                searchProgressBar.ProgressPercentage = loadingStats.EstimateSearchProgressPercentage();
            }
        }
        else if (!Dedicator.IsDedicatedServer && searchProgressBar.isVisible)
        {
            searchProgressBar.isVisible = false;
            UpdateLoadingBarPositions();
        }
    }

    private static void UpdateReadProgress(AssetLoadingStats loadingStats)
    {
        bool flag = loadingStats.FilesRead < loadingStats.FilesFound;
        bool flag2 = flag || wasReading;
        if (flag != wasReading)
        {
            if (!wasReading)
            {
                previousReadFilesRead = -1;
                previousReadFilesFound = -1;
            }
            wasReading = flag;
        }
        if (flag2)
        {
            int filesRead = loadingStats.FilesRead;
            int filesFound = loadingStats.FilesFound;
            if (filesRead != previousReadFilesRead || filesFound != previousReadFilesFound)
            {
                previousReadFilesRead = filesRead;
                previousReadFilesFound = filesFound;
                string text = localization.format("Loading_Read", filesRead, filesFound);
                if (Dedicator.IsDedicatedServer)
                {
                    CommandWindow.Log(text);
                }
                else
                {
                    readProgressBar.DescriptionText = text;
                }
            }
            if (!Dedicator.IsDedicatedServer)
            {
                if (!readProgressBar.isVisible)
                {
                    readProgressBar.isVisible = true;
                    UpdateLoadingBarPositions();
                }
                readProgressBar.ProgressPercentage = loadingStats.EstimateReadProgressPercentage();
            }
        }
        else if (!Dedicator.IsDedicatedServer && readProgressBar.isVisible)
        {
            readProgressBar.isVisible = false;
            UpdateLoadingBarPositions();
        }
    }

    private static void UpdateAssetLoadingProgress(AssetLoadingStats loadingStats)
    {
        int filesLoaded = loadingStats.FilesLoaded;
        int filesFound = loadingStats.FilesFound;
        if (filesLoaded == previousAssetLoadingFilesLoaded && filesFound == previousAssetLoadingFilesFound)
        {
            return;
        }
        previousAssetLoadingFilesLoaded = filesLoaded;
        previousAssetLoadingFilesFound = filesFound;
        string text = localization.format("Loading_Asset_Definitions", filesLoaded, filesFound);
        if (!Dedicator.IsDedicatedServer)
        {
            if (loadingProgressBar != null)
            {
                loadingProgressBar.DescriptionText = text;
                loadingProgressBar.ProgressPercentage = loadingStats.EstimateFileProgressPercentage();
            }
        }
        else
        {
            CommandWindow.Log(text);
        }
    }

    private static void HideAllLoadingBars()
    {
        if (assetBundleProgressBar != null)
        {
            assetBundleProgressBar.isVisible = false;
            searchProgressBar.isVisible = false;
            readProgressBar.isVisible = false;
            downloadProgressBar.isVisible = false;
        }
    }

    internal static void NotifyAssetDefinitionLoadingProgress()
    {
        AssetLoadingStats loadingStats = Assets.loadingStats;
        UpdateAssetBundleProgress(loadingStats);
        UpdateSearchProgress(loadingStats);
        UpdateReadProgress(loadingStats);
        UpdateAssetLoadingProgress(loadingStats);
    }

    public static void SetIsDownloading(bool isDownloading)
    {
        if (downloadProgressBar != null)
        {
            downloadProgressBar.isVisible = isDownloading;
            UpdateLoadingBarPositions();
        }
    }

    public static void SetDownloadFileName(string name)
    {
        if (downloadProgressBar != null)
        {
            downloadProgressBar.DescriptionText = localization.format("Download_Progress", name);
        }
    }

    public static void NotifyDownloadProgress(float progress)
    {
        if (downloadProgressBar != null)
        {
            downloadProgressBar.ProgressPercentage = progress;
        }
    }

    private static bool loadBackgroundImage(string path)
    {
        if (backgroundImage.texture != null && backgroundImage.shouldDestroyTexture)
        {
            UnityEngine.Object.Destroy(backgroundImage.texture);
            backgroundImage.texture = null;
        }
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        if (!File.Exists(path))
        {
            return false;
        }
        backgroundImage.texture = ReadWrite.readTextureFromFile(path);
        backgroundImage.shouldDestroyTexture = true;
        return true;
    }

    internal static string GetRandomImagePathInDirectory(string path, bool onlyWithoutHud)
    {
        try
        {
            List<string> list = new List<string>();
            foreach (FileInfo item in new DirectoryInfo(path).EnumerateFiles())
            {
                if (item.Length <= 10000000 && (!onlyWithoutHud || item.Name.Contains("NoUI")))
                {
                    string extension = item.Extension;
                    if (string.Equals(extension, ".png", StringComparison.InvariantCultureIgnoreCase) || string.Equals(extension, ".jpg", StringComparison.InvariantCultureIgnoreCase))
                    {
                        list.Add(item.FullName);
                    }
                }
            }
            return list.RandomOrDefault();
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception loading background image:");
        }
        return null;
    }

    private static bool pickBackgroundImage(string path, bool onlyWithoutHud)
    {
        if (!Directory.Exists(path))
        {
            loadBackgroundImage(null);
            return false;
        }
        string randomImagePathInDirectory = GetRandomImagePathInDirectory(path, onlyWithoutHud);
        if (!string.IsNullOrEmpty(randomImagePathInDirectory))
        {
            loadBackgroundImage(randomImagePathInDirectory);
            return true;
        }
        loadBackgroundImage(null);
        return false;
    }

    private static void PickNonLevelBackgroundImage()
    {
        if (!OptionsSettings.enableScreenshotsOnLoadingScreen || !pickBackgroundImage(PathEx.Join(UnturnedPaths.RootDirectory, "Screenshots"), onlyWithoutHud: true))
        {
            pickBackgroundImage(PathEx.Join(UnturnedPaths.RootDirectory, "LoadingScreens"), onlyWithoutHud: false);
        }
    }

    public static void updateScene()
    {
        if (Dedicator.IsDedicatedServer || backgroundImage == null || loadingProgressBar == null)
        {
            return;
        }
        HideAllLoadingBars();
        UpdateLoadingBarPositions();
        NotifyLevelLoadingProgress(0f);
        Local local = Localization.read("/Menu/MenuTips.dat");
        byte b;
        do
        {
            b = (byte)UnityEngine.Random.Range(1, TIP_COUNT + 1);
        }
        while (b == (byte)tip);
        tip = (ELoadingTip)b;
        string s = ((OptionsSettings.streamer && Provider.streamerNames != null && Provider.streamerNames.Count > 0 && Provider.streamerNames[0] == "Nelson AI") ? local.format("Streamer") : (tip switch
        {
            ELoadingTip.HOTKEY => local.format("Hotkey"), 
            ELoadingTip.EQUIP => local.format("Equip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other)), 
            ELoadingTip.DROP => local.format("Drop", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other)), 
            ELoadingTip.SIRENS => local.format("Sirens", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other)), 
            ELoadingTip.TRANSFORM => local.format("Transform"), 
            ELoadingTip.QUALITY => local.format("Quality"), 
            ELoadingTip.UMBRELLA => local.format("Umbrella"), 
            ELoadingTip.HEAL => local.format("Heal"), 
            ELoadingTip.ROTATE => local.format("Rotate"), 
            ELoadingTip.BASE => local.format("Base"), 
            ELoadingTip.DEQUIP => local.format("Dequip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.dequip)), 
            ELoadingTip.NIGHTVISION => local.format("Nightvision", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.vision)), 
            ELoadingTip.TRANSFER => local.format("Transfer", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other)), 
            ELoadingTip.SURFACE => local.format("Surface", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.jump)), 
            ELoadingTip.ARREST => local.format("Arrest", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.leanLeft), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.leanRight)), 
            ELoadingTip.SAFEZONE => local.format("Safezone"), 
            ELoadingTip.CLAIM => local.format("Claim"), 
            ELoadingTip.GROUP => local.format("Group"), 
            ELoadingTip.MAP => local.format("Map", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.map)), 
            ELoadingTip.BEACON => local.format("Beacon"), 
            ELoadingTip.HORN => local.format("Horn", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary)), 
            ELoadingTip.LIGHTS => local.format("Lights", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary)), 
            ELoadingTip.SNAP => local.format("Snap", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.snap)), 
            ELoadingTip.UPGRADE => local.format("Upgrade", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other)), 
            ELoadingTip.GRAB => local.format("Grab", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other)), 
            ELoadingTip.SKYCRANE => local.format("Skycrane", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other)), 
            ELoadingTip.SEAT => local.format("Seat"), 
            ELoadingTip.RARITY => local.format("Rarity"), 
            ELoadingTip.ORIENTATION => local.format("Orientation", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.rotate)), 
            ELoadingTip.RED => local.format("Red"), 
            ELoadingTip.STEADY => local.format("Steady", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.sprint)), 
            _ => "#" + tip, 
        }));
        if (Level.info != null)
        {
            if (pickBackgroundImage(Level.info.path + "/Screenshots", onlyWithoutHud: false))
            {
                _ = Level.getAsset()?.shouldAnimateBackgroundImage;
            }
            else if (loadBackgroundImage(Level.info.path + "/Level.png"))
            {
                _ = Level.getAsset()?.shouldAnimateBackgroundImage;
            }
            else
            {
                PickNonLevelBackgroundImage();
            }
            if (false)
            {
                EnableBackgroundAnim();
            }
            else
            {
                DisableBackgroundAnim();
            }
            string localizedName = Level.info.getLocalizedName();
            Local local2 = Level.info.getLocalization();
            if (Level.info.configData.Tips > 0 && local2 != null)
            {
                string key = "Tip_" + UnityEngine.Random.Range(0, Level.info.configData.Tips);
                s = local2.format(key);
            }
            if (Provider.isConnected)
            {
                string text;
                if (!Provider.isServer)
                {
                    text = ((!Provider.currentServerInfo.IsVACSecure) ? localization.format("VAC_Insecure") : localization.format("VAC_Secure"));
                    text = ((!Provider.currentServerInfo.IsBattlEyeSecure) ? (text + " + " + localization.format("BattlEye_Insecure")) : (text + " + " + localization.format("BattlEye_Secure")));
                }
                else
                {
                    text = localization.format("Offline");
                }
                loadingProgressBar.DescriptionText = localization.format("Loading_Level_Play", localizedName, Level.version, OptionsSettings.streamer ? localization.format("Streamer") : Provider.currentServerInfo.name, text);
            }
            else
            {
                loadingProgressBar.DescriptionText = localization.format("Loading_Level_Edit", localizedName);
            }
            if (Level.info.configData.Creators.Length != 0 || Level.info.configData.Collaborators.Length != 0 || Level.info.configData.Thanks.Length != 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (Level.info.configData.Creators.Length != 0)
                {
                    stringBuilder.Append("<color=#f0f0f0>");
                    stringBuilder.Append(localization.format("Creators"));
                    stringBuilder.AppendLine("</color>");
                    for (int i = 0; i < Level.info.configData.Creators.Length; i++)
                    {
                        stringBuilder.AppendLine(Level.info.configData.Creators[i]);
                    }
                }
                if (Level.info.configData.Collaborators.Length != 0)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.Append("<color=#f0f0f0>");
                    stringBuilder.AppendLine(localization.format("Collaborators"));
                    stringBuilder.AppendLine("</color>");
                    for (int j = 0; j < Level.info.configData.Collaborators.Length; j++)
                    {
                        stringBuilder.AppendLine(Level.info.configData.Collaborators[j]);
                    }
                }
                if (Level.info.configData.Thanks.Length != 0)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.Append("<color=#f0f0f0>");
                    stringBuilder.AppendLine(localization.format("Thanks"));
                    stringBuilder.AppendLine("</color>");
                    for (int k = 0; k < Level.info.configData.Thanks.Length; k++)
                    {
                        stringBuilder.AppendLine(Level.info.configData.Thanks[k]);
                    }
                }
                creditsLabel.text = stringBuilder.ToString();
                creditsLabel.isVisible = true;
            }
            else
            {
                creditsLabel.isVisible = false;
            }
        }
        else
        {
            PickNonLevelBackgroundImage();
            DisableBackgroundAnim();
            loadingProgressBar.DescriptionText = localization.format("Loading");
            creditsLabel.isVisible = false;
        }
        RichTextUtil.replaceNewlineMarkup(ref s);
        s = ItemTool.filterRarityRichText(s);
        tipLabel.text = local.format("Tip", s);
        loadingProgressBar.sizeOffset_X = -20;
        cancelButton.isVisible = false;
    }

    private static void onQueuePositionUpdated()
    {
        loadingProgressBar.DescriptionText = localization.format("Queue_Position", Provider.queuePosition + 1);
        loadingProgressBar.sizeOffset_X = -130;
        cancelButton.isVisible = true;
    }

    private static void onClickedCancelButton(ISleekElement button)
    {
        Provider.RequestDisconnect("clicked queue cancel button");
    }

    private void Update()
    {
        if (!Dedicator.IsDedicatedServer && (Assets.isLoading || Provider.isLoading || Level.isLoading || Player.isLoading || Level.isExiting))
        {
            lastLoading = Time.frameCount + 1;
        }
        bool flag2 = (UnturnedMasterVolume.mutedByLoadingScreen = isBlocked);
        placeholderCamera.enabled = flag2;
        Level.LoadingScreenWantsMusic = flag2;
        if (flag2)
        {
            Glazier.Get().Root = window;
        }
        else if (PlayerUI.instance != null)
        {
            PlayerUI.instance.Player_OnGUI();
        }
        else if (MenuUI.instance != null)
        {
            MenuUI.instance.Menu_OnGUI();
        }
        else if (EditorUI.instance != null)
        {
            EditorUI.instance.Editor_OnGUI();
        }
    }

    private void Awake()
    {
        if (isInitialized)
        {
            UnityEngine.Object.Destroy(base.gameObject);
            return;
        }
        _isInitialized = true;
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
    }

    private void Start()
    {
        localization = Localization.read("/Menu/MenuLoading.dat");
        loader = base.gameObject;
        if (Dedicator.IsDedicatedServer)
        {
            UnityEngine.Object.Destroy(base.gameObject);
            return;
        }
        if (placeholderCamera == null)
        {
            UnturnedLog.warn("LoadingUI.placeholderCamera is null");
        }
        else
        {
            placeholderCamera.enabled = true;
        }
        window = new SleekWindow();
        window.showTooltips = false;
        window.hackSortOrder = true;
        Glazier.Get().Root = window;
        backgroundImage = Glazier.Get().CreateImage();
        backgroundImage.sizeScale_X = 1f;
        backgroundImage.sizeScale_Y = 1f;
        window.AddChild(backgroundImage);
        tipLabel = Glazier.Get().CreateLabel();
        tipLabel.positionOffset_X = 10;
        tipLabel.positionScale_Y = 1f;
        tipLabel.sizeOffset_X = -20;
        tipLabel.sizeOffset_Y = 100;
        tipLabel.sizeScale_X = 1f;
        tipLabel.fontSize = ESleekFontSize.Medium;
        tipLabel.enableRichText = true;
        tipLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        tipLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        tipLabel.fontAlignment = TextAnchor.LowerCenter;
        window.AddChild(tipLabel);
        loadingBarBox = Glazier.Get().CreateBox();
        loadingBarBox.positionOffset_X = 10;
        loadingBarBox.positionScale_Y = 1f;
        loadingBarBox.sizeOffset_X = -20;
        loadingBarBox.sizeScale_X = 1f;
        window.AddChild(loadingBarBox);
        loadingProgressBar = new SleekLoadingScreenProgressBar();
        loadingProgressBar.positionOffset_X = 10;
        loadingProgressBar.sizeOffset_X = -20;
        loadingProgressBar.sizeOffset_Y = 20;
        loadingProgressBar.sizeScale_X = 1f;
        loadingBarBox.AddChild(loadingProgressBar);
        downloadProgressBar = new SleekLoadingScreenProgressBar();
        downloadProgressBar.positionOffset_X = 10;
        downloadProgressBar.sizeOffset_X = -20;
        downloadProgressBar.sizeOffset_Y = 20;
        downloadProgressBar.sizeScale_X = 1f;
        downloadProgressBar.isVisible = false;
        loadingBarBox.AddChild(downloadProgressBar);
        assetBundleProgressBar = new SleekLoadingScreenProgressBar();
        assetBundleProgressBar.positionOffset_X = 10;
        assetBundleProgressBar.sizeOffset_X = -20;
        assetBundleProgressBar.sizeOffset_Y = 20;
        assetBundleProgressBar.sizeScale_X = 1f;
        assetBundleProgressBar.isVisible = false;
        loadingBarBox.AddChild(assetBundleProgressBar);
        searchProgressBar = new SleekLoadingScreenProgressBar();
        searchProgressBar.positionOffset_X = 10;
        searchProgressBar.sizeOffset_X = -20;
        searchProgressBar.sizeOffset_Y = 20;
        searchProgressBar.sizeScale_X = 1f;
        searchProgressBar.isVisible = false;
        loadingBarBox.AddChild(searchProgressBar);
        readProgressBar = new SleekLoadingScreenProgressBar();
        readProgressBar.positionOffset_X = 10;
        readProgressBar.sizeOffset_X = -20;
        readProgressBar.sizeOffset_Y = 20;
        readProgressBar.sizeScale_X = 1f;
        readProgressBar.isVisible = false;
        loadingBarBox.AddChild(readProgressBar);
        creditsLabel = Glazier.Get().CreateLabel();
        creditsLabel.positionOffset_X = -250;
        creditsLabel.positionOffset_Y = -500;
        creditsLabel.positionScale_X = 0.75f;
        creditsLabel.positionScale_Y = 0.5f;
        creditsLabel.sizeOffset_X = 500;
        creditsLabel.sizeOffset_Y = 1000;
        creditsLabel.isVisible = false;
        creditsLabel.enableRichText = true;
        creditsLabel.fontAlignment = TextAnchor.MiddleCenter;
        creditsLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        creditsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        window.AddChild(creditsLabel);
        cancelButton = Glazier.Get().CreateButton();
        cancelButton.positionOffset_X = -110;
        cancelButton.positionOffset_Y = -60;
        cancelButton.positionScale_X = 1f;
        cancelButton.positionScale_Y = 1f;
        cancelButton.sizeOffset_X = 100;
        cancelButton.sizeOffset_Y = 50;
        cancelButton.fontSize = ESleekFontSize.Medium;
        cancelButton.text = localization.format("Queue_Cancel");
        cancelButton.tooltipText = localization.format("Queue_Cancel_Tooltip");
        cancelButton.onClickedButton += onClickedCancelButton;
        cancelButton.isVisible = false;
        window.AddChild(cancelButton);
        tip = ELoadingTip.NONE;
        Provider.onQueuePositionUpdated = (Provider.QueuePositionUpdated)Delegate.Combine(Provider.onQueuePositionUpdated, new Provider.QueuePositionUpdated(onQueuePositionUpdated));
        UpdateLoadingBarPositions();
    }

    private void OnDestroy()
    {
    }

    private static void UpdateLoadingBarPositions()
    {
        if (!Dedicator.IsDedicatedServer)
        {
            int num = 10;
            if (downloadProgressBar.isVisible)
            {
                downloadProgressBar.positionOffset_Y = num;
                num += downloadProgressBar.sizeOffset_Y;
                num += 10;
            }
            if (assetBundleProgressBar.isVisible)
            {
                assetBundleProgressBar.positionOffset_Y = num;
                num += assetBundleProgressBar.sizeOffset_Y;
                num += 10;
            }
            if (searchProgressBar.isVisible)
            {
                searchProgressBar.positionOffset_Y = num;
                num += searchProgressBar.sizeOffset_Y;
                num += 10;
            }
            if (readProgressBar.isVisible)
            {
                readProgressBar.positionOffset_Y = num;
                num += readProgressBar.sizeOffset_Y;
                num += 10;
            }
            loadingProgressBar.positionOffset_Y = num;
            num += loadingProgressBar.sizeOffset_Y;
            num += 10;
            loadingBarBox.sizeOffset_Y = num;
            loadingBarBox.positionOffset_Y = -num - 10;
            tipLabel.positionOffset_Y = Mathf.Min(-210, loadingBarBox.positionOffset_Y - 10 - tipLabel.sizeOffset_Y);
        }
    }

    private static void UpdateBackgroundAnim(float progress)
    {
        progress = Mathf.Max(animMaxProgress, progress);
        animMaxProgress = progress;
        backgroundImage.positionScale_X = Mathf.Lerp(animStart_X, animEnd_X, progress);
        backgroundImage.positionScale_Y = Mathf.Lerp(animStart_Y, animEnd_Y, progress);
    }

    private static void DisableBackgroundAnim()
    {
        animMaxProgress = 0f;
        backgroundImage.positionScale_X = 0f;
        backgroundImage.positionScale_Y = 0f;
        backgroundImage.sizeScale_X = 1f;
        backgroundImage.sizeScale_Y = 1f;
    }

    private static void EnableBackgroundAnim()
    {
        animMaxProgress = 0f;
        backgroundImage.sizeScale_X = 1.01f;
        backgroundImage.sizeScale_Y = 1.01f;
        if (UnityEngine.Random.value < 0.5f)
        {
            animStart_X = -0.01f;
            animEnd_X = 0f;
        }
        else
        {
            animStart_X = 0f;
            animEnd_X = -0.01f;
        }
        float num = UnityEngine.Random.Range(0f, 0.01f);
        float num2 = UnityEngine.Random.Range(0f, 0.01f - num);
        if (UnityEngine.Random.value < 0.5f)
        {
            animStart_Y = 0f - num2 - num;
            animEnd_Y = 0f - num2;
        }
        else
        {
            animStart_Y = 0f - num2;
            animEnd_Y = 0f - num2 - num;
        }
        backgroundImage.positionScale_X = animStart_X;
        backgroundImage.positionScale_Y = animStart_Y;
    }
}
