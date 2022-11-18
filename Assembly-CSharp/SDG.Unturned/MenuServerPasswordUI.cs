using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuServerPasswordUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool isActive;

    private static SteamServerInfo serverInfo;

    private static List<PublishedFileId_t> expectedWorkshopItems;

    private static SleekButtonIcon backButton;

    private static ISleekLabel explanationLabel;

    private static ISleekField passwordField;

    private static ISleekToggle showPasswordToggle;

    private static ISleekButton connectButton;

    public static void open(SteamServerInfo newServerInfo, List<PublishedFileId_t> newExpectedWorkshopItems)
    {
        if (!isActive)
        {
            isActive = true;
            container.AnimateIntoView();
            serverInfo = newServerInfo;
            expectedWorkshopItems = newExpectedWorkshopItems;
            connectButton.isClickable = false;
            passwordField.text = string.Empty;
            passwordField.replace = '*';
            showPasswordToggle.state = false;
        }
    }

    public static void close()
    {
        if (isActive)
        {
            isActive = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public MenuServerPasswordUI()
    {
        localization = Localization.read("/Menu/Play/MenuServerPassword.dat");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        isActive = false;
        explanationLabel = Glazier.Get().CreateLabel();
        explanationLabel.positionOffset_Y = -75;
        explanationLabel.positionScale_X = 0.25f;
        explanationLabel.positionScale_Y = 0.5f;
        explanationLabel.sizeScale_X = 0.5f;
        explanationLabel.sizeOffset_Y = 30;
        explanationLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        explanationLabel.text = localization.format("Explanation");
        container.AddChild(explanationLabel);
        passwordField = Glazier.Get().CreateStringField();
        passwordField.positionOffset_X = -100;
        passwordField.positionOffset_Y = -35;
        passwordField.positionScale_X = 0.5f;
        passwordField.positionScale_Y = 0.5f;
        passwordField.sizeOffset_X = 200;
        passwordField.sizeOffset_Y = 30;
        passwordField.addLabel(localization.format("Password_Label"), ESleekSide.RIGHT);
        passwordField.replace = '*';
        passwordField.maxLength = 0;
        passwordField.onTyped += OnTypedPasswordField;
        passwordField.onEntered += OnPasswordFieldSubmitted;
        container.AddChild(passwordField);
        showPasswordToggle = Glazier.Get().CreateToggle();
        showPasswordToggle.positionOffset_X = -100;
        showPasswordToggle.positionOffset_Y = 5;
        showPasswordToggle.positionScale_X = 0.5f;
        showPasswordToggle.positionScale_Y = 0.5f;
        showPasswordToggle.sizeOffset_X = 40;
        showPasswordToggle.sizeOffset_Y = 40;
        showPasswordToggle.onToggled += OnToggledShowPassword;
        showPasswordToggle.addLabel(localization.format("Show_Password_Label"), ESleekSide.RIGHT);
        container.AddChild(showPasswordToggle);
        connectButton = Glazier.Get().CreateButton();
        connectButton.positionOffset_X = -100;
        connectButton.positionOffset_Y = 55;
        connectButton.positionScale_X = 0.5f;
        connectButton.positionScale_Y = 0.5f;
        connectButton.sizeOffset_X = 200;
        connectButton.sizeOffset_Y = 30;
        connectButton.text = localization.format("Connect_Button");
        connectButton.tooltipText = localization.format("Connect_Button");
        connectButton.onClickedButton += OnClickedConnectButton;
        container.AddChild(connectButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += OnClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }

    private static void OnClickedConnectButton(ISleekElement button)
    {
        if (!string.IsNullOrEmpty(passwordField.text))
        {
            Provider.connect(serverInfo, passwordField.text, expectedWorkshopItems);
        }
    }

    private static void OnToggledShowPassword(ISleekToggle toggle, bool show)
    {
        passwordField.replace = (show ? ' ' : '*');
    }

    private static void OnTypedPasswordField(ISleekField field, string text)
    {
        connectButton.isClickable = !string.IsNullOrEmpty(text);
    }

    private static void OnPasswordFieldSubmitted(ISleekField field)
    {
        OnClickedConnectButton(connectButton);
    }

    private static void OnClickedBackButton(ISleekElement button)
    {
        MenuPlayServerInfoUI.OpenWithoutRefresh();
        close();
    }
}
