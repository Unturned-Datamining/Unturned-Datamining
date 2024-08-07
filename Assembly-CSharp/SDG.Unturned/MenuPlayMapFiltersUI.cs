using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayMapFiltersUI : SleekFullscreenBox
{
    public Local localization;

    public Bundle icons;

    public bool active;

    private EMenuPlayMapFiltersUIOpenContext openContext;

    private LevelInfo[] levels;

    private SleekFilterLevel[] levelButtons;

    private int previousLayoutWidth = -1;

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
            int widthForLayout = ScreenEx.GetWidthForLayout();
            if (levels == null || levels.Length < 1 || widthForLayout != previousLayoutWidth)
            {
                PopulateLevelButtons();
            }
            UpdateFiltersLabel();
            SynchronizeCheckBoxes();
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
        SynchronizeCheckBoxes();
    }

    private void OnClickedBackButton(ISleekElement button)
    {
        OpenPreviousMenu();
        close();
    }

    private void OnClickedLevel(SleekLevel levelButton, byte index)
    {
        bool isIncludedInFilter = FilterSettings.activeFilters.ToggleMap(levelButton.level);
        ((SleekFilterLevel)levelButton).IsIncludedInFilter = isIncludedInFilter;
        FilterSettings.MarkActiveFilterModified();
        UpdateFiltersLabel();
    }

    private void PopulateLevelButtons()
    {
        int num = (previousLayoutWidth = ScreenEx.GetWidthForLayout());
        levelScrollBox.RemoveAllChildren();
        levels = Level.getLevels(ESingleplayerMapCategory.ALL);
        int num2 = Mathf.Max(1, (num - 200) / 410);
        int num3 = 0;
        int num4 = 0;
        levelButtons = new SleekFilterLevel[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                SleekFilterLevel sleekFilterLevel = new SleekFilterLevel(levels[i]);
                sleekFilterLevel.PositionOffset_X = num3 % num2 * 410;
                sleekFilterLevel.PositionOffset_Y = num3 / num2 * 110;
                sleekFilterLevel.onClickedLevel = (ClickedLevel)Delegate.Combine(sleekFilterLevel.onClickedLevel, new ClickedLevel(OnClickedLevel));
                levelScrollBox.AddChild(sleekFilterLevel);
                num4 += 110;
                levelButtons[i] = sleekFilterLevel;
                num3++;
            }
        }
        float num5 = MathfEx.GetPageCount(num3, num2) * 110;
        levelScrollBox.ContentSizeOffset = new Vector2(0f, num5 - 10f);
        int num6 = num2 * 410 - 10;
        headerBox.PositionOffset_X = -num6 / 2;
        headerBox.SizeOffset_X = num6;
        resetButton.PositionOffset_X = -num6 / 2;
        resetButton.SizeOffset_X = num6;
        levelScrollBox.PositionOffset_X = -num6 / 2;
        levelScrollBox.SizeOffset_X = num6 + 30;
    }

    private void SynchronizeCheckBoxes()
    {
        if (levelButtons != null)
        {
            List<LevelInfo> list = new List<LevelInfo>();
            FilterSettings.activeFilters.GetLevels(list);
            SleekFilterLevel[] array = levelButtons;
            foreach (SleekFilterLevel sleekFilterLevel in array)
            {
                bool isIncludedInFilter = list.Contains(sleekFilterLevel.level);
                sleekFilterLevel.IsIncludedInFilter = isIncludedInFilter;
            }
        }
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
        headerBox.PositionOffset_Y = 100f;
        headerBox.PositionScale_X = 0.5f;
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
        resetButton.PositionOffset_Y = 210f;
        resetButton.PositionScale_X = 0.5f;
        resetButton.SizeOffset_Y = 50f;
        resetButton.Text = localization.format("MapFilter_ResetButton_Label");
        resetButton.TooltipText = localization.format("MapFilter_ResetButton_Tooltip");
        resetButton.FontSize = ESleekFontSize.Medium;
        resetButton.OnClicked += OnClickedResetButton;
        AddChild(resetButton);
        levelScrollBox = Glazier.Get().CreateScrollView();
        levelScrollBox.PositionOffset_Y = 270f;
        levelScrollBox.PositionScale_X = 0.5f;
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
