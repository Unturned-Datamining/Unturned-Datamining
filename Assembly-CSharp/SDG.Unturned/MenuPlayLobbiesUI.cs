using SDG.Provider.Services.Browser;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class MenuPlayLobbiesUI
{
    public class SleekLobbyPlayerButton : SleekWrapper
    {
        private CSteamID steamID;

        private ISleekButton button;

        private ISleekImage avatarImage;

        private ISleekLabel nameLabel;

        private void onClickedPlayerButton(ISleekElement button)
        {
            IBrowserService browserService = Provider.provider.browserService;
            CSteamID cSteamID = steamID;
            browserService.open("http://steamcommunity.com/profiles/" + cSteamID.ToString());
        }

        public SleekLobbyPlayerButton(CSteamID newSteamID)
        {
            steamID = newSteamID;
            button = Glazier.Get().CreateButton();
            button.sizeScale_X = 1f;
            button.sizeScale_Y = 1f;
            button.onClickedButton += onClickedPlayerButton;
            AddChild(button);
            avatarImage = Glazier.Get().CreateImage();
            avatarImage.positionOffset_X = 9;
            avatarImage.positionOffset_Y = 9;
            avatarImage.sizeOffset_X = 32;
            avatarImage.sizeOffset_Y = 32;
            avatarImage.texture = Provider.provider.communityService.getIcon(steamID);
            avatarImage.shouldDestroyTexture = true;
            button.AddChild(avatarImage);
            nameLabel = Glazier.Get().CreateLabel();
            nameLabel.positionOffset_X = 40;
            nameLabel.sizeOffset_X = -40;
            nameLabel.sizeScale_X = 1f;
            nameLabel.sizeScale_Y = 1f;
            nameLabel.text = SteamFriends.GetFriendPersonaName(steamID);
            nameLabel.fontSize = ESleekFontSize.Medium;
            button.AddChild(nameLabel);
        }
    }

    public static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static ISleekLabel membersLabel;

    private static ISleekScrollView membersBox;

    private static SleekButtonIcon inviteButton;

    private static ISleekLabel waitingLabel;

    private static SleekButtonIcon backButton;

    public static void open()
    {
        if (!active)
        {
            active = true;
            if (Lobbies.inLobby)
            {
                setWaitingForLobby(waiting: false);
                refresh();
            }
            else
            {
                setWaitingForLobby(waiting: true);
                Lobbies.createLobby();
            }
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

    private static void refresh()
    {
        membersBox.RemoveAllChildren();
        int lobbyMemberCount = Lobbies.getLobbyMemberCount();
        for (int i = 0; i < lobbyMemberCount; i++)
        {
            SleekLobbyPlayerButton sleekLobbyPlayerButton = new SleekLobbyPlayerButton(Lobbies.getLobbyMemberByIndex(i));
            sleekLobbyPlayerButton.positionOffset_Y = i * 50;
            sleekLobbyPlayerButton.sizeOffset_Y = 50;
            sleekLobbyPlayerButton.sizeScale_X = 1f;
            membersBox.AddChild(sleekLobbyPlayerButton);
        }
        membersBox.contentSizeOffset = new Vector2(0f, lobbyMemberCount * 50);
    }

    private static void handleLobbiesRefreshed()
    {
        if (active)
        {
            refresh();
        }
    }

    private static void handleLobbiesEntered()
    {
        if (active)
        {
            setWaitingForLobby(waiting: false);
            return;
        }
        MenuUI.closeAll();
        open();
    }

    private static void onClickedInviteButton(ISleekElement button)
    {
        if (!Lobbies.canOpenInvitations)
        {
            MenuUI.alert(localization.format("Overlay"));
        }
        else
        {
            Lobbies.openInvitations();
        }
    }

    private static void onClickedBackButton(ISleekElement button)
    {
        MenuPlayUI.open();
        close();
    }

    private static void setWaitingForLobby(bool waiting)
    {
        inviteButton.isClickable = !waiting;
        waitingLabel.isVisible = waiting;
    }

    public void OnDestroy()
    {
        Lobbies.lobbiesRefreshed -= handleLobbiesRefreshed;
        Lobbies.lobbiesEntered -= handleLobbiesEntered;
    }

    public MenuPlayLobbiesUI()
    {
        localization = Localization.read("/Menu/Play/MenuPlayLobbies.dat");
        Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Play/MenuPlayLobbies/MenuPlayLobbies.unity3d");
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
        bundle.unload();
        membersLabel = Glazier.Get().CreateLabel();
        membersLabel.positionOffset_X = -200;
        membersLabel.positionOffset_Y = 100;
        membersLabel.positionScale_X = 0.5f;
        membersLabel.sizeOffset_X = 400;
        membersLabel.sizeOffset_Y = 50;
        membersLabel.text = localization.format("Members");
        membersLabel.fontSize = ESleekFontSize.Medium;
        container.AddChild(membersLabel);
        membersBox = Glazier.Get().CreateScrollView();
        membersBox.positionOffset_X = -200;
        membersBox.positionOffset_Y = 150;
        membersBox.positionScale_X = 0.5f;
        membersBox.sizeOffset_X = 430;
        membersBox.sizeOffset_Y = -300;
        membersBox.sizeScale_Y = 1f;
        membersBox.scaleContentToWidth = true;
        container.AddChild(membersBox);
        inviteButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Invite"));
        inviteButton.positionOffset_X = -200;
        inviteButton.positionOffset_Y = -150;
        inviteButton.positionScale_X = 0.5f;
        inviteButton.positionScale_Y = 1f;
        inviteButton.sizeOffset_X = 400;
        inviteButton.sizeOffset_Y = 50;
        inviteButton.text = localization.format("Invite_Button");
        inviteButton.tooltip = localization.format("Invite_Button_Tooltip");
        inviteButton.onClickedButton += onClickedInviteButton;
        inviteButton.fontSize = ESleekFontSize.Medium;
        inviteButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(inviteButton);
        waitingLabel = Glazier.Get().CreateLabel();
        waitingLabel.positionOffset_X = -200;
        waitingLabel.positionOffset_Y = -200;
        waitingLabel.positionScale_X = 0.5f;
        waitingLabel.positionScale_Y = 1f;
        waitingLabel.sizeOffset_X = 400;
        waitingLabel.sizeOffset_Y = 50;
        waitingLabel.text = localization.format("Waiting");
        waitingLabel.isVisible = false;
        container.AddChild(waitingLabel);
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
        Lobbies.lobbiesRefreshed += handleLobbiesRefreshed;
        Lobbies.lobbiesEntered += handleLobbiesEntered;
    }
}
