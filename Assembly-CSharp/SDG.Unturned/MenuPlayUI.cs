using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayUI
{
    private static SleekFullscreenBox container;

    public static bool active;

    private static SleekButtonIcon connectButton;

    private static SleekButtonIcon serversButton;

    private static SleekButtonIcon singleplayerButton;

    private static SleekButtonIcon matchmakingButton;

    private static SleekButtonIcon lobbiesButton;

    private static SleekButtonIcon tutorialButton;

    private static SleekButtonIcon backButton;

    private MenuPlayConnectUI connectUI;

    private MenuPlayServersUI serverListUI;

    private MenuPlayServerInfoUI serverInfoUI;

    private MenuPlaySingleplayerUI singleplayerUI;

    private MenuPlayMatchmakingUI matchmakingUI;

    private MenuPlayLobbiesUI lobbiesUI;

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

    private static void onClickedConnectButton(ISleekElement button)
    {
        MenuPlayConnectUI.open();
        close();
    }

    private static void onClickedServersButton(ISleekElement button)
    {
        MenuPlayServersUI.open();
        close();
    }

    private static void onClickedSingleplayerButton(ISleekElement button)
    {
        MenuPlaySingleplayerUI.open();
        close();
    }

    private static void onClickedMatchmakingButton(ISleekElement button)
    {
        MenuPlayMatchmakingUI.open();
        close();
    }

    private static void onClickedLobbiesButton(ISleekElement button)
    {
        MenuPlayLobbiesUI.open();
        close();
    }

    private static void onClickedTutorialButton(ISleekElement button)
    {
        if (ReadWrite.folderExists("/Worlds/Singleplayer_" + Characters.selected + "/Level/Tutorial"))
        {
            ReadWrite.deleteFolder("/Worlds/Singleplayer_" + Characters.selected + "/Level/Tutorial");
        }
        if (ReadWrite.folderExists("/Worlds/Singleplayer_" + Characters.selected + "/Players/" + Provider.user.ToString() + "_" + Characters.selected + "/Tutorial"))
        {
            ReadWrite.deleteFolder("/Worlds/Singleplayer_" + Characters.selected + "/Players/" + Provider.user.ToString() + "_" + Characters.selected + "/Tutorial");
        }
        Provider.map = "Tutorial";
        Provider.singleplayer(EGameMode.TUTORIAL, singleplayerCheats: false);
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuDashboardUI.open();
        MenuTitleUI.open();
        close();
    }

    public void OnDestroy()
    {
        connectUI.OnDestroy();
        serverListUI.OnDestroy();
        serverInfoUI.OnDestroy();
        singleplayerUI.OnDestroy();
        matchmakingUI.OnDestroy();
        lobbiesUI.OnDestroy();
    }

    public MenuPlayUI()
    {
        Local local = Localization.read("/Menu/Play/MenuPlay.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Play/MenuPlay/MenuPlay.unity3d");
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
        connectButton = new SleekButtonIcon(bundle.load<Texture2D>("Connect"));
        connectButton.positionOffset_X = -100;
        connectButton.positionOffset_Y = 95;
        connectButton.positionScale_X = 0.5f;
        connectButton.positionScale_Y = 0.5f;
        connectButton.sizeOffset_X = 200;
        connectButton.sizeOffset_Y = 50;
        connectButton.text = local.format("ConnectButtonText");
        connectButton.tooltip = local.format("ConnectButtonTooltip");
        connectButton.iconColor = ESleekTint.FOREGROUND;
        connectButton.onClickedButton += onClickedConnectButton;
        connectButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(connectButton);
        serversButton = new SleekButtonIcon(bundle.load<Texture2D>("Servers"));
        serversButton.positionOffset_X = -100;
        serversButton.positionOffset_Y = 35;
        serversButton.positionScale_X = 0.5f;
        serversButton.positionScale_Y = 0.5f;
        serversButton.sizeOffset_X = 200;
        serversButton.sizeOffset_Y = 50;
        serversButton.text = local.format("ServersButtonText");
        serversButton.tooltip = local.format("ServersButtonTooltip");
        serversButton.iconColor = ESleekTint.FOREGROUND;
        serversButton.onClickedButton += onClickedServersButton;
        serversButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(serversButton);
        singleplayerButton = new SleekButtonIcon(bundle.load<Texture2D>("Singleplayer"));
        singleplayerButton.positionOffset_X = -100;
        singleplayerButton.positionOffset_Y = -145;
        singleplayerButton.positionScale_X = 0.5f;
        singleplayerButton.positionScale_Y = 0.5f;
        singleplayerButton.sizeOffset_X = 200;
        singleplayerButton.sizeOffset_Y = 50;
        singleplayerButton.text = local.format("SingleplayerButtonText");
        singleplayerButton.tooltip = local.format("SingleplayerButtonTooltip");
        singleplayerButton.onClickedButton += onClickedSingleplayerButton;
        singleplayerButton.iconColor = ESleekTint.FOREGROUND;
        singleplayerButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(singleplayerButton);
        matchmakingButton = new SleekButtonIcon(bundle.load<Texture2D>("Matchmaking"));
        matchmakingButton.positionOffset_X = -100;
        matchmakingButton.positionOffset_Y = -85;
        matchmakingButton.positionScale_X = 0.5f;
        matchmakingButton.positionScale_Y = 0.5f;
        matchmakingButton.sizeOffset_X = 200;
        matchmakingButton.sizeOffset_Y = 50;
        matchmakingButton.text = local.format("MatchmakingButtonText");
        matchmakingButton.tooltip = local.format("MatchmakingButtonTooltip");
        matchmakingButton.onClickedButton += onClickedMatchmakingButton;
        matchmakingButton.iconColor = ESleekTint.FOREGROUND;
        matchmakingButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(matchmakingButton);
        lobbiesButton = new SleekButtonIcon(bundle.load<Texture2D>("Lobbies"));
        lobbiesButton.positionOffset_X = -100;
        lobbiesButton.positionOffset_Y = -25;
        lobbiesButton.positionScale_X = 0.5f;
        lobbiesButton.positionScale_Y = 0.5f;
        lobbiesButton.sizeOffset_X = 200;
        lobbiesButton.sizeOffset_Y = 50;
        lobbiesButton.text = local.format("LobbiesButtonText");
        lobbiesButton.tooltip = local.format("LobbiesButtonTooltip");
        lobbiesButton.onClickedButton += onClickedLobbiesButton;
        lobbiesButton.iconColor = ESleekTint.FOREGROUND;
        lobbiesButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(lobbiesButton);
        tutorialButton = new SleekButtonIcon(bundle.load<Texture2D>("Tutorial"));
        tutorialButton.positionOffset_X = -100;
        tutorialButton.positionOffset_Y = -205;
        tutorialButton.positionScale_X = 0.5f;
        tutorialButton.positionScale_Y = 0.5f;
        tutorialButton.sizeOffset_X = 200;
        tutorialButton.sizeOffset_Y = 50;
        tutorialButton.text = local.format("TutorialButtonText");
        tutorialButton.tooltip = local.format("TutorialButtonTooltip");
        tutorialButton.onClickedButton += onClickedTutorialButton;
        tutorialButton.fontSize = ESleekFontSize.Medium;
        tutorialButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(tutorialButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.positionOffset_X = -100;
        backButton.positionOffset_Y = 155;
        backButton.positionScale_X = 0.5f;
        backButton.positionScale_Y = 0.5f;
        backButton.sizeOffset_X = 200;
        backButton.sizeOffset_Y = 50;
        backButton.text = MenuDashboardUI.localization.format("BackButtonText");
        backButton.tooltip = MenuDashboardUI.localization.format("BackButtonTooltip");
        backButton.onClickedButton += onClickedBackButton;
        backButton.fontSize = ESleekFontSize.Medium;
        backButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(backButton);
        bundle.unload();
        connectUI = new MenuPlayConnectUI();
        serverListUI = new MenuPlayServersUI();
        serverInfoUI = new MenuPlayServerInfoUI();
        singleplayerUI = new MenuPlaySingleplayerUI();
        matchmakingUI = new MenuPlayMatchmakingUI();
        lobbiesUI = new MenuPlayLobbiesUI();
    }
}
