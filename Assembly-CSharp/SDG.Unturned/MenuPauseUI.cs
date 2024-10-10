using System;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPauseUI
{
    public static Local localization;

    public static Bundle icons;

    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon returnButton;

    private static SleekButtonIconConfirm quitButton;

    private static SleekButtonIcon supportButton;

    private static SleekButtonIcon bskyButton;

    private static SleekButtonIcon steamButton;

    private static SleekButtonIcon creditsButton;

    private static SleekButtonIcon forumButton;

    private static SleekButtonIcon blogButton;

    private static SleekButtonIcon wikiButton;

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

    private static void onClickedSupportButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("https://support.smartlydressedgames.com/hc/en-us");
        }
    }

    private static void onClickedBskyButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("https://bsky.app/profile/nelson.smartlydressed.games");
        }
    }

    private static void onClickedSteamButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("http://steamcommunity.com/app/304930/announcements/");
        }
    }

    private static void onClickedCreditsButton(ISleekElement button)
    {
        close();
        MenuCreditsUI.open();
    }

    private static void onClickedForumButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("https://forum.smartlydressedgames.com/");
        }
    }

    private static void onClickedBlogButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("https://blog.smartlydressedgames.com/");
        }
    }

    private static void onClickedWikiButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("https://unturned.wiki.gg");
        }
    }

    public MenuPauseUI()
    {
        localization = Localization.read("/Menu/MenuPause.dat");
        if (icons != null)
        {
            icons.unload();
            icons = null;
        }
        icons = Bundles.getBundle("/Bundles/Textures/Menu/Icons/MenuPause/MenuPause.unity3d");
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
        quitButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Quit"), localization.format("Exit_Button"), localization.format("Exit_Button_Tooltip"), localization.format("Return_Button"), string.Empty);
        quitButton.PositionOffset_X = -100f;
        quitButton.PositionOffset_Y = -265f;
        quitButton.PositionScale_X = 0.5f;
        quitButton.PositionScale_Y = 0.5f;
        quitButton.SizeOffset_X = 200f;
        quitButton.SizeOffset_Y = 50f;
        quitButton.text = localization.format("Exit_Button");
        quitButton.tooltip = localization.format("Exit_Button_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm = quitButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(onClickedQuitButton));
        quitButton.fontSize = ESleekFontSize.Medium;
        quitButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(quitButton);
        returnButton = new SleekButtonIcon(icons.load<Texture2D>("Return"));
        returnButton.PositionOffset_X = -100f;
        returnButton.PositionOffset_Y = -205f;
        returnButton.PositionScale_X = 0.5f;
        returnButton.PositionScale_Y = 0.5f;
        returnButton.SizeOffset_X = 200f;
        returnButton.SizeOffset_Y = 50f;
        returnButton.text = localization.format("Return_Button");
        returnButton.tooltip = localization.format("Return_Button_Tooltip");
        returnButton.onClickedButton += onClickedReturnButton;
        returnButton.fontSize = ESleekFontSize.Medium;
        returnButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(returnButton);
        supportButton = new SleekButtonIcon(icons.load<Texture2D>("Support"));
        supportButton.PositionOffset_X = -100f;
        supportButton.PositionOffset_Y = -145f;
        supportButton.PositionScale_X = 0.5f;
        supportButton.PositionScale_Y = 0.5f;
        supportButton.SizeOffset_X = 200f;
        supportButton.SizeOffset_Y = 50f;
        supportButton.text = localization.format("Support_Label");
        supportButton.tooltip = localization.format("Support_Tooltip");
        supportButton.onClickedButton += onClickedSupportButton;
        supportButton.fontSize = ESleekFontSize.Medium;
        supportButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(supportButton);
        bskyButton = new SleekButtonIcon(icons.load<Texture2D>("Bsky"), 40);
        bskyButton.PositionOffset_X = -100f;
        bskyButton.PositionOffset_Y = -85f;
        bskyButton.PositionScale_X = 0.5f;
        bskyButton.PositionScale_Y = 0.5f;
        bskyButton.SizeOffset_X = 200f;
        bskyButton.SizeOffset_Y = 50f;
        bskyButton.text = localization.format("Bsky_Button");
        bskyButton.tooltip = localization.format("Bsky_Button_Tooltip");
        bskyButton.onClickedButton += onClickedBskyButton;
        bskyButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(bskyButton);
        steamButton = new SleekButtonIcon(icons.load<Texture2D>("Steam"));
        steamButton.PositionOffset_X = -100f;
        steamButton.PositionOffset_Y = -25f;
        steamButton.PositionScale_X = 0.5f;
        steamButton.PositionScale_Y = 0.5f;
        steamButton.SizeOffset_X = 200f;
        steamButton.SizeOffset_Y = 50f;
        steamButton.text = localization.format("Steam_Button");
        steamButton.tooltip = localization.format("Steam_Button_Tooltip");
        steamButton.onClickedButton += onClickedSteamButton;
        steamButton.fontSize = ESleekFontSize.Medium;
        steamButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(steamButton);
        forumButton = new SleekButtonIcon(icons.load<Texture2D>("Forum"));
        forumButton.PositionOffset_X = -100f;
        forumButton.PositionOffset_Y = 35f;
        forumButton.PositionScale_X = 0.5f;
        forumButton.PositionScale_Y = 0.5f;
        forumButton.SizeOffset_X = 200f;
        forumButton.SizeOffset_Y = 50f;
        forumButton.text = localization.format("Forum_Button");
        forumButton.tooltip = localization.format("Forum_Button_Tooltip");
        forumButton.onClickedButton += onClickedForumButton;
        forumButton.fontSize = ESleekFontSize.Medium;
        forumButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(forumButton);
        blogButton = new SleekButtonIcon(icons.load<Texture2D>("Blog"));
        blogButton.PositionOffset_X = -100f;
        blogButton.PositionOffset_Y = 95f;
        blogButton.PositionScale_X = 0.5f;
        blogButton.PositionScale_Y = 0.5f;
        blogButton.SizeOffset_X = 200f;
        blogButton.SizeOffset_Y = 50f;
        blogButton.text = localization.format("Blog_Button");
        blogButton.tooltip = localization.format("Blog_Button_Tooltip");
        blogButton.onClickedButton += onClickedBlogButton;
        blogButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(blogButton);
        wikiButton = new SleekButtonIcon(icons.load<Texture2D>("Wiki"));
        wikiButton.PositionOffset_X = -100f;
        wikiButton.PositionOffset_Y = 155f;
        wikiButton.PositionScale_X = 0.5f;
        wikiButton.PositionScale_Y = 0.5f;
        wikiButton.SizeOffset_X = 200f;
        wikiButton.SizeOffset_Y = 50f;
        wikiButton.text = localization.format("Wiki_Button");
        wikiButton.tooltip = localization.format("Wiki_Button_Tooltip");
        wikiButton.onClickedButton += onClickedWikiButton;
        wikiButton.fontSize = ESleekFontSize.Medium;
        wikiButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(wikiButton);
        creditsButton = new SleekButtonIcon(icons.load<Texture2D>("Credits"));
        creditsButton.PositionOffset_X = -100f;
        creditsButton.PositionOffset_Y = 215f;
        creditsButton.PositionScale_X = 0.5f;
        creditsButton.PositionScale_Y = 0.5f;
        creditsButton.SizeOffset_X = 200f;
        creditsButton.SizeOffset_Y = 50f;
        creditsButton.text = localization.format("Credits_Button");
        creditsButton.tooltip = localization.format("Credits_Button_Tooltip");
        creditsButton.onClickedButton += onClickedCreditsButton;
        creditsButton.fontSize = ESleekFontSize.Medium;
        creditsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(creditsButton);
    }
}
