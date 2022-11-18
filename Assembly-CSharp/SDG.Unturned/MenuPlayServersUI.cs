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
        mapField.text = map;
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
        infoBox.isVisible = false;
        serverBox.NotifyDataChanged();
    }

    private static void onMasterServerResorted()
    {
        infoBox.isVisible = false;
        serverBox.NotifyDataChanged();
    }

    private static void onMasterServerRefreshed(EMatchMakingServerResponse response)
    {
        if (Provider.provider.matchmakingService.serverList.Count == 0)
        {
            infoBox.isVisible = true;
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
        Provider.provider.browserService.open("https://github.com/SmartlyDressedGames/U3-Docs/blob/master/ServerHosting.md#Server-Hosting");
    }

    private static ISleekElement onCreateServerElement(SteamServerInfo server)
    {
        return new SleekServer(Provider.provider.matchmakingService.currentList, server)
        {
            onClickedServer = onClickedServer,
            sizeOffset_X = -30
        };
    }

    private static void updateAll()
    {
        nameField.text = PlaySettings.serversName;
        mapField.text = FilterSettings.filterMap;
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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        list = Glazier.Get().CreateFrame();
        list.positionOffset_X = 210;
        list.sizeOffset_X = -210;
        list.sizeScale_X = 1f;
        list.sizeScale_Y = 1f;
        container.AddChild(list);
        sortName = Glazier.Get().CreateButton();
        sortName.sizeOffset_X = -270;
        sortName.sizeOffset_Y = 30;
        sortName.sizeScale_X = 1f;
        sortName.text = localization.format("Sort_Name");
        sortName.tooltipText = localization.format("Sort_Name_Tooltip");
        sortName.onClickedButton += onClickedSortNameButton;
        list.AddChild(sortName);
        sortMap = Glazier.Get().CreateButton();
        sortMap.positionOffset_X = -260;
        sortMap.positionScale_X = 1f;
        sortMap.sizeOffset_X = 100;
        sortMap.sizeOffset_Y = 30;
        sortMap.text = localization.format("Sort_Map");
        sortMap.tooltipText = localization.format("Sort_Map_Tooltip");
        sortMap.onClickedButton += onClickedSortMapButton;
        list.AddChild(sortMap);
        sortPlayers = Glazier.Get().CreateButton();
        sortPlayers.positionOffset_X = -150;
        sortPlayers.positionScale_X = 1f;
        sortPlayers.sizeOffset_X = 60;
        sortPlayers.sizeOffset_Y = 30;
        sortPlayers.text = localization.format("Sort_Players");
        sortPlayers.tooltipText = localization.format("Sort_Players_Tooltip");
        sortPlayers.onClickedButton += onClickedSortPlayersButton;
        list.AddChild(sortPlayers);
        sortPing = Glazier.Get().CreateButton();
        sortPing.positionOffset_X = -80;
        sortPing.positionScale_X = 1f;
        sortPing.sizeOffset_X = 50;
        sortPing.sizeOffset_Y = 30;
        sortPing.text = localization.format("Sort_Ping");
        sortPing.tooltipText = localization.format("Sort_Ping_Tooltip");
        sortPing.onClickedButton += onClickedSortPingButton;
        list.AddChild(sortPing);
        infoBox = Glazier.Get().CreateBox();
        infoBox.positionOffset_Y = 40;
        infoBox.sizeScale_X = 1f;
        infoBox.sizeOffset_X = -30;
        infoBox.sizeOffset_Y = 50;
        list.AddChild(infoBox);
        infoBox.isVisible = false;
        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
        sleekLabel.sizeScale_X = 1f;
        sleekLabel.sizeOffset_Y = 30;
        sleekLabel.text = localization.format("No_Servers", Provider.APP_VERSION);
        sleekLabel.fontSize = ESleekFontSize.Medium;
        infoBox.AddChild(sleekLabel);
        ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
        sleekLabel2.positionOffset_Y = 20;
        sleekLabel2.sizeScale_X = 1f;
        sleekLabel2.sizeOffset_Y = 30;
        sleekLabel2.text = localization.format("No_Servers_Hint");
        infoBox.AddChild(sleekLabel2);
        resetFiltersButton = Glazier.Get().CreateButton();
        resetFiltersButton.positionOffset_X = -150;
        resetFiltersButton.positionOffset_Y = 10;
        resetFiltersButton.positionScale_X = 0.5f;
        resetFiltersButton.positionScale_Y = 1f;
        resetFiltersButton.sizeOffset_X = 300;
        resetFiltersButton.sizeOffset_Y = 30;
        resetFiltersButton.text = localization.format("Reset_Filters");
        resetFiltersButton.tooltipText = localization.format("Reset_Filters_Tooltip");
        resetFiltersButton.onClickedButton += onClickedResetFilters;
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
        nameField.positionOffset_Y = -110;
        nameField.positionScale_Y = 1f;
        nameField.sizeOffset_X = -5;
        nameField.sizeOffset_Y = 30;
        nameField.sizeScale_X = 0.4f;
        nameField.hint = localization.format("Name_Field_Hint");
        nameField.onTyped += onTypedNameField;
        list.AddChild(nameField);
        refreshLabel = Glazier.Get().CreateLabel();
        refreshLabel.sizeOffset_X = 200;
        refreshLabel.sizeOffset_Y = 30;
        refreshLabel.text = localization.format("Refresh_Label");
        refreshLabel.fontAlignment = TextAnchor.MiddleCenter;
        container.AddChild(refreshLabel);
        refreshInternetButton = Glazier.Get().CreateButton();
        refreshInternetButton.positionOffset_Y = 30;
        refreshInternetButton.sizeOffset_X = 200;
        refreshInternetButton.sizeOffset_Y = 50;
        refreshInternetButton.text = localization.format("Refresh_Internet_Button");
        refreshInternetButton.tooltipText = localization.format("Refresh_Internet_Button_Tooltip");
        refreshInternetButton.onClickedButton += onClickedRefreshInternetButton;
        refreshInternetButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(refreshInternetButton);
        refreshLANButton = Glazier.Get().CreateButton();
        refreshLANButton.positionOffset_Y = 210;
        refreshLANButton.sizeOffset_X = 200;
        refreshLANButton.sizeOffset_Y = 50;
        refreshLANButton.text = localization.format("Refresh_LAN_Button");
        refreshLANButton.tooltipText = localization.format("Refresh_LAN_Button_Tooltip");
        refreshLANButton.onClickedButton += onClickedRefreshLANButton;
        refreshLANButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(refreshLANButton);
        refreshHistoryButton = Glazier.Get().CreateButton();
        refreshHistoryButton.positionOffset_Y = 150;
        refreshHistoryButton.sizeOffset_X = 200;
        refreshHistoryButton.sizeOffset_Y = 50;
        refreshHistoryButton.text = localization.format("Refresh_History_Button");
        refreshHistoryButton.tooltipText = localization.format("Refresh_History_Button_Tooltip");
        refreshHistoryButton.onClickedButton += onClickedRefreshHistoryButton;
        refreshHistoryButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(refreshHistoryButton);
        refreshFavoritesButton = Glazier.Get().CreateButton();
        refreshFavoritesButton.positionOffset_Y = 90;
        refreshFavoritesButton.sizeOffset_X = 200;
        refreshFavoritesButton.sizeOffset_Y = 50;
        refreshFavoritesButton.text = localization.format("Refresh_Favorites_Button");
        refreshFavoritesButton.tooltipText = localization.format("Refresh_Favorites_Button_Tooltip");
        refreshFavoritesButton.onClickedButton += onClickedRefreshFavoritesButton;
        refreshFavoritesButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(refreshFavoritesButton);
        refreshFriendsButton = Glazier.Get().CreateButton();
        refreshFriendsButton.positionOffset_Y = 270;
        refreshFriendsButton.sizeOffset_X = 200;
        refreshFriendsButton.sizeOffset_Y = 50;
        refreshFriendsButton.text = localization.format("Refresh_Friends_Button");
        refreshFriendsButton.tooltipText = localization.format("Refresh_Friends_Button_Tooltip");
        refreshFriendsButton.onClickedButton += onClickedRefreshFriendsButton;
        refreshFriendsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(refreshFriendsButton);
        hostingButton = new SleekButtonIcon(icons.load<Texture2D>("Hosting"));
        hostingButton.positionOffset_Y = -110;
        hostingButton.positionScale_Y = 1f;
        hostingButton.sizeOffset_X = 200;
        hostingButton.sizeOffset_Y = 50;
        hostingButton.text = localization.format("HostingButtonText");
        hostingButton.tooltip = localization.format("HostingButtonTooltip");
        hostingButton.onClickedButton += onClickedHostingButton;
        hostingButton.fontSize = ESleekFontSize.Medium;
        hostingButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(hostingButton);
        if (Provider.isPro)
        {
            refreshLANButton.positionOffset_Y += 60;
            refreshHistoryButton.positionOffset_Y += 60;
            refreshFavoritesButton.positionOffset_Y += 60;
            refreshFriendsButton.positionOffset_Y += 60;
            refreshGoldButton = Glazier.Get().CreateButton();
            refreshGoldButton.positionOffset_Y = 90;
            refreshGoldButton.sizeOffset_X = 200;
            refreshGoldButton.sizeOffset_Y = 50;
            refreshGoldButton.text = localization.format("Refresh_Gold_Button");
            refreshGoldButton.tooltipText = localization.format("Refresh_Gold_Button_Tooltip");
            refreshGoldButton.onClickedButton += onClickedRefreshGoldButton;
            refreshGoldButton.fontSize = ESleekFontSize.Medium;
            refreshGoldButton.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
            refreshGoldButton.textColor = Palette.PRO;
            refreshGoldButton.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            container.AddChild(refreshGoldButton);
        }
        mapField = Glazier.Get().CreateStringField();
        mapField.positionOffset_X = 5;
        mapField.positionOffset_Y = -70;
        mapField.positionScale_X = 0.2f;
        mapField.positionScale_Y = 1f;
        mapField.sizeOffset_X = -10;
        mapField.sizeOffset_Y = 30;
        mapField.sizeScale_X = 0.2f;
        mapField.hint = localization.format("Map_Field_Hint");
        mapField.onTyped += onTypedMapField;
        mapField.maxLength = 64;
        list.AddChild(mapField);
        monetizationButtonState = new SleekButtonState(new GUIContent(localization.format("Monetization_Any_Button")), new GUIContent(localization.format("Monetization_None_Button")), new GUIContent(localization.format("Monetization_NonGameplay_Button")));
        monetizationButtonState.positionOffset_X = 5;
        monetizationButtonState.positionOffset_Y = -110;
        monetizationButtonState.positionScale_X = 0.6f;
        monetizationButtonState.positionScale_Y = 1f;
        monetizationButtonState.sizeOffset_X = -10;
        monetizationButtonState.sizeOffset_Y = 30;
        monetizationButtonState.sizeScale_X = 0.2f;
        monetizationButtonState.onSwappedState = onSwappedMonetizationState;
        list.AddChild(monetizationButtonState);
        passwordButtonState = new SleekButtonState(new GUIContent(localization.format("No_Password_Button")), new GUIContent(localization.format("Yes_Password_Button")), new GUIContent(localization.format("Any_Password_Button")));
        passwordButtonState.positionOffset_X = 5;
        passwordButtonState.positionOffset_Y = -110;
        passwordButtonState.positionScale_X = 0.8f;
        passwordButtonState.positionScale_Y = 1f;
        passwordButtonState.sizeOffset_X = -5;
        passwordButtonState.sizeOffset_Y = 30;
        passwordButtonState.sizeScale_X = 0.2f;
        passwordButtonState.onSwappedState = onSwappedPasswordState;
        list.AddChild(passwordButtonState);
        workshopButtonState = new SleekButtonState(new GUIContent(localization.format("No_Workshop_Button")), new GUIContent(localization.format("Yes_Workshop_Button")), new GUIContent(localization.format("Any_Workshop_Button")));
        workshopButtonState.positionOffset_Y = -30;
        workshopButtonState.positionScale_Y = 1f;
        workshopButtonState.sizeOffset_X = -5;
        workshopButtonState.sizeOffset_Y = 30;
        workshopButtonState.sizeScale_X = 0.2f;
        workshopButtonState.onSwappedState = onSwappedWorkshopState;
        list.AddChild(workshopButtonState);
        pluginsButtonState = new SleekButtonState(new GUIContent(localization.format("No_Plugins_Button")), new GUIContent(localization.format("Yes_Plugins_Button")), new GUIContent(localization.format("Any_Plugins_Button")));
        pluginsButtonState.positionOffset_X = 5;
        pluginsButtonState.positionOffset_Y = -70;
        pluginsButtonState.positionScale_X = 0.8f;
        pluginsButtonState.positionScale_Y = 1f;
        pluginsButtonState.sizeOffset_X = -5;
        pluginsButtonState.sizeOffset_Y = 30;
        pluginsButtonState.sizeScale_X = 0.2f;
        pluginsButtonState.onSwappedState = onSwappedPluginsState;
        list.AddChild(pluginsButtonState);
        cheatsButtonState = new SleekButtonState(new GUIContent(localization.format("No_Cheats_Button")), new GUIContent(localization.format("Yes_Cheats_Button")), new GUIContent(localization.format("Any_Cheats_Button")));
        cheatsButtonState.positionOffset_X = 5;
        cheatsButtonState.positionOffset_Y = -30;
        cheatsButtonState.positionScale_X = 0.8f;
        cheatsButtonState.positionScale_Y = 1f;
        cheatsButtonState.sizeOffset_X = -5;
        cheatsButtonState.sizeOffset_Y = 30;
        cheatsButtonState.sizeScale_X = 0.2f;
        cheatsButtonState.onSwappedState = onSwappedCheatsState;
        list.AddChild(cheatsButtonState);
        attendanceButtonState = new SleekButtonState(new GUIContent(localization.format("Empty_Button"), icons.load<Texture>("Empty")), new GUIContent(localization.format("HasPlayers_Button"), icons.load<Texture>("HasPlayers")), new GUIContent(localization.format("Any_Attendance_Button")));
        attendanceButtonState.positionOffset_X = 5;
        attendanceButtonState.positionOffset_Y = -30;
        attendanceButtonState.positionScale_X = 0.4f;
        attendanceButtonState.positionScale_Y = 1f;
        attendanceButtonState.sizeOffset_X = -10;
        attendanceButtonState.sizeOffset_Y = 30;
        attendanceButtonState.sizeScale_X = 0.2f;
        attendanceButtonState.onSwappedState = onSwappedAttendanceState;
        list.AddChild(attendanceButtonState);
        notFullButtonState = new SleekButtonState(new GUIContent(localization.format("Any_Space_Button")), new GUIContent(localization.format("Space_Button"), icons.load<Texture>("Space")));
        notFullButtonState.positionOffset_X = 5;
        notFullButtonState.positionOffset_Y = -110;
        notFullButtonState.positionScale_X = 0.4f;
        notFullButtonState.positionScale_Y = 1f;
        notFullButtonState.sizeOffset_X = -10;
        notFullButtonState.sizeOffset_Y = 30;
        notFullButtonState.sizeScale_X = 0.2f;
        notFullButtonState.onSwappedState = OnSwappedNotFullState;
        list.AddChild(notFullButtonState);
        VACProtectionButtonState = new SleekButtonState(new GUIContent(localization.format("VAC_Secure_Button"), icons.load<Texture>("VAC")), new GUIContent(localization.format("VAC_Insecure_Button")), new GUIContent(localization.format("VAC_Any_Button")));
        VACProtectionButtonState.positionOffset_X = 5;
        VACProtectionButtonState.positionOffset_Y = -70;
        VACProtectionButtonState.positionScale_X = 0.4f;
        VACProtectionButtonState.positionScale_Y = 1f;
        VACProtectionButtonState.sizeOffset_X = -10;
        VACProtectionButtonState.sizeOffset_Y = 30;
        VACProtectionButtonState.sizeScale_X = 0.2f;
        VACProtectionButtonState.onSwappedState = onSwappedVACProtectionState;
        list.AddChild(VACProtectionButtonState);
        battlEyeProtectionButtonState = new SleekButtonState(new GUIContent(localization.format("BattlEye_Secure_Button"), icons.load<Texture>("BattlEye")), new GUIContent(localization.format("BattlEye_Insecure_Button")), new GUIContent(localization.format("BattlEye_Any_Button")));
        battlEyeProtectionButtonState.positionOffset_X = 5;
        battlEyeProtectionButtonState.positionOffset_Y = -70;
        battlEyeProtectionButtonState.positionScale_X = 0.6f;
        battlEyeProtectionButtonState.positionScale_Y = 1f;
        battlEyeProtectionButtonState.sizeOffset_X = -10;
        battlEyeProtectionButtonState.sizeOffset_Y = 30;
        battlEyeProtectionButtonState.sizeScale_X = 0.2f;
        battlEyeProtectionButtonState.onSwappedState = onSwappedBattlEyeProtectionState;
        list.AddChild(battlEyeProtectionButtonState);
        combatButtonState = new SleekButtonState(new GUIContent(localization.format("PvP_Button"), icons.load<Texture>("PvP")), new GUIContent(localization.format("PvE_Button"), icons.load<Texture>("PvE")), new GUIContent(localization.format("Any_Combat_Button")));
        combatButtonState.positionOffset_Y = -70;
        combatButtonState.positionScale_Y = 1f;
        combatButtonState.sizeOffset_X = -5;
        combatButtonState.sizeOffset_Y = 30;
        combatButtonState.sizeScale_X = 0.2f;
        combatButtonState.onSwappedState = onSwappedCombatState;
        list.AddChild(combatButtonState);
        modeButtonState = new SleekButtonState(new GUIContent(localization.format("Easy_Button"), icons.load<Texture>("Easy")), new GUIContent(localization.format("Normal_Button"), icons.load<Texture>("Normal")), new GUIContent(localization.format("Hard_Button"), icons.load<Texture>("Hard")), new GUIContent(localization.format("Any_Mode_Button")));
        modeButtonState.positionOffset_X = 5;
        modeButtonState.positionOffset_Y = -30;
        modeButtonState.positionScale_X = 0.6f;
        modeButtonState.positionScale_Y = 1f;
        modeButtonState.sizeOffset_X = -10;
        modeButtonState.sizeOffset_Y = 30;
        modeButtonState.sizeScale_X = 0.2f;
        modeButtonState.onSwappedState = onSwappedModeState;
        list.AddChild(modeButtonState);
        cameraButtonState = new SleekButtonState(new GUIContent(localization.format("First_Button"), icons.load<Texture>("First")), new GUIContent(localization.format("Third_Button"), icons.load<Texture>("Third")), new GUIContent(localization.format("Both_Button"), icons.load<Texture>("Both")), new GUIContent(localization.format("Vehicle_Button"), icons.load<Texture>("Vehicle")), new GUIContent(localization.format("Any_Camera_Button")));
        cameraButtonState.positionOffset_X = 5;
        cameraButtonState.positionOffset_Y = -30;
        cameraButtonState.positionScale_X = 0.2f;
        cameraButtonState.positionScale_Y = 1f;
        cameraButtonState.sizeOffset_X = -10;
        cameraButtonState.sizeOffset_Y = 30;
        cameraButtonState.sizeScale_X = 0.2f;
        cameraButtonState.onSwappedState = onSwappedCameraState;
        list.AddChild(cameraButtonState);
        serverBox = new SleekList<SteamServerInfo>();
        serverBox.positionOffset_Y = 40;
        serverBox.sizeOffset_Y = -160;
        serverBox.sizeScale_X = 1f;
        serverBox.sizeScale_Y = 1f;
        serverBox.itemHeight = 40;
        serverBox.scrollView.reduceWidthWhenScrollbarVisible = false;
        serverBox.onCreateElement = onCreateServerElement;
        serverBox.SetData(Provider.provider.matchmakingService.serverList);
        list.AddChild(serverBox);
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
        updateAll();
    }
}
