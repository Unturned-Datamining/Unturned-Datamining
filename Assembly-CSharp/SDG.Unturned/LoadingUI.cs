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
        CommandWindow.Log(localization.format(key));
    }

    public static void updateProgress(float progress)
    {
        CommandWindow.Log(localization.format("Level_Load", (int)(progress * 100f)));
    }

    public static void assetsLoad(string key, int count, float progress, float step, bool formatKey = true)
    {
        assetsLoadCount = assetsScanCount - count;
        CommandWindow.Log(localization.format("Assets_Load", formatKey ? localization.format(key) : key, assetsLoadCount, assetsScanCount));
    }

    public static void assetsScan(string key, int count, bool formatKey = true)
    {
        assetsScanCount = count;
        CommandWindow.Log(localization.format("Assets_Scan", formatKey ? localization.format(key) : key, assetsScanCount));
    }

    public static void notifyMasterBundleProgress(string key, string name, float progress)
    {
        int num = Mathf.RoundToInt(progress * 100f);
        if (masterBundleProgress != num)
        {
            masterBundleProgress = num;
            string text = localization.format(key, name, num);
            CommandWindow.Log(text);
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
        bool flag2 = (UnturnedMasterVolume.mutedByLoadingScreen = isBlocked);
        placeholderCamera.enabled = flag2;
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
        UnityEngine.Object.Destroy(base.gameObject);
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
