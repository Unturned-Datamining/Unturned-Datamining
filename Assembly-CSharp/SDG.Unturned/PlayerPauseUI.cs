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

    /// <summary>
    /// Exit button only needs to wait for timer in certain conditions.
    /// </summary>
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
                serverBox.Text = localization.format("Server_WithVersion", localizedName, Level.version, OptionsSettings.streamer ? localization.format("Streamer") : Provider.currentServerInfo.name, text);
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
        spyImage.Texture = texture2D;
        returnButton.PositionOffset_X = -435f;
        optionsButton.PositionOffset_X = -435f;
        displayButton.PositionOffset_X = -435f;
        graphicsButton.PositionOffset_X = -435f;
        controlsButton.PositionOffset_X = -435f;
        exitButton.PositionOffset_X = -435f;
        quitButton.PositionOffset_X = -435f;
        suicideButton.PositionOffset_X = -435f;
        spyBox.PositionOffset_X = -225f;
        spyBox.IsVisible = true;
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
        suicideDisabledLabel.IsVisible = !can_Suicide;
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
        container.PositionScale_Y = 1f;
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        returnButton = new SleekButtonIcon(icons.load<Texture2D>("Return"));
        returnButton.PositionOffset_X = -100f;
        returnButton.PositionOffset_Y = -205f;
        returnButton.PositionScale_X = 0.5f;
        returnButton.PositionScale_Y = 0.5f;
        returnButton.SizeOffset_X = 200f;
        returnButton.SizeOffset_Y = 50f;
        returnButton.text = localization.format("Return_Button_Text");
        returnButton.tooltip = localization.format("Return_Button_Tooltip");
        returnButton.iconColor = ESleekTint.FOREGROUND;
        returnButton.onClickedButton += onClickedReturnButton;
        returnButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(returnButton);
        optionsButton = new SleekButtonIcon(icons.load<Texture2D>("Options"));
        optionsButton.PositionOffset_X = -100f;
        optionsButton.PositionOffset_Y = -145f;
        optionsButton.PositionScale_X = 0.5f;
        optionsButton.PositionScale_Y = 0.5f;
        optionsButton.SizeOffset_X = 200f;
        optionsButton.SizeOffset_Y = 50f;
        optionsButton.text = localization.format("Options_Button_Text");
        optionsButton.tooltip = localization.format("Options_Button_Tooltip");
        optionsButton.onClickedButton += onClickedOptionsButton;
        optionsButton.iconColor = ESleekTint.FOREGROUND;
        optionsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(optionsButton);
        displayButton = new SleekButtonIcon(icons.load<Texture2D>("Display"));
        displayButton.PositionOffset_X = -100f;
        displayButton.PositionOffset_Y = -85f;
        displayButton.PositionScale_X = 0.5f;
        displayButton.PositionScale_Y = 0.5f;
        displayButton.SizeOffset_X = 200f;
        displayButton.SizeOffset_Y = 50f;
        displayButton.text = localization.format("Display_Button_Text");
        displayButton.tooltip = localization.format("Display_Button_Tooltip");
        displayButton.iconColor = ESleekTint.FOREGROUND;
        displayButton.onClickedButton += onClickedDisplayButton;
        displayButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(displayButton);
        graphicsButton = new SleekButtonIcon(icons.load<Texture2D>("Graphics"));
        graphicsButton.PositionOffset_X = -100f;
        graphicsButton.PositionOffset_Y = -25f;
        graphicsButton.PositionScale_X = 0.5f;
        graphicsButton.PositionScale_Y = 0.5f;
        graphicsButton.SizeOffset_X = 200f;
        graphicsButton.SizeOffset_Y = 50f;
        graphicsButton.text = localization.format("Graphics_Button_Text");
        graphicsButton.tooltip = localization.format("Graphics_Button_Tooltip");
        graphicsButton.iconColor = ESleekTint.FOREGROUND;
        graphicsButton.onClickedButton += onClickedGraphicsButton;
        graphicsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(graphicsButton);
        controlsButton = new SleekButtonIcon(icons.load<Texture2D>("Controls"));
        controlsButton.PositionOffset_X = -100f;
        controlsButton.PositionOffset_Y = 35f;
        controlsButton.PositionScale_X = 0.5f;
        controlsButton.PositionScale_Y = 0.5f;
        controlsButton.SizeOffset_X = 200f;
        controlsButton.SizeOffset_Y = 50f;
        controlsButton.text = localization.format("Controls_Button_Text");
        controlsButton.tooltip = localization.format("Controls_Button_Tooltip");
        controlsButton.iconColor = ESleekTint.FOREGROUND;
        controlsButton.onClickedButton += onClickedControlsButton;
        controlsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(controlsButton);
        exitButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Exit"), localization.format("Exit_Button_Text"), localization.format("Exit_Button_Tooltip"), localization.format("Return_Button_Text"), string.Empty);
        exitButton.PositionOffset_X = -100f;
        exitButton.PositionOffset_Y = 155f;
        exitButton.PositionScale_X = 0.5f;
        exitButton.PositionScale_Y = 0.5f;
        exitButton.SizeOffset_X = 200f;
        exitButton.SizeOffset_Y = 50f;
        exitButton.text = localization.format("Exit_Button_Text");
        exitButton.tooltip = localization.format("Exit_Button_Tooltip");
        exitButton.iconColor = ESleekTint.FOREGROUND;
        SleekButtonIconConfirm sleekButtonIconConfirm = exitButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(onClickedExitButton));
        exitButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(exitButton);
        quitButton = new SleekButtonIconConfirm(MenuPauseUI.icons.load<Texture2D>("Quit"), localization.format("Quit_Button"), localization.format("Quit_Button_Tooltip"), localization.format("Return_Button_Text"), string.Empty);
        quitButton.PositionOffset_X = -100f;
        quitButton.PositionOffset_Y = 215f;
        quitButton.PositionScale_X = 0.5f;
        quitButton.PositionScale_Y = 0.5f;
        quitButton.SizeOffset_X = 200f;
        quitButton.SizeOffset_Y = 50f;
        quitButton.text = localization.format("Quit_Button");
        quitButton.tooltip = localization.format("Quit_Button_Tooltip");
        quitButton.iconColor = ESleekTint.FOREGROUND;
        SleekButtonIconConfirm sleekButtonIconConfirm2 = quitButton;
        sleekButtonIconConfirm2.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm2.onConfirmed, new Confirm(onClickedQuitButton));
        quitButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(quitButton);
        suicideButton = new SleekButtonIconConfirm(icons.load<Texture2D>("Suicide"), localization.format("Suicide_Button_Confirm"), localization.format("Suicide_Button_Confirm_Tooltip"), localization.format("Suicide_Button_Deny"), localization.format("Suicide_Button_Deny_Tooltip"));
        suicideButton.PositionOffset_X = -100f;
        suicideButton.PositionOffset_Y = 95f;
        suicideButton.PositionScale_X = 0.5f;
        suicideButton.PositionScale_Y = 0.5f;
        suicideButton.SizeOffset_X = 200f;
        suicideButton.SizeOffset_Y = 50f;
        suicideButton.text = localization.format("Suicide_Button_Text");
        suicideButton.tooltip = localization.format("Suicide_Button_Tooltip");
        suicideButton.iconColor = ESleekTint.FOREGROUND;
        suicideButton.onConfirmed = onClickedSuicideButton;
        suicideButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(suicideButton);
        suicideDisabledLabel = Glazier.Get().CreateLabel();
        suicideDisabledLabel.PositionOffset_X = -100f;
        suicideDisabledLabel.PositionOffset_Y = 95f;
        suicideDisabledLabel.PositionScale_X = 0.5f;
        suicideDisabledLabel.PositionScale_Y = 0.5f;
        suicideDisabledLabel.SizeOffset_X = 200f;
        suicideDisabledLabel.SizeOffset_Y = 50f;
        suicideDisabledLabel.Text = localization.format("Suicide_Disabled");
        suicideDisabledLabel.TextColor = ESleekTint.BAD;
        suicideDisabledLabel.FontSize = ESleekFontSize.Large;
        suicideDisabledLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        suicideDisabledLabel.IsVisible = false;
        container.AddChild(suicideDisabledLabel);
        spyBox = Glazier.Get().CreateBox();
        spyBox.PositionOffset_Y = -310f;
        spyBox.PositionScale_X = 0.5f;
        spyBox.PositionScale_Y = 0.5f;
        spyBox.SizeOffset_X = 660f;
        spyBox.SizeOffset_Y = 500f;
        container.AddChild(spyBox);
        spyBox.IsVisible = false;
        spyImage = Glazier.Get().CreateImage();
        spyImage.PositionOffset_X = 10f;
        spyImage.PositionOffset_Y = 10f;
        spyImage.SizeOffset_X = 640f;
        spyImage.SizeOffset_Y = 480f;
        spyBox.AddChild(spyImage);
        spyRefreshButton = Glazier.Get().CreateButton();
        spyRefreshButton.PositionOffset_X = -205f;
        spyRefreshButton.PositionOffset_Y = 10f;
        spyRefreshButton.PositionScale_X = 0.5f;
        spyRefreshButton.PositionScale_Y = 1f;
        spyRefreshButton.SizeOffset_X = 200f;
        spyRefreshButton.SizeOffset_Y = 50f;
        spyRefreshButton.Text = localization.format("Spy_Refresh_Button_Text");
        spyRefreshButton.TooltipText = localization.format("Spy_Refresh_Button_Tooltip");
        spyRefreshButton.OnClicked += onClickedSpyRefreshButton;
        spyRefreshButton.FontSize = ESleekFontSize.Medium;
        spyBox.AddChild(spyRefreshButton);
        spySlayButton = Glazier.Get().CreateButton();
        spySlayButton.PositionOffset_X = 5f;
        spySlayButton.PositionOffset_Y = 10f;
        spySlayButton.PositionScale_X = 0.5f;
        spySlayButton.PositionScale_Y = 1f;
        spySlayButton.SizeOffset_X = 200f;
        spySlayButton.SizeOffset_Y = 50f;
        spySlayButton.Text = localization.format("Spy_Slay_Button_Text");
        spySlayButton.TooltipText = localization.format("Spy_Slay_Button_Tooltip");
        spySlayButton.OnClicked += onClickedSpySlayButton;
        spySlayButton.FontSize = ESleekFontSize.Medium;
        spyBox.AddChild(spySlayButton);
        serverBox = Glazier.Get().CreateBox();
        serverBox.PositionOffset_Y = -50f;
        serverBox.PositionScale_Y = 1f;
        serverBox.SizeOffset_X = -5f;
        serverBox.SizeOffset_Y = 50f;
        serverBox.SizeScale_X = 0.75f;
        serverBox.FontSize = ESleekFontSize.Medium;
        container.AddChild(serverBox);
        favoriteButton = new SleekButtonIcon(null);
        favoriteButton.PositionScale_X = 0.75f;
        favoriteButton.PositionOffset_Y = -50f;
        favoriteButton.PositionOffset_X = 5f;
        favoriteButton.PositionScale_Y = 1f;
        favoriteButton.SizeOffset_X = -5f;
        favoriteButton.SizeOffset_Y = 50f;
        favoriteButton.SizeScale_X = 0.25f;
        favoriteButton.tooltip = localization.format("Favorite_Button_Tooltip");
        favoriteButton.fontSize = ESleekFontSize.Medium;
        favoriteButton.onClickedButton += onClickedFavoriteButton;
        container.AddChild(favoriteButton);
        quicksaveButton = Glazier.Get().CreateButton();
        quicksaveButton.PositionScale_X = 0.75f;
        quicksaveButton.PositionOffset_Y = -50f;
        quicksaveButton.PositionOffset_X = 5f;
        quicksaveButton.PositionScale_Y = 1f;
        quicksaveButton.SizeOffset_X = -5f;
        quicksaveButton.SizeOffset_Y = 50f;
        quicksaveButton.SizeScale_X = 0.25f;
        quicksaveButton.Text = localization.format("Quicksave_Button");
        quicksaveButton.TooltipText = localization.format("Quicksave_Button_Tooltip");
        quicksaveButton.FontSize = ESleekFontSize.Medium;
        quicksaveButton.OnClicked += onClickedQuicksaveButton;
        container.AddChild(quicksaveButton);
        favoriteButton.IsVisible = !Provider.isServer;
        quicksaveButton.IsVisible = Provider.isServer;
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
