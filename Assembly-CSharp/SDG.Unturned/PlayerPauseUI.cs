using System;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerPauseUI
{
    private static SleekFullscreenBox container;

    public static Local localization;

    private static Bundle icons;

    public static bool active;

    private static SleekButtonIcon returnButton;

    private static SleekButtonIcon optionsButton;

    private static SleekButtonIcon displayButton;

    private static SleekButtonIcon graphicsButton;

    private static SleekButtonIcon controlsButton;

    public static SleekButtonIconConfirm exitButton;

    public static SleekButtonIconConfirm quitButton;

    private static SleekButtonIconConfirm suicideButton;

    private static ISleekLabel suicideDisabledLabel;

    private static ISleekBox spyBox;

    private static ISleekImage spyImage;

    private static ISleekButton spyRefreshButton;

    private static ISleekButton spySlayButton;

    private static ISleekBox serverBox;

    private static SleekButtonIcon favoriteButton;

    private static ISleekButton quicksaveButton;

    private static CSteamID spySteamID;

    public static float lastLeave;

    public static bool shouldExitButtonRespectTimer
    {
        get
        {
            if (Provider.isServer)
            {
                return false;
            }
            if (!Provider.isPvP)
            {
                return false;
            }
            if (Provider.clients.Count < 2)
            {
                return false;
            }
            if (Player.player == null)
            {
                return false;
            }
            if (Player.player.life.isDead)
            {
                return false;
            }
            if (Player.player.movement.isSafe && Player.player.movement.isSafeInfo.noWeapons)
            {
                return false;
            }
            return true;
        }
    }

    public static void open()
    {
        if (!active)
        {
            active = true;
            lastLeave = Time.realtimeSinceStartup;
            if (Provider.currentServerInfo != null && Level.info != null)
            {
                string localizedName = Level.info.getLocalizedName();
                string text = ((!Provider.currentServerInfo.IsVACSecure) ? localization.format("VAC_Insecure") : localization.format("VAC_Secure"));
                text = ((!Provider.currentServerInfo.IsBattlEyeSecure) ? (text + " + " + localization.format("BattlEye_Insecure")) : (text + " + " + localization.format("BattlEye_Secure")));
                serverBox.text = localization.format("Server_WithVersion", localizedName, Level.version, OptionsSettings.streamer ? localization.format("Streamer") : Provider.currentServerInfo.name, text);
            }
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            exitButton.reset();
            quitButton.reset();
            suicideButton.reset();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public static void closeAndGotoAppropriateHUD()
    {
        close();
        if (Player.player.life.isDead)
        {
            PlayerDeathUI.open(fromDeath: false);
        }
        else
        {
            PlayerLifeUI.open();
        }
    }

    private static void onClickedReturnButton(ISleekElement button)
    {
        closeAndGotoAppropriateHUD();
    }

    private static void onClickedOptionsButton(ISleekElement button)
    {
        close();
        MenuConfigurationOptionsUI.open();
    }

    private static void onClickedDisplayButton(ISleekElement button)
    {
        close();
        MenuConfigurationDisplayUI.open();
    }

    private static void onClickedGraphicsButton(ISleekElement button)
    {
        close();
        MenuConfigurationGraphicsUI.open();
    }

    private static void onClickedControlsButton(ISleekElement button)
    {
        close();
        MenuConfigurationControlsUI.open();
    }

    private static void onClickedSpyRefreshButton(ISleekElement button)
    {
        CSteamID cSteamID = spySteamID;
        ChatManager.sendChat(EChatMode.GLOBAL, "/spy " + cSteamID.ToString());
    }

    private static void onClickedSpySlayButton(ISleekElement button)
    {
        CSteamID cSteamID = spySteamID;
        ChatManager.sendChat(EChatMode.GLOBAL, "/slay " + cSteamID.ToString() + "/Screenshot Evidence");
    }

    private static void onClickedExitButton(SleekButtonIconConfirm button)
    {
        if (!shouldExitButtonRespectTimer || !(Time.realtimeSinceStartup - lastLeave < (float)Provider.modeConfigData.Gameplay.Timer_Exit))
        {
            Provider.RequestDisconnect("clicked exit button from in-game pause menu");
        }
    }

    private static void onClickedQuitButton(SleekButtonIconConfirm button)
    {
        if (!shouldExitButtonRespectTimer || !(Time.realtimeSinceStartup - lastLeave < (float)Provider.modeConfigData.Gameplay.Timer_Exit))
        {
            Provider.QuitGame("clicked quit from in-game pause menu");
        }
    }

    private static void onClickedSuicideButton(SleekButtonIconConfirm button)
    {
        if (((Level.info != null && Level.info.type == ELevelType.SURVIVAL) || !Player.player.movement.isSafe || !Player.player.movement.isSafeInfo.noWeapons) && Provider.modeConfigData.Gameplay.Can_Suicide)
        {
            closeAndGotoAppropriateHUD();
            Player.player.life.sendSuicide();
        }
    }

    private static void onClickedFavoriteButton(ISleekElement button)
    {
        Provider.toggleCurrentServerFavorited();
        updateFavorite();
    }

    private static void onClickedQuicksaveButton(ISleekElement button)
    {
        SaveManager.save();
    }

    private static void updateFavorite()
    {
        if (Provider.isCurrentServerFavorited)
        {
            favoriteButton.text = localization.format("Favorite_Off_Button_Text");
            favoriteButton.icon = icons.load<Texture2D>("Favorite_Off");
        }
        else
        {
            favoriteButton.text = localization.format("Favorite_On_Button_Text");
            favoriteButton.icon = icons.load<Texture2D>("Favorite_On");
        }
    }

    private static void onSpyReady(CSteamID steamID, byte[] data)
    {
        spySteamID = steamID;
        Texture2D texture2D = new Texture2D(640, 480, TextureFormat.RGB24, mipChain: false);
        texture2D.name = "Spy";
        texture2D.filterMode = FilterMode.Trilinear;
        texture2D.hideFlags = HideFlags.HideAndDontSave;
        texture2D.LoadImage(data, markNonReadable: true);
        spyImage.texture = texture2D;
        returnButton.positionOffset_X = -435;
        optionsButton.positionOffset_X = -435;
        displayButton.positionOffset_X = -435;
        graphicsButton.positionOffset_X = -435;
        controlsButton.positionOffset_X = -435;
        exitButton.positionOffset_X = -435;
        quitButton.positionOffset_X = -435;
        suicideButton.positionOffset_X = -435;
        spyBox.positionOffset_X = -225;
        spyBox.isVisible = true;
    }

    internal void OnDestroy()
    {
        ClientMessageHandler_Accepted.OnGameplayConfigReceived -= OnGameplayConfigReceived;
    }

    private void OnGameplayConfigReceived()
    {
        SyncSuicideButtonAvailable();
    }

    private void SyncSuicideButtonAvailable()
    {
        bool can_Suicide = Provider.modeConfigData.Gameplay.Can_Suicide;
        suicideButton.isClickable = can_Suicide;
        suicideDisabledLabel.isVisible = !can_Suicide;
    }

    public PlayerPauseUI()
    {
        localization = Localization.read("/Player/PlayerPause.dat");
        if (icons != null)
        {
            icons.unload();
            icons = null;
        }
        icons = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerPause/PlayerPause.unity3d");
        container = new SleekFullscreenBox();
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        returnButton = new SleekButtonIcon(icons.load<Texture2D>("Return"));
        returnButton.positionOffset_X = -100;
        returnButton.positionOffset_Y = -205;
        returnButton.positionScale_X = 0.5f;
        returnButton.positionScale_Y = 0.5f;
        returnButton.sizeOffset_X = 200;
        returnButton.sizeOffset_Y = 50;
        returnButton.text = localization.format("Return_Button_Text");
        returnButton.tooltip = localization.format("Return_Button_Tooltip");
        returnButton.iconColor = ESleekTint.FOREGROUND;
        returnButton.onClickedButton += onClickedReturnButton;
        returnButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(returnButton);
        optionsButton = new SleekButtonIcon(icons.load<Texture2D>("Options"));
        optionsButton.positionOffset_X = -100;
        optionsButton.positionOffset_Y = -145;
        optionsButton.positionScale_X = 0.5f;
        optionsButton.positionScale_Y = 0.5f;
        optionsButton.sizeOffset_X = 200;
        optionsButton.sizeOffset_Y = 50;
        optionsButton.text = localization.format("Options_Button_Text");
        optionsButton.tooltip = localization.format("Options_Button_Tooltip");
        optionsButton.onClickedButton += onClickedOptionsButton;
        optionsButton.iconColor = ESleekTint.FOREGROUND;
        optionsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(optionsButton);
        displayButton = new SleekButtonIcon(icons.load<Texture2D>("Display"));
        displayButton.positionOffset_X = -100;
        displayButton.positionOffset_Y = -85;
        displayButton.positionScale_X = 0.5f;
        displayButton.positionScale_Y = 0.5f;
        displayButton.sizeOffset_X = 200;
        displayButton.sizeOffset_Y = 50;
        displayButton.text = localization.format("Display_Button_Text");
        displayButton.tooltip = localization.format("Display_Button_Tooltip");
        displayButton.iconColor = ESleekTint.FOREGROUND;
        displayButton.onClickedButton += onClickedDisplayButton;
        displayButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(displayButton);
        graphicsButton = new SleekButtonIcon(icons.load<Texture2D>("Graphics"));
        graphicsButton.positionOffset_X = -100;
        graphicsButton.positionOffset_Y = -25;
        graphicsButton.positionScale_X = 0.5f;
        graphicsButton.positionScale_Y = 0.5f;
        graphicsButton.sizeOffset_X = 200;
        graphicsButton.sizeOffset_Y = 50;
        graphicsButton.text = localization.format("Graphics_Button_Text");
        graphicsButton.tooltip = localization.format("Graphics_Button_Tooltip");
        graphicsButton.iconColor = ESleekTint.FOREGROUND;
        graphicsButton.onClickedButton += onClickedGraphicsButton;
        graphicsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(graphicsButton);
        controlsButton = new SleekButtonIcon(icons.load<Texture2D>("Controls"));
        controlsButton.positionOffset_X = -100;
        controlsButton.positionOffset_Y = 35;
        controlsButton.positionScale_X = 0.5f;
        controlsButton.positionScale_Y = 0.5f;
        controlsButton.sizeOffset_X = 200;
        controlsButton.sizeOffset_Y = 50;
        controlsButton.text = localization.format("Controls_Button_Text");
        controlsButton.tooltip = localization.format("Controls_Button_Tooltip");
        controlsButton.iconColor = ESleekTint.FOREGROUND;
        controlsButton.onClickedButton += onClickedControlsButton;
        controlsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(controlsButton);
        exitButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Exit"), localization.format("Exit_Button_Text"), localization.format("Exit_Button_Tooltip"), localization.format("Return_Button_Text"), string.Empty);
        exitButton.positionOffset_X = -100;
        exitButton.positionOffset_Y = 155;
        exitButton.positionScale_X = 0.5f;
        exitButton.positionScale_Y = 0.5f;
        exitButton.sizeOffset_X = 200;
        exitButton.sizeOffset_Y = 50;
        exitButton.text = localization.format("Exit_Button_Text");
        exitButton.tooltip = localization.format("Exit_Button_Tooltip");
        exitButton.iconColor = ESleekTint.FOREGROUND;
        SleekButtonIconConfirm sleekButtonIconConfirm = exitButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(onClickedExitButton));
        exitButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(exitButton);
        quitButton = new SleekButtonIconConfirm(MenuPauseUI.icons.load<Texture2D>("Quit"), localization.format("Quit_Button"), localization.format("Quit_Button_Tooltip"), localization.format("Return_Button_Text"), string.Empty);
        quitButton.positionOffset_X = -100;
        quitButton.positionOffset_Y = 215;
        quitButton.positionScale_X = 0.5f;
        quitButton.positionScale_Y = 0.5f;
        quitButton.sizeOffset_X = 200;
        quitButton.sizeOffset_Y = 50;
        quitButton.text = localization.format("Quit_Button");
        quitButton.tooltip = localization.format("Quit_Button_Tooltip");
        quitButton.iconColor = ESleekTint.FOREGROUND;
        SleekButtonIconConfirm sleekButtonIconConfirm2 = quitButton;
        sleekButtonIconConfirm2.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm2.onConfirmed, new Confirm(onClickedQuitButton));
        quitButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(quitButton);
        suicideButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Suicide"), localization.format("Suicide_Button_Confirm"), localization.format("Suicide_Button_Confirm_Tooltip"), localization.format("Suicide_Button_Deny"), localization.format("Suicide_Button_Deny_Tooltip"));
        suicideButton.positionOffset_X = -100;
        suicideButton.positionOffset_Y = 95;
        suicideButton.positionScale_X = 0.5f;
        suicideButton.positionScale_Y = 0.5f;
        suicideButton.sizeOffset_X = 200;
        suicideButton.sizeOffset_Y = 50;
        suicideButton.text = localization.format("Suicide_Button_Text");
        suicideButton.tooltip = localization.format("Suicide_Button_Tooltip");
        suicideButton.iconColor = ESleekTint.FOREGROUND;
        suicideButton.onConfirmed = onClickedSuicideButton;
        suicideButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(suicideButton);
        suicideDisabledLabel = Glazier.Get().CreateLabel();
        suicideDisabledLabel.positionOffset_X = -100;
        suicideDisabledLabel.positionOffset_Y = 95;
        suicideDisabledLabel.positionScale_X = 0.5f;
        suicideDisabledLabel.positionScale_Y = 0.5f;
        suicideDisabledLabel.sizeOffset_X = 200;
        suicideDisabledLabel.sizeOffset_Y = 50;
        suicideDisabledLabel.text = localization.format("Suicide_Disabled");
        suicideDisabledLabel.textColor = ESleekTint.BAD;
        suicideDisabledLabel.fontSize = ESleekFontSize.Large;
        suicideDisabledLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        suicideDisabledLabel.isVisible = false;
        container.AddChild(suicideDisabledLabel);
        spyBox = Glazier.Get().CreateBox();
        spyBox.positionOffset_Y = -310;
        spyBox.positionScale_X = 0.5f;
        spyBox.positionScale_Y = 0.5f;
        spyBox.sizeOffset_X = 660;
        spyBox.sizeOffset_Y = 500;
        container.AddChild(spyBox);
        spyBox.isVisible = false;
        spyImage = Glazier.Get().CreateImage();
        spyImage.positionOffset_X = 10;
        spyImage.positionOffset_Y = 10;
        spyImage.sizeOffset_X = 640;
        spyImage.sizeOffset_Y = 480;
        spyBox.AddChild(spyImage);
        spyRefreshButton = Glazier.Get().CreateButton();
        spyRefreshButton.positionOffset_X = -205;
        spyRefreshButton.positionOffset_Y = 10;
        spyRefreshButton.positionScale_X = 0.5f;
        spyRefreshButton.positionScale_Y = 1f;
        spyRefreshButton.sizeOffset_X = 200;
        spyRefreshButton.sizeOffset_Y = 50;
        spyRefreshButton.text = localization.format("Spy_Refresh_Button_Text");
        spyRefreshButton.tooltipText = localization.format("Spy_Refresh_Button_Tooltip");
        spyRefreshButton.onClickedButton += onClickedSpyRefreshButton;
        spyRefreshButton.fontSize = ESleekFontSize.Medium;
        spyBox.AddChild(spyRefreshButton);
        spySlayButton = Glazier.Get().CreateButton();
        spySlayButton.positionOffset_X = 5;
        spySlayButton.positionOffset_Y = 10;
        spySlayButton.positionScale_X = 0.5f;
        spySlayButton.positionScale_Y = 1f;
        spySlayButton.sizeOffset_X = 200;
        spySlayButton.sizeOffset_Y = 50;
        spySlayButton.text = localization.format("Spy_Slay_Button_Text");
        spySlayButton.tooltipText = localization.format("Spy_Slay_Button_Tooltip");
        spySlayButton.onClickedButton += onClickedSpySlayButton;
        spySlayButton.fontSize = ESleekFontSize.Medium;
        spyBox.AddChild(spySlayButton);
        serverBox = Glazier.Get().CreateBox();
        serverBox.positionOffset_Y = -50;
        serverBox.positionScale_Y = 1f;
        serverBox.sizeOffset_X = -5;
        serverBox.sizeOffset_Y = 50;
        serverBox.sizeScale_X = 0.75f;
        serverBox.fontSize = ESleekFontSize.Medium;
        container.AddChild(serverBox);
        favoriteButton = new SleekButtonIcon(null);
        favoriteButton.positionScale_X = 0.75f;
        favoriteButton.positionOffset_Y = -50;
        favoriteButton.positionOffset_X = 5;
        favoriteButton.positionScale_Y = 1f;
        favoriteButton.sizeOffset_X = -5;
        favoriteButton.sizeOffset_Y = 50;
        favoriteButton.sizeScale_X = 0.25f;
        favoriteButton.tooltip = localization.format("Favorite_Button_Tooltip");
        favoriteButton.fontSize = ESleekFontSize.Medium;
        favoriteButton.onClickedButton += onClickedFavoriteButton;
        container.AddChild(favoriteButton);
        quicksaveButton = Glazier.Get().CreateButton();
        quicksaveButton.positionScale_X = 0.75f;
        quicksaveButton.positionOffset_Y = -50;
        quicksaveButton.positionOffset_X = 5;
        quicksaveButton.positionScale_Y = 1f;
        quicksaveButton.sizeOffset_X = -5;
        quicksaveButton.sizeOffset_Y = 50;
        quicksaveButton.sizeScale_X = 0.25f;
        quicksaveButton.text = localization.format("Quicksave_Button");
        quicksaveButton.tooltipText = localization.format("Quicksave_Button_Tooltip");
        quicksaveButton.fontSize = ESleekFontSize.Medium;
        quicksaveButton.onClickedButton += onClickedQuicksaveButton;
        container.AddChild(quicksaveButton);
        favoriteButton.isVisible = !Provider.isServer;
        quicksaveButton.isVisible = Provider.isServer;
        new MenuConfigurationOptionsUI();
        new MenuConfigurationDisplayUI();
        new MenuConfigurationGraphicsUI();
        new MenuConfigurationControlsUI();
        updateFavorite();
        Player.onSpyReady = onSpyReady;
        ClientMessageHandler_Accepted.OnGameplayConfigReceived += OnGameplayConfigReceived;
        SyncSuicideButtonAvailable();
    }
}
