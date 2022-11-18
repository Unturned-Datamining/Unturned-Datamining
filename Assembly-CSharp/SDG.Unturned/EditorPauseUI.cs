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
        ushort state = legacyIDField.state;
        if (state != 0)
        {
            SpawnTableTool.export(state, isLegacy: true);
        }
    }

    private static void onClickedProxyButton(ISleekElement button)
    {
        ushort state = proxyIDField.state;
        if (state != 0)
        {
            SpawnTableTool.export(state, isLegacy: false);
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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_X = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        saveButton = new SleekButtonIcon(bundle.load<Texture2D>("Save"));
        saveButton.positionOffset_X = -100;
        saveButton.positionOffset_Y = -115;
        saveButton.positionScale_X = 0.5f;
        saveButton.positionScale_Y = 0.5f;
        saveButton.sizeOffset_X = 200;
        saveButton.sizeOffset_Y = 30;
        saveButton.text = local.format("Save_Button");
        saveButton.tooltip = local.format("Save_Button_Tooltip");
        saveButton.onClickedButton += onClickedSaveButton;
        container.AddChild(saveButton);
        mapButton = new SleekButtonIcon(bundle.load<Texture2D>("Map"));
        mapButton.positionOffset_X = -100;
        mapButton.positionOffset_Y = -75;
        mapButton.positionScale_X = 0.5f;
        mapButton.positionScale_Y = 0.5f;
        mapButton.sizeOffset_X = 200;
        mapButton.sizeOffset_Y = 30;
        mapButton.text = local.format("Map_Button");
        mapButton.tooltip = local.format("Map_Button_Tooltip");
        mapButton.onClickedButton += onClickedMapButton;
        container.AddChild(mapButton);
        chartButton = new SleekButtonIcon(bundle.load<Texture2D>("Chart"));
        chartButton.positionOffset_X = -100;
        chartButton.positionOffset_Y = -35;
        chartButton.positionScale_X = 0.5f;
        chartButton.positionScale_Y = 0.5f;
        chartButton.sizeOffset_X = 200;
        chartButton.sizeOffset_Y = 30;
        chartButton.text = local.format("Chart_Button");
        chartButton.tooltip = local.format("Chart_Button_Tooltip");
        chartButton.onClickedButton += onClickedChartButton;
        container.AddChild(chartButton);
        legacyIDField = Glazier.Get().CreateUInt16Field();
        legacyIDField.positionOffset_X = -100;
        legacyIDField.positionOffset_Y = 5;
        legacyIDField.positionScale_X = 0.5f;
        legacyIDField.positionScale_Y = 0.5f;
        legacyIDField.sizeOffset_X = 50;
        legacyIDField.sizeOffset_Y = 30;
        container.AddChild(legacyIDField);
        legacyButton = Glazier.Get().CreateButton();
        legacyButton.positionOffset_X = -40;
        legacyButton.positionOffset_Y = 5;
        legacyButton.positionScale_X = 0.5f;
        legacyButton.positionScale_Y = 0.5f;
        legacyButton.sizeOffset_X = 140;
        legacyButton.sizeOffset_Y = 30;
        legacyButton.text = local.format("Legacy_Spawns");
        legacyButton.tooltipText = local.format("Legacy_Spawns_Tooltip");
        legacyButton.onClickedButton += onClickedLegacyButton;
        container.AddChild(legacyButton);
        proxyIDField = Glazier.Get().CreateUInt16Field();
        proxyIDField.positionOffset_X = -100;
        proxyIDField.positionOffset_Y = 45;
        proxyIDField.positionScale_X = 0.5f;
        proxyIDField.positionScale_Y = 0.5f;
        proxyIDField.sizeOffset_X = 50;
        proxyIDField.sizeOffset_Y = 30;
        container.AddChild(proxyIDField);
        proxyButton = Glazier.Get().CreateButton();
        proxyButton.positionOffset_X = -40;
        proxyButton.positionOffset_Y = 45;
        proxyButton.positionScale_X = 0.5f;
        proxyButton.positionScale_Y = 0.5f;
        proxyButton.sizeOffset_X = 140;
        proxyButton.sizeOffset_Y = 30;
        proxyButton.text = local.format("Proxy_Spawns");
        proxyButton.tooltipText = local.format("Proxy_Spawns_Tooltip");
        proxyButton.onClickedButton += onClickedProxyButton;
        container.AddChild(proxyButton);
        Local local2 = Localization.read("/Player/PlayerPause.dat");
        Bundle bundle2 = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerPause/PlayerPause.unity3d");
        optionsButton = new SleekButtonIcon(bundle2.load<Texture2D>("Options"));
        optionsButton.positionOffset_X = 110;
        optionsButton.positionOffset_Y = -115;
        optionsButton.positionScale_X = 0.5f;
        optionsButton.positionScale_Y = 0.5f;
        optionsButton.sizeOffset_X = 200;
        optionsButton.sizeOffset_Y = 50;
        optionsButton.text = local2.format("Options_Button_Text");
        optionsButton.tooltip = local2.format("Options_Button_Tooltip");
        optionsButton.onClickedButton += onClickedOptionsButton;
        optionsButton.iconColor = ESleekTint.FOREGROUND;
        optionsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(optionsButton);
        displayButton = new SleekButtonIcon(bundle2.load<Texture2D>("Display"));
        displayButton.positionOffset_X = 110;
        displayButton.positionOffset_Y = -55;
        displayButton.positionScale_X = 0.5f;
        displayButton.positionScale_Y = 0.5f;
        displayButton.sizeOffset_X = 200;
        displayButton.sizeOffset_Y = 50;
        displayButton.text = local2.format("Display_Button_Text");
        displayButton.tooltip = local2.format("Display_Button_Tooltip");
        displayButton.iconColor = ESleekTint.FOREGROUND;
        displayButton.onClickedButton += onClickedDisplayButton;
        displayButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(displayButton);
        graphicsButton = new SleekButtonIcon(bundle2.load<Texture2D>("Graphics"));
        graphicsButton.positionOffset_X = 110;
        graphicsButton.positionOffset_Y = 5;
        graphicsButton.positionScale_X = 0.5f;
        graphicsButton.positionScale_Y = 0.5f;
        graphicsButton.sizeOffset_X = 200;
        graphicsButton.sizeOffset_Y = 50;
        graphicsButton.text = local2.format("Graphics_Button_Text");
        graphicsButton.tooltip = local2.format("Graphics_Button_Tooltip");
        graphicsButton.iconColor = ESleekTint.FOREGROUND;
        graphicsButton.onClickedButton += onClickedGraphicsButton;
        graphicsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(graphicsButton);
        controlsButton = new SleekButtonIcon(bundle2.load<Texture2D>("Controls"));
        controlsButton.positionOffset_X = 110;
        controlsButton.positionOffset_Y = 65;
        controlsButton.positionScale_X = 0.5f;
        controlsButton.positionScale_Y = 0.5f;
        controlsButton.sizeOffset_X = 200;
        controlsButton.sizeOffset_Y = 50;
        controlsButton.text = local2.format("Controls_Button_Text");
        controlsButton.tooltip = local2.format("Controls_Button_Tooltip");
        controlsButton.iconColor = ESleekTint.FOREGROUND;
        controlsButton.onClickedButton += onClickedControlsButton;
        controlsButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(controlsButton);
        bundle2.unload();
        exitButton = new SleekButtonIconConfirm(bundle.load<Texture2D>("Exit"), local.format("Exit_Button"), local.format("Exit_Button_Tooltip"), "Cancel", string.Empty);
        exitButton.positionOffset_X = -100;
        exitButton.positionOffset_Y = 85;
        exitButton.positionScale_X = 0.5f;
        exitButton.positionScale_Y = 0.5f;
        exitButton.sizeOffset_X = 200;
        exitButton.sizeOffset_Y = 30;
        exitButton.text = local.format("Exit_Button");
        exitButton.tooltip = local.format("Exit_Button_Tooltip");
        SleekButtonIconConfirm sleekButtonIconConfirm = exitButton;
        sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(onClickedExitButton));
        container.AddChild(exitButton);
        quitButton = new SleekButtonIconConfirm(MenuPauseUI.icons.load<Texture2D>("Quit"), MenuPauseUI.localization.format("Exit_Button"), MenuPauseUI.localization.format("Exit_Button_Tooltip"), "Cancel", string.Empty);
        quitButton.positionOffset_X = -100;
        quitButton.positionOffset_Y = 125;
        quitButton.positionScale_X = 0.5f;
        quitButton.positionScale_Y = 0.5f;
        quitButton.sizeOffset_X = 200;
        quitButton.sizeOffset_Y = 50;
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
        bundle.unload();
    }
}
