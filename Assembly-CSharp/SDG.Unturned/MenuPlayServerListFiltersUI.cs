using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayServerListFiltersUI : SleekFullscreenBox
{
    public bool active;

    private SleekButtonIcon backButton;

    private ISleekBox presetsTitleBox;

    private ISleekScrollView presetsScrollView;

    private ISleekElement customPresetsContainer;

    private ISleekElement defaultPresetsContainer;

    private ISleekBox filtersTitleBox;

    private ISleekScrollView filtersScrollView;

    private SleekButtonIconConfirm deletePresetButton;

    private ISleekField presetNameField;

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

    public void open()
    {
        if (!active)
        {
            active = true;
            SynchronizeFilterButtons();
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

    public void OpenForMap(string map)
    {
        mapField.Text = map;
        FilterSettings.activeFilters.mapName = map;
        FilterSettings.MarkActiveFilterModified();
        open();
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

    private void SynchronizeDeletePresetButtonVisible()
    {
        deletePresetButton.IsVisible = FilterSettings.activeFilters.presetId > 0;
        if (deletePresetButton.IsVisible)
        {
            filtersScrollView.ContentSizeOffset = new Vector2(0f, deletePresetButton.PositionOffset_Y + deletePresetButton.SizeOffset_Y);
        }
        else
        {
            filtersScrollView.ContentSizeOffset = new Vector2(0f, presetNameField.PositionOffset_Y + presetNameField.SizeOffset_Y);
        }
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

    private void OnClickedCreatePreset(ISleekElement button)
    {
        if (!string.IsNullOrWhiteSpace(presetNameField.Text))
        {
            FilterSettings.activeFilters.presetName = presetNameField.Text.Trim();
            FilterSettings.activeFilters.presetId = FilterSettings.CreatePresetId();
            presetNameField.Text = string.Empty;
            ServerListFilters serverListFilters = new ServerListFilters();
            serverListFilters.CopyFrom(FilterSettings.activeFilters);
            FilterSettings.customPresets.Add(serverListFilters);
            FilterSettings.customPresets.Sort((ServerListFilters lhs, ServerListFilters rhs) => (!string.IsNullOrEmpty(lhs.presetName) && !string.IsNullOrEmpty(rhs.presetName)) ? lhs.presetName.CompareTo(rhs.presetName) : 0);
            FilterSettings.InvokeActiveFiltersReplaced();
            FilterSettings.InvokeCustomFiltersListChanged();
        }
    }

    private void OnClickedDeletePreset(ISleekElement button)
    {
        if (FilterSettings.activeFilters.presetId > 0)
        {
            FilterSettings.RemovePreset(FilterSettings.activeFilters.presetId);
            FilterSettings.activeFilters.presetName = string.Empty;
            FilterSettings.activeFilters.presetId = -1;
            FilterSettings.InvokeActiveFiltersReplaced();
            FilterSettings.InvokeCustomFiltersListChanged();
        }
    }

    private void onClickedBackButton(ISleekElement button)
    {
        MenuPlayUI.serverListUI.open();
        close();
    }

    private void SynchronizePresetTitle()
    {
        if (string.IsNullOrEmpty(FilterSettings.activeFilters.presetName))
        {
            filtersTitleBox.Text = MenuPlayUI.serverListUI.localization.format("PresetName_Empty");
        }
        else
        {
            filtersTitleBox.Text = FilterSettings.activeFilters.presetName;
        }
    }

    private void SynchronizePresetsList()
    {
        customPresetsContainer.RemoveAllChildren();
        float num = 0f;
        foreach (ServerListFilters customPreset in FilterSettings.customPresets)
        {
            SleekCustomServerListPresetButton sleekCustomServerListPresetButton = new SleekCustomServerListPresetButton(customPreset);
            sleekCustomServerListPresetButton.PositionOffset_Y = num;
            sleekCustomServerListPresetButton.SizeOffset_X = 200f;
            sleekCustomServerListPresetButton.SizeOffset_Y = 30f;
            customPresetsContainer.AddChild(sleekCustomServerListPresetButton);
            num += sleekCustomServerListPresetButton.SizeOffset_Y;
        }
        customPresetsContainer.SizeOffset_Y = num;
        if (num > 0f)
        {
            defaultPresetsContainer.PositionOffset_Y = customPresetsContainer.SizeOffset_Y + 10f;
        }
        else
        {
            defaultPresetsContainer.PositionOffset_Y = 0f;
        }
        presetsScrollView.ContentSizeOffset = new Vector2(0f, defaultPresetsContainer.PositionOffset_Y + defaultPresetsContainer.SizeOffset_Y);
    }

    private void OnActiveFiltersModified()
    {
        SynchronizePresetTitle();
        SynchronizeDeletePresetButtonVisible();
    }

    private void OnActiveFiltersReplaced()
    {
        SynchronizePresetTitle();
        SynchronizeFilterButtons();
        SynchronizeDeletePresetButtonVisible();
    }

    private void OnCustomPresetsListChanged()
    {
        SynchronizePresetsList();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        FilterSettings.OnActiveFiltersModified -= OnActiveFiltersModified;
        FilterSettings.OnActiveFiltersReplaced -= OnActiveFiltersReplaced;
        FilterSettings.OnCustomPresetsListChanged -= OnCustomPresetsListChanged;
    }

    public MenuPlayServerListFiltersUI(MenuPlayServersUI serverListUI)
    {
        Local localization = serverListUI.localization;
        Bundle icons = serverListUI.icons;
        active = false;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.PositionOffset_X = -335f;
        sleekElement.PositionOffset_Y = 100f;
        sleekElement.PositionScale_X = 0.5f;
        sleekElement.SizeOffset_X = 230f;
        sleekElement.SizeOffset_Y = -200f;
        sleekElement.SizeScale_Y = 1f;
        AddChild(sleekElement);
        ISleekElement sleekElement2 = Glazier.Get().CreateFrame();
        sleekElement2.PositionOffset_X = -95f;
        sleekElement2.PositionOffset_Y = 100f;
        sleekElement2.PositionScale_X = 0.5f;
        sleekElement2.SizeOffset_X = 430f;
        sleekElement2.SizeOffset_Y = -200f;
        sleekElement2.SizeScale_Y = 1f;
        AddChild(sleekElement2);
        presetsTitleBox = Glazier.Get().CreateBox();
        presetsTitleBox.SizeOffset_X = 200f;
        presetsTitleBox.SizeOffset_Y = 50f;
        presetsTitleBox.FontSize = ESleekFontSize.Medium;
        presetsTitleBox.Text = localization.format("Presets_Label");
        presetsTitleBox.TooltipText = localization.format("Presets_Tooltip");
        sleekElement.AddChild(presetsTitleBox);
        presetsScrollView = Glazier.Get().CreateScrollView();
        presetsScrollView.PositionOffset_Y = 60f;
        presetsScrollView.SizeOffset_X = 230f;
        presetsScrollView.SizeOffset_Y = -60f;
        presetsScrollView.SizeScale_Y = 1f;
        presetsScrollView.ScaleContentToWidth = true;
        sleekElement.AddChild(presetsScrollView);
        customPresetsContainer = Glazier.Get().CreateFrame();
        customPresetsContainer.SizeScale_X = 1f;
        presetsScrollView.AddChild(customPresetsContainer);
        defaultPresetsContainer = Glazier.Get().CreateFrame();
        defaultPresetsContainer.SizeScale_X = 1f;
        presetsScrollView.AddChild(defaultPresetsContainer);
        float num = 0f;
        SleekDefaultServerListPresetButton sleekDefaultServerListPresetButton = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetInternet, localization, icons)
        {
            PositionOffset_Y = num,
            SizeOffset_X = 200f,
            SizeOffset_Y = 30f
        };
        defaultPresetsContainer.AddChild(sleekDefaultServerListPresetButton);
        num += sleekDefaultServerListPresetButton.SizeOffset_Y + 0f;
        SleekDefaultServerListPresetButton sleekDefaultServerListPresetButton2 = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetLAN, localization, icons)
        {
            PositionOffset_Y = num,
            SizeOffset_X = 200f,
            SizeOffset_Y = 30f
        };
        defaultPresetsContainer.AddChild(sleekDefaultServerListPresetButton2);
        num += sleekDefaultServerListPresetButton2.SizeOffset_Y + 0f;
        SleekDefaultServerListPresetButton sleekDefaultServerListPresetButton3 = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetHistory, localization, icons)
        {
            PositionOffset_Y = num,
            SizeOffset_X = 200f,
            SizeOffset_Y = 30f
        };
        defaultPresetsContainer.AddChild(sleekDefaultServerListPresetButton3);
        num += sleekDefaultServerListPresetButton3.SizeOffset_Y + 0f;
        SleekDefaultServerListPresetButton sleekDefaultServerListPresetButton4 = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetFavorites, localization, icons)
        {
            PositionOffset_Y = num,
            SizeOffset_X = 200f,
            SizeOffset_Y = 30f
        };
        defaultPresetsContainer.AddChild(sleekDefaultServerListPresetButton4);
        num += sleekDefaultServerListPresetButton4.SizeOffset_Y + 0f;
        SleekDefaultServerListPresetButton sleekDefaultServerListPresetButton5 = new SleekDefaultServerListPresetButton(FilterSettings.defaultPresetFriends, localization, icons)
        {
            PositionOffset_Y = num,
            SizeOffset_X = 200f,
            SizeOffset_Y = 30f
        };
        defaultPresetsContainer.AddChild(sleekDefaultServerListPresetButton5);
        num += sleekDefaultServerListPresetButton5.SizeOffset_Y + 0f;
        defaultPresetsContainer.SizeOffset_Y = num;
        SynchronizePresetsList();
        filtersTitleBox = Glazier.Get().CreateBox();
        filtersTitleBox.SizeOffset_X = 400f;
        filtersTitleBox.SizeOffset_Y = 50f;
        filtersTitleBox.FontSize = ESleekFontSize.Medium;
        sleekElement2.AddChild(filtersTitleBox);
        filtersScrollView = Glazier.Get().CreateScrollView();
        filtersScrollView.PositionOffset_Y = 60f;
        filtersScrollView.SizeOffset_X = 430f;
        filtersScrollView.SizeOffset_Y = -60f;
        filtersScrollView.SizeScale_Y = 1f;
        filtersScrollView.ScaleContentToWidth = true;
        sleekElement2.AddChild(filtersScrollView);
        float num2 = 0f;
        listSourceButtonState = new SleekButtonState(20, new GUIContent(localization.format("List_Internet_Label"), icons.load<Texture>("List_Internet"), localization.format("List_Internet_Tooltip")), new GUIContent(localization.format("List_LAN_Label"), icons.load<Texture>("List_LAN"), localization.format("List_LAN_Tooltip")), new GUIContent(localization.format("List_History_Label"), icons.load<Texture>("List_History"), localization.format("List_History_Tooltip")), new GUIContent(localization.format("List_Favorites_Label"), icons.load<Texture>("List_Favorites"), localization.format("List_Favorites_Tooltip")), new GUIContent(localization.format("List_Friends_Label"), icons.load<Texture2D>("List_Friends"), localization.format("List_Friends_Tooltip")));
        listSourceButtonState.PositionOffset_Y = num2;
        listSourceButtonState.SizeOffset_X = 200f;
        listSourceButtonState.SizeOffset_Y = 30f;
        listSourceButtonState.onSwappedState = OnSwappedListSourceState;
        listSourceButtonState.button.iconColor = ESleekTint.FOREGROUND;
        listSourceButtonState.UseContentTooltip = true;
        listSourceButtonState.AddLabel(localization.format("List_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(listSourceButtonState);
        num2 += listSourceButtonState.SizeOffset_Y + 10f;
        nameField = Glazier.Get().CreateStringField();
        nameField.PositionOffset_Y = num2;
        nameField.SizeOffset_X = 200f;
        nameField.SizeOffset_Y = 30f;
        nameField.PlaceholderText = localization.format("Name_Filter_Hint");
        nameField.OnTextChanged += onTypedNameField;
        nameField.AddLabel(localization.format("Name_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(nameField);
        num2 += nameField.SizeOffset_Y + 10f;
        mapField = Glazier.Get().CreateStringField();
        mapField.PositionOffset_Y = num2;
        mapField.SizeOffset_X = 200f;
        mapField.SizeOffset_Y = 30f;
        mapField.PlaceholderText = localization.format("Map_Filter_Hint");
        mapField.AddLabel(localization.format("Map_Filter_Label"), ESleekSide.RIGHT);
        mapField.OnTextChanged += onTypedMapField;
        mapField.MaxLength = 64;
        filtersScrollView.AddChild(mapField);
        num2 += mapField.SizeOffset_Y + 10f;
        passwordButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Password_Button"), icons.load<Texture2D>("NotPasswordProtected"), localization.format("Password_Filter_No_Tooltip")), new GUIContent(localization.format("Yes_Password_Button"), icons.load<Texture2D>("PasswordProtected"), localization.format("Password_Filter_Yes_Tooltip")), new GUIContent(localization.format("Any_Password_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Password_Filter_Any_Tooltip")));
        passwordButtonState.PositionOffset_Y = num2;
        passwordButtonState.SizeOffset_X = 200f;
        passwordButtonState.SizeOffset_Y = 30f;
        passwordButtonState.onSwappedState = onSwappedPasswordState;
        passwordButtonState.button.iconColor = ESleekTint.FOREGROUND;
        passwordButtonState.UseContentTooltip = true;
        passwordButtonState.AddLabel(localization.format("Password_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(passwordButtonState);
        num2 += passwordButtonState.SizeOffset_Y + 10f;
        attendanceButtonState = new SleekButtonState(20, new GUIContent(localization.format("Empty_Button"), icons.load<Texture>("Empty"), localization.format("Attendance_Filter_Empty_Tooltip")), new GUIContent(localization.format("HasPlayers_Button"), icons.load<Texture>("HasPlayers"), localization.format("Attendance_Filter_HasPlayers_Tooltip")), new GUIContent(localization.format("Any_Attendance_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Attendance_Filter_Any_Tooltip")));
        attendanceButtonState.PositionOffset_Y = num2;
        attendanceButtonState.SizeOffset_X = 200f;
        attendanceButtonState.SizeOffset_Y = 30f;
        attendanceButtonState.onSwappedState = onSwappedAttendanceState;
        attendanceButtonState.button.iconColor = ESleekTint.FOREGROUND;
        attendanceButtonState.UseContentTooltip = true;
        attendanceButtonState.AddLabel(localization.format("Attendance_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(attendanceButtonState);
        num2 += attendanceButtonState.SizeOffset_Y + 10f;
        notFullButtonState = new SleekButtonState(20, new GUIContent(localization.format("Any_Space_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Space_Filter_Any_Tooltip")), new GUIContent(localization.format("Space_Button"), icons.load<Texture>("Space"), localization.format("Space_Filter_HasSpace_Tooltip")));
        notFullButtonState.PositionOffset_Y = num2;
        notFullButtonState.SizeOffset_X = 200f;
        notFullButtonState.SizeOffset_Y = 30f;
        notFullButtonState.onSwappedState = OnSwappedNotFullState;
        notFullButtonState.button.iconColor = ESleekTint.FOREGROUND;
        notFullButtonState.UseContentTooltip = true;
        notFullButtonState.AddLabel(localization.format("Space_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(notFullButtonState);
        num2 += notFullButtonState.SizeOffset_Y + 10f;
        combatButtonState = new SleekButtonState(20, new GUIContent(localization.format("PvP_Button"), icons.load<Texture>("PvP"), localization.format("Combat_Filter_PvP_Tooltip")), new GUIContent(localization.format("PvE_Button"), icons.load<Texture>("PvE"), localization.format("Combat_Filter_PvE_Tooltip")), new GUIContent(localization.format("Any_Combat_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Combat_Filter_Any_Tooltip")));
        combatButtonState.PositionOffset_Y = num2;
        combatButtonState.SizeOffset_X = 200f;
        combatButtonState.SizeOffset_Y = 30f;
        combatButtonState.onSwappedState = onSwappedCombatState;
        combatButtonState.button.iconColor = ESleekTint.FOREGROUND;
        combatButtonState.UseContentTooltip = true;
        combatButtonState.AddLabel(localization.format("Combat_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(combatButtonState);
        num2 += combatButtonState.SizeOffset_Y + 10f;
        cameraButtonState = new SleekButtonState(20, new GUIContent(localization.format("First_Button"), icons.load<Texture>("Perspective_FirstPerson"), localization.format("Perspective_Filter_FirstPerson_Tooltip")), new GUIContent(localization.format("Third_Button"), icons.load<Texture>("Perspective_ThirdPerson"), localization.format("Perspective_Filter_ThirdPerson_Tooltip")), new GUIContent(localization.format("Both_Button"), icons.load<Texture>("Perspective_Both"), localization.format("Perspective_Filter_Both_Tooltip")), new GUIContent(localization.format("Vehicle_Button"), icons.load<Texture>("Perspective_Vehicle"), localization.format("Perspective_Filter_Vehicle_Tooltip")), new GUIContent(localization.format("Any_Camera_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Perspective_Filter_Any_Tooltip")));
        cameraButtonState.PositionOffset_Y = num2;
        cameraButtonState.SizeOffset_X = 200f;
        cameraButtonState.SizeOffset_Y = 30f;
        cameraButtonState.onSwappedState = onSwappedCameraState;
        cameraButtonState.button.iconColor = ESleekTint.FOREGROUND;
        cameraButtonState.UseContentTooltip = true;
        cameraButtonState.AddLabel(localization.format("Perspective_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(cameraButtonState);
        num2 += cameraButtonState.SizeOffset_Y + 10f;
        goldFilterButtonState = new SleekButtonState(20, new GUIContent(localization.format("Gold_Filter_Any_Label"), icons.load<Texture>("AnyFilter"), localization.format("Gold_Filter_Any_Tooltip")), new GUIContent(localization.format("Gold_Filter_DoesNotRequireGold_Label"), icons.load<Texture>("GoldNotRequired"), localization.format("Gold_Filter_DoesNotRequireGold_Tooltip")), new GUIContent(localization.format("Gold_Filter_RequiresGold_Label"), icons.load<Texture>("GoldRequired"), localization.format("Gold_Filter_RequiresGold_Tooltip")));
        goldFilterButtonState.PositionOffset_Y = num2;
        goldFilterButtonState.SizeOffset_X = 200f;
        goldFilterButtonState.SizeOffset_Y = 30f;
        goldFilterButtonState.UseContentTooltip = true;
        goldFilterButtonState.onSwappedState = OnSwappedGoldFilterState;
        goldFilterButtonState.button.textColor = Palette.PRO;
        goldFilterButtonState.button.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
        goldFilterButtonState.button.iconColor = Palette.PRO;
        goldFilterButtonState.AddLabel(localization.format("Gold_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(goldFilterButtonState);
        num2 += goldFilterButtonState.SizeOffset_Y + 10f;
        monetizationButtonState = new SleekButtonState(20, new GUIContent(localization.format("Monetization_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Monetization_Filter_Any_Tooltip")), new GUIContent(localization.format("Monetization_None_Button"), icons.load<Texture2D>("Monetization_None"), localization.format("Monetization_Filter_None_Tooltip")), new GUIContent(localization.format("Monetization_NonGameplay_Button"), icons.load<Texture2D>("NonGameplayMonetization"), localization.format("Monetization_Filter_NonGameplay_Tooltip")));
        monetizationButtonState.PositionOffset_Y = num2;
        monetizationButtonState.SizeOffset_X = 200f;
        monetizationButtonState.SizeOffset_Y = 30f;
        monetizationButtonState.onSwappedState = onSwappedMonetizationState;
        monetizationButtonState.button.iconColor = ESleekTint.FOREGROUND;
        monetizationButtonState.UseContentTooltip = true;
        monetizationButtonState.AddLabel(localization.format("Monetization_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(monetizationButtonState);
        num2 += monetizationButtonState.SizeOffset_Y + 10f;
        workshopButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Workshop_Button"), icons.load<Texture2D>("NoMods"), localization.format("Workshop_Filter_No_Tooltip")), new GUIContent(localization.format("Yes_Workshop_Button"), icons.load<Texture2D>("HasMods"), localization.format("Workshop_Filter_Yes_Tooltip")), new GUIContent(localization.format("Any_Workshop_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Workshop_Filter_Any_Tooltip")));
        workshopButtonState.PositionOffset_Y = num2;
        workshopButtonState.SizeOffset_X = 200f;
        workshopButtonState.SizeOffset_Y = 30f;
        workshopButtonState.onSwappedState = onSwappedWorkshopState;
        workshopButtonState.button.iconColor = ESleekTint.FOREGROUND;
        workshopButtonState.UseContentTooltip = true;
        workshopButtonState.AddLabel(localization.format("Workshop_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(workshopButtonState);
        num2 += workshopButtonState.SizeOffset_Y + 10f;
        pluginsButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Plugins_Button"), icons.load<Texture2D>("Plugins_None"), localization.format("Plugins_Filter_No_Tooltip")), new GUIContent(localization.format("Yes_Plugins_Button"), icons.load<Texture2D>("Plugins"), localization.format("Plugins_Filter_Yes_Tooltip")), new GUIContent(localization.format("Any_Plugins_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Plugins_Filter_Any_Tooltip")));
        pluginsButtonState.PositionOffset_Y = num2;
        pluginsButtonState.SizeOffset_X = 200f;
        pluginsButtonState.SizeOffset_Y = 30f;
        pluginsButtonState.onSwappedState = onSwappedPluginsState;
        pluginsButtonState.button.iconColor = ESleekTint.FOREGROUND;
        pluginsButtonState.UseContentTooltip = true;
        pluginsButtonState.AddLabel(localization.format("Plugins_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(pluginsButtonState);
        num2 += pluginsButtonState.SizeOffset_Y + 10f;
        cheatsButtonState = new SleekButtonState(20, new GUIContent(localization.format("No_Cheats_Button"), icons.load<Texture2D>("CheatCodes_None"), localization.format("Cheats_Filter_No_Tooltip")), new GUIContent(localization.format("Yes_Cheats_Button"), icons.load<Texture2D>("CheatCodes"), localization.format("Cheats_Filter_Yes_Tooltip")), new GUIContent(localization.format("Any_Cheats_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("Cheats_Filter_Any_Tooltip")));
        cheatsButtonState.PositionOffset_Y = num2;
        cheatsButtonState.SizeOffset_X = 200f;
        cheatsButtonState.SizeOffset_Y = 30f;
        cheatsButtonState.onSwappedState = onSwappedCheatsState;
        cheatsButtonState.button.iconColor = ESleekTint.FOREGROUND;
        cheatsButtonState.UseContentTooltip = true;
        cheatsButtonState.AddLabel(localization.format("Cheats_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(cheatsButtonState);
        num2 += cheatsButtonState.SizeOffset_Y + 10f;
        VACProtectionButtonState = new SleekButtonState(20, new GUIContent(localization.format("VAC_Secure_Button"), icons.load<Texture>("VAC"), localization.format("VAC_Filter_Secure_Tooltip")), new GUIContent(localization.format("VAC_Insecure_Button"), icons.load<Texture2D>("VAC_Off"), localization.format("VAC_Filter_Insecure_Tooltip")), new GUIContent(localization.format("VAC_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("VAC_Filter_Any_Tooltip")));
        VACProtectionButtonState.PositionOffset_Y = num2;
        VACProtectionButtonState.SizeOffset_X = 200f;
        VACProtectionButtonState.SizeOffset_Y = 30f;
        VACProtectionButtonState.onSwappedState = onSwappedVACProtectionState;
        VACProtectionButtonState.button.iconColor = ESleekTint.FOREGROUND;
        VACProtectionButtonState.UseContentTooltip = true;
        VACProtectionButtonState.AddLabel(localization.format("VAC_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(VACProtectionButtonState);
        num2 += VACProtectionButtonState.SizeOffset_Y + 10f;
        battlEyeProtectionButtonState = new SleekButtonState(20, new GUIContent(localization.format("BattlEye_Secure_Button"), icons.load<Texture>("BattlEye"), localization.format("BattlEye_Filter_Secure_Tooltip")), new GUIContent(localization.format("BattlEye_Insecure_Button"), icons.load<Texture2D>("BattlEye_Off"), localization.format("BattlEye_Filter_Insecure_Tooltip")), new GUIContent(localization.format("BattlEye_Any_Button"), icons.load<Texture2D>("AnyFilter"), localization.format("BattlEye_Filter_Any_Tooltip")));
        battlEyeProtectionButtonState.PositionOffset_Y = num2;
        battlEyeProtectionButtonState.SizeOffset_X = 200f;
        battlEyeProtectionButtonState.SizeOffset_Y = 30f;
        battlEyeProtectionButtonState.onSwappedState = onSwappedBattlEyeProtectionState;
        battlEyeProtectionButtonState.button.iconColor = ESleekTint.FOREGROUND;
        battlEyeProtectionButtonState.UseContentTooltip = true;
        battlEyeProtectionButtonState.AddLabel(localization.format("BattlEye_Filter_Label"), ESleekSide.RIGHT);
        filtersScrollView.AddChild(battlEyeProtectionButtonState);
        num2 += battlEyeProtectionButtonState.SizeOffset_Y + 10f;
        num2 += 10f;
        num2 += 10f;
        presetNameField = Glazier.Get().CreateStringField();
        presetNameField.PositionOffset_Y = num2;
        presetNameField.SizeOffset_X = 200f;
        presetNameField.SizeOffset_Y = 30f;
        presetNameField.PlaceholderText = localization.format("PresetNameField_Hint");
        presetNameField.TooltipText = localization.format("PresetNameField_Tooltip");
        filtersScrollView.AddChild(presetNameField);
        SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(icons.load<Texture2D>("NewPreset"), 20);
        sleekButtonIcon.PositionOffset_X = 200f;
        sleekButtonIcon.PositionOffset_Y = num2;
        sleekButtonIcon.SizeOffset_X = 200f;
        sleekButtonIcon.SizeOffset_Y = 30f;
        sleekButtonIcon.text = localization.format("NewPreset_Label");
        sleekButtonIcon.tooltip = localization.format("NewPreset_Tooltip");
        sleekButtonIcon.onClickedButton += OnClickedCreatePreset;
        sleekButtonIcon.iconColor = ESleekTint.FOREGROUND;
        filtersScrollView.AddChild(sleekButtonIcon);
        num2 += sleekButtonIcon.SizeOffset_Y + 10f;
        deletePresetButton = new SleekButtonIconConfirm(icons.load<Texture2D>("DeletePreset"), localization.format("DeletePreset_Confirm_Label"), localization.format("DeletePreset_Confirm_Tooltip"), localization.format("DeletePreset_Deny_Label"), localization.format("DeletePreset_Deny_Tooltip"));
        deletePresetButton.PositionOffset_X = 100f;
        deletePresetButton.PositionOffset_Y = num2;
        deletePresetButton.SizeOffset_X = 200f;
        deletePresetButton.SizeOffset_Y = 30f;
        deletePresetButton.text = localization.format("DeletePreset_Label");
        deletePresetButton.tooltip = localization.format("DeletePreset_Tooltip");
        deletePresetButton.onConfirmed = OnClickedDeletePreset;
        deletePresetButton.iconColor = ESleekTint.FOREGROUND;
        filtersScrollView.AddChild(deletePresetButton);
        _ = num2 + (deletePresetButton.SizeOffset_Y + 10f);
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
        FilterSettings.OnActiveFiltersModified += OnActiveFiltersModified;
        FilterSettings.OnActiveFiltersReplaced += OnActiveFiltersReplaced;
        FilterSettings.OnCustomPresetsListChanged += OnCustomPresetsListChanged;
        SynchronizePresetTitle();
        SynchronizeFilterButtons();
        SynchronizeDeletePresetButtonVisible();
    }
}
