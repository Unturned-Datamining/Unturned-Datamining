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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = -1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        connectButton = new SleekButtonIcon(bundle.load<Texture2D>("Connect"));
        connectButton.PositionOffset_X = -100f;
        connectButton.PositionOffset_Y = 95f;
        connectButton.PositionScale_X = 0.5f;
        connectButton.PositionScale_Y = 0.5f;
        connectButton.SizeOffset_X = 200f;
        connectButton.SizeOffset_Y = 50f;
        connectButton.text = local.format("ConnectButtonText");
        connectButton.tooltip = local.format("ConnectButtonTooltip");
        connectButton.iconColor = ESleekTint.FOREGROUND;
        connectButton.onClickedButton += onClickedConnectButton;
        connectButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(connectButton);
        serversButton = new SleekButtonIcon(bundle.load<Texture2D>("Servers"));
        serversButton.PositionOffset_X = -100f;
        serversButton.PositionOffset_Y = 35f;
        serversButton.PositionScale_X = 0.5f;
        serversButton.PositionScale_Y = 0.5f;
        serversButton.SizeOffset_X = 200f;
        serversButton.SizeOffset_Y = 50f;
        serversButton.text = local.format("ServersButtonText");
        serversButton.tooltip = local.format("ServersButtonTooltip");
        serversButton.iconColor = ESleekTint.FOREGROUND;
        serversButton.onClickedButton += onClickedServersButton;
        serversButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(serversButton);
        singleplayerButton = new SleekButtonIcon(bundle.load<Texture2D>("Singleplayer"));
        singleplayerButton.PositionOffset_X = -100f;
        singleplayerButton.PositionOffset_Y = -145f;
        singleplayerButton.PositionScale_X = 0.5f;
        singleplayerButton.PositionScale_Y = 0.5f;
        singleplayerButton.SizeOffset_X = 200f;
        singleplayerButton.SizeOffset_Y = 50f;
        singleplayerButton.text = local.format("SingleplayerButtonText");
        singleplayerButton.tooltip = local.format("SingleplayerButtonTooltip");
        singleplayerButton.onClickedButton += onClickedSingleplayerButton;
        singleplayerButton.iconColor = ESleekTint.FOREGROUND;
        singleplayerButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(singleplayerButton);
        matchmakingButton = new SleekButtonIcon(bundle.load<Texture2D>("Matchmaking"));
        matchmakingButton.PositionOffset_X = -100f;
        matchmakingButton.PositionOffset_Y = -85f;
        matchmakingButton.PositionScale_X = 0.5f;
        matchmakingButton.PositionScale_Y = 0.5f;
        matchmakingButton.SizeOffset_X = 200f;
        matchmakingButton.SizeOffset_Y = 50f;
        matchmakingButton.text = local.format("MatchmakingButtonText");
        matchmakingButton.tooltip = local.format("MatchmakingButtonTooltip");
        matchmakingButton.onClickedButton += onClickedMatchmakingButton;
        matchmakingButton.iconColor = ESleekTint.FOREGROUND;
        matchmakingButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(matchmakingButton);
        lobbiesButton = new SleekButtonIcon(bundle.load<Texture2D>("Lobbies"));
        lobbiesButton.PositionOffset_X = -100f;
        lobbiesButton.PositionOffset_Y = -25f;
        lobbiesButton.PositionScale_X = 0.5f;
        lobbiesButton.PositionScale_Y = 0.5f;
        lobbiesButton.SizeOffset_X = 200f;
        lobbiesButton.SizeOffset_Y = 50f;
        lobbiesButton.text = local.format("LobbiesButtonText");
        lobbiesButton.tooltip = local.format("LobbiesButtonTooltip");
        lobbiesButton.onClickedButton += onClickedLobbiesButton;
        lobbiesButton.iconColor = ESleekTint.FOREGROUND;
        lobbiesButton.fontSize = ESleekFontSize.Medium;
        container.AddChild(lobbiesButton);
        tutorialButton = new SleekButtonIcon(bundle.load<Texture2D>("Tutorial"));
        tutorialButton.PositionOffset_X = -100f;
        tutorialButton.PositionOffset_Y = -205f;
        tutorialButton.PositionScale_X = 0.5f;
        tutorialButton.PositionScale_Y = 0.5f;
        tutorialButton.SizeOffset_X = 200f;
        tutorialButton.SizeOffset_Y = 50f;
        tutorialButton.text = local.format("TutorialButtonText");
        tutorialButton.tooltip = local.format("TutorialButtonTooltip");
        tutorialButton.onClickedButton += onClickedTutorialButton;
        tutorialButton.fontSize = ESleekFontSize.Medium;
        tutorialButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(tutorialButton);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_X = -100f;
        backButton.PositionOffset_Y = 155f;
        backButton.PositionScale_X = 0.5f;
        backButton.PositionScale_Y = 0.5f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
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
