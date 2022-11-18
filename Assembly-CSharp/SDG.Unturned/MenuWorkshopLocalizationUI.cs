using UnityEngine;

namespace SDG.Unturned;

public class MenuWorkshopLocalizationUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon backButton;

    private static ISleekButton refreshButton;

    private static ISleekBox headerBox;

    private static ISleekBox infoBox;

    private static ISleekScrollView messageBox;

    public static void open()
    {
        if (!active)
        {
            active = true;
            Localization.refresh();
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
        messageBox.RemoveAllChildren();
        for (int i = 0; i < Localization.messages.Count; i++)
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.positionOffset_Y = i * 30;
            sleekBox.sizeOffset_Y = 30;
            sleekBox.sizeScale_X = 1f;
            sleekBox.text = Localization.messages[i];
            messageBox.AddChild(sleekBox);
        }
        messageBox.contentSizeOffset = new Vector2(0f, Localization.messages.Count * 30);
        infoBox.isVisible = Localization.messages.Count == 0;
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuWorkshopUI.open();
        close();
    }

    private static void onClickedRefreshButton(ISleekElement button)
    {
        Localization.refresh();
        refresh();
    }

    public MenuWorkshopLocalizationUI()
    {
        localization = Localization.read("/Menu/Workshop/MenuWorkshopLocalization.dat");
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
        headerBox.text = localization.format("Header", Provider.language, "English");
        container.AddChild(headerBox);
        infoBox = Glazier.Get().CreateBox();
        infoBox.positionOffset_Y = 60;
        infoBox.sizeOffset_Y = 50;
        infoBox.sizeScale_X = 1f;
        infoBox.fontSize = ESleekFontSize.Medium;
        infoBox.text = localization.format("No_Differences");
        container.AddChild(infoBox);
        infoBox.isVisible = false;
        messageBox = Glazier.Get().CreateScrollView();
        messageBox.positionOffset_Y = 60;
        messageBox.sizeOffset_Y = -120;
        messageBox.sizeScale_X = 1f;
        messageBox.sizeScale_Y = 1f;
        messageBox.scaleContentToWidth = true;
        container.AddChild(messageBox);
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
