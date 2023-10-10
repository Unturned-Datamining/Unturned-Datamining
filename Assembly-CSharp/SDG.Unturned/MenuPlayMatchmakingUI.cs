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
                selectedBox.Text = local.format("Name");
            }
            else
            {
                selectedBox.Text = PlaySettings.matchmakingMap;
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
                sleekLevel.PositionOffset_Y = i * 110;
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
        levelScrollBox.ContentSizeOffset = new Vector2(0f, levels.Length * 110 - 10);
    }

    private static void onLevelsRefreshed()
    {
        refreshLevels();
    }

    private static void updateProgressDisplay()
    {
        if (progressBox != null)
        {
            progressBox.Text = localization.format("Progress_Matches", (Provider.provider.matchmakingService.serverList != null) ? Provider.provider.matchmakingService.serverList.Count : 0);
            progressBox.Text += "\n";
            if (Provider.provider.matchmakingService.matchmakingBestServer != null)
            {
                progressBox.Text += localization.format("Progress_Best", localization.format("Match_Yes", Provider.provider.matchmakingService.matchmakingBestServer.players, Provider.provider.matchmakingService.matchmakingBestServer.maxPlayers, Provider.provider.matchmakingService.matchmakingBestServer.ping));
            }
            else
            {
                progressBox.Text += localization.format("Progress_Best", localization.format("Match_No"));
            }
        }
    }

    private static void updateMatchDisplay()
    {
        if (infoButton != null && luckyButton != null)
        {
            infoButton.IsVisible = Provider.provider.matchmakingService.matchmakingBestServer != null;
            luckyButton.IsVisible = Provider.provider.matchmakingService.matchmakingBestServer != null;
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        levelScrollBox = Glazier.Get().CreateScrollView();
        levelScrollBox.PositionOffset_X = -200f;
        levelScrollBox.PositionOffset_Y = 280f;
        levelScrollBox.PositionScale_X = 0.5f;
        levelScrollBox.SizeOffset_X = 430f;
        levelScrollBox.SizeOffset_Y = -380f;
        levelScrollBox.SizeScale_Y = 1f;
        levelScrollBox.ScaleContentToWidth = true;
        container.AddChild(levelScrollBox);
        searchButton = new SleekButtonIcon(bundle.load<Texture2D>("Search"));
        searchButton.PositionOffset_X = -200f;
        searchButton.PositionOffset_Y = 100f;
        searchButton.PositionScale_X = 0.5f;
        searchButton.SizeOffset_X = 400f;
        searchButton.SizeOffset_Y = 30f;
        searchButton.text = localization.format("Search_Button");
        searchButton.tooltip = localization.format("Search_Button_Tooltip");
        searchButton.iconColor = ESleekTint.FOREGROUND;
        searchButton.onClickedButton += onClickedSearchButton;
        container.AddChild(searchButton);
        selectedBox = Glazier.Get().CreateBox();
        selectedBox.PositionOffset_X = -200f;
        selectedBox.PositionOffset_Y = 250f;
        selectedBox.PositionScale_X = 0.5f;
        selectedBox.SizeOffset_X = 400f;
        selectedBox.SizeOffset_Y = 30f;
        container.AddChild(selectedBox);
        progressBox = Glazier.Get().CreateBox();
        progressBox.PositionOffset_X = -200f;
        progressBox.PositionOffset_Y = 130f;
        progressBox.PositionScale_X = 0.5f;
        progressBox.SizeOffset_X = 400f;
        progressBox.SizeOffset_Y = 50f;
        container.AddChild(progressBox);
        infoButton = new SleekButtonIcon(bundle.load<Texture2D>("Info"));
        infoButton.PositionOffset_X = -200f;
        infoButton.PositionOffset_Y = 180f;
        infoButton.PositionScale_X = 0.5f;
        infoButton.SizeOffset_X = 195f;
        infoButton.SizeOffset_Y = 30f;
        infoButton.text = localization.format("Info_Button");
        infoButton.tooltip = localization.format("Info_Button_Tooltip");
        infoButton.iconColor = ESleekTint.FOREGROUND;
        infoButton.onClickedButton += onClickedInfoButton;
        container.AddChild(infoButton);
        luckyButton = new SleekButtonIcon(bundle.load<Texture2D>("Lucky"));
        luckyButton.PositionOffset_X = 5f;
        luckyButton.PositionOffset_Y = 180f;
        luckyButton.PositionScale_X = 0.5f;
        luckyButton.SizeOffset_X = 195f;
        luckyButton.SizeOffset_Y = 30f;
        luckyButton.text = localization.format("Lucky_Button");
        luckyButton.tooltip = localization.format("Lucky_Button_Tooltip");
        luckyButton.iconColor = ESleekTint.FOREGROUND;
        luckyButton.onClickedButton += onClickedLuckyButton;
        container.AddChild(luckyButton);
        updateProgressDisplay();
        updateMatchDisplay();
        modeButtonState = new SleekButtonState(new GUIContent(localization.format("Easy_Button"), bundle.load<Texture2D>("Easy")), new GUIContent(localization.format("Normal_Button"), bundle.load<Texture2D>("Normal")), new GUIContent(localization.format("Hard_Button"), bundle.load<Texture2D>("Hard")));
        modeButtonState.PositionOffset_X = -200f;
        modeButtonState.PositionOffset_Y = 220f;
        modeButtonState.PositionScale_X = 0.5f;
        modeButtonState.SizeOffset_X = 400f;
        modeButtonState.SizeOffset_Y = 30f;
        modeButtonState.state = (int)PlaySettings.matchmakingMode;
        modeButtonState.onSwappedState = onSwappedModeState;
        container.AddChild(modeButtonState);
        bundle.unload();
        refreshLevels();
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Combine(Level.onLevelsRefreshed, new LevelsRefreshed(onLevelsRefreshed));
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
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
