using System;
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayServersUI
{
    public static Local localization;

    public static Bundle icons;

    private static SleekFullscreenBox container;

    private static ISleekElement list;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static LevelInfo[] levels;

    private static SleekList<SteamServerInfo> serverBox;

    private static ISleekBox infoBox;

    private static ISleekButton resetFiltersButton;

    private static ISleekButton sortName;

    private static ISleekButton sortMap;

    private static ISleekButton sortPlayers;

    private static ISleekButton sortPing;

    private static ISleekField nameField;

    private static ISleekLabel refreshLabel;

    private static ISleekButton refreshInternetButton;

    private static ISleekButton refreshGoldButton;

    private static ISleekButton refreshLANButton;

    private static ISleekButton refreshHistoryButton;

    private static ISleekButton refreshFavoritesButton;

    private static ISleekButton refreshFriendsButton;

    private static SleekButtonIcon hostingButton;

    private static ISleekField mapField;

    private static SleekButtonState monetizationButtonState;

    private static SleekButtonState passwordButtonState;

    private static SleekButtonState workshopButtonState;

    private static SleekButtonState pluginsButtonState;

    private static SleekButtonState cheatsButtonState;

    private static SleekButtonState attendanceButtonState;

    private static SleekButtonState notFullButtonState;

    private static SleekButtonState VACProtectionButtonState;

    private static SleekButtonState battlEyeProtectionButtonState;

    private static SleekButtonState combatButtonState;

    private static SleekButtonState modeButtonState;

    private static SleekButtonState cameraButtonState;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void openForMap(string map)
    {
        FilterSettings.filterMap = map;
        mapField.Text = map;
        onClickedRefreshInternetButton(refreshInternetButton);
        open();
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

    private static void onClickedServer(SleekServer server, SteamServerInfo info)
    {
        if (!info.isPro || Provider.isPro)
        {
            MenuSettings.save();
            MenuPlayServerInfoUI.open(info, string.Empty, MenuPlayServerInfoUI.EServerInfoOpenContext.SERVERS);
            close();
        }
    }

    private static void onMasterServerAdded(int insert, SteamServerInfo info)
    {
        serverBox.NotifyDataChanged();
    }

    private static void onMasterServerRemoved()
    {
        infoBox.IsVisible = false;
        serverBox.NotifyDataChanged();
    }

    private static void onMasterServerResorted()
    {
        infoBox.IsVisible = false;
        serverBox.NotifyDataChanged();
    }

    private static void onMasterServerRefreshed(EMatchMakingServerResponse response)
    {
        if (Provider.provider.matchmakingService.serverList.Count == 0)
        {
            infoBox.IsVisible = true;
        }
    }

    private static void onClickedSortNameButton(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer is SteamServerInfoNameAscendingComparator)
        {
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoNameDescendingComparator());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoNameAscendingComparator());
        }
    }

    private static void onClickedSortMapButton(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer is SteamServerInfoMapAscendingComparator)
        {
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoMapDescendingComparator());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoMapAscendingComparator());
        }
    }

    private static void onClickedSortPlayersButton(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer is SteamServerInfoPlayersAscendingComparator)
        {
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoPlayersDescendingComparator());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoPlayersAscendingComparator());
        }
    }

    private static void onClickedSortPingButton(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer is SteamServerInfoPingAscendingComparator)
        {
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoPingDescendingComparator());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new SteamServerInfoPingAscendingComparator());
        }
    }

    private static void onClickedRefreshInternetButton(ISleekElement button)
    {
        Provider.provider.matchmakingService.refreshMasterServer(ESteamServerList.INTERNET, FilterSettings.filterMap, FilterSettings.filterPassword, FilterSettings.filterWorkshop, FilterSettings.filterPlugins, FilterSettings.filterAttendance, FilterSettings.filterNotFull, FilterSettings.filterVACProtection, FilterSettings.filterBattlEyeProtection, filterPro: false, FilterSettings.filterCombat, FilterSettings.filterCheats, FilterSettings.filterMode, FilterSettings.filterCamera, FilterSettings.filterMonetization);
    }

    private static void onClickedRefreshGoldButton(ISleekElement button)
    {
        Provider.provider.matchmakingService.refreshMasterServer(ESteamServerList.INTERNET, FilterSettings.filterMap, FilterSettings.filterPassword, FilterSettings.filterWorkshop, FilterSettings.filterPlugins, FilterSettings.filterAttendance, FilterSettings.filterNotFull, FilterSettings.filterVACProtection, FilterSettings.filterBattlEyeProtection, filterPro: true, FilterSettings.filterCombat, FilterSettings.filterCheats, FilterSettings.filterMode, FilterSettings.filterCamera, FilterSettings.filterMonetization);
    }

    private static void onClickedRefreshLANButton(ISleekElement button)
    {
        Provider.provider.matchmakingService.refreshMasterServer(ESteamServerList.LAN, FilterSettings.filterMap, FilterSettings.filterPassword, FilterSettings.filterWorkshop, FilterSettings.filterPlugins, FilterSettings.filterAttendance, FilterSettings.filterNotFull, FilterSettings.filterVACProtection, FilterSettings.filterBattlEyeProtection, filterPro: false, FilterSettings.filterCombat, FilterSettings.filterCheats, FilterSettings.filterMode, FilterSettings.filterCamera, FilterSettings.filterMonetization);
    }

    private static void onClickedRefreshHistoryButton(ISleekElement button)
    {
        Provider.provider.matchmakingService.refreshMasterServer(ESteamServerList.HISTORY, FilterSettings.filterMap, FilterSettings.filterPassword, FilterSettings.filterWorkshop, FilterSettings.filterPlugins, FilterSettings.filterAttendance, FilterSettings.filterNotFull, FilterSettings.filterVACProtection, FilterSettings.filterBattlEyeProtection, filterPro: false, FilterSettings.filterCombat, FilterSettings.filterCheats, FilterSettings.filterMode, FilterSettings.filterCamera, FilterSettings.filterMonetization);
    }

    private static void onClickedRefreshFavoritesButton(ISleekElement button)
    {
        Provider.provider.matchmakingService.refreshMasterServer(ESteamServerList.FAVORITES, FilterSettings.filterMap, FilterSettings.filterPassword, FilterSettings.filterWorkshop, FilterSettings.filterPlugins, FilterSettings.filterAttendance, FilterSettings.filterNotFull, FilterSettings.filterVACProtection, FilterSettings.filterBattlEyeProtection, filterPro: false, FilterSettings.filterCombat, FilterSettings.filterCheats, FilterSettings.filterMode, FilterSettings.filterCamera, FilterSettings.filterMonetization);
    }

    private static void onClickedRefreshFriendsButton(ISleekElement button)
    {
        Provider.provider.matchmakingService.refreshMasterServer(ESteamServerList.FRIENDS, FilterSettings.filterMap, FilterSettings.filterPassword, FilterSettings.filterWorkshop, FilterSettings.filterPlugins, FilterSettings.filterAttendance, FilterSettings.filterNotFull, FilterSettings.filterVACProtection, FilterSettings.filterBattlEyeProtection, filterPro: false, FilterSettings.filterCombat, FilterSettings.filterCheats, FilterSettings.filterMode, FilterSettings.filterCamera, FilterSettings.filterMonetization);
    }

    private static void onClickedHostingButton(ISleekElement button)
    {
        Provider.provider.browserService.open("https://docs.smartlydressedgames.com/en/stable/servers/server-hosting.html");
    }

    private static ISleekElement onCreateServerElement(SteamServerInfo server)
    {
        return new SleekServer(Provider.provider.matchmakingService.currentList, server)
        {
            onClickedServer = onClickedServer,
            SizeOffset_X = -30f
        };
    }

    /// <summary>
    /// Synchronize widgets with their values.
    /// </summary>
    private static void updateAll()
    {
        nameField.Text = PlaySettings.serversName;
        mapField.Text = FilterSettings.filterMap;
        passwordButtonState.state = (int)FilterSettings.filterPassword;
        workshopButtonState.state = (int)FilterSettings.filterWorkshop;
        pluginsButtonState.state = (int)FilterSettings.filterPlugins;
        cheatsButtonState.state = (int)FilterSettings.filterCheats;
        attendanceButtonState.state = (int)FilterSettings.filterAttendance;
        notFullButtonState.state = (FilterSettings.filterNotFull ? 1 : 0);
        VACProtectionButtonState.state = (int)FilterSettings.filterVACProtection;
        battlEyeProtectionButtonState.state = (int)FilterSettings.filterBattlEyeProtection;
        combatButtonState.state = (int)FilterSettings.filterCombat;
        modeButtonState.state = (int)FilterSettings.filterMode;
        cameraButtonState.state = (int)FilterSettings.filterCamera;
        monetizationButtonState.state = (int)(FilterSettings.filterMonetization - 1);
    }

    private static void onClickedResetFilters(ISleekElement button)
    {
        PlaySettings.restoreFilterDefaultValues();
        FilterSettings.restoreDefaultValues();
        updateAll();
    }

    private static void onTypedNameField(ISleekField field, string text)
    {
        PlaySettings.serversName = text;
    }

    private static void onTypedMapField(ISleekField field, string text)
    {
        FilterSettings.filterMap = text;
    }

    private static void onSwappedMonetizationState(SleekButtonState button, int index)
    {
        FilterSettings.filterMonetization = (EServerMonetizationTag)(index + 1);
    }

    private static void onSwappedPasswordState(SleekButtonState button, int index)
    {
        FilterSettings.filterPassword = (EPassword)index;
    }

    private static void onSwappedWorkshopState(SleekButtonState button, int index)
    {
        FilterSettings.filterWorkshop = (EWorkshop)index;
    }

    private static void onSwappedPluginsState(SleekButtonState button, int index)
    {
        FilterSettings.filterPlugins = (EPlugins)index;
    }

    private static void onSwappedCheatsState(SleekButtonState button, int index)
    {
        FilterSettings.filterCheats = (ECheats)index;
    }

    private static void onSwappedAttendanceState(SleekButtonState button, int index)
    {
        FilterSettings.filterAttendance = (EAttendance)index;
    }

    private static void OnSwappedNotFullState(SleekButtonState button, int index)
    {
        FilterSettings.filterNotFull = index > 0;
    }

    private static void onSwappedVACProtectionState(SleekButtonState button, int index)
    {
        FilterSettings.filterVACProtection = (EVACProtectionFilter)index;
    }

    private static void onSwappedBattlEyeProtectionState(SleekButtonState button, int index)
    {
        FilterSettings.filterBattlEyeProtection = (EBattlEyeProtectionFilter)index;
    }

    private static void onSwappedCombatState(SleekButtonState button, int index)
    {
        FilterSettings.filterCombat = (ECombat)index;
    }

    private static void onSwappedModeState(SleekButtonState button, int index)
    {
        FilterSettings.filterMode = (EGameMode)index;
    }

    private static void onSwappedCameraState(SleekButtonState button, int index)
    {
        FilterSettings.filterCamera = (ECameraMode)index;
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuPlayUI.open();
        close();
    }

    public void OnDestroy()
    {
        TempSteamworksMatchmaking matchmakingService = Provider.provider.matchmakingService;
        matchmakingService.onMasterServerAdded = (TempSteamworksMatchmaking.MasterServerAdded)Delegate.Remove(matchmakingService.onMasterServerAdded, new TempSteamworksMatchmaking.MasterServerAdded(onMasterServerAdded));
        TempSteamworksMatchmaking matchmakingService2 = Provider.provider.matchmakingService;
        matchmakingService2.onMasterServerRemoved = (TempSteamworksMatchmaking.MasterServerRemoved)Delegate.Remove(matchmakingService2.onMasterServerRemoved, new TempSteamworksMatchmaking.MasterServerRemoved(onMasterServerRemoved));
        TempSteamworksMatchmaking matchmakingService3 = Provider.provider.matchmakingService;
        matchmakingService3.onMasterServerResorted = (TempSteamworksMatchmaking.MasterServerResorted)Delegate.Remove(matchmakingService3.onMasterServerResorted, new TempSteamworksMatchmaking.MasterServerResorted(onMasterServerResorted));
        TempSteamworksMatchmaking matchmakingService4 = Provider.provider.matchmakingService;
        matchmakingService4.onMasterServerRefreshed = (TempSteamworksMatchmaking.MasterServerRefreshed)Delegate.Remove(matchmakingService4.onMasterServerRefreshed, new TempSteamworksMatchmaking.MasterServerRefreshed(onMasterServerRefreshed));
    }

    public MenuPlayServersUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Menu/Play/MenuPlayServers.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Play/MenuPlayServers/MenuPlayServers.unity3d");
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
        list = Glazier.Get().CreateFrame();
        list.PositionOffset_X = 210f;
        list.SizeOffset_X = -210f;
        list.SizeScale_X = 1f;
        list.SizeScale_Y = 1f;
        container.AddChild(list);
        sortName = Glazier.Get().CreateButton();
        sortName.SizeOffset_X = -270f;
        sortName.SizeOffset_Y = 30f;
        sortName.SizeScale_X = 1f;
        sortName.Text = localization.format("Sort_Name");
        sortName.TooltipText = localization.format("Sort_Name_Tooltip");
        sortName.OnClicked += onClickedSortNameButton;
        list.AddChild(sortName);
        sortMap = Glazier.Get().CreateButton();
        sortMap.PositionOffset_X = -260f;
        sortMap.PositionScale_X = 1f;
        sortMap.SizeOffset_X = 100f;
        sortMap.SizeOffset_Y = 30f;
        sortMap.Text = localization.format("Sort_Map");
        sortMap.TooltipText = localization.format("Sort_Map_Tooltip");
        sortMap.OnClicked += onClickedSortMapButton;
        list.AddChild(sortMap);
        sortPlayers = Glazier.Get().CreateButton();
        sortPlayers.PositionOffset_X = -150f;
        sortPlayers.PositionScale_X = 1f;
        sortPlayers.SizeOffset_X = 60f;
        sortPlayers.SizeOffset_Y = 30f;
        sortPlayers.Text = localization.format("Sort_Players");
        sortPlayers.TooltipText = localization.format("Sort_Players_Tooltip");
        sortPlayers.OnClicked += onClickedSortPlayersButton;
        list.AddChild(sortPlayers);
        sortPing = Glazier.Get().CreateButton();
        sortPing.PositionOffset_X = -80f;
        sortPing.PositionScale_X = 1f;
        sortPing.SizeOffset_X = 50f;
        sortPing.SizeOffset_Y = 30f;
        sortPing.Text = localization.format("Sort_Ping");
        sortPing.TooltipText = localization.format("Sort_Ping_Tooltip");
        sortPing.OnClicked += onClickedSortPingButton;
        list.AddChild(sortPing);
        infoBox = Glazier.Get().CreateBox();
        infoBox.PositionOffset_Y = 40f;
        infoBox.SizeScale_X = 1f;
        infoBox.SizeOffset_X = -30f;
        infoBox.SizeOffset_Y = 50f;
        list.AddChild(infoBox);
        infoBox.IsVisible = false;
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.SizeScale_X = 1f;
        sleekLabel.SizeOffset_Y = 30f;
        sleekLabel.Text = localization.format("No_Servers", Provider.APP_VERSION);
        sleekLabel.FontSize = ESleekFontSize.Medium;
        infoBox.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.PositionOffset_Y = 20f;
        sleekLabel2.SizeScale_X = 1f;
        sleekLabel2.SizeOffset_Y = 30f;
        sleekLabel2.Text = localization.format("No_Servers_Hint");
        infoBox.AddChild(sleekLabel2);
        resetFiltersButton = Glazier.Get().CreateButton();
        resetFiltersButton.PositionOffset_X = -150f;
        resetFiltersButton.PositionOffset_Y = 10f;
        resetFiltersButton.PositionScale_X = 0.5f;
        resetFiltersButton.PositionScale_Y = 1f;
        resetFiltersButton.SizeOffset_X = 300f;
        resetFiltersButton.SizeOffset_Y = 30f;
        resetFiltersButton.Text = localization.format("Reset_Filters");
        resetFiltersButton.TooltipText = localization.format("Reset_Filters_Tooltip");
        resetFiltersButton.OnClicked += onClickedResetFilters;
        infoBox.AddChild(resetFiltersButton);
        TempSteamworksMatchmaking matchmakingService = Provider.provider.matchmakingService;
        matchmakingService.onMasterServerAdded = (TempSteamworksMatchmaking.MasterServerAdded)Delegate.Combine(matchmakingService.onMasterServerAdded, new TempSteamworksMatchmaking.MasterServerAdded(onMasterServerAdded));
        TempSteamworksMatchmaking matchmakingService2 = Provider.provider.matchmakingService;
        matchmakingService2.onMasterServerRemoved = (TempSteamworksMatchmaking.MasterServerRemoved)Delegate.Combine(matchmakingService2.onMasterServerRemoved, new TempSteamworksMatchmaking.MasterServerRemoved(onMasterServerRemoved));
        TempSteamworksMatchmaking matchmakingService3 = Provider.provider.matchmakingService;
        matchmakingService3.onMasterServerResorted = (TempSteamworksMatchmaking.MasterServerResorted)Delegate.Combine(matchmakingService3.onMasterServerResorted, new TempSteamworksMatchmaking.MasterServerResorted(onMasterServerResorted));
        TempSteamworksMatchmaking matchmakingService4 = Provider.provider.matchmakingService;
        matchmakingService4.onMasterServerRefreshed = (TempSteamworksMatchmaking.MasterServerRefreshed)Delegate.Combine(matchmakingService4.onMasterServerRefreshed, new TempSteamworksMatchmaking.MasterServerRefreshed(onMasterServerRefreshed));
        nameField = Glazier.Get().CreateStringField();
        nameField.PositionOffset_Y = -110f;
        nameField.PositionScale_Y = 1f;
        nameField.SizeOffset_X = -5f;
        nameField.SizeOffset_Y = 30f;
        nameField.SizeScale_X = 0.4f;
        nameField.PlaceholderText = localization.format("Name_Field_Hint");
        nameField.OnTextChanged += onTypedNameField;
        list.AddChild(nameField);
        refreshLabel = Glazier.Get().CreateLabel();
        refreshLabel.SizeOffset_X = 200f;
        refreshLabel.SizeOffset_Y = 30f;
        refreshLabel.Text = localization.format("Refresh_Label");
        refreshLabel.TextAlignment = TextAnchor.MiddleCenter;
        container.AddChild(refreshLabel);
        refreshInternetButton = Glazier.Get().CreateButton();
        refreshInternetButton.PositionOffset_Y = 30f;
        refreshInternetButton.SizeOffset_X = 200f;
        refreshInternetButton.SizeOffset_Y = 50f;
        refreshInternetButton.Text = localization.format("Refresh_Internet_Button");
        refreshInternetButton.TooltipText = localization.format("Refresh_Internet_Button_Tooltip");
        refreshInternetButton.OnClicked += onClickedRefreshInternetButton;
        refreshInternetButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(refreshInternetButton);
        refreshLANButton = Glazier.Get().CreateButton();
        refreshLANButton.PositionOffset_Y = 210f;
        refreshLANButton.SizeOffset_X = 200f;
        refreshLANButton.SizeOffset_Y = 50f;
        refreshLANButton.Text = localization.format("Refresh_LAN_Button");
        refreshLANButton.TooltipText = localization.format("Refresh_LAN_Button_Tooltip");
        refreshLANButton.OnClicked += onClickedRefreshLANButton;
        refreshLANButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(refreshLANButton);
        refreshHistoryButton = Glazier.Get().CreateButton();
        refreshHistoryButton.PositionOffset_Y = 150f;
        refreshHistoryButton.SizeOffset_X = 200f;
        refreshHistoryButton.SizeOffset_Y = 50f;
        refreshHistoryButton.Text = localization.format("Refresh_History_Button");
        refreshHistoryButton.TooltipText = localization.format("Refresh_History_Button_Tooltip");
        refreshHistoryButton.OnClicked += onClickedRefreshHistoryButton;
        refreshHistoryButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(refreshHistoryButton);
        refreshFavoritesButton = Glazier.Get().CreateButton();
        refreshFavoritesButton.PositionOffset_Y = 90f;
        refreshFavoritesButton.SizeOffset_X = 200f;
        refreshFavoritesButton.SizeOffset_Y = 50f;
        refreshFavoritesButton.Text = localization.format("Refresh_Favorites_Button");
        refreshFavoritesButton.TooltipText = localization.format("Refresh_Favorites_Button_Tooltip");
        refreshFavoritesButton.OnClicked += onClickedRefreshFavoritesButton;
        refreshFavoritesButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(refreshFavoritesButton);
        refreshFriendsButton = Glazier.Get().CreateButton();
        refreshFriendsButton.PositionOffset_Y = 270f;
        refreshFriendsButton.SizeOffset_X = 200f;
        refreshFriendsButton.SizeOffset_Y = 50f;
        refreshFriendsButton.Text = localization.format("Refresh_Friends_Button");
        refreshFriendsButton.TooltipText = localization.format("Refresh_Friends_Button_Tooltip");
        refreshFriendsButton.OnClicked += onClickedRefreshFriendsButton;
        refreshFriendsButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(refreshFriendsButton);
        hostingButton = new SleekButtonIcon(icons.load<Texture2D>("Hosting"));
        hostingButton.PositionOffset_Y = -110f;
        hostingButton.PositionScale_Y = 1f;
        hostingButton.SizeOffset_X = 200f;
        hostingButton.SizeOffset_Y = 50f;
        hostingButton.text = localization.format("HostingButtonText");
        hostingButton.tooltip = localization.format("HostingButtonTooltip");
        hostingButton.onClickedButton += onClickedHostingButton;
        hostingButton.fontSize = ESleekFontSize.Medium;
        hostingButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(hostingButton);
        if (Provider.isPro)
        {
            refreshLANButton.PositionOffset_Y += 60f;
            refreshHistoryButton.PositionOffset_Y += 60f;
            refreshFavoritesButton.PositionOffset_Y += 60f;
            refreshFriendsButton.PositionOffset_Y += 60f;
            refreshGoldButton = Glazier.Get().CreateButton();
            refreshGoldButton.PositionOffset_Y = 90f;
            refreshGoldButton.SizeOffset_X = 200f;
            refreshGoldButton.SizeOffset_Y = 50f;
            refreshGoldButton.Text = localization.format("Refresh_Gold_Button");
            refreshGoldButton.TooltipText = localization.format("Refresh_Gold_Button_Tooltip");
            refreshGoldButton.OnClicked += onClickedRefreshGoldButton;
            refreshGoldButton.FontSize = ESleekFontSize.Medium;
            refreshGoldButton.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
            refreshGoldButton.TextColor = Palette.PRO;
            refreshGoldButton.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            container.AddChild(refreshGoldButton);
        }
        mapField = Glazier.Get().CreateStringField();
        mapField.PositionOffset_X = 5f;
        mapField.PositionOffset_Y = -70f;
        mapField.PositionScale_X = 0.2f;
        mapField.PositionScale_Y = 1f;
        mapField.SizeOffset_X = -10f;
        mapField.SizeOffset_Y = 30f;
        mapField.SizeScale_X = 0.2f;
        mapField.PlaceholderText = localization.format("Map_Field_Hint");
        mapField.OnTextChanged += onTypedMapField;
        mapField.MaxLength = 64;
        list.AddChild(mapField);
        monetizationButtonState = new SleekButtonState(new GUIContent(localization.format("Monetization_Any_Button")), new GUIContent(localization.format("Monetization_None_Button")), new GUIContent(localization.format("Monetization_NonGameplay_Button")));
        monetizationButtonState.PositionOffset_X = 5f;
        monetizationButtonState.PositionOffset_Y = -110f;
        monetizationButtonState.PositionScale_X = 0.6f;
        monetizationButtonState.PositionScale_Y = 1f;
        monetizationButtonState.SizeOffset_X = -10f;
        monetizationButtonState.SizeOffset_Y = 30f;
        monetizationButtonState.SizeScale_X = 0.2f;
        monetizationButtonState.onSwappedState = onSwappedMonetizationState;
        list.AddChild(monetizationButtonState);
        passwordButtonState = new SleekButtonState(new GUIContent(localization.format("No_Password_Button")), new GUIContent(localization.format("Yes_Password_Button")), new GUIContent(localization.format("Any_Password_Button")));
        passwordButtonState.PositionOffset_X = 5f;
        passwordButtonState.PositionOffset_Y = -110f;
        passwordButtonState.PositionScale_X = 0.8f;
        passwordButtonState.PositionScale_Y = 1f;
        passwordButtonState.SizeOffset_X = -5f;
        passwordButtonState.SizeOffset_Y = 30f;
        passwordButtonState.SizeScale_X = 0.2f;
        passwordButtonState.onSwappedState = onSwappedPasswordState;
        list.AddChild(passwordButtonState);
        workshopButtonState = new SleekButtonState(new GUIContent(localization.format("No_Workshop_Button")), new GUIContent(localization.format("Yes_Workshop_Button")), new GUIContent(localization.format("Any_Workshop_Button")));
        workshopButtonState.PositionOffset_Y = -30f;
        workshopButtonState.PositionScale_Y = 1f;
        workshopButtonState.SizeOffset_X = -5f;
        workshopButtonState.SizeOffset_Y = 30f;
        workshopButtonState.SizeScale_X = 0.2f;
        workshopButtonState.onSwappedState = onSwappedWorkshopState;
        list.AddChild(workshopButtonState);
        pluginsButtonState = new SleekButtonState(new GUIContent(localization.format("No_Plugins_Button")), new GUIContent(localization.format("Yes_Plugins_Button")), new GUIContent(localization.format("Any_Plugins_Button")));
        pluginsButtonState.PositionOffset_X = 5f;
        pluginsButtonState.PositionOffset_Y = -70f;
        pluginsButtonState.PositionScale_X = 0.8f;
        pluginsButtonState.PositionScale_Y = 1f;
        pluginsButtonState.SizeOffset_X = -5f;
        pluginsButtonState.SizeOffset_Y = 30f;
        pluginsButtonState.SizeScale_X = 0.2f;
        pluginsButtonState.onSwappedState = onSwappedPluginsState;
        list.AddChild(pluginsButtonState);
        cheatsButtonState = new SleekButtonState(new GUIContent(localization.format("No_Cheats_Button")), new GUIContent(localization.format("Yes_Cheats_Button")), new GUIContent(localization.format("Any_Cheats_Button")));
        cheatsButtonState.PositionOffset_X = 5f;
        cheatsButtonState.PositionOffset_Y = -30f;
        cheatsButtonState.PositionScale_X = 0.8f;
        cheatsButtonState.PositionScale_Y = 1f;
        cheatsButtonState.SizeOffset_X = -5f;
        cheatsButtonState.SizeOffset_Y = 30f;
        cheatsButtonState.SizeScale_X = 0.2f;
        cheatsButtonState.onSwappedState = onSwappedCheatsState;
        list.AddChild(cheatsButtonState);
        attendanceButtonState = new SleekButtonState(new GUIContent(localization.format("Empty_Button"), icons.load<Texture>("Empty")), new GUIContent(localization.format("HasPlayers_Button"), icons.load<Texture>("HasPlayers")), new GUIContent(localization.format("Any_Attendance_Button")));
        attendanceButtonState.PositionOffset_X = 5f;
        attendanceButtonState.PositionOffset_Y = -30f;
        attendanceButtonState.PositionScale_X = 0.4f;
        attendanceButtonState.PositionScale_Y = 1f;
        attendanceButtonState.SizeOffset_X = -10f;
        attendanceButtonState.SizeOffset_Y = 30f;
        attendanceButtonState.SizeScale_X = 0.2f;
        attendanceButtonState.onSwappedState = onSwappedAttendanceState;
        list.AddChild(attendanceButtonState);
        notFullButtonState = new SleekButtonState(new GUIContent(localization.format("Any_Space_Button")), new GUIContent(localization.format("Space_Button"), icons.load<Texture>("Space")));
        notFullButtonState.PositionOffset_X = 5f;
        notFullButtonState.PositionOffset_Y = -110f;
        notFullButtonState.PositionScale_X = 0.4f;
        notFullButtonState.PositionScale_Y = 1f;
        notFullButtonState.SizeOffset_X = -10f;
        notFullButtonState.SizeOffset_Y = 30f;
        notFullButtonState.SizeScale_X = 0.2f;
        notFullButtonState.onSwappedState = OnSwappedNotFullState;
        list.AddChild(notFullButtonState);
        VACProtectionButtonState = new SleekButtonState(new GUIContent(localization.format("VAC_Secure_Button"), icons.load<Texture>("VAC")), new GUIContent(localization.format("VAC_Insecure_Button")), new GUIContent(localization.format("VAC_Any_Button")));
        VACProtectionButtonState.PositionOffset_X = 5f;
        VACProtectionButtonState.PositionOffset_Y = -70f;
        VACProtectionButtonState.PositionScale_X = 0.4f;
        VACProtectionButtonState.PositionScale_Y = 1f;
        VACProtectionButtonState.SizeOffset_X = -10f;
        VACProtectionButtonState.SizeOffset_Y = 30f;
        VACProtectionButtonState.SizeScale_X = 0.2f;
        VACProtectionButtonState.onSwappedState = onSwappedVACProtectionState;
        list.AddChild(VACProtectionButtonState);
        battlEyeProtectionButtonState = new SleekButtonState(new GUIContent(localization.format("BattlEye_Secure_Button"), icons.load<Texture>("BattlEye")), new GUIContent(localization.format("BattlEye_Insecure_Button")), new GUIContent(localization.format("BattlEye_Any_Button")));
        battlEyeProtectionButtonState.PositionOffset_X = 5f;
        battlEyeProtectionButtonState.PositionOffset_Y = -70f;
        battlEyeProtectionButtonState.PositionScale_X = 0.6f;
        battlEyeProtectionButtonState.PositionScale_Y = 1f;
        battlEyeProtectionButtonState.SizeOffset_X = -10f;
        battlEyeProtectionButtonState.SizeOffset_Y = 30f;
        battlEyeProtectionButtonState.SizeScale_X = 0.2f;
        battlEyeProtectionButtonState.onSwappedState = onSwappedBattlEyeProtectionState;
        list.AddChild(battlEyeProtectionButtonState);
        combatButtonState = new SleekButtonState(new GUIContent(localization.format("PvP_Button"), icons.load<Texture>("PvP")), new GUIContent(localization.format("PvE_Button"), icons.load<Texture>("PvE")), new GUIContent(localization.format("Any_Combat_Button")));
        combatButtonState.PositionOffset_Y = -70f;
        combatButtonState.PositionScale_Y = 1f;
        combatButtonState.SizeOffset_X = -5f;
        combatButtonState.SizeOffset_Y = 30f;
        combatButtonState.SizeScale_X = 0.2f;
        combatButtonState.onSwappedState = onSwappedCombatState;
        list.AddChild(combatButtonState);
        modeButtonState = new SleekButtonState(new GUIContent(localization.format("Easy_Button"), icons.load<Texture>("Easy")), new GUIContent(localization.format("Normal_Button"), icons.load<Texture>("Normal")), new GUIContent(localization.format("Hard_Button"), icons.load<Texture>("Hard")), new GUIContent(localization.format("Any_Mode_Button")));
        modeButtonState.PositionOffset_X = 5f;
        modeButtonState.PositionOffset_Y = -30f;
        modeButtonState.PositionScale_X = 0.6f;
        modeButtonState.PositionScale_Y = 1f;
        modeButtonState.SizeOffset_X = -10f;
        modeButtonState.SizeOffset_Y = 30f;
        modeButtonState.SizeScale_X = 0.2f;
        modeButtonState.onSwappedState = onSwappedModeState;
        list.AddChild(modeButtonState);
        cameraButtonState = new SleekButtonState(new GUIContent(localization.format("First_Button"), icons.load<Texture>("First")), new GUIContent(localization.format("Third_Button"), icons.load<Texture>("Third")), new GUIContent(localization.format("Both_Button"), icons.load<Texture>("Both")), new GUIContent(localization.format("Vehicle_Button"), icons.load<Texture>("Vehicle")), new GUIContent(localization.format("Any_Camera_Button")));
        cameraButtonState.PositionOffset_X = 5f;
        cameraButtonState.PositionOffset_Y = -30f;
        cameraButtonState.PositionScale_X = 0.2f;
        cameraButtonState.PositionScale_Y = 1f;
        cameraButtonState.SizeOffset_X = -10f;
        cameraButtonState.SizeOffset_Y = 30f;
        cameraButtonState.SizeScale_X = 0.2f;
        cameraButtonState.onSwappedState = onSwappedCameraState;
        list.AddChild(cameraButtonState);
        serverBox = new SleekList<SteamServerInfo>();
        serverBox.PositionOffset_Y = 40f;
        serverBox.SizeOffset_Y = -160f;
        serverBox.SizeScale_X = 1f;
        serverBox.SizeScale_Y = 1f;
        serverBox.itemHeight = 40;
        serverBox.scrollView.ReduceWidthWhenScrollbarVisible = false;
        serverBox.onCreateElement = onCreateServerElement;
        serverBox.SetData(Provider.provider.matchmakingService.serverList);
        list.AddChild(serverBox);
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
        updateAll();
    }
}
