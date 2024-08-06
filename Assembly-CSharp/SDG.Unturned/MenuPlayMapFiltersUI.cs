using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayMapFiltersUI : SleekFullscreenBox
{
    public Local localization;

    public Bundle icons;

    public bool active;

    private EMenuPlayMapFiltersUIOpenContext openContext;

    private LevelInfo[] levels;

    private SleekLevel[] levelButtons;

    private ISleekBox headerBox;

    private ISleekLabel titleLabel;

    private ISleekLabel filtersLabel;

    private ISleekButton resetButton;

    private ISleekScrollView levelScrollBox;

    private SleekButtonIcon backButton;

    public void open(EMenuPlayMapFiltersUIOpenContext openContext)
    {
        if (!active)
        {
            active = true;
            this.openContext = openContext;
            if (levels == null || levels.Length < 1)
            {
                PopulateLevelButtons();
            }
            UpdateFiltersLabel();
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

    public void OpenPreviousMenu()
    {
        switch (openContext)
        {
        case EMenuPlayMapFiltersUIOpenContext.ServerList:
            MenuPlayUI.serverListUI.open(shouldRefresh: true);
            break;
        case EMenuPlayMapFiltersUIOpenContext.Filters:
            MenuPlayServersUI.serverListFiltersUI.open();
            break;
        }
    }

    public override void OnDestroy()
    {
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Remove(Level.onLevelsRefreshed, new LevelsRefreshed(OnLevelsRefreshed));
        base.OnDestroy();
    }

    private void UpdateFiltersLabel()
    {
        string mapDisplayText = FilterSettings.activeFilters.GetMapDisplayText();
        if (string.IsNullOrEmpty(mapDisplayText))
        {
            filtersLabel.Text = localization.format("MapFilter_Button_EmptyLabel");
            resetButton.IsClickable = false;
        }
        else
        {
            filtersLabel.Text = mapDisplayText;
            resetButton.IsClickable = true;
        }
    }

    private void OnClickedResetButton(ISleekElement button)
    {
        FilterSettings.activeFilters.ClearMaps();
        FilterSettings.MarkActiveFilterModified();
        UpdateFiltersLabel();
    }

    private void OnClickedBackButton(ISleekElement button)
    {
        OpenPreviousMenu();
        close();
    }

    private void OnClickedLevel(SleekLevel levelButton, byte index)
    {
        LevelInfo levelInfo = levels[index];
        FilterSettings.activeFilters.ToggleMap(levelInfo);
        FilterSettings.MarkActiveFilterModified();
        UpdateFiltersLabel();
    }

    private void PopulateLevelButtons()
    {
        levelScrollBox.RemoveAllChildren();
        levels = Level.getLevels(ESingleplayerMapCategory.ALL);
        int num = 0;
        levelButtons = new SleekLevel[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                SleekLevel sleekLevel = new SleekLevel(levels[i], isEditor: false);
                sleekLevel.PositionOffset_Y = num;
                sleekLevel.onClickedLevel = (ClickedLevel)Delegate.Combine(sleekLevel.onClickedLevel, new ClickedLevel(OnClickedLevel));
                levelScrollBox.AddChild(sleekLevel);
                num += 110;
                levelButtons[i] = sleekLevel;
            }
        }
        levelScrollBox.ContentSizeOffset = new Vector2(0f, num - 10);
    }

    private void OnLevelsRefreshed()
    {
        PopulateLevelButtons();
    }

    public MenuPlayMapFiltersUI(MenuPlayServersUI serverListUI)
    {
        localization = serverListUI.localization;
        icons = serverListUI.icons;
        active = false;
        headerBox = Glazier.Get().CreateBox();
        headerBox.PositionOffset_X = -200f;
        headerBox.PositionOffset_Y = 100f;
        headerBox.PositionScale_X = 0.5f;
        headerBox.SizeOffset_X = 400f;
        headerBox.SizeOffset_Y = 100f;
        headerBox.TooltipText = localization.format("MapFilter_Header_Tooltip");
        AddChild(headerBox);
        titleLabel = Glazier.Get().CreateLabel();
        titleLabel.SizeScale_X = 1f;
        titleLabel.SizeOffset_Y = 50f;
        titleLabel.Text = localization.format("MapFilter_Header_Label");
        titleLabel.FontSize = ESleekFontSize.Medium;
        headerBox.AddChild(titleLabel);
        filtersLabel = Glazier.Get().CreateLabel();
        filtersLabel.SizeScale_X = 1f;
        filtersLabel.PositionOffset_Y = 50f;
        filtersLabel.SizeOffset_Y = 50f;
        headerBox.AddChild(filtersLabel);
        resetButton = Glazier.Get().CreateButton();
        resetButton.PositionOffset_X = -200f;
        resetButton.PositionOffset_Y = 210f;
        resetButton.PositionScale_X = 0.5f;
        resetButton.SizeOffset_X = 400f;
        resetButton.SizeOffset_Y = 50f;
        resetButton.Text = localization.format("MapFilter_ResetButton_Label");
        resetButton.TooltipText = localization.format("MapFilter_ResetButton_Tooltip");
        resetButton.FontSize = ESleekFontSize.Medium;
        resetButton.OnClicked += OnClickedResetButton;
        AddChild(resetButton);
        levelScrollBox = Glazier.Get().CreateScrollView();
        levelScrollBox.PositionOffset_X = -200f;
        levelScrollBox.PositionOffset_Y = 270f;
        levelScrollBox.PositionScale_X = 0.5f;
        levelScrollBox.SizeOffset_X = 430f;
        levelScrollBox.SizeOffset_Y = -370f;
        levelScrollBox.SizeScale_Y = 1f;
        levelScrollBox.ScaleContentToWidth = true;
        AddChild(levelScrollBox);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += OnClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        AddChild(backButton);
        Level.onLevelsRefreshed = (LevelsRefreshed)Delegate.Combine(Level.onLevelsRefreshed, new LevelsRefreshed(OnLevelsRefreshed));
    }
}
