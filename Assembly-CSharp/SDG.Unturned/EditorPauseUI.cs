using System;
using UnityEngine;

namespace SDG.Unturned;

public class EditorPauseUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon saveButton;

    private static SleekButtonIcon mapButton;

    private static SleekButtonIconConfirm exitButton;

    private static SleekButtonIconConfirm quitButton;

    private static ISleekUInt16Field legacyIDField;

    private static ISleekButton legacyButton;

    private static ISleekUInt16Field proxyIDField;

    private static ISleekButton proxyButton;

    private static SleekButtonIcon chartButton;

    private static SleekButtonIcon optionsButton;

    private static SleekButtonIcon displayButton;

    private static SleekButtonIcon graphicsButton;

    private static SleekButtonIcon controlsButton;

    private static SleekButtonIcon audioButton;

    internal static MenuConfigurationAudioUI audioMenu;

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
            exitButton.reset();
            quitButton.reset();
            container.AnimateOutOfView(1f, 0f);
        }
    }

    private static void onClickedSaveButton(ISleekElement button)
    {
        Level.save();
    }

    private static void onClickedMapButton(ISleekElement button)
    {
        Level.CaptureSatelliteImage();
    }

    private static void onClickedChartButton(ISleekElement button)
    {
        Level.CaptureChartImage();
    }

    private static void onClickedLegacyButton(ISleekElement button)
    {
        ushort value = legacyIDField.Value;
        if (value != 0)
        {
            SpawnTableTool.export(value, isLegacy: true);
        }
    }

    private static void onClickedProxyButton(ISleekElement button)
    {
        ushort value = proxyIDField.Value;
        if (value != 0)
        {
            SpawnTableTool.export(value, isLegacy: false);
        }
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

    private static void onClickedAudioButton(ISleekElement button)
    {
        close();
        audioMenu.open();
    }

    private static void onClickedExitButton(SleekButtonIconConfirm button)
    {
        Level.exit();
    }

    private static void onClickedQuitButton(SleekButtonIconConfirm button)
    {
        Provider.QuitGame("clicked quit in level editor");
    }

    public EditorPauseUI()
    {
        Local local = Localization.read("/Editor/EditorPause.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Edit/Icons/EditorPause/EditorPause.unity3d");
        container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_X = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        saveButton = new SleekButtonIcon(bundle.load<Texture2D>("Save"));
        saveButton.PositionOffset_X = -100f;
        saveButton.PositionOffset_Y = -115f;
        saveButton.PositionScale_X = 0.5f;
        saveButton.PositionScale_Y = 0.5f;
        saveButton.SizeOffset_X = 200f;
        saveButton.SizeOffset_Y = 30f;
        saveButton.text = local.format("Save_Button");
        saveButton.tooltip = local.format("Save_Button_Tooltip");
        saveButton.onClickedButton += onClickedSaveButton;
        container.AddChild(saveButton);
        mapButton = new SleekButtonIcon(bundle.load<Texture2D>("Map"));
        mapButton.PositionOffset_X = -100f;
        mapButton.PositionOffset_Y = -75f;
        mapButton.PositionScale_X = 0.5f;
        mapButton.PositionScale_Y = 0.5f;
        mapButton.SizeOffset_X = 200f;
        mapButton.SizeOffset_Y = 30f;
        mapButton.text = local.format("Map_Button");
        mapButton.tooltip = local.format("Map_Button_Tooltip");
        mapButton.onClickedButton += onClickedMapButton;
        container.AddChild(mapButton);
        chartButton = new SleekButtonIcon(bundle.load<Texture2D>("Chart"));
        chartButton.PositionOffset_X = -100f;
        chartButton.PositionOffset_Y = -35f;
        chartButton.PositionScale_X = 0.5f;
        chartButton.PositionScale_Y = 0.5f;
        chartButton.SizeOffset_X = 200f;
        chartButton.SizeOffset_Y = 30f;
        chartButton.text = local.format("Chart_Button");
        chartButton.tooltip = local.format("Chart_Button_Tooltip");
        chartButton.onClickedButton += onClickedChartButton;
        container.AddChild(chartButton);
        legacyIDField = Glazier.Get().CreateUInt16Field();
        legacyIDField.PositionOffset_X = -100f;
        legacyIDField.PositionOffset_Y = 5f;
        legacyIDField.PositionScale_X = 0.5f;
        legacyIDField.PositionScale_Y = 0.5f;
        legacyIDField.SizeOffset_X = 50f;
        legacyIDField.SizeOffset_Y = 30f;
        container.AddChild(legacyIDField);
        legacyButton = Glazier.Get().CreateButton();
        legacyButton.PositionOffset_X = -40f;
        legacyButton.PositionOffset_Y = 5f;
        legacyButton.PositionScale_X = 0.5f;
        legacyButton.PositionScale_Y = 0.5f;
        legacyButton.SizeOffset_X = 140f;
        legacyButton.SizeOffset_Y = 30f;
        legacyButton.Text = local.format("Legacy_Spawns");
        legacyButton.TooltipText = local.format("Legacy_Spawns_Tooltip");
        legacyButton.OnClicked += onClickedLegacyButton;
        container.AddChild(legacyButton);
        proxyIDField = Glazier.Get().CreateUInt16Field();
        proxyIDField.PositionOffset_X = -100f;
        proxyIDField.PositionOffset_Y = 45f;
        proxyIDField.PositionScale_X = 0.5f;
        proxyIDField.PositionScale_Y = 0.5f;
        proxyIDField.SizeOffset_X = 50f;
        proxyIDField.SizeOffset_Y = 30f;
        container.AddChild(proxyIDField);
        proxyButton = Glazier.Get().CreateButton();
        proxyButton.PositionOffset_X = -40f;
        proxyButton.PositionOffset_Y = 45f;
        proxyButton.PositionScale_X = 0.5f;
        proxyButton.PositionScale_Y = 0.5f;
        proxyButton.SizeOffset_X = 140f;
        proxyButton.SizeOffset_Y = 30f;
        proxyButton.Text = local.format("Proxy_Spawns");
        proxyButton.TooltipText = local.format("Proxy_Spawns_Tooltip");
        proxyButton.OnClicked += onClickedProxyButton;
        container.AddChild(proxyButton);
        Local local2 = Localization.read("/Player/PlayerPause.dat");
        Bundle bundle2 = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerPause/PlayerPause.unity3d");
        optionsButton = new SleekButtonIcon(bundle2.load<Texture2D>("Options"));
        optionsButton.PositionOffset_X = 110f;
        optionsButton.PositionOffset_Y = -115f;
        optionsButton.PositionScale_X = 0.5f;
        optionsButton.PositionScale_Y = 0.5f;
        optionsButton.SizeOffset_X = 200f;
        optionsButton.SizeOffset_Y = 50f;
        optionsButton.text = local2.format("Options_Button_Text");
        optionsButton.tooltip = local2.format("Options_Button_Tooltip");
        optionsButton.onClickedButton += onClickedOptionsButton;
        optionsButton.iconColor = ESleekTint.FOREGROUND;
        optionsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(optionsButton);
        displayButton = new SleekButtonIcon(bundle2.load<Texture2D>("Display"));
        displayButton.PositionOffset_X = 110f;
        displayButton.PositionOffset_Y = -55f;
        displayButton.PositionScale_X = 0.5f;
        displayButton.PositionScale_Y = 0.5f;
        displayButton.SizeOffset_X = 200f;
        displayButton.SizeOffset_Y = 50f;
        displayButton.text = local2.format("Display_Button_Text");
        displayButton.tooltip = local2.format("Display_Button_Tooltip");
        displayButton.iconColor = ESleekTint.FOREGROUND;
        displayButton.onClickedButton += onClickedDisplayButton;
        displayButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(displayButton);
        graphicsButton = new SleekButtonIcon(bundle2.load<Texture2D>("Graphics"));
        graphicsButton.PositionOffset_X = 110f;
        graphicsButton.PositionOffset_Y = 5f;
        graphicsButton.PositionScale_X = 0.5f;
        graphicsButton.PositionScale_Y = 0.5f;
        graphicsButton.SizeOffset_X = 200f;
        graphicsButton.SizeOffset_Y = 50f;
        graphicsButton.text = local2.format("Graphics_Button_Text");
        graphicsButton.tooltip = local2.format("Graphics_Button_Tooltip");
        graphicsButton.iconColor = ESleekTint.FOREGROUND;
        graphicsButton.onClickedButton += onClickedGraphicsButton;
        graphicsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(graphicsButton);
        controlsButton = new SleekButtonIcon(bundle2.load<Texture2D>("Controls"));
        controlsButton.PositionOffset_X = 110f;
        controlsButton.PositionOffset_Y = 65f;
        controlsButton.PositionScale_X = 0.5f;
        controlsButton.PositionScale_Y = 0.5f;
        controlsButton.SizeOffset_X = 200f;
        controlsButton.SizeOffset_Y = 50f;
        controlsButton.text = local2.format("Controls_Button_Text");
        controlsButton.tooltip = local2.format("Controls_Button_Tooltip");
        controlsButton.iconColor = ESleekTint.FOREGROUND;
        controlsButton.onClickedButton += onClickedControlsButton;
        controlsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(controlsButton);
        audioButton = new SleekButtonIcon(bundle2.load<Texture2D>("Audio"));
        audioButton.PositionOffset_X = 110f;
        audioButton.PositionOffset_Y = 125f;
        audioButton.PositionScale_X = 0.5f;
        audioButton.PositionScale_Y = 0.5f;
        audioButton.SizeOffset_X = 200f;
        audioButton.SizeOffset_Y = 50f;
        audioButton.text = local2.format("Audio_Button_Text");
        audioButton.tooltip = local2.format("Audio_Button_Tooltip");
        audioButton.iconColor = ESleekTint.FOREGROUND;
        audioButton.onClickedButton += onClickedAudioButton;
        audioButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(audioButton);
        bundle2.unload();
        exitButton = new SleekButtonIconConfirm(bundle.load<Texture2D>("Exit"), local.format("Exit_Button"), local.format("Exit_Button_Tooltip"), "Cancel", string.Empty);
        exitButton.PositionOffset_X = -100f;
        exitButton.PositionOffset_Y = 85f;
        exitButton.PositionScale_X = 0.5f;
        exitButton.PositionScale_Y = 0.5f;
        exitButton.SizeOffset_X = 200f;
        exitButton.SizeOffset_Y = 30f;
        exitButton.text = local.format("Exit_Button");
        exitButton.tooltip = local.format("Exit_Button_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm = exitButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(onClickedExitButton));
        container.AddChild(exitButton);
        quitButton = new SleekButtonIconConfirm(MenuPauseUI.icons.load<Texture2D>("Quit"), MenuPauseUI.localization.format("Exit_Button"), MenuPauseUI.localization.format("Exit_Button_Tooltip"), "Cancel", string.Empty);
        quitButton.PositionOffset_X = -100f;
        quitButton.PositionOffset_Y = 125f;
        quitButton.PositionScale_X = 0.5f;
        quitButton.PositionScale_Y = 0.5f;
        quitButton.SizeOffset_X = 200f;
        quitButton.SizeOffset_Y = 50f;
        quitButton.text = MenuPauseUI.localization.format("Exit_Button");
        quitButton.tooltip = MenuPauseUI.localization.format("Exit_Button_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm2 = quitButton;
        sleekButtonIconConfirm2.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm2.onConfirmed, new Confirm(onClickedQuitButton));
        quitButton.fontSize = ESleekFontSize.Medium;
        quitButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(quitButton);
        new MenuConfigurationOptionsUI();
        new MenuConfigurationDisplayUI();
        new MenuConfigurationGraphicsUI();
        new MenuConfigurationControlsUI();
        audioMenu = new MenuConfigurationAudioUI();
        audioMenu.PositionOffset_X = 10f;
        audioMenu.PositionOffset_Y = 10f;
        audioMenu.PositionScale_Y = 1f;
        audioMenu.SizeOffset_X = -20f;
        audioMenu.SizeOffset_Y = -20f;
        audioMenu.SizeScale_X = 1f;
        audioMenu.SizeScale_Y = 1f;
        EditorUI.window.AddChild(audioMenu);
        bundle.unload();
    }
}
