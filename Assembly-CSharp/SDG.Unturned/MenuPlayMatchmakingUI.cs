using System;
using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayMatchmakingUI
{
    public static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static LevelInfo[] levels;

    private static ISleekScrollView levelScrollBox;

    private static SleekLevel[] levelButtons;

    private static SleekButtonIcon searchButton;

    private static SleekButtonState modeButtonState;

    private static ISleekBox selectedBox;

    private static ISleekBox progressBox;

    private static SleekButtonIcon infoButton;

    private static SleekButtonIcon luckyButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            MenuSettings.save();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void updateSelection()
    {
        if (PlaySettings.matchmakingMap == null || PlaySettings.matchmakingMap.Length == 0)
        {
            return;
        }
        LevelInfo level = Level.getLevel(PlaySettings.matchmakingMap);
        if (level != null)
        {
            Local local = level.getLocalization();
            if (local != null && local.has("Name"))
            {
                selectedBox.text = local.format("Name");
            }
            else
            {
                selectedBox.text = PlaySettings.matchmakingMap;
            }
        }
    }

    private static void onClickedLevel(SleekLevel level, byte index)
    {
        if (index < levels.Length && levels[index] != null)
        {
            PlaySettings.matchmakingMap = levels[index].name;
            updateSelection();
        }
    }

    private static void onClickedSearchButton(ISleekElement button)
    {
        if (PlaySettings.matchmakingMap != null && PlaySettings.matchmakingMap.Length != 0)
        {
            MenuSettings.save();
            Provider.provider.matchmakingService.initializeMatchmaking();
            Provider.provider.matchmakingService.refreshMasterServer(ESteamServerList.INTERNET, PlaySettings.matchmakingMap, EPassword.NO, EWorkshop.NO, EPlugins.ANY, EAttendance.HasPlayers, filterNotFull: true, EVACProtectionFilter.Secure, EBattlEyeProtectionFilter.Secure, filterPro: false, ECombat.PVP, ECheats.NO, PlaySettings.matchmakingMode, ECameraMode.BOTH, EServerMonetizationTag.Any);
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoMatchmakingComparator());
            updateProgressDisplay();
            updateMatchDisplay();
        }
    }

    private static void onClickedInfoButton(ISleekElement button)
    {
        MenuPlayServerInfoUI.open(Provider.provider.matchmakingService.matchmakingBestServer, "", MenuPlayServerInfoUI.EServerInfoOpenContext.MATCHMAKING);
        close();
    }

    private static void onClickedLuckyButton(ISleekElement button)
    {
        EInternetMultiplayerAvailability internetMultiplayerAvailability = Provider.GetInternetMultiplayerAvailability();
        if (internetMultiplayerAvailability != 0)
        {
            MenuUI.AlertInternetMultiplayerAvailability(internetMultiplayerAvailability);
        }
        else
        {
            Provider.connect(Provider.provider.matchmakingService.matchmakingBestServer, "", null);
        }
    }

    private static void onSwappedModeState(SleekButtonState button, int index)
    {
        PlaySettings.matchmakingMode = (EGameMode)index;
    }

    private static void refreshLevels()
    {
        levelScrollBox.RemoveAllChildren();
        levels = Level.getLevels(OptionsSettings.matchmakingShowAllMaps ? ESingleplayerMapCategory.ALL : ESingleplayerMapCategory.MATCHMAKING);
        bool flag = false;
        levelButtons = new SleekLevel[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                SleekLevel sleekLevel = new SleekLevel(levels[i], isEditor: false);
                sleekLevel.positionOffset_Y = i * 110;
                sleekLevel.onClickedLevel = onClickedLevel;
                levelScrollBox.AddChild(sleekLevel);
                levelButtons[i] = sleekLevel;
                if (!flag && levels[i].name == PlaySettings.matchmakingMap)
                {
                    flag = true;
                }
            }
        }
        if (levels.Length == 0)
        {
            PlaySettings.matchmakingMap = "";
        }
        else if (!flag || PlaySettings.matchmakingMap == null || PlaySettings.matchmakingMap.Length == 0)
        {
            PlaySettings.matchmakingMap = levels[0].name;
        }
        updateSelection();
        levelScrollBox.contentSizeOffset = new Vector2(0f, levels.Length * 110 - 10);
    }

    private static void onLevelsRefreshed()
    {
        refreshLevels();
    }

    private static void updateProgressDisplay()
    {
        if (progressBox != null)
        {
            progressBox.text = localization.format("Progress_Matches", (Provider.provider.matchmakingService.serverList != null) ? Provider.provider.matchmakingService.serverList.Count : 0);
            progressBox.text += "\n";
            if (Provider.provider.matchmakingService.matchmakingBestServer != null)
            {
                progressBox.text += localization.format("Progress_Best", localization.format("Match_Yes", Provider.provider.matchmakingService.matchmakingBestServer.players, Provider.provider.matchmakingService.matchmakingBestServer.maxPlayers, Provider.provider.matchmakingService.matchmakingBestServer.ping));
            }
            else
            {
                progressBox.text += localization.format("Progress_Best", localization.format("Match_No"));
            }
        }
    }

    private static void updateMatchDisplay()
    {
        if (infoButton != null && luckyButton != null)
        {
            infoButton.isVisible = Provider.provider.matchmakingService.matchmakingBestServer != null;
            luckyButton.isVisible = Provider.provider.matchmakingService.matchmakingBestServer != null;
        }
    }

    private static void handleMatchmakingProgressed()
    {
        updateProgressDisplay();
    }

    private static void handleMatchmakingFinished()
    {
        updateMatchDisplay();
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuPlayUI.open();
        close();
    }

    public void OnDestroy()
    {
        TempSteamworksMatchmaking matchmakingService = Provider.provider.matchmakingService;
        matchmakingService.matchmakingProgressed = (TempSteamworksMatchmaking.MatchmakingProgressedHandler)Delegate.Remove(matchmakingService.matchmakingProgressed, new TempSteamworksMatchmaking.MatchmakingProgressedHandler(handleMatchmakingProgressed));
        TempSteamworksMatchmaking matchmakingService2 = Provider.provider.matchmakingService;
        matchmakingService2.matchmakingFinished = (TempSteamworksMatchmaking.MatchmakingFinishedHandler)Delegate.Remove(matchmakingService2.matchmakingFinished, new TempSteamworksMatchmaking.MatchmakingFinishedHandler(handleMatchmakingFinished));
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Remove(Level.onLevelsRefreshed, new LevelsRefreshed(onLevelsRefreshed));
    }

    public MenuPlayMatchmakingUI()
    {
        localization = Localization.read("/Menu/Play/MenuPlayMatchmaking.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Play/MenuPlayMatchmaking/MenuPlayMatchmaking.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        levelScrollBox = Glazier.Get().CreateScrollView();
        levelScrollBox.positionOffset_X = -200;
        levelScrollBox.positionOffset_Y = 280;
        levelScrollBox.positionScale_X = 0.5f;
        levelScrollBox.sizeOffset_X = 430;
        levelScrollBox.sizeOffset_Y = -380;
        levelScrollBox.sizeScale_Y = 1f;
        levelScrollBox.scaleContentToWidth = true;
        container.AddChild(levelScrollBox);
        searchButton = new SleekButtonIcon(bundle.load<Texture2D>("Search"));
        searchButton.positionOffset_X = -200;
        searchButton.positionOffset_Y = 100;
        searchButton.positionScale_X = 0.5f;
        searchButton.sizeOffset_X = 400;
        searchButton.sizeOffset_Y = 30;
        searchButton.text = localization.format("Search_Button");
        searchButton.tooltip = localization.format("Search_Button_Tooltip");
        searchButton.iconColor = ESleekTint.FOREGROUND;
        searchButton.onClickedButton += onClickedSearchButton;
        container.AddChild(searchButton);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.positionOffset_X = -200;
        selectedBox.positionOffset_Y = 250;
        selectedBox.positionScale_X = 0.5f;
        selectedBox.sizeOffset_X = 400;
        selectedBox.sizeOffset_Y = 30;
        container.AddChild(selectedBox);
        progressBox = Glazier.Get().CreateBox();
        progressBox.positionOffset_X = -200;
        progressBox.positionOffset_Y = 130;
        progressBox.positionScale_X = 0.5f;
        progressBox.sizeOffset_X = 400;
        progressBox.sizeOffset_Y = 50;
        container.AddChild(progressBox);
        infoButton = new SleekButtonIcon(bundle.load<Texture2D>("Info"));
        infoButton.positionOffset_X = -200;
        infoButton.positionOffset_Y = 180;
        infoButton.positionScale_X = 0.5f;
        infoButton.sizeOffset_X = 195;
        infoButton.sizeOffset_Y = 30;
        infoButton.text = localization.format("Info_Button");
        infoButton.tooltip = localization.format("Info_Button_Tooltip");
        infoButton.iconColor = ESleekTint.FOREGROUND;
        infoButton.onClickedButton += onClickedInfoButton;
        container.AddChild(infoButton);
        luckyButton = new SleekButtonIcon(bundle.load<Texture2D>("Lucky"));
        luckyButton.positionOffset_X = 5;
        luckyButton.positionOffset_Y = 180;
        luckyButton.positionScale_X = 0.5f;
        luckyButton.sizeOffset_X = 195;
        luckyButton.sizeOffset_Y = 30;
        luckyButton.text = localization.format("Lucky_Button");
        luckyButton.tooltip = localization.format("Lucky_Button_Tooltip");
        luckyButton.iconColor = ESleekTint.FOREGROUND;
        luckyButton.onClickedButton += onClickedLuckyButton;
        container.AddChild(luckyButton);
        updateProgressDisplay();
        updateMatchDisplay();
        modeButtonState = new SleekButtonState(new GUIContent(localization.format("Easy_Button"), bundle.load<Texture2D>("Easy")), new GUIContent(localization.format("Normal_Button"), bundle.load<Texture2D>("Normal")), new GUIContent(localization.format("Hard_Button"), bundle.load<Texture2D>("Hard")));
        modeButtonState.positionOffset_X = -200;
        modeButtonState.positionOffset_Y = 220;
        modeButtonState.positionScale_X = 0.5f;
        modeButtonState.sizeOffset_X = 400;
        modeButtonState.sizeOffset_Y = 30;
        modeButtonState.state = (int)PlaySettings.matchmakingMode;
        modeButtonState.onSwappedState = onSwappedModeState;
        container.AddChild(modeButtonState);
        bundle.unload();
        refreshLevels();
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Combine(Level.onLevelsRefreshed, new LevelsRefreshed(onLevelsRefreshed));
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        TempSteamworksMatchmaking matchmakingService = Provider.provider.matchmakingService;
        matchmakingService.matchmakingProgressed = (TempSteamworksMatchmaking.MatchmakingProgressedHandler)Delegate.Combine(matchmakingService.matchmakingProgressed, new TempSteamworksMatchmaking.MatchmakingProgressedHandler(handleMatchmakingProgressed));
        TempSteamworksMatchmaking matchmakingService2 = Provider.provider.matchmakingService;
        matchmakingService2.matchmakingFinished = (TempSteamworksMatchmaking.MatchmakingFinishedHandler)Delegate.Combine(matchmakingService2.matchmakingFinished, new TempSteamworksMatchmaking.MatchmakingFinishedHandler(handleMatchmakingFinished));
    }
}
