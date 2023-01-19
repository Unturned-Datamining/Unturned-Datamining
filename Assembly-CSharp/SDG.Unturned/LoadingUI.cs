using System;
using System.Collections.Generic;
using System.IO;
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

    private static ISleekBox tipBox;

    private static ISleekBox loadingBox;

    private static ISleekImage loadingBarImage;

    private static ISleekLabel loadingLabel;

    private static ISleekButton cancelButton;

    private static ISleekBox creditsBox;

    private static int lastLoading;

    private static float lastPlayingSplashcreen;

    private static ELoadingTip tip;

    private static int assetsLoadCount;

    private static int assetsScanCount;

    private static int masterBundleProgress = -1;

    private static float animMaxProgress;

    private static float animStart_X;

    private static float animStart_Y;

    private static float animEnd_X;

    private static float animEnd_Y;

    public static bool isInitialized => _isInitialized;

    public static GameObject loader { get; private set; }

    public static bool isBlocked => Time.frameCount <= lastLoading;

    public static void updateKey(string key)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            if (loadingLabel != null)
            {
                loadingLabel.text = localization.format(key);
                if (loadingBarImage != null)
                {
                    loadingBarImage.sizeScale_X = 1f;
                    loadingBarImage.sizeOffset_X = -20;
                }
            }
        }
        else
        {
            CommandWindow.Log(localization.format(key));
        }
    }

    public static void updateProgress(float progress)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            if (loadingBarImage != null)
            {
                loadingBarImage.sizeScale_X = progress;
                loadingBarImage.sizeOffset_X = (int)(-20f * progress);
                UpdateBackgroundAnim(progress);
            }
        }
        else
        {
            CommandWindow.Log(localization.format("Level_Load", (int)(progress * 100f)));
        }
    }

    public static void assetsLoad(string key, int count, float progress, float step, bool formatKey = true)
    {
        assetsLoadCount = assetsScanCount - count;
        if (!Dedicator.IsDedicatedServer)
        {
            if (loadingLabel != null)
            {
                loadingLabel.text = localization.format("Assets_Load", formatKey ? localization.format(key) : key, assetsLoadCount, assetsScanCount);
                if (loadingBarImage != null)
                {
                    progress += (float)assetsLoadCount / (float)assetsScanCount * step;
                    loadingBarImage.sizeScale_X = progress;
                    loadingBarImage.sizeOffset_X = (int)(-20f * progress);
                    UpdateBackgroundAnim(progress);
                }
            }
        }
        else
        {
            CommandWindow.Log(localization.format("Assets_Load", formatKey ? localization.format(key) : key, assetsLoadCount, assetsScanCount));
        }
    }

    public static void assetsScan(string key, int count, bool formatKey = true)
    {
        assetsScanCount = count;
        if (!Dedicator.IsDedicatedServer)
        {
            if (loadingLabel != null)
            {
                loadingLabel.text = localization.format("Assets_Scan", formatKey ? localization.format(key) : key, assetsScanCount);
            }
        }
        else
        {
            CommandWindow.Log(localization.format("Assets_Scan", formatKey ? localization.format(key) : key, assetsScanCount));
        }
    }

    public static void notifyMasterBundleProgress(string key, string name, float progress)
    {
        int num = Mathf.RoundToInt(progress * 100f);
        if (masterBundleProgress != num)
        {
            masterBundleProgress = num;
            string text = localization.format(key, name, num);
            if (Dedicator.IsDedicatedServer)
            {
                CommandWindow.Log(text);
            }
            else if (loadingLabel != null)
            {
                loadingLabel.text = text;
            }
        }
    }

    public static void notifyDownloadProgress(string name)
    {
        if (loadingLabel != null)
        {
            loadingLabel.text = localization.format("Download_Progress", name);
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
        if (Dedicator.IsDedicatedServer || backgroundImage == null || loadingBarImage == null)
        {
            return;
        }
        updateProgress(0f);
        Local local = Localization.read("/Menu/MenuTips.dat");
        byte b;
        do
        {
            b = (byte)UnityEngine.Random.Range(1, TIP_COUNT + 1);
        }
        while (b == (byte)tip);
        tip = (ELoadingTip)b;
        string arg = ((OptionsSettings.streamer && Provider.streamerNames != null && Provider.streamerNames.Count > 0 && Provider.streamerNames[0] == "Nelson AI") ? local.format("Streamer") : (tip switch
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
                arg = local2.format(key);
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
                loadingLabel.text = localization.format("Loading_Level_Play", localizedName, Level.version, OptionsSettings.streamer ? localization.format("Streamer") : Provider.currentServerInfo.name, text);
            }
            else
            {
                loadingLabel.text = localization.format("Loading_Level_Edit", localizedName);
            }
            if (Level.info.configData.Creators.Length != 0 || Level.info.configData.Collaborators.Length != 0 || Level.info.configData.Thanks.Length != 0)
            {
                int num = 0;
                string text2 = "";
                if (Level.info.configData.Creators.Length != 0)
                {
                    text2 += localization.format("Creators");
                    num += 15;
                    for (int i = 0; i < Level.info.configData.Creators.Length; i++)
                    {
                        text2 = text2 + "\n" + Level.info.configData.Creators[i];
                        num += 15;
                    }
                }
                if (Level.info.configData.Collaborators.Length != 0)
                {
                    if (text2.Length > 0)
                    {
                        text2 += "\n\n";
                        num += 30;
                    }
                    text2 += localization.format("Collaborators");
                    num += 15;
                    for (int j = 0; j < Level.info.configData.Collaborators.Length; j++)
                    {
                        text2 = text2 + "\n" + Level.info.configData.Collaborators[j];
                        num += 15;
                    }
                }
                if (Level.info.configData.Thanks.Length != 0)
                {
                    if (text2.Length > 0)
                    {
                        text2 += "\n\n";
                        num += 30;
                    }
                    text2 += localization.format("Thanks");
                    num += 15;
                    for (int k = 0; k < Level.info.configData.Thanks.Length; k++)
                    {
                        text2 = text2 + "\n" + Level.info.configData.Thanks[k];
                        num += 15;
                    }
                }
                num = Mathf.Max(num, 40);
                creditsBox.positionOffset_Y = -num / 2;
                creditsBox.sizeOffset_Y = num;
                creditsBox.text = text2;
                creditsBox.isVisible = true;
            }
            else
            {
                creditsBox.isVisible = false;
            }
        }
        else
        {
            PickNonLevelBackgroundImage();
            DisableBackgroundAnim();
            loadingLabel.text = localization.format("Loading");
            creditsBox.isVisible = false;
        }
        tipBox.text = ItemTool.filterRarityRichText(local.format("Tip", arg));
        loadingBox.sizeOffset_X = -20;
        cancelButton.isVisible = false;
    }

    private static void onQueuePositionUpdated()
    {
        loadingLabel.text = localization.format("Queue_Position", Provider.queuePosition + 1);
        loadingBox.sizeOffset_X = -130;
        cancelButton.isVisible = true;
    }

    private static void onClickedCancelButton(ISleekElement button)
    {
        Provider.RequestDisconnect("clicked queue cancel button");
    }

    private void Update()
    {
        if (!Dedicator.IsDedicatedServer && (Assets.isLoading || Provider.isLoading || Level.isLoading || Player.isLoading))
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
        tipBox = Glazier.Get().CreateBox();
        tipBox.enableRichText = true;
        tipBox.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        tipBox.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        tipBox.positionOffset_X = 10;
        tipBox.positionOffset_Y = -100;
        tipBox.positionScale_Y = 1f;
        tipBox.sizeOffset_X = -20;
        tipBox.sizeOffset_Y = 30;
        tipBox.sizeScale_X = 1f;
        window.AddChild(tipBox);
        loadingBox = Glazier.Get().CreateBox();
        loadingBox.positionOffset_X = 10;
        loadingBox.positionOffset_Y = -60;
        loadingBox.positionScale_Y = 1f;
        loadingBox.sizeOffset_X = -20;
        loadingBox.sizeOffset_Y = 50;
        loadingBox.sizeScale_X = 1f;
        window.AddChild(loadingBox);
        loadingBarImage = Glazier.Get().CreateImage();
        loadingBarImage.positionOffset_X = 10;
        loadingBarImage.positionOffset_Y = 10;
        loadingBarImage.sizeOffset_X = -20;
        loadingBarImage.sizeOffset_Y = -20;
        loadingBarImage.sizeScale_X = 1f;
        loadingBarImage.sizeScale_Y = 1f;
        loadingBarImage.texture = (Texture2D)GlazierResources.PixelTexture;
        loadingBarImage.color = ESleekTint.FOREGROUND;
        loadingBox.AddChild(loadingBarImage);
        loadingLabel = Glazier.Get().CreateLabel();
        loadingLabel.positionOffset_X = 10;
        loadingLabel.positionOffset_Y = -15;
        loadingLabel.positionScale_Y = 0.5f;
        loadingLabel.sizeOffset_X = -20;
        loadingLabel.sizeOffset_Y = 30;
        loadingLabel.sizeScale_X = 1f;
        loadingLabel.fontSize = ESleekFontSize.Medium;
        loadingLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        loadingBox.AddChild(loadingLabel);
        creditsBox = Glazier.Get().CreateBox();
        creditsBox.positionOffset_X = -125;
        creditsBox.positionScale_X = 0.75f;
        creditsBox.positionScale_Y = 0.5f;
        creditsBox.sizeOffset_X = 250;
        window.AddChild(creditsBox);
        creditsBox.isVisible = false;
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
    }

    private void OnDestroy()
    {
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
