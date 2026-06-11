using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPauseUI
{
    private class CustomMenuLinkButton : SleekButtonIcon
    {
        public string url;

        public void OnClickedLink(ISleekElement element)
        {
            Provider.provider.browserService.open(url);
        }

        public CustomMenuLinkButton(Texture2D icon, int newSize)
            : base(icon, newSize)
        {
            base.onClickedButton += OnClickedLink;
        }
    }

    public static Local localization;

    public static IconsBundle icons;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon returnButton;

    private static SleekButtonIconConfirm quitButton;

    private static SleekButtonIcon creditsButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(0f, -1f);
        }
    }

    private static void onClickedReturnButton(ISleekElement button)
    {
        close();
        MenuDashboardUI.open();
        MenuTitleUI.open();
    }

    private static void onClickedQuitButton(SleekButtonIconConfirm button)
    {
        Provider.QuitGame("clicked quit in main menu");
    }

    private static void onClickedCreditsButton(ISleekElement button)
    {
        close();
        MenuCreditsUI.open();
    }

    public MenuPauseUI()
    {
        localization = Localization.read("/Menu/MenuPause.dat");
        icons = Bundles.getIconsBundle("UI/Menu/Icons/MenuPause");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = -1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        ISleekElement sleekElement = Glazier.Get().CreateFrame();
        sleekElement.PositionScale_X = 0.5f;
        sleekElement.PositionScale_Y = 0.5f;
        int num = 0;
        quitButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Quit"), localization.format("Exit_Button"), localization.format("Exit_Button_Tooltip"), localization.format("Return_Button"), string.Empty);
        quitButton.PositionOffset_X = -100f;
        quitButton.PositionOffset_Y = num;
        quitButton.PositionScale_X = 0.5f;
        quitButton.SizeOffset_X = 200f;
        quitButton.SizeOffset_Y = 50f;
        quitButton.text = localization.format("Exit_Button");
        quitButton.tooltip = localization.format("Exit_Button_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm = quitButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(onClickedQuitButton));
        quitButton.fontSize = ESleekFontSize.Medium;
        quitButton.iconColor = ESleekTint.FOREGROUND;
        sleekElement.AddChild(quitButton);
        num += 60;
        returnButton = new SleekButtonIcon(icons.load<Texture2D>("Return"));
        returnButton.PositionOffset_X = -100f;
        returnButton.PositionOffset_Y = num;
        returnButton.PositionScale_X = 0.5f;
        returnButton.SizeOffset_X = 200f;
        returnButton.SizeOffset_Y = 50f;
        returnButton.text = localization.format("Return_Button");
        returnButton.tooltip = localization.format("Return_Button_Tooltip");
        returnButton.onClickedButton += onClickedReturnButton;
        returnButton.fontSize = ESleekFontSize.Medium;
        returnButton.iconColor = ESleekTint.FOREGROUND;
        sleekElement.AddChild(returnButton);
        num += 60;
        foreach (CustomMenuLink custom_Menu_Link in Provider.statusData.Menu.Custom_Menu_Links)
        {
            sleekElement.AddChild(new CustomMenuLinkButton(icons.load<Texture2D>(custom_Menu_Link.Icon), 40)
            {
                PositionOffset_X = -100f,
                PositionOffset_Y = num,
                PositionScale_X = 0.5f,
                SizeOffset_X = 200f,
                SizeOffset_Y = 50f,
                text = localization.format(custom_Menu_Link.Label_Key),
                tooltip = localization.format(custom_Menu_Link.Tooltip_Key),
                url = custom_Menu_Link.Web_Link,
                fontSize = ESleekFontSize.Medium,
                iconColor = ESleekTint.FOREGROUND
            });
            num += 60;
        }
        creditsButton = new SleekButtonIcon(icons.load<Texture2D>("Credits"));
        creditsButton.PositionOffset_X = -100f;
        creditsButton.PositionOffset_Y = num;
        creditsButton.PositionScale_X = 0.5f;
        creditsButton.SizeOffset_X = 200f;
        creditsButton.SizeOffset_Y = 50f;
        creditsButton.text = localization.format("Credits_Button");
        creditsButton.tooltip = localization.format("Credits_Button_Tooltip");
        creditsButton.onClickedButton += onClickedCreditsButton;
        creditsButton.fontSize = ESleekFontSize.Medium;
        creditsButton.iconColor = ESleekTint.FOREGROUND;
        sleekElement.AddChild(creditsButton);
        num += 60;
        sleekElement.SizeOffset_Y = num - 10;
        sleekElement.PositionOffset_Y = sleekElement.SizeOffset_Y * -0.5f;
        container.AddChild(sleekElement);
    }
}
