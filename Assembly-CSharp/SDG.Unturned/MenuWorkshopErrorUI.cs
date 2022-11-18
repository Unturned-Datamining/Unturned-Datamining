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
        infoBox.isVisible = reportedErrorsList.Count == 0;
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
        sleekBox.text = message;
        return sleekBox;
    }

    public MenuWorkshopErrorUI()
    {
        localization = Localization.read("/Menu/Workshop/MenuWorkshopError.dat");
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
        headerBox = Glazier.Get().CreateBox();
        headerBox.sizeOffset_Y = 50;
        headerBox.sizeScale_X = 1f;
        headerBox.fontSize = ESleekFontSize.Medium;
        headerBox.text = localization.format("Header");
        container.AddChild(headerBox);
        if (ReadWrite.SupportsOpeningFileBrowser)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_X = -210;
            sleekButton.positionOffset_Y = -15;
            sleekButton.positionScale_X = 1f;
            sleekButton.positionScale_Y = 0.5f;
            sleekButton.sizeOffset_X = 200;
            sleekButton.sizeOffset_Y = 30;
            sleekButton.text = localization.format("BrowseLogs_Label");
            sleekButton.tooltipText = localization.format("BrowseLogs_Tooltip");
            sleekButton.onClickedButton += OnClickedBrowseLogs;
            headerBox.AddChild(sleekButton);
        }
        infoBox = Glazier.Get().CreateBox();
        infoBox.positionOffset_Y = 60;
        infoBox.sizeOffset_Y = 50;
        infoBox.sizeScale_X = 1f;
        infoBox.fontSize = ESleekFontSize.Medium;
        infoBox.text = localization.format("No_Errors");
        container.AddChild(infoBox);
        infoBox.isVisible = false;
        errorBox = new SleekList<string>();
        errorBox.positionOffset_Y = 60;
        errorBox.sizeOffset_Y = -120;
        errorBox.sizeScale_X = 1f;
        errorBox.sizeScale_Y = 1f;
        errorBox.itemHeight = 50;
        errorBox.onCreateElement = onCreateErrorMessage;
        errorBox.SetData(Assets.getReportedErrorsList());
        container.AddChild(errorBox);
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
        refreshButton = Glazier.Get().CreateButton();
        refreshButton.positionOffset_X = -200;
        refreshButton.positionOffset_Y = -50;
        refreshButton.positionScale_X = 1f;
        refreshButton.positionScale_Y = 1f;
        refreshButton.sizeOffset_X = 200;
        refreshButton.sizeOffset_Y = 50;
        refreshButton.text = localization.format("Refresh");
        refreshButton.tooltipText = localization.format("Refresh_Tooltip");
        refreshButton.onClickedButton += onClickedRefreshButton;
        refreshButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(refreshButton);
    }
}
