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
            sleekBox.PositionOffset_Y = i * 30;
            sleekBox.SizeOffset_Y = 30f;
            sleekBox.SizeScale_X = 1f;
            sleekBox.Text = Localization.messages[i];
            messageBox.AddChild(sleekBox);
        }
        messageBox.ContentSizeOffset = new Vector2(0f, Localization.messages.Count * 30);
        infoBox.IsVisible = Localization.messages.Count == 0;
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
        headerBox.Text = localization.format("Header", Provider.language, "English");
        container.AddChild(headerBox);
        infoBox = Glazier.Get().CreateBox();
        infoBox.PositionOffset_Y = 60f;
        infoBox.SizeOffset_Y = 50f;
        infoBox.SizeScale_X = 1f;
        infoBox.FontSize = ESleekFontSize.Medium;
        infoBox.Text = localization.format("No_Differences");
        container.AddChild(infoBox);
        infoBox.IsVisible = false;
        messageBox = Glazier.Get().CreateScrollView();
        messageBox.PositionOffset_Y = 60f;
        messageBox.SizeOffset_Y = -120f;
        messageBox.SizeScale_X = 1f;
        messageBox.SizeScale_Y = 1f;
        messageBox.ScaleContentToWidth = true;
        container.AddChild(messageBox);
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
