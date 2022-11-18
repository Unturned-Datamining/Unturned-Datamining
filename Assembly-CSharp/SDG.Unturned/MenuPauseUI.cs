using System;
using Steamworks;
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

    private static SleekButtonIcon reportButton;

    private static SleekButtonIcon twitterButton;

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

    private static void onClickedReportButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("http://steamcommunity.com/app/" + SteamUtils.GetAppID().ToString() + "/discussions/9/613936673439628788/");
        }
    }

    private static void onClickedTrelloButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("https://trello.com/b/ezUtMJif");
        }
    }

    private static void onClickedTwitterButton(ISleekElement button)
    {
        if (!Provider.provider.browserService.canOpenBrowser)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Provider.provider.browserService.open("https://twitter.com/SDGNelson");
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
            Provider.provider.browserService.open("https://wiki.smartlydressedgames.com");
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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = -1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        quitButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Quit"), localization.format("Exit_Button"), localization.format("Exit_Button_Tooltip"), localization.format("Return_Button"), string.Empty);
        quitButton.positionOffset_X = -100;
        quitButton.positionOffset_Y = -265;
        quitButton.positionScale_X = 0.5f;
        quitButton.positionScale_Y = 0.5f;
        quitButton.sizeOffset_X = 200;
        quitButton.sizeOffset_Y = 50;
        quitButton.text = localization.format("Exit_Button");
        quitButton.tooltip = localization.format("Exit_Button_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm = quitButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(onClickedQuitButton));
        quitButton.fontSize = ESleekFontSize.Medium;
        quitButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(quitButton);
        returnButton = new SleekButtonIcon(icons.load<Texture2D>("Return"));
        returnButton.positionOffset_X = -100;
        returnButton.positionOffset_Y = -205;
        returnButton.positionScale_X = 0.5f;
        returnButton.positionScale_Y = 0.5f;
        returnButton.sizeOffset_X = 200;
        returnButton.sizeOffset_Y = 50;
        returnButton.text = localization.format("Return_Button");
        returnButton.tooltip = localization.format("Return_Button_Tooltip");
        returnButton.onClickedButton += onClickedReturnButton;
        returnButton.fontSize = ESleekFontSize.Medium;
        returnButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(returnButton);
        reportButton = new SleekButtonIcon(icons.load<Texture2D>("Report"));
        reportButton.positionOffset_X = -100;
        reportButton.positionOffset_Y = -145;
        reportButton.positionScale_X = 0.5f;
        reportButton.positionScale_Y = 0.5f;
        reportButton.sizeOffset_X = 200;
        reportButton.sizeOffset_Y = 50;
        reportButton.text = localization.format("Report_Button");
        reportButton.tooltip = localization.format("Report_Button_Tooltip");
        reportButton.onClickedButton += onClickedReportButton;
        reportButton.fontSize = ESleekFontSize.Medium;
        reportButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(reportButton);
        twitterButton = new SleekButtonIcon(icons.load<Texture2D>("Twitter"));
        twitterButton.positionOffset_X = -100;
        twitterButton.positionOffset_Y = -85;
        twitterButton.positionScale_X = 0.5f;
        twitterButton.positionScale_Y = 0.5f;
        twitterButton.sizeOffset_X = 200;
        twitterButton.sizeOffset_Y = 50;
        twitterButton.text = localization.format("Twitter_Button");
        twitterButton.tooltip = localization.format("Twitter_Button_Tooltip");
        twitterButton.onClickedButton += onClickedTwitterButton;
        twitterButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(twitterButton);
        steamButton = new SleekButtonIcon(icons.load<Texture2D>("Steam"));
        steamButton.positionOffset_X = -100;
        steamButton.positionOffset_Y = -25;
        steamButton.positionScale_X = 0.5f;
        steamButton.positionScale_Y = 0.5f;
        steamButton.sizeOffset_X = 200;
        steamButton.sizeOffset_Y = 50;
        steamButton.text = localization.format("Steam_Button");
        steamButton.tooltip = localization.format("Steam_Button_Tooltip");
        steamButton.onClickedButton += onClickedSteamButton;
        steamButton.fontSize = ESleekFontSize.Medium;
        steamButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(steamButton);
        forumButton = new SleekButtonIcon(icons.load<Texture2D>("Forum"));
        forumButton.positionOffset_X = -100;
        forumButton.positionOffset_Y = 35;
        forumButton.positionScale_X = 0.5f;
        forumButton.positionScale_Y = 0.5f;
        forumButton.sizeOffset_X = 200;
        forumButton.sizeOffset_Y = 50;
        forumButton.text = localization.format("Forum_Button");
        forumButton.tooltip = localization.format("Forum_Button_Tooltip");
        forumButton.onClickedButton += onClickedForumButton;
        forumButton.fontSize = ESleekFontSize.Medium;
        forumButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(forumButton);
        blogButton = new SleekButtonIcon(icons.load<Texture2D>("Blog"));
        blogButton.positionOffset_X = -100;
        blogButton.positionOffset_Y = 95;
        blogButton.positionScale_X = 0.5f;
        blogButton.positionScale_Y = 0.5f;
        blogButton.sizeOffset_X = 200;
        blogButton.sizeOffset_Y = 50;
        blogButton.text = localization.format("Blog_Button");
        blogButton.tooltip = localization.format("Blog_Button_Tooltip");
        blogButton.onClickedButton += onClickedBlogButton;
        blogButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(blogButton);
        wikiButton = new SleekButtonIcon(icons.load<Texture2D>("Wiki"));
        wikiButton.positionOffset_X = -100;
        wikiButton.positionOffset_Y = 155;
        wikiButton.positionScale_X = 0.5f;
        wikiButton.positionScale_Y = 0.5f;
        wikiButton.sizeOffset_X = 200;
        wikiButton.sizeOffset_Y = 50;
        wikiButton.text = localization.format("Wiki_Button");
        wikiButton.tooltip = localization.format("Wiki_Button_Tooltip");
        wikiButton.onClickedButton += onClickedWikiButton;
        wikiButton.fontSize = ESleekFontSize.Medium;
        wikiButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(wikiButton);
        creditsButton = new SleekButtonIcon(icons.load<Texture2D>("Credits"));
        creditsButton.positionOffset_X = -100;
        creditsButton.positionOffset_Y = 215;
        creditsButton.positionScale_X = 0.5f;
        creditsButton.positionScale_Y = 0.5f;
        creditsButton.sizeOffset_X = 200;
        creditsButton.sizeOffset_Y = 50;
        creditsButton.text = localization.format("Credits_Button");
        creditsButton.tooltip = localization.format("Credits_Button_Tooltip");
        creditsButton.onClickedButton += onClickedCreditsButton;
        creditsButton.fontSize = ESleekFontSize.Medium;
        creditsButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(creditsButton);
    }
}
