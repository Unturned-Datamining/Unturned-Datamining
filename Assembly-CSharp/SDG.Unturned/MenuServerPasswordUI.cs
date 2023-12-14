using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class MenuServerPasswordUI
{
    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool isActive;

    private static SteamServerAdvertisement serverInfo;

    private static List<PublishedFileId_t> expectedWorkshopItems;

    private static SleekButtonIcon backButton;

    private static ISleekLabel explanationLabel;

    private static ISleekField passwordField;

    private static ISleekToggle showPasswordToggle;

    private static ISleekButton connectButton;

    public static void open(SteamServerAdvertisement newServerInfo, List<PublishedFileId_t> newExpectedWorkshopItems)
    {
        if (!isActive)
        {
            isActive = true;
            container.AnimateIntoView();
            serverInfo = newServerInfo;
            expectedWorkshopItems = newExpectedWorkshopItems;
            connectButton.IsClickable = false;
            passwordField.Text = string.Empty;
            passwordField.IsPasswordField = true;
            showPasswordToggle.Value = false;
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        isActive = false;
        explanationLabel = Glazier.Get().CreateLabel();
        explanationLabel.PositionOffset_Y = -75f;
        explanationLabel.PositionScale_X = 0.25f;
        explanationLabel.PositionScale_Y = 0.5f;
        explanationLabel.SizeScale_X = 0.5f;
        explanationLabel.SizeOffset_Y = 30f;
        explanationLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        explanationLabel.Text = localization.format("Explanation");
        container.AddChild(explanationLabel);
        passwordField = Glazier.Get().CreateStringField();
        passwordField.PositionOffset_X = -100f;
        passwordField.PositionOffset_Y = -35f;
        passwordField.PositionScale_X = 0.5f;
        passwordField.PositionScale_Y = 0.5f;
        passwordField.SizeOffset_X = 200f;
        passwordField.SizeOffset_Y = 30f;
        passwordField.AddLabel(localization.format("Password_Label"), ESleekSide.RIGHT);
        passwordField.IsPasswordField = true;
        passwordField.MaxLength = 0;
        passwordField.OnTextChanged += OnTypedPasswordField;
        passwordField.OnTextSubmitted += OnPasswordFieldSubmitted;
        container.AddChild(passwordField);
        showPasswordToggle = Glazier.Get().CreateToggle();
        showPasswordToggle.PositionOffset_X = -100f;
        showPasswordToggle.PositionOffset_Y = 5f;
        showPasswordToggle.PositionScale_X = 0.5f;
        showPasswordToggle.PositionScale_Y = 0.5f;
        showPasswordToggle.SizeOffset_X = 40f;
        showPasswordToggle.SizeOffset_Y = 40f;
        showPasswordToggle.OnValueChanged += OnToggledShowPassword;
        showPasswordToggle.AddLabel(localization.format("Show_Password_Label"), ESleekSide.RIGHT);
        container.AddChild(showPasswordToggle);
        connectButton = Glazier.Get().CreateButton();
        connectButton.PositionOffset_X = -100f;
        connectButton.PositionOffset_Y = 55f;
        connectButton.PositionScale_X = 0.5f;
        connectButton.PositionScale_Y = 0.5f;
        connectButton.SizeOffset_X = 200f;
        connectButton.SizeOffset_Y = 30f;
        connectButton.Text = localization.format("Connect_Button");
        connectButton.TooltipText = localization.format("Connect_Button");
        connectButton.OnClicked += OnClickedConnectButton;
        container.AddChild(connectButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += OnClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }

    private static void OnClickedConnectButton(ISleekElement button)
    {
        if (!string.IsNullOrEmpty(passwordField.Text))
        {
            Provider.connect(new ServerConnectParameters(new IPv4Address(serverInfo.ip), serverInfo.queryPort, serverInfo.connectionPort, passwordField.Text), serverInfo, expectedWorkshopItems);
        }
    }

    private static void OnToggledShowPassword(ISleekToggle toggle, bool show)
    {
        passwordField.IsPasswordField = !show;
    }

    private static void OnTypedPasswordField(ISleekField field, string text)
    {
        connectButton.IsClickable = !string.IsNullOrEmpty(text);
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
