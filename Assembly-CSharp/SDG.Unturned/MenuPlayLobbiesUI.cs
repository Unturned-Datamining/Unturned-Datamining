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
            button.SizeScale_X = 1f;
            button.SizeScale_Y = 1f;
            button.OnClicked += onClickedPlayerButton;
            AddChild(button);
            avatarImage = Glazier.Get().CreateImage();
            avatarImage.PositionOffset_X = 9f;
            avatarImage.PositionOffset_Y = 9f;
            avatarImage.SizeOffset_X = 32f;
            avatarImage.SizeOffset_Y = 32f;
            avatarImage.Texture = Provider.provider.communityService.getIcon(steamID);
            avatarImage.ShouldDestroyTexture = true;
            button.AddChild(avatarImage);
            nameLabel = Glazier.Get().CreateLabel();
            nameLabel.PositionOffset_X = 40f;
            nameLabel.SizeOffset_X = -40f;
            nameLabel.SizeScale_X = 1f;
            nameLabel.SizeScale_Y = 1f;
            nameLabel.Text = SteamFriends.GetFriendPersonaName(steamID);
            nameLabel.FontSize = ESleekFontSize.Medium;
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
            sleekLobbyPlayerButton.PositionOffset_Y = i * 50;
            sleekLobbyPlayerButton.SizeOffset_Y = 50f;
            sleekLobbyPlayerButton.SizeScale_X = 1f;
            membersBox.AddChild(sleekLobbyPlayerButton);
        }
        membersBox.ContentSizeOffset = new Vector2(0f, lobbyMemberCount * 50);
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
        waitingLabel.IsVisible = waiting;
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
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.PositionScale_Y = 1f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        MenuUI.container.AddChild(container);
        active = false;
        bundle.unload();
        membersLabel = Glazier.Get().CreateLabel();
        membersLabel.PositionOffset_X = -200f;
        membersLabel.PositionOffset_Y = 100f;
        membersLabel.PositionScale_X = 0.5f;
        membersLabel.SizeOffset_X = 400f;
        membersLabel.SizeOffset_Y = 50f;
        membersLabel.Text = localization.format("Members");
        membersLabel.FontSize = ESleekFontSize.Medium;
        container.AddChild(membersLabel);
        membersBox = Glazier.Get().CreateScrollView();
        membersBox.PositionOffset_X = -200f;
        membersBox.PositionOffset_Y = 150f;
        membersBox.PositionScale_X = 0.5f;
        membersBox.SizeOffset_X = 430f;
        membersBox.SizeOffset_Y = -300f;
        membersBox.SizeScale_Y = 1f;
        membersBox.ScaleContentToWidth = true;
        container.AddChild(membersBox);
        inviteButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Invite"));
        inviteButton.PositionOffset_X = -200f;
        inviteButton.PositionOffset_Y = -150f;
        inviteButton.PositionScale_X = 0.5f;
        inviteButton.PositionScale_Y = 1f;
        inviteButton.SizeOffset_X = 400f;
        inviteButton.SizeOffset_Y = 50f;
        inviteButton.text = localization.format("Invite_Button");
        inviteButton.tooltip = localization.format("Invite_Button_Tooltip");
        inviteButton.onClickedButton += onClickedInviteButton;
        inviteButton.fontSize = ESleekFontSize.Medium;
        inviteButton.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(inviteButton);
        waitingLabel = Glazier.Get().CreateLabel();
        waitingLabel.PositionOffset_X = -200f;
        waitingLabel.PositionOffset_Y = -200f;
        waitingLabel.PositionScale_X = 0.5f;
        waitingLabel.PositionScale_Y = 1f;
        waitingLabel.SizeOffset_X = 400f;
        waitingLabel.SizeOffset_Y = 50f;
        waitingLabel.Text = localization.format("Waiting");
        waitingLabel.IsVisible = false;
        container.AddChild(waitingLabel);
        backButton = new SleekButtonIcon(MenuDashboardUI.icons.load<Texture2D>("Exit"));
        backButton.PositionOffset_Y = -50f;
        backButton.PositionScale_Y = 1f;
        backButton.SizeOffset_X = 200f;
        backButton.SizeOffset_Y = 50f;
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
