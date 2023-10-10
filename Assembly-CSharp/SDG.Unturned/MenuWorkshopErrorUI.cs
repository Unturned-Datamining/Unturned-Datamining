using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class MenuWorkshopErrorUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekButton refreshButton;

    private static ISleekBox headerBox;

    private static ISleekBox infoBox;

    private static SleekList<string> errorBox;

    public static void open()
    {
        if (!active)
        {
            active = true;
            refresh();
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void refresh()
    {
        errorBox.NotifyDataChanged();
        List<string> reportedErrorsList = Assets.getReportedErrorsList();
        infoBox.IsVisible = reportedErrorsList.Count == 0;
    }

    private static void OnClickedBrowseLogs(ISleekElement button)
    {
        ReadWrite.OpenFileBrowser(ReadWrite.folderPath(Logs.getLogFilePath()));
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuWorkshopUI.open();
        close();
    }

    private static void onClickedRefreshButton(ISleekElement button)
    {
        refresh();
    }

    private static ISleekElement onCreateErrorMessage(string message)
    {
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.Text = message;
        return sleekBox;
    }

    public MenuWorkshopErrorUI()
    {
        localization = Localization.read("/Menu/Workshop/MenuWorkshopError.dat");
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
        headerBox = Glazier.Get().CreateBox();
        headerBox.SizeOffset_Y = 50f;
        headerBox.SizeScale_X = 1f;
        headerBox.FontSize = ESleekFontSize.Medium;
        headerBox.Text = localization.format("Header");
        container.AddChild(headerBox);
        if (ReadWrite.SupportsOpeningFileBrowser)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = -210f;
            sleekButton.PositionOffset_Y = -15f;
            sleekButton.PositionScale_X = 1f;
            sleekButton.PositionScale_Y = 0.5f;
            sleekButton.SizeOffset_X = 200f;
            sleekButton.SizeOffset_Y = 30f;
            sleekButton.Text = localization.format("BrowseLogs_Label");
            sleekButton.TooltipText = localization.format("BrowseLogs_Tooltip");
            sleekButton.OnClicked += OnClickedBrowseLogs;
            headerBox.AddChild(sleekButton);
        }
        infoBox = Glazier.Get().CreateBox();
        infoBox.PositionOffset_Y = 60f;
        infoBox.SizeOffset_Y = 50f;
        infoBox.SizeScale_X = 1f;
        infoBox.FontSize = ESleekFontSize.Medium;
        infoBox.Text = localization.format("No_Errors");
        container.AddChild(infoBox);
        infoBox.IsVisible = false;
        errorBox = new SleekList<string>();
        errorBox.PositionOffset_Y = 60f;
        errorBox.SizeOffset_Y = -120f;
        errorBox.SizeScale_X = 1f;
        errorBox.SizeScale_Y = 1f;
        errorBox.itemHeight = 50;
        errorBox.onCreateElement = onCreateErrorMessage;
        errorBox.SetData(Assets.getReportedErrorsList());
        container.AddChild(errorBox);
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
        refreshButton = Glazier.Get().CreateButton();
        refreshButton.PositionOffset_X = -200f;
        refreshButton.PositionOffset_Y = -50f;
        refreshButton.PositionScale_X = 1f;
        refreshButton.PositionScale_Y = 1f;
        refreshButton.SizeOffset_X = 200f;
        refreshButton.SizeOffset_Y = 50f;
        refreshButton.Text = localization.format("Refresh");
        refreshButton.TooltipText = localization.format("Refresh_Tooltip");
        refreshButton.OnClicked += onClickedRefreshButton;
        refreshButton.FontSize = ESleekFontSize.Medium;
        container.AddChild(refreshButton);
    }
}
