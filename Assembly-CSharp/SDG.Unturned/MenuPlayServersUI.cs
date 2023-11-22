using System;
using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayServersUI : SleekFullscreenBox
{
    public Local localization;

    public Bundle icons;

    private ISleekScrollView presetsScrollView;

    private ISleekElement customPresetsContainer;

    private ISleekElement defaultPresetsContainer;

    private ISleekElement columnTogglesContainer;

    private ISleekElement filtersEditorContainer;

    private ISleekElement list;

    public bool active;

    private SleekButtonIcon backButton;

    private SleekList<SteamServerInfo> serverBox;

    private ISleekBox infoBox;

    private ISleekButton resetFiltersButton;

    private ISleekButton nameColumnButton;

    private ISleekButton mapColumnButton;

    private ISleekButton playersColumnButton;

    private ISleekButton maxPlayersColumnButton;

    private ISleekButton pingColumnButton;

    private ISleekButton anticheatColumnButton;

    private SleekButtonIcon perspectiveColumnButton;

    private SleekButtonIcon combatColumnButton;

    private SleekButtonIcon passwordColumnButton;

    private SleekButtonIcon workshopColumnButton;

    private SleekButtonIcon goldColumnButton;

    private SleekButtonIcon cheatsColumnButton;

    private SleekButtonIcon monetizationColumnButton;

    private SleekButtonIcon pluginsColumnButton;

    private ISleekField nameField;

    private ISleekField mapField;

    private SleekButtonState monetizationButtonState;

    private SleekButtonState passwordButtonState;

    private SleekButtonState workshopButtonState;

    private SleekButtonState pluginsButtonState;

    private SleekButtonState cheatsButtonState;

    private SleekButtonState attendanceButtonState;

    private SleekButtonState notFullButtonState;

    private SleekButtonState VACProtectionButtonState;

    private SleekButtonState battlEyeProtectionButtonState;

    private SleekButtonState combatButtonState;

    private SleekButtonState goldFilterButtonState;

    private SleekButtonState cameraButtonState;

    private SleekButtonState listSourceButtonState;

    private ISleekButton refreshButton;

    private ISleekImage refreshIcon;

    private SleekButtonIcon presetsEditorButton;

    private bool isRefreshing;

    public static MenuPlayServerListFiltersUI serverListFiltersUI;

    public void open()
    {
        if (!active)
        {
            active = true;
            SynchronizeFilterButtons();
            if (FilterSettings.activeFilters.presetId == 0)
            {
                FilterSettings.activeFilters.CopyFrom(FilterSettings.defaultPresetInternet);
                FilterSettings.activeFilters.presetName = localization.format("DefaultPreset_Internet_Label");
                FilterSettings.InvokeActiveFiltersReplaced();
            }
            else
            {
                CancelAndRefresh();
            }
            AnimateIntoView();
        }
    }

    public void close()
    {
        if (active)
        {
            active = false;
            MenuSettings.save();
            AnimateOutOfView(0f, 1f);
        }
    }

    private void onClickedServer(SleekServer server, SteamServerInfo info)
    {
        if (!info.isPro || Provider.isPro)
        {
            MenuSettings.save();
            MenuPlayServerInfoUI.open(info, string.Empty, MenuPlayServerInfoUI.EServerInfoOpenContext.SERVERS);
            close();
        }
    }

    private void onMasterServerAdded(int insert, SteamServerInfo info)
    {
        serverBox.NotifyDataChanged();
    }

    private void onMasterServerRemoved()
    {
        infoBox.IsVisible = false;
        serverBox.NotifyDataChanged();
    }

    private void onMasterServerResorted()
    {
        infoBox.IsVisible = false;
        serverBox.NotifyDataChanged();
    }

    private void onMasterServerRefreshed(EMatchMakingServerResponse response)
    {
        SetIsRefreshing(value: false);
        if (Provider.provider.matchmakingService.serverList.Count == 0)
        {
            infoBox.IsVisible = true;
        }
    }

    private void CancelAndRefresh()
    {
        if (isRefreshing)
        {
            Provider.provider.matchmakingService.cancelRequest();
        }
        SetIsRefreshing(value: true);
        Provider.provider.matchmakingService.refreshMasterServer(FilterSettings.activeFilters);
    }

    private void OnActiveFiltersModified()
    {
        SynchronizePresetsEditorButtonLabel();
    }

    private void OnActiveFiltersReplaced()
    {
        SynchronizeFilterButtons();
        SynchronizePresetsEditorButtonLabel();
        if (active)
        {
            CancelAndRefresh();
        }
    }

    private void OnCustomPresetsListChanged()
    {
        SynchronizePresetsList();
    }

    private void OnNameColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_NameAscending))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_NameDescending());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_NameAscending());
        }
    }

    private void OnMapColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_MapAscending))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MapDescending());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MapAscending());
        }
    }

    private void OnPlayersColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PlayersDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PlayersInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PlayersDefault());
        }
    }

    private void OnMaxPlayersColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_MaxPlayersDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MaxPlayersInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MaxPlayersDefault());
        }
    }

    private void OnPingColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PingAscending))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PingDescending());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PingAscending());
        }
    }

    private void OnAnticheatColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_AnticheatDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_AnticheatInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_AnticheatDefault());
        }
    }

    private void OnPerspectiveColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PerspectiveDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PerspectiveInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PerspectiveDefault());
        }
    }

    private void OnCombatColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_CombatDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_CombatInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_CombatDefault());
        }
    }

    private void OnPasswordColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PasswordDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PasswordInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PasswordDefault());
        }
    }

    private void OnWorkshopColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_WorkshopDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_WorkshopInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_WorkshopDefault());
        }
    }

    private void OnGoldColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_GoldDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_GoldInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_GoldDefault());
        }
    }

    private void OnCheatsColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_CheatsDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_CheatsInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_CheatsDefault());
        }
    }

    private void OnMonetizationColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_MonetizationDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MonetizationInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_MonetizationDefault());
        }
    }

    private void OnPluginsColumnClicked(ISleekElement button)
    {
        if (Provider.provider.matchmakingService.serverInfoComparer.GetType() == typeof(ServerListComparer_PluginsDefault))
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PluginsInverted());
        }
        else
        {
            Provider.provider.matchmakingService.sortMasterServer(new ServerListComparer_PluginsDefault());
        }
    }

    private void OnClickedColumnsButton(ISleekElement button)
    {
        FilterSettings.isColumnsEditorOpen = !FilterSettings.isColumnsEditorOpen;
        AnimateOpenSubcontainers();
    }

    private void OnMapColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.map = value;
        SynchronizeVisibleColumns();
    }

    private void OnPlayersColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.players = value;
        SynchronizeVisibleColumns();
    }

    private void OnMaxPlayersColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.maxPlayers = value;
        SynchronizeVisibleColumns();
    }

    private void OnPingColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.ping = value;
        SynchronizeVisibleColumns();
    }

    private void OnAnticheatColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.anticheat = value;
        SynchronizeVisibleColumns();
    }

    private void OnPerspectiveColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.perspective = value;
        SynchronizeVisibleColumns();
    }

    private void OnCombatColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.combat = value;
        SynchronizeVisibleColumns();
    }

    private void OnPasswordColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.password = value;
        SynchronizeVisibleColumns();
    }

    private void OnWorkshopColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.workshop = value;
        SynchronizeVisibleColumns();
    }

    private void OnGoldColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.gold = value;
        SynchronizeVisibleColumns();
    }

    private void OnCheatsColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.cheats = value;
        SynchronizeVisibleColumns();
    }

    private void OnMonetizationColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.monetization = value;
        SynchronizeVisibleColumns();
    }

    private void OnPluginsColumnToggled(ISleekToggle toggle, bool value)
    {
        FilterSettings.columns.plugins = value;
        SynchronizeVisibleColumns();
    }

    private void onTypedNameField(ISleekField field, string text)
    {
        FilterSettings.activeFilters.serverName = text;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onTypedMapField(ISleekField field, string text)
    {
        FilterSettings.activeFilters.mapName = text;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedMonetizationState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.monetization = (EServerMonetizationTag)(index + 1);
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedPasswordState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.password = (EPassword)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedWorkshopState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.workshop = (EWorkshop)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedPluginsState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.plugins = (EPlugins)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedCheatsState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.cheats = (ECheats)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedAttendanceState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.attendance = (EAttendance)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void OnSwappedNotFullState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.notFull = index > 0;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedVACProtectionState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.vacProtection = (EVACProtectionFilter)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedBattlEyeProtectionState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.battlEyeProtection = (EBattlEyeProtectionFilter)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedCombatState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.combat = (ECombat)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void OnSwappedGoldFilterState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.gold = (EServerListGoldFilter)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onSwappedCameraState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.camera = (ECameraMode)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void OnSwappedListSourceState(SleekButtonState button, int index)
    {
        FilterSettings.activeFilters.listSource = (ESteamServerList)index;
        FilterSettings.MarkActiveFilterModified();
    }

    private void onClickedRefreshButton(ISleekElement button)
    {
        if (isRefreshing)
        {
            SetIsRefreshing(value: false);
            Provider.provider.matchmakingService.cancelRequest();
        }
        else
        {
            SetIsRefreshing(value: true);
            Provider.provider.matchmakingService.refreshMasterServer(FilterSettings.activeFilters);
        }
    }

    private void onClickedHostingButton(ISleekElement button)
    {
        Provider.provider.browserService.open("https://docs.smartlydressedgames.com/en/stable/servers/server-hosting.html");
    }

    private void OnPresetsEditorButtonClicked(ISleekElement button)
    {
        serverListFiltersUI.open();
        close();
    }

    private void onClickedPresetsButton(ISleekElement button)
    {
        FilterSettings.isPresetsListOpen = !FilterSettings.isPresetsListOpen;
        AnimateOpenSubcontainers();
    }

    private void OnQuickFiltersButtonClicked(ISleekElement button)
    {
        FilterSettings.isQuickFiltersEditorOpen = !FilterSettings.isQuickFiltersEditorOpen;
        AnimateOpenSubcontainers();
    }

    private ISleekElement onCreateServerElement(SteamServerInfo server)
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
    private void SynchronizeFilterButtons()
    {
        nameField.Text = FilterSettings.activeFilters.serverName;
        mapField.Text = FilterSettings.activeFilters.mapName;
        passwordButtonState.state = (int)FilterSettings.activeFilters.password;
        workshopButtonState.state = (int)FilterSettings.activeFilters.workshop;
        pluginsButtonState.state = (int)FilterSettings.activeFilters.plugins;
        cheatsButtonState.state = (int)FilterSettings.activeFilters.cheats;
        attendanceButtonState.state = (int)FilterSettings.activeFilters.attendance;
        notFullButtonState.state = (FilterSettings.activeFilters.notFull ? 1 : 0);
        VACProtectionButtonState.state = (int)FilterSettings.activeFilters.vacProtection;
        battlEyeProtectionButtonState.state = (int)FilterSettings.activeFilters.battlEyeProtection;
        combatButtonState.state = (int)FilterSettings.activeFilters.combat;
        goldFilterButtonState.state = (int)FilterSettings.activeFilters.gold;
        cameraButtonState.state = (int)FilterSettings.activeFilters.camera;
        monetizationButtonState.state = (int)(FilterSettings.activeFilters.monetization - 1);
        listSourceButtonState.state = (int)FilterSettings.activeFilters.listSource;
    }

    private void SynchronizePresetsEditorButtonLabel()
    {
        string text = FilterSettings.activeFilters.presetName;
        if (string.IsNullOrEmpty(text))
        {
            text = localization.format("PresetName_Empty");
        }
        presetsEditorButton.text = localization.format("PresetsEditorButton_Label", text);
    }

    private void SynchronizePresetsList()
    {
        customPresetsContainer.RemoveAllChildren();
        int num = 0;
        foreach (ServerListFilters customPreset in FilterSettings.customPresets)
        {
            SleekCustomServerListPresetButton sleekCustomServerListPresetButton = new SleekCustomServerListPresetButton(customPreset);
            sleekCustomServerListPresetButton.PositionScale_X = (float)(num % 5) * 0.2f;
            sleekCustomServerListPresetButton.PositionOffset_Y = (float)(num / 5) * 30f;
            sleekCustomServerListPresetButton.SizeScale_X = 0.2f;
            sleekCustomServerListPresetButton.SizeOffset_Y = 30f;
            customPresetsContainer.AddChild(sleekCustomServerListPresetButton);
            num++;
        }
        customPresetsContainer.SizeOffset_Y = (float)((num - 1) / 5 + 1) * 30f;
        if (num > 0)
        {
            defaultPresetsContainer.PositionOffset_Y = customPresetsContainer.SizeOffset_Y + 10f;
        }
        else
        {
            defaultPresetsContainer.PositionOffset_Y = 0f;
        }
        float num2 = defaultPresetsContainer.PositionOffset_Y + defaultPresetsContainer.SizeOffset_Y;
        presetsScrollView.ContentSizeOffset = new Vector2(0f, num2);
        presetsScrollView.SizeOffset_Y = Mathf.Min(num2, 100f);
        presetsScrollView.PositionOffset_Y = 0f - presetsScrollView.SizeOffset_Y - 60f;
        AnimateOpenSubcontainers();
    }

    private void CreateQuickFilterButtons()
    {
        int num = 0;
        listSourceButtonState = new SleekButtonState(20, new GUIContent(localization.format("List_Internet_Label"), icons.load<Texture>("List_Internet"), localization.format("List_Internet_Tooltip")), new GUIContent(localization.format("List_LAN_Label"), icons.load<Texture>("List_LAN"), localization.format("List_LAN_Tooltip")), new GUIContent(localization.format("List_History_Label"), icons.load<Texture>("List_History"), localization.format("List_History_Tooltip")), new GUIContent(localization.format("List_Favorites_Label"), icons.load<Texture>("List_Favorites"), localization.format("List_Favorites_Tooltip")), new GUIContent(localization.format("List_Friends_Label"), icons.load<Texture2D>("List_Friends"), localization.format("List_Friends_Tooltip")));
        listSourceButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        listSourceButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        listSourceButtonState.SizeScale_X = 0.2f;
        listSourceButtonState.SizeOffset_Y = 30f;
        listSourceButtonState.onSwappedState = OnSwappedListSourceState;
        listSourceButtonState.button.iconColor = ESleekTint.FOREGROUND;
        listSourceButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(listSourceButtonState);
        num++;
        nameField = Glazier.Get().CreateStringField();
        nameField.PositionScale_X = (float)(num % 5) * 0.2f;
        nameField.PositionOffset_Y = (float)(num / 5) * 30f;
        nameField.SizeScale_X = 0.2f;
        nameField.SizeOffset_Y = 30f;
        nameField.PlaceholderText = localization.format("Name_Filter_Hint");
        nameField.TooltipText = localization.format("Name_Filter_Label");
        nameField.OnTextChanged += onTypedNameField;
        filtersEditorContainer.AddChild(nameField);
        num++;
        mapField = Glazier.Get().CreateStringField();
        mapField.PositionScale_X = (float)(num % 5) * 0.2f;
        mapField.PositionOffset_Y = (float)(num / 5) * 30f;
        mapField.SizeScale_X = 0.2f;
        mapField.SizeOffset_Y = 30f;
        mapField.PlaceholderText = localization.format("Map_Filter_Hint");
        mapField.TooltipText = localization.format("Map_Filter_Label");
        mapField.OnTextChanged += onTypedMapField;
        mapField.MaxLength = 64;
        filtersEditorContainer.AddChild(mapField);
        num++;
        passwordButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Password_Button"), icons.load<Texture2D>("NotPasswordProtected"), localization.format("Password_Filter_No_Tooltip")), new GUIContent(localization.format("Yes_Password_Button"), icons.load<Texture2D>("PasswordProtected"), localization.format("Password_Filter_Yes_Tooltip")), new GUIContent(localization.format("Any_Password_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Password_Filter_Any_Tooltip")));
        passwordButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        passwordButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        passwordButtonState.SizeScale_X = 0.2f;
        passwordButtonState.SizeOffset_Y = 30f;
        passwordButtonState.onSwappedState = onSwappedPasswordState;
        passwordButtonState.button.iconColor = ESleekTint.FOREGROUND;
        passwordButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(passwordButtonState);
        num++;
        attendanceButtonState = new SleekButtonState(20, new GUIContent(localization.format("Empty_Button"), icons.load<Texture>("Empty"), localization.format("Attendance_Filter_Empty_Tooltip")), new GUIContent(localization.format("HasPlayers_Button"), icons.load<Texture>("HasPlayers"), localization.format("Attendance_Filter_HasPlayers_Tooltip")), new GUIContent(localization.format("Any_Attendance_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Attendance_Filter_Any_Tooltip")));
        attendanceButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        attendanceButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        attendanceButtonState.SizeScale_X = 0.2f;
        attendanceButtonState.SizeOffset_Y = 30f;
        attendanceButtonState.onSwappedState = onSwappedAttendanceState;
        attendanceButtonState.button.iconColor = ESleekTint.FOREGROUND;
        attendanceButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(attendanceButtonState);
        num++;
        notFullButtonState = new SleekButtonState(20, new GUIContent(localization.format("Any_Space_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Space_Filter_Any_Tooltip")), new GUIContent(localization.format("Space_Button"), icons.load<Texture>("Space"), localization.format("Space_Filter_HasSpace_Tooltip")));
        notFullButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        notFullButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        notFullButtonState.SizeScale_X = 0.2f;
        notFullButtonState.SizeOffset_Y = 30f;
        notFullButtonState.onSwappedState = OnSwappedNotFullState;
        notFullButtonState.button.iconColor = ESleekTint.FOREGROUND;
        notFullButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(notFullButtonState);
        num++;
        combatButtonState = new SleekButtonState(20, new GUIContent(localization.format("PvP_Button"), icons.load<Texture>("PvP"), localization.format("Combat_Filter_PvP_Tooltip")), new GUIContent(localization.format("PvE_Button"), icons.load<Texture>("PvE"), localization.format("Combat_Filter_PvE_Tooltip")), new GUIContent(localization.format("Any_Combat_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Combat_Filter_Any_Tooltip")));
        combatButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        combatButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        combatButtonState.SizeScale_X = 0.2f;
        combatButtonState.SizeOffset_Y = 30f;
        combatButtonState.onSwappedState = onSwappedCombatState;
        combatButtonState.button.iconColor = ESleekTint.FOREGROUND;
        combatButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(combatButtonState);
        num++;
        cameraButtonState = new SleekButtonState(20, new GUIContent(localization.format("First_Button"), icons.load<Texture>("Perspective_FirstPerson"), localization.format("Perspective_Filter_FirstPerson_Tooltip")), new GUIContent(localization.format("Third_Button"), icons.load<Texture>("Perspective_ThirdPerson"), localization.format("Perspective_Filter_ThirdPerson_Tooltip")), new GUIContent(localization.format("Both_Button"), icons.load<Texture>("Perspective_Both"), localization.format("Perspective_Filter_Both_Tooltip")), new GUIContent(localization.format("Vehicle_Button"), icons.load<Texture>("Perspective_Vehicle"), localization.format("Perspective_Filter_Vehicle_Tooltip")), new GUIContent(localization.format("Any_Camera_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Perspective_Filter_Any_Tooltip")));
        cameraButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        cameraButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        cameraButtonState.SizeScale_X = 0.2f;
        cameraButtonState.SizeOffset_Y = 30f;
        cameraButtonState.onSwappedState = onSwappedCameraState;
        cameraButtonState.button.iconColor = ESleekTint.FOREGROUND;
        cameraButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(cameraButtonState);
        num++;
        goldFilterButtonState = new SleekButtonState(20, new GUIContent(localization.format("Gold_Filter_Any_Label"), icons.load<Texture>("AnyFilter"), localization.format("Gold_Filter_Any_Tooltip")), new GUIContent(localization.format("Gold_Filter_DoesNotRequireGold_Label"), icons.load<Texture>("GoldNotRequired"), localization.format("Gold_Filter_DoesNotRequireGold_Tooltip")), new GUIContent(localization.format("Gold_Filter_RequiresGold_Label"), icons.load<Texture>("GoldRequired"), localization.format("Gold_Filter_RequiresGold_Tooltip")));
        goldFilterButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        goldFilterButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        goldFilterButtonState.SizeScale_X = 0.2f;
        goldFilterButtonState.SizeOffset_Y = 30f;
        goldFilterButtonState.UseContentTooltip = true;
        goldFilterButtonState.onSwappedState = OnSwappedGoldFilterState;
        goldFilterButtonState.button.textColor = Palette.PRO;
        goldFilterButtonState.button.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
        goldFilterButtonState.button.iconColor = Palette.PRO;
        filtersEditorContainer.AddChild(goldFilterButtonState);
        num++;
        monetizationButtonState = new SleekButtonState(20, new GUIContent(localization.format("Monetization_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Monetization_Filter_Any_Tooltip")), new GUIContent(localization.format("Monetization_None_Button"), icons.load<Texture2D>("Monetization_None"), localization.format("Monetization_Filter_None_Tooltip")), new GUIContent(localization.format("Monetization_NonGameplay_Button"), icons.load<Texture2D>("NonGameplayMonetization"), localization.format("Monetization_Filter_NonGameplay_Tooltip")));
        monetizationButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        monetizationButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        monetizationButtonState.SizeScale_X = 0.2f;
        monetizationButtonState.SizeOffset_Y = 30f;
        monetizationButtonState.onSwappedState = onSwappedMonetizationState;
        monetizationButtonState.button.iconColor = ESleekTint.FOREGROUND;
        monetizationButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(monetizationButtonState);
        num++;
        workshopButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Workshop_Button"), icons.load<Texture2D>("NoMods"), localization.format("Workshop_Filter_No_Tooltip")), new GUIContent(localization.format("Yes_Workshop_Button"), icons.load<Texture2D>("HasMods"), localization.format("Workshop_Filter_Yes_Tooltip")), new GUIContent(localization.format("Any_Workshop_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Workshop_Filter_Any_Tooltip")));
        workshopButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        workshopButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        workshopButtonState.SizeScale_X = 0.2f;
        workshopButtonState.SizeOffset_Y = 30f;
        workshopButtonState.onSwappedState = onSwappedWorkshopState;
        workshopButtonState.button.iconColor = ESleekTint.FOREGROUND;
        workshopButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(workshopButtonState);
        num++;
        pluginsButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Plugins_Button"), icons.load<Texture2D>("Plugins_None"), localization.format("Plugins_Filter_No_Tooltip")), new GUIContent(localization.format("Yes_Plugins_Button"), icons.load<Texture2D>("Plugins"), localization.format("Plugins_Filter_Yes_Tooltip")), new GUIContent(localization.format("Any_Plugins_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Plugins_Filter_Any_Tooltip")));
        pluginsButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        pluginsButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        pluginsButtonState.SizeScale_X = 0.2f;
        pluginsButtonState.SizeOffset_Y = 30f;
        pluginsButtonState.onSwappedState = onSwappedPluginsState;
        pluginsButtonState.button.iconColor = ESleekTint.FOREGROUND;
        pluginsButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(pluginsButtonState);
        num++;
        cheatsButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Cheats_Button"), icons.load<Texture2D>("CheatCodes_None"), localization.format("Cheats_Filter_No_Tooltip")), new GUIContent(localization.format("Yes_Cheats_Button"), icons.load<Texture2D>("CheatCodes"), localization.format("Cheats_Filter_Yes_Tooltip")), new GUIContent(localization.format("Any_Cheats_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Cheats_Filter_Any_Tooltip")));
        cheatsButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        cheatsButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        cheatsButtonState.SizeScale_X = 0.2f;
        cheatsButtonState.SizeOffset_Y = 30f;
        cheatsButtonState.onSwappedState = onSwappedCheatsState;
        cheatsButtonState.button.iconColor = ESleekTint.FOREGROUND;
        cheatsButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(cheatsButtonState);
        num++;
        VACProtectionButtonState = new SleekButtonState(20, new GUIContent(localization.format("VAC_Secure_Button"), icons.load<Texture>("VAC"), localization.format("VAC_Filter_Secure_Tooltip")), new GUIContent(localization.format("VAC_Insecure_Button"), icons.load<Texture2D>("VAC_Off"), localization.format("VAC_Filter_Insecure_Tooltip")), new GUIContent(localization.format("VAC_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("VAC_Filter_Any_Tooltip")));
        VACProtectionButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        VACProtectionButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        VACProtectionButtonState.SizeScale_X = 0.2f;
        VACProtectionButtonState.SizeOffset_Y = 30f;
        VACProtectionButtonState.onSwappedState = onSwappedVACProtectionState;
        VACProtectionButtonState.button.iconColor = ESleekTint.FOREGROUND;
        VACProtectionButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(VACProtectionButtonState);
        num++;
        battlEyeProtectionButtonState = new SleekButtonState(20, new GUIContent(localization.format("BattlEye_Secure_Button"), icons.load<Texture>("BattlEye"), localization.format("BattlEye_Filter_Secure_Tooltip")), new GUIContent(localization.format("BattlEye_Insecure_Button"), icons.load<Texture2D>("BattlEye_Off"), localization.format("BattlEye_Filter_Insecure_Tooltip")), new GUIContent(localization.format("BattlEye_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("BattlEye_Filter_Any_Tooltip")));
        battlEyeProtectionButtonState.PositionScale_X = (float)(num % 5) * 0.2f;
        battlEyeProtectionButtonState.PositionOffset_Y = (float)(num / 5) * 30f;
        battlEyeProtectionButtonState.SizeScale_X = 0.2f;
        battlEyeProtectionButtonState.SizeOffset_Y = 30f;
        battlEyeProtectionButtonState.onSwappedState = onSwappedBattlEyeProtectionState;
        battlEyeProtectionButtonState.button.iconColor = ESleekTint.FOREGROUND;
        battlEyeProtectionButtonState.UseContentTooltip = true;
        filtersEditorContainer.AddChild(battlEyeProtectionButtonState);
    }

    private void AnimateOpenSubcontainers()
    {
        if (FilterSettings.isColumnsEditorOpen)
        {
            columnTogglesContainer.AnimatePositionOffset(0f, columnTogglesContainer.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20f);
            columnTogglesContainer.AnimatePositionScale(0f, columnTogglesContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20f);
        }
        else
        {
            columnTogglesContainer.AnimatePositionOffset(20f, columnTogglesContainer.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20f);
            columnTogglesContainer.AnimatePositionScale(1f, columnTogglesContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20f);
        }
        if (FilterSettings.isPresetsListOpen)
        {
            presetsScrollView.AnimatePositionOffset(0f, presetsScrollView.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20f);
            presetsScrollView.AnimatePositionScale(0f, presetsScrollView.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20f);
        }
        else
        {
            presetsScrollView.AnimatePositionOffset(20f, presetsScrollView.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20f);
            presetsScrollView.AnimatePositionScale(1f, presetsScrollView.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20f);
        }
        float num = (FilterSettings.isPresetsListOpen ? (presetsScrollView.SizeOffset_Y + 10f) : 0f);
        if (FilterSettings.isQuickFiltersEditorOpen)
        {
            filtersEditorContainer.AnimatePositionOffset(0f, -60f - filtersEditorContainer.SizeOffset_Y - num, ESleekLerp.EXPONENTIAL, 20f);
            filtersEditorContainer.AnimatePositionScale(0f, filtersEditorContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20f);
        }
        else
        {
            filtersEditorContainer.AnimatePositionOffset(20f, -60f - filtersEditorContainer.SizeOffset_Y - num, ESleekLerp.EXPONENTIAL, 20f);
            filtersEditorContainer.AnimatePositionScale(1f, filtersEditorContainer.PositionScale_Y, ESleekLerp.EXPONENTIAL, 20f);
        }
        float num2 = (FilterSettings.isColumnsEditorOpen ? (columnTogglesContainer.SizeOffset_Y + 10f) : 0f);
        float num3 = (FilterSettings.isQuickFiltersEditorOpen ? (filtersEditorContainer.SizeOffset_Y + 10f) : 0f);
        list.AnimatePositionOffset(list.PositionOffset_X, num2, ESleekLerp.EXPONENTIAL, 20f);
        list.AnimateSizeOffset(list.SizeOffset_X, 0f - num2 - num - num3, ESleekLerp.EXPONENTIAL, 20f);
    }

    private void SynchronizeVisibleColumns()
    {
        float num = -30f;
        if (FilterSettings.columns.anticheat)
        {
            num -= anticheatColumnButton.SizeOffset_X;
            anticheatColumnButton.PositionOffset_X = num;
            anticheatColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            anticheatColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.cheats)
        {
            num -= cheatsColumnButton.SizeOffset_X;
            cheatsColumnButton.PositionOffset_X = num;
            cheatsColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            cheatsColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.plugins)
        {
            num -= pluginsColumnButton.SizeOffset_X;
            pluginsColumnButton.PositionOffset_X = num;
            pluginsColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            pluginsColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.workshop)
        {
            num -= workshopColumnButton.SizeOffset_X;
            workshopColumnButton.PositionOffset_X = num;
            workshopColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            workshopColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.monetization)
        {
            num -= monetizationColumnButton.SizeOffset_X;
            monetizationColumnButton.PositionOffset_X = num;
            monetizationColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            monetizationColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.gold)
        {
            num -= goldColumnButton.SizeOffset_X;
            goldColumnButton.PositionOffset_X = num;
            goldColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            goldColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.perspective)
        {
            num -= perspectiveColumnButton.SizeOffset_X;
            perspectiveColumnButton.PositionOffset_X = num;
            perspectiveColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            perspectiveColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.combat)
        {
            num -= combatColumnButton.SizeOffset_X;
            combatColumnButton.PositionOffset_X = num;
            combatColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            combatColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.password)
        {
            num -= passwordColumnButton.SizeOffset_X;
            passwordColumnButton.PositionOffset_X = num;
            passwordColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            passwordColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.maxPlayers)
        {
            num -= maxPlayersColumnButton.SizeOffset_X;
            maxPlayersColumnButton.PositionOffset_X = num;
            maxPlayersColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            maxPlayersColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.players)
        {
            if (FilterSettings.columns.maxPlayers)
            {
                playersColumnButton.SizeOffset_X = 80f;
            }
            else
            {
                playersColumnButton.SizeOffset_X = 120f;
            }
            num -= playersColumnButton.SizeOffset_X;
            playersColumnButton.PositionOffset_X = num;
            playersColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            playersColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.ping)
        {
            num -= pingColumnButton.SizeOffset_X;
            pingColumnButton.PositionOffset_X = num;
            pingColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            pingColumnButton.IsVisible = false;
        }
        if (FilterSettings.columns.map)
        {
            num -= mapColumnButton.SizeOffset_X;
            mapColumnButton.PositionOffset_X = num;
            mapColumnButton.IsVisible = true;
            num -= 0f;
        }
        else
        {
            mapColumnButton.IsVisible = false;
        }
        num -= nameColumnButton.PositionOffset_X;
        nameColumnButton.SizeOffset_X = num;
        for (int num2 = serverBox.ElementCount - 1; num2 >= 0; num2--)
        {
            (serverBox.GetElement(num2) as SleekServer).SynchronizeVisibleColumns();
        }
    }

    private void onClickedResetFilters(ISleekElement button)
    {
        FilterSettings.activeFilters.CopyFrom(FilterSettings.defaultPresetInternet);
        FilterSettings.activeFilters.presetName = localization.format("DefaultPreset_Internet_Label");
        FilterSettings.InvokeActiveFiltersReplaced();
    }

    private void onClickedBackButton(ISleekElement button)
    {
        MenuPlayUI.open();
        close();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        TempSteamworksMatchmaking matchmakingService = Provider.provider.matchmakingService;
        matchmakingService.onMasterServerAdded = (TempSteamworksMatchmaking.MasterServerAdded)Delegate.Remove(matchmakingService.onMasterServerAdded, new TempSteamworksMatchmaking.MasterServerAdded(onMasterServerAdded));
        TempSteamworksMatchmaking matchmakingService2 = Provider.provider.matchmakingService;
        matchmakingService2.onMasterServerRemoved = (TempSteamworksMatchmaking.MasterServerRemoved)Delegate.Remove(matchmakingService2.onMasterServerRemoved, new TempSteamworksMatchmaking.MasterServerRemoved(onMasterServerRemoved));
        TempSteamworksMatchmaking matchmakingService3 = Provider.provider.matchmakingService;
        matchmakingService3.onMasterServerResorted = (TempSteamworksMatchmaking.MasterServerResorted)Delegate.Remove(matchmakingService3.onMasterServerResorted, new TempSteamworksMatchmaking.MasterServerResorted(onMasterServerResorted));
        TempSteamworksMatchmaking matchmakingService4 = Provider.provider.matchmakingService;
        matchmakingService4.onMasterServerRefreshed = (TempSteamworksMatchmaking.MasterServerRefreshed)Delegate.Remove(matchmakingService4.onMasterServerRefreshed, new TempSteamworksMatchmaking.MasterServerRefreshed(onMasterServerRefreshed));
        FilterSettings.OnActiveFiltersModified -= OnActiveFiltersModified;
        FilterSettings.OnActiveFiltersReplaced -= OnActiveFiltersReplaced;
        FilterSettings.OnCustomPresetsListChanged -= OnCustomPresetsListChanged;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (isRefreshing)
        {
            float num = refreshIcon.RotationAngle + Time.deltaTime * 90f;
            num %= 360f;
            refreshIcon.RotationAngle = num;
        }
    }

    private void SetIsRefreshing(bool value)
    {
        isRefreshing = value;
        if (isRefreshing)
        {
            refreshButton.Text = localization.format("Refresh_Cancel_Label");
            refreshButton.TooltipText = localization.format("Refresh_Cancel_Tooltip");
        }
        else
        {
            refreshButton.Text = localization.format("Refresh_Label");
            refreshButton.TooltipText = localization.format("Refresh_Tooltip");
            refreshIcon.RotationAngle = 0f;
        }
    }

    public MenuPlayServersUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Menu/Play/MenuPlayServers.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Play/MenuPlayServers/MenuPlayServers.unity3d");
        active = false;
        columnTogglesContainer = Glazier.Get().CreateFrame();
        columnTogglesContainer.PositionOffset_X = 20f;
        columnTogglesContainer.PositionScale_X = 1f;
        columnTogglesContainer.SizeScale_X = 1f;
        AddChild(columnTogglesContainer);
        int num = 0;
        ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
        sleekToggle.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle.Value = FilterSettings.columns.map;
        sleekToggle.AddLabel(localization.format("Map_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle.TooltipText = localization.format("Map_Column_Toggle_Tooltip");
        sleekToggle.OnValueChanged += OnMapColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle);
        num++;
        ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
        sleekToggle2.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle2.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle2.Value = FilterSettings.columns.ping;
        sleekToggle2.AddLabel(localization.format("Ping_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle2.TooltipText = localization.format("Ping_Column_Toggle_Tooltip");
        sleekToggle2.OnValueChanged += OnPingColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle2);
        num++;
        ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
        sleekToggle3.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle3.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle3.Value = FilterSettings.columns.players;
        sleekToggle3.AddLabel(localization.format("Players_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle3.TooltipText = localization.format("Players_Column_Toggle_Tooltip");
        sleekToggle3.OnValueChanged += OnPlayersColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle3);
        num++;
        ISleekToggle sleekToggle4 = Glazier.Get().CreateToggle();
        sleekToggle4.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle4.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle4.Value = FilterSettings.columns.maxPlayers;
        sleekToggle4.AddLabel(localization.format("MaxPlayers_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle4.TooltipText = localization.format("MaxPlayers_Column_Toggle_Tooltip");
        sleekToggle4.OnValueChanged += OnMaxPlayersColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle4);
        num++;
        ISleekToggle sleekToggle5 = Glazier.Get().CreateToggle();
        sleekToggle5.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle5.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle5.Value = FilterSettings.columns.password;
        sleekToggle5.AddLabel(localization.format("Password_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle5.TooltipText = localization.format("Password_Column_Toggle_Tooltip");
        sleekToggle5.OnValueChanged += OnPasswordColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle5);
        num++;
        ISleekToggle sleekToggle6 = Glazier.Get().CreateToggle();
        sleekToggle6.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle6.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle6.Value = FilterSettings.columns.combat;
        sleekToggle6.AddLabel(localization.format("Combat_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle6.TooltipText = localization.format("Combat_Column_Toggle_Tooltip");
        sleekToggle6.OnValueChanged += OnCombatColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle6);
        num++;
        ISleekToggle sleekToggle7 = Glazier.Get().CreateToggle();
        sleekToggle7.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle7.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle7.Value = FilterSettings.columns.perspective;
        sleekToggle7.AddLabel(localization.format("Perspective_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle7.TooltipText = localization.format("Perspective_Column_Toggle_Tooltip");
        sleekToggle7.OnValueChanged += OnPerspectiveColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle7);
        num++;
        ISleekToggle sleekToggle8 = Glazier.Get().CreateToggle();
        sleekToggle8.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle8.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle8.Value = FilterSettings.columns.maxPlayers;
        sleekToggle8.AddLabel(localization.format("Gold_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle8.TooltipText = localization.format("Gold_Column_Toggle_Tooltip");
        sleekToggle8.OnValueChanged += OnGoldColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle8);
        num++;
        ISleekToggle sleekToggle9 = Glazier.Get().CreateToggle();
        sleekToggle9.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle9.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle9.Value = FilterSettings.columns.cheats;
        sleekToggle9.AddLabel(localization.format("Monetization_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle9.TooltipText = localization.format("Monetization_Column_Toggle_Tooltip");
        sleekToggle9.OnValueChanged += OnMonetizationColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle9);
        num++;
        ISleekToggle sleekToggle10 = Glazier.Get().CreateToggle();
        sleekToggle10.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle10.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle10.Value = FilterSettings.columns.workshop;
        sleekToggle10.AddLabel(localization.format("Workshop_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle10.TooltipText = localization.format("Workshop_Column_Toggle_Tooltip");
        sleekToggle10.OnValueChanged += OnWorkshopColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle10);
        num++;
        ISleekToggle sleekToggle11 = Glazier.Get().CreateToggle();
        sleekToggle11.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle11.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle11.Value = FilterSettings.columns.cheats;
        sleekToggle11.AddLabel(localization.format("Plugins_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle11.TooltipText = localization.format("Plugins_Column_Toggle_Tooltip");
        sleekToggle11.OnValueChanged += OnPluginsColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle11);
        num++;
        ISleekToggle sleekToggle12 = Glazier.Get().CreateToggle();
        sleekToggle12.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle12.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle12.Value = FilterSettings.columns.cheats;
        sleekToggle12.AddLabel(localization.format("Cheats_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle12.TooltipText = localization.format("Cheats_Column_Toggle_Tooltip");
        sleekToggle12.OnValueChanged += OnCheatsColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle12);
        num++;
        ISleekToggle sleekToggle13 = Glazier.Get().CreateToggle();
        sleekToggle13.PositionScale_X = (float)(num % 5) * 0.2f;
        sleekToggle13.PositionOffset_Y = (float)(num / 5) * 40f;
        sleekToggle13.Value = FilterSettings.columns.anticheat;
        sleekToggle13.AddLabel(localization.format("Anticheat_Column_Toggle_Label"), ESleekSide.RIGHT);
        sleekToggle13.TooltipText = localization.format("Anticheat_Column_Toggle_Tooltip");
        sleekToggle13.OnValueChanged += OnAnticheatColumnToggled;
        columnTogglesContainer.AddChild(sleekToggle13);
        num++;
        columnTogglesContainer.SizeOffset_Y = (float)((num - 1) / 5 + 1) * 40f;
        filtersEditorContainer = Glazier.Get().CreateFrame();
        filtersEditorContainer.PositionOffset_X = 20f;
        filtersEditorContainer.PositionOffset_Y = -150f;
        filtersEditorContainer.PositionScale_X = 1f;
        filtersEditorContainer.PositionScale_Y = 1f;
        filtersEditorContainer.SizeScale_X = 1f;
        filtersEditorContainer.SizeOffset_Y = 90f;
        AddChild(filtersEditorContainer);
        CreateQuickFilterButtons();
        presetsScrollView = Glazier.Get().CreateScrollView();
        presetsScrollView.PositionOffset_X = 20f;
        presetsScrollView.PositionScale_X = 1f;
        presetsScrollView.PositionScale_Y = 1f;
        presetsScrollView.SizeScale_X = 1f;
        presetsScrollView.ScaleContentToWidth = true;
        AddChild(presetsScrollView);
        customPresetsContainer = Glazier.Get().CreateFrame();
        customPresetsContainer.SizeScale_X = 1f;
        presetsScrollView.AddChild(customPresetsContainer);
        defaultPresetsContainer = Glazier.Get().CreateFrame();
        defaultPresetsContainer.SizeScale_X = 1f;
        defaultPresetsContainer.SizeOffset_Y = 30f;
        presetsScrollView.AddChild(defaultPresetsContainer);
        SleekDefaultServerListPresetButton child = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetInternet, localization, icons)
        {
            SizeOffset_Y = 30f,
            SizeScale_X = 0.2f
        };
        defaultPresetsContainer.AddChild(child);
        SleekDefaultServerListPresetButton child2 = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetLAN, localization, icons)
        {
            PositionScale_X = 0.2f,
            SizeOffset_Y = 30f,
            SizeScale_X = 0.2f
        };
        defaultPresetsContainer.AddChild(child2);
        SleekDefaultServerListPresetButton child3 = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetHistory, localization, icons)
        {
            PositionScale_X = 0.4f,
            SizeOffset_Y = 30f,
            SizeScale_X = 0.2f
        };
        defaultPresetsContainer.AddChild(child3);
        SleekDefaultServerListPresetButton child4 = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetFavorites, localization, icons)
        {
            PositionScale_X = 0.6f,
            SizeOffset_Y = 30f,
            SizeScale_X = 0.2f
        };
        defaultPresetsContainer.AddChild(child4);
        SleekDefaultServerListPresetButton child5 = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetFriends, localization, icons)
        {
            PositionScale_X = 0.8f,
            SizeOffset_Y = 30f,
            SizeScale_X = 0.2f
        };
        defaultPresetsContainer.AddChild(child5);
        list = Glazier.Get().CreateFrame();
        list.SizeScale_X = 1f;
        list.SizeScale_Y = 1f;
        AddChild(list);
        SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(icons.load<Texture2D>("Columns"));
        sleekButtonIcon.SizeOffset_X = 40f;
        sleekButtonIcon.SizeOffset_Y = 40f;
        sleekButtonIcon.iconPositionOffset = 10;
        sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
        sleekButtonIcon.tooltip = localization.format("Columns_Tooltip");
        sleekButtonIcon.onClickedButton += OnClickedColumnsButton;
        list.AddChild(sleekButtonIcon);
        nameColumnButton = Glazier.Get().CreateButton();
        nameColumnButton.PositionOffset_X = 40f;
        nameColumnButton.SizeOffset_X = -310f;
        nameColumnButton.SizeOffset_Y = 40f;
        nameColumnButton.SizeScale_X = 1f;
        nameColumnButton.Text = localization.format("Sort_Name");
        nameColumnButton.TooltipText = localization.format("Sort_Name_Tooltip");
        nameColumnButton.OnClicked += OnNameColumnClicked;
        list.AddChild(nameColumnButton);
        mapColumnButton = Glazier.Get().CreateButton();
        mapColumnButton.PositionOffset_X = -260f;
        mapColumnButton.PositionScale_X = 1f;
        mapColumnButton.SizeOffset_X = 153f;
        mapColumnButton.SizeOffset_Y = 40f;
        mapColumnButton.Text = localization.format("Sort_Map");
        mapColumnButton.TooltipText = localization.format("Sort_Map_Tooltip");
        mapColumnButton.OnClicked += OnMapColumnClicked;
        list.AddChild(mapColumnButton);
        playersColumnButton = Glazier.Get().CreateButton();
        playersColumnButton.PositionOffset_X = -150f;
        playersColumnButton.PositionScale_X = 1f;
        playersColumnButton.SizeOffset_X = 80f;
        playersColumnButton.SizeOffset_Y = 40f;
        playersColumnButton.Text = localization.format("Sort_Players");
        playersColumnButton.TooltipText = localization.format("Sort_Players_Tooltip");
        playersColumnButton.OnClicked += OnPlayersColumnClicked;
        list.AddChild(playersColumnButton);
        maxPlayersColumnButton = Glazier.Get().CreateButton();
        maxPlayersColumnButton.PositionOffset_X = -150f;
        maxPlayersColumnButton.PositionScale_X = 1f;
        maxPlayersColumnButton.SizeOffset_X = 80f;
        maxPlayersColumnButton.SizeOffset_Y = 40f;
        maxPlayersColumnButton.Text = localization.format("MaxPlayers_Column_Label");
        maxPlayersColumnButton.TooltipText = localization.format("MaxPlayers_Column_Tooltip");
        maxPlayersColumnButton.OnClicked += OnMaxPlayersColumnClicked;
        list.AddChild(maxPlayersColumnButton);
        pingColumnButton = Glazier.Get().CreateButton();
        pingColumnButton.PositionOffset_X = -80f;
        pingColumnButton.PositionScale_X = 1f;
        pingColumnButton.SizeOffset_X = 80f;
        pingColumnButton.SizeOffset_Y = 40f;
        pingColumnButton.Text = localization.format("Sort_Ping");
        pingColumnButton.TooltipText = localization.format("Sort_Ping_Tooltip");
        pingColumnButton.OnClicked += OnPingColumnClicked;
        list.AddChild(pingColumnButton);
        anticheatColumnButton = Glazier.Get().CreateButton();
        anticheatColumnButton.PositionScale_X = 1f;
        anticheatColumnButton.SizeOffset_X = 80f;
        anticheatColumnButton.SizeOffset_Y = 40f;
        anticheatColumnButton.Text = localization.format("Anticheat_Column_Label");
        anticheatColumnButton.TooltipText = localization.format("Anticheat_Column_Tooltip");
        anticheatColumnButton.OnClicked += OnAnticheatColumnClicked;
        list.AddChild(anticheatColumnButton);
        perspectiveColumnButton = new SleekButtonIcon(icons.load<Texture2D>("Perspective"), 20);
        perspectiveColumnButton.PositionScale_X = 1f;
        perspectiveColumnButton.SizeOffset_X = 40f;
        perspectiveColumnButton.SizeOffset_Y = 40f;
        perspectiveColumnButton.tooltip = localization.format("Perspective_Column_Tooltip");
        perspectiveColumnButton.onClickedButton += OnPerspectiveColumnClicked;
        perspectiveColumnButton.iconColor = ESleekTint.FOREGROUND;
        perspectiveColumnButton.iconPositionOffset = 10;
        list.AddChild(perspectiveColumnButton);
        combatColumnButton = new SleekButtonIcon(icons.load<Texture2D>("Combat"), 20);
        combatColumnButton.PositionScale_X = 1f;
        combatColumnButton.SizeOffset_X = 40f;
        combatColumnButton.SizeOffset_Y = 40f;
        combatColumnButton.tooltip = localization.format("Combat_Column_Tooltip");
        combatColumnButton.onClickedButton += OnCombatColumnClicked;
        combatColumnButton.iconColor = ESleekTint.FOREGROUND;
        combatColumnButton.iconPositionOffset = 10;
        list.AddChild(combatColumnButton);
        passwordColumnButton = new SleekButtonIcon(icons.load<Texture2D>("PasswordProtected"), 20);
        passwordColumnButton.PositionScale_X = 1f;
        passwordColumnButton.SizeOffset_X = 40f;
        passwordColumnButton.SizeOffset_Y = 40f;
        passwordColumnButton.tooltip = localization.format("Password_Column_Tooltip");
        passwordColumnButton.onClickedButton += OnPasswordColumnClicked;
        passwordColumnButton.iconColor = ESleekTint.FOREGROUND;
        passwordColumnButton.iconPositionOffset = 10;
        list.AddChild(passwordColumnButton);
        workshopColumnButton = new SleekButtonIcon(icons.load<Texture2D>("HasMods"), 20);
        workshopColumnButton.PositionScale_X = 1f;
        workshopColumnButton.SizeOffset_X = 40f;
        workshopColumnButton.SizeOffset_Y = 40f;
        workshopColumnButton.tooltip = localization.format("Workshop_Column_Tooltip");
        workshopColumnButton.onClickedButton += OnWorkshopColumnClicked;
        workshopColumnButton.iconColor = ESleekTint.FOREGROUND;
        workshopColumnButton.iconPositionOffset = 10;
        list.AddChild(workshopColumnButton);
        goldColumnButton = new SleekButtonIcon(icons.load<Texture2D>("GoldRequired"), 20);
        goldColumnButton.PositionScale_X = 1f;
        goldColumnButton.SizeOffset_X = 40f;
        goldColumnButton.SizeOffset_Y = 40f;
        goldColumnButton.tooltip = localization.format("Gold_Column_Tooltip");
        goldColumnButton.onClickedButton += OnGoldColumnClicked;
        goldColumnButton.textColor = Palette.PRO;
        goldColumnButton.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
        goldColumnButton.iconColor = Palette.PRO;
        goldColumnButton.iconPositionOffset = 10;
        list.AddChild(goldColumnButton);
        cheatsColumnButton = new SleekButtonIcon(icons.load<Texture2D>("CheatCodes"), 20);
        cheatsColumnButton.PositionScale_X = 1f;
        cheatsColumnButton.SizeOffset_X = 40f;
        cheatsColumnButton.SizeOffset_Y = 40f;
        cheatsColumnButton.tooltip = localization.format("Cheats_Column_Tooltip");
        cheatsColumnButton.onClickedButton += OnCheatsColumnClicked;
        cheatsColumnButton.iconColor = ESleekTint.FOREGROUND;
        cheatsColumnButton.iconPositionOffset = 10;
        list.AddChild(cheatsColumnButton);
        monetizationColumnButton = new SleekButtonIcon(icons.load<Texture2D>("Monetized"), 20);
        monetizationColumnButton.PositionOffset_X = -260f;
        monetizationColumnButton.PositionScale_X = 1f;
        monetizationColumnButton.SizeOffset_X = 40f;
        monetizationColumnButton.SizeOffset_Y = 40f;
        monetizationColumnButton.tooltip = localization.format("Monetization_Column_Tooltip");
        monetizationColumnButton.onClickedButton += OnMonetizationColumnClicked;
        monetizationColumnButton.iconColor = ESleekTint.FOREGROUND;
        monetizationColumnButton.iconPositionOffset = 10;
        list.AddChild(monetizationColumnButton);
        pluginsColumnButton = new SleekButtonIcon(icons.load<Texture2D>("Plugins"), 20);
        pluginsColumnButton.PositionScale_X = 1f;
        pluginsColumnButton.SizeOffset_X = 40f;
        pluginsColumnButton.SizeOffset_Y = 40f;
        pluginsColumnButton.tooltip = localization.format("Plugins_Column_Tooltip");
        pluginsColumnButton.onClickedButton += OnPluginsColumnClicked;
        pluginsColumnButton.iconColor = ESleekTint.FOREGROUND;
        pluginsColumnButton.iconPositionOffset = 10;
        list.AddChild(pluginsColumnButton);
        infoBox = Glazier.Get().CreateBox();
        infoBox.PositionOffset_Y = 50f;
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
        FilterSettings.OnActiveFiltersModified += OnActiveFiltersModified;
        FilterSettings.OnActiveFiltersReplaced += OnActiveFiltersReplaced;
        FilterSettings.OnCustomPresetsListChanged += OnCustomPresetsListChanged;
        refreshButton = Glazier.Get().CreateButton();
        refreshButton.PositionOffset_X = -200f;
        refreshButton.PositionOffset_Y = -50f;
        refreshButton.PositionScale_X = 1f;
        refreshButton.PositionScale_Y = 1f;
        refreshButton.SizeOffset_X = 200f;
        refreshButton.SizeOffset_Y = 50f;
        refreshButton.Text = localization.format("Refresh_Label");
        refreshButton.TooltipText = localization.format("Refresh_Tooltip");
        refreshButton.OnClicked += onClickedRefreshButton;
        refreshButton.FontSize = ESleekFontSize.Medium;
        AddChild(refreshButton);
        refreshIcon = Glazier.Get().CreateImage(icons.load<Texture2D>("Refresh"));
        refreshIcon.PositionOffset_X = 5f;
        refreshIcon.PositionOffset_Y = 5f;
        refreshIcon.SizeOffset_X = 40f;
        refreshIcon.SizeOffset_Y = 40f;
        refreshIcon.CanRotate = true;
        refreshIcon.TintColor = ESleekTint.FOREGROUND;
        refreshButton.AddChild(refreshIcon);
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.PositionOffset_X = 205f;
        sleekElement.PositionOffset_Y = -50f;
        sleekElement.PositionScale_Y = 1f;
        sleekElement.SizeOffset_X = -410f;
        sleekElement.SizeScale_X = 1f;
        sleekElement.SizeOffset_Y = 50f;
        AddChild(sleekElement);
        SleekButtonIcon sleekButtonIcon2 = new SleekButtonIcon(icons.load<Texture2D>("Hosting"));
        sleekButtonIcon2.PositionOffset_X = 5f;
        sleekButtonIcon2.SizeOffset_X = -10f;
        sleekButtonIcon2.SizeOffset_Y = 50f;
        sleekButtonIcon2.SizeScale_X = 0.25f;
        sleekButtonIcon2.text = localization.format("HostingButtonText");
        sleekButtonIcon2.tooltip = localization.format("HostingButtonTooltip");
        sleekButtonIcon2.onClickedButton += onClickedHostingButton;
        sleekButtonIcon2.fontSize = ESleekFontSize.Medium;
        sleekButtonIcon2.iconColor = ESleekTint.FOREGROUND;
        sleekElement.AddChild(sleekButtonIcon2);
        SleekButtonIcon sleekButtonIcon3 = new SleekButtonIcon(icons.load<Texture2D>("Presets"), 40);
        sleekButtonIcon3.PositionOffset_X = 5f;
        sleekButtonIcon3.PositionScale_X = 0.25f;
        sleekButtonIcon3.SizeOffset_X = -10f;
        sleekButtonIcon3.SizeOffset_Y = 50f;
        sleekButtonIcon3.SizeScale_X = 0.25f;
        sleekButtonIcon3.text = localization.format("ViewPresetsButton_Label");
        sleekButtonIcon3.tooltip = localization.format("ViewPresetsButton_Tooltip");
        sleekButtonIcon3.onClickedButton += onClickedPresetsButton;
        sleekButtonIcon3.fontSize = ESleekFontSize.Medium;
        sleekButtonIcon3.iconColor = ESleekTint.FOREGROUND;
        sleekElement.AddChild(sleekButtonIcon3);
        SleekButtonIcon sleekButtonIcon4 = new SleekButtonIcon(icons.load<Texture2D>("Filters"), 40);
        sleekButtonIcon4.PositionOffset_X = 5f;
        sleekButtonIcon4.PositionScale_X = 0.5f;
        sleekButtonIcon4.SizeOffset_X = -10f;
        sleekButtonIcon4.SizeOffset_Y = 50f;
        sleekButtonIcon4.SizeScale_X = 0.25f;
        sleekButtonIcon4.text = localization.format("QuickFiltersButton_Label");
        sleekButtonIcon4.tooltip = localization.format("QuickFiltersButton_Tooltip");
        sleekButtonIcon4.onClickedButton += OnQuickFiltersButtonClicked;
        sleekButtonIcon4.fontSize = ESleekFontSize.Medium;
        sleekButtonIcon4.iconColor = ESleekTint.FOREGROUND;
        sleekElement.AddChild(sleekButtonIcon4);
        presetsEditorButton = new SleekButtonIcon(icons.load<Texture2D>("PresetsEditor"), 40);
        presetsEditorButton.PositionOffset_X = 5f;
        presetsEditorButton.PositionScale_X = 0.75f;
        presetsEditorButton.SizeOffset_X = -10f;
        presetsEditorButton.SizeOffset_Y = 50f;
        presetsEditorButton.SizeScale_X = 0.25f;
        presetsEditorButton.tooltip = localization.format("PresetsEditorButton_Tooltip");
        presetsEditorButton.onClickedButton += OnPresetsEditorButtonClicked;
        presetsEditorButton.fontSize = ESleekFontSize.Medium;
        presetsEditorButton.iconColor = ESleekTint.FOREGROUND;
        sleekElement.AddChild(presetsEditorButton);
        serverBox = new SleekList<SteamServerInfo>();
        serverBox.PositionOffset_Y = 50f;
        serverBox.SizeOffset_Y = -110f;
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
        AddChild(backButton);
        SynchronizeVisibleColumns();
        SynchronizePresetsList();
        SynchronizePresetsEditorButtonLabel();
        serverListFiltersUI = new MenuPlayServerListFiltersUI(this);
        serverListFiltersUI.PositionOffset_X = 10f;
        serverListFiltersUI.PositionOffset_Y = 10f;
        serverListFiltersUI.PositionScale_Y = 1f;
        serverListFiltersUI.SizeOffset_X = -20f;
        serverListFiltersUI.SizeOffset_Y = -20f;
        serverListFiltersUI.SizeScale_X = 1f;
        serverListFiltersUI.SizeScale_Y = 1f;
        MenuUI.container.AddChild(serverListFiltersUI);
    }
}
