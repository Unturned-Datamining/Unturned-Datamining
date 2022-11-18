using System;
using System.Net;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayConnectUI
{
    public static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    public static bool hasPendingServerRelay;

    public static uint serverRelayIP;

    public static ushort serverRelayPort;

    public static string serverRelayPassword;

    public static bool serverRelayWaitOnMenu;

    private static SleekButtonIcon backButton;

    private static ISleekField ipField;

    private static ISleekUInt16Field portField;

    private static ISleekField passwordField;

    private static SleekButtonIcon connectButton;

    private static bool isLaunched;

    public static void connect(SteamConnectionInfo info, bool shouldAutoJoin)
    {
        Provider.provider.matchmakingService.connect(info);
        Provider.provider.matchmakingService.autoJoinServerQuery = shouldAutoJoin;
    }

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
            MenuSettings.save();
            container.AnimateOutOfView(0f, 1f);
        }
    }

    private static void onClickedConnectButton(ISleekElement button)
    {
        if (portField.state == 0)
        {
            return;
        }
        string text = ipField.text.ToLower();
        text = text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            UnturnedLog.info("Input IP was empty");
            return;
        }
        string text2;
        if (text == "localhost")
        {
            text2 = "127.0.0.1";
        }
        else
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(text);
            text2 = ((hostAddresses.Length == 0 || hostAddresses[0] == null) ? null : hostAddresses[0].ToString());
        }
        if (string.IsNullOrEmpty(text2))
        {
            UnturnedLog.info("Parsed IP was empty");
        }
        else if (Parser.checkIP(text2))
        {
            SteamConnectionInfo info = new SteamConnectionInfo(text2, portField.state, passwordField.text);
            MenuSettings.save();
            connect(info, shouldAutoJoin: false);
        }
        else
        {
            UnturnedLog.info("Check IP '{0}' failed", text2);
        }
    }

    private static void onTypedIPField(ISleekField field, string text)
    {
        PlaySettings.connectIP = text;
    }

    private static void onTypedPortField(ISleekUInt16Field field, ushort state)
    {
        PlaySettings.connectPort = state;
    }

    private static void onTypedPasswordField(ISleekField field, string text)
    {
        PlaySettings.connectPassword = text;
    }

    private static void onAttemptUpdated(int attempt)
    {
        MenuUI.openAlert(localization.format("Connecting", attempt));
    }

    private static void onTimedOut()
    {
        if (Provider.connectionFailureInfo != 0)
        {
            ESteamConnectionFailureInfo connectionFailureInfo = Provider.connectionFailureInfo;
            Provider.resetConnectionFailure();
            switch (connectionFailureInfo)
            {
            case ESteamConnectionFailureInfo.PRO_SERVER:
                MenuUI.alert(localization.format("Pro_Server"));
                break;
            case ESteamConnectionFailureInfo.PASSWORD:
                MenuUI.alert(localization.format("Password"));
                break;
            case ESteamConnectionFailureInfo.FULL:
                MenuUI.alert(localization.format("Full"));
                break;
            case ESteamConnectionFailureInfo.TIMED_OUT:
                MenuUI.alert(localization.format("Timed_Out"));
                break;
            }
        }
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuPlayUI.open();
        close();
    }

    public void OnDestroy()
    {
        Provider.provider.matchmakingService.onAttemptUpdated -= onAttemptUpdated;
        Provider.provider.matchmakingService.onTimedOut -= onTimedOut;
    }

    public MenuPlayConnectUI()
    {
        localization = Localization.read("/Menu/Play/MenuPlayConnect.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Play/MenuPlayConnect/MenuPlayConnect.unity3d");
        container = new SleekFullscreenBox();
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.positionScale_Y = 1f;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        ipField = Glazier.Get().CreateStringField();
        ipField.positionOffset_X = -100;
        ipField.positionOffset_Y = -75;
        ipField.positionScale_X = 0.5f;
        ipField.positionScale_Y = 0.5f;
        ipField.sizeOffset_X = 200;
        ipField.sizeOffset_Y = 30;
        ipField.maxLength = 64;
        ipField.addLabel(localization.format("IP_Field_Label"), ESleekSide.RIGHT);
        ipField.text = PlaySettings.connectIP;
        ipField.onTyped += onTypedIPField;
        container.AddChild(ipField);
        portField = Glazier.Get().CreateUInt16Field();
        portField.positionOffset_X = -100;
        portField.positionOffset_Y = -35;
        portField.positionScale_X = 0.5f;
        portField.positionScale_Y = 0.5f;
        portField.sizeOffset_X = 200;
        portField.sizeOffset_Y = 30;
        portField.addLabel(localization.format("Port_Field_Label"), ESleekSide.RIGHT);
        portField.state = PlaySettings.connectPort;
        portField.onTypedUInt16 += onTypedPortField;
        container.AddChild(portField);
        passwordField = Glazier.Get().CreateStringField();
        passwordField.positionOffset_X = -100;
        passwordField.positionOffset_Y = 5;
        passwordField.positionScale_X = 0.5f;
        passwordField.positionScale_Y = 0.5f;
        passwordField.sizeOffset_X = 200;
        passwordField.sizeOffset_Y = 30;
        passwordField.addLabel(localization.format("Password_Field_Label"), ESleekSide.RIGHT);
        passwordField.replace = localization.format("Password_Field_Replace").ToCharArray()[0];
        passwordField.maxLength = 0;
        passwordField.text = PlaySettings.connectPassword;
        passwordField.onTyped += onTypedPasswordField;
        container.AddChild(passwordField);
        connectButton = new SleekButtonIcon(bundle.load<Texture2D>("Connect"));
        connectButton.positionOffset_X = -100;
        connectButton.positionOffset_Y = 45;
        connectButton.positionScale_X = 0.5f;
        connectButton.positionScale_Y = 0.5f;
        connectButton.sizeOffset_X = 200;
        connectButton.sizeOffset_Y = 30;
        connectButton.text = localization.format("Connect_Button");
        connectButton.tooltip = localization.format("Connect_Button_Tooltip");
        connectButton.iconColor = ESleekTint.FOREGROUND;
        connectButton.onClickedButton += onClickedConnectButton;
        container.AddChild(connectButton);
        Provider.provider.matchmakingService.onAttemptUpdated += onAttemptUpdated;
        Provider.provider.matchmakingService.onTimedOut += onTimedOut;
        if (!isLaunched)
        {
            isLaunched = true;
            ulong lobby;
            if (CommandLine.TryGetSteamConnect(Environment.CommandLine, out var ip, out var queryPort, out var pass))
            {
                SteamConnectionInfo steamConnectionInfo = new SteamConnectionInfo(ip, queryPort, pass);
                UnturnedLog.info("Command-line connect IP: {0} Port: {1} Password: '{2}'", Parser.getIPFromUInt32(steamConnectionInfo.ip), steamConnectionInfo.port, steamConnectionInfo.password);
                connect(steamConnectionInfo, shouldAutoJoin: false);
            }
            else if (CommandLine.tryGetLobby(Environment.CommandLine, out lobby))
            {
                UnturnedLog.info("Lobby: " + lobby);
                Lobbies.joinLobby(new CSteamID(lobby));
            }
        }
        else if (hasPendingServerRelay)
        {
            hasPendingServerRelay = false;
            SteamConnectionInfo steamConnectionInfo2 = new SteamConnectionInfo(serverRelayIP, serverRelayPort, serverRelayPassword);
            UnturnedLog.info("Relay connect IP: {0} Port: {1} Password: '{2}'", Parser.getIPFromUInt32(steamConnectionInfo2.ip), steamConnectionInfo2.port, steamConnectionInfo2.password);
            bool shouldAutoJoin = !serverRelayWaitOnMenu;
            connect(steamConnectionInfo2, shouldAutoJoin);
        }
        bundle.unload();
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_Y = -50;
        backButton.positionScale_Y = 1f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
    }
}
