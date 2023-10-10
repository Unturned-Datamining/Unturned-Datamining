using UnityEngine;

namespace SDG.Unturned;

public class SleekPlayer : SleekWrapper
{
    public enum ESleekPlayerDisplayContext
    {
        NONE,
        GROUP_ROSTER,
        PLAYER_LIST
    }

    private ISleekElement box;

    private ISleekImage avatarImage;

    private ISleekImage repImage;

    private ISleekLabel nameLabel;

    private ISleekLabel repLabel;

    private ISleekImage icon;

    private ISleekImage voice;

    private ISleekImage skillset;

    private ISleekButton muteVoiceChatButton;

    private ISleekButton muteTextChatButton;

    private ESleekPlayerDisplayContext context;

    public SteamPlayer player { get; private set; }

    private void onClickedPlayerButton(ISleekElement button)
    {
        Provider.provider.browserService.open("http://steamcommunity.com/profiles/" + player.playerID.steamID.ToString());
    }

    private void OnMuteVoiceChatClicked(ISleekElement button)
    {
        player.SetVoiceChatLocallyMuted(!player.isVoiceChatLocallyMuted);
        UpdateMuteVoiceChatLabel();
    }

    private void OnMuteTextChatClicked(ISleekElement button)
    {
        player.SetTextChatLocallyMuted(!player.isTextChatLocallyMuted);
        UpdateMuteTextChatLabel();
    }

    private void onClickedPromoteButton(ISleekElement button)
    {
        Player.player.quests.sendPromote(player.playerID.steamID);
    }

    private void onClickedDemoteButton(ISleekElement button)
    {
        Player.player.quests.sendDemote(player.playerID.steamID);
    }

    private void onClickedKickButton(ISleekElement button)
    {
        if (context == ESleekPlayerDisplayContext.GROUP_ROSTER)
        {
            Player.player.quests.sendKickFromGroup(player.playerID.steamID);
        }
        else if (context == ESleekPlayerDisplayContext.PLAYER_LIST)
        {
            ChatManager.sendCallVote(player.playerID.steamID);
            PlayerDashboardUI.close();
            PlayerLifeUI.open();
        }
    }

    private void onClickedInviteButton(ISleekElement button)
    {
        Player.player.quests.sendAskAddGroupInvite(player.playerID.steamID);
    }

    private void onClickedSpyButton(ISleekElement button)
    {
        ChatManager.sendChat(EChatMode.GLOBAL, "/spy " + player.playerID.steamID.ToString());
    }

    private void onTalked(bool isTalking)
    {
        voice.IsVisible = isTalking;
    }

    public override void OnDestroy()
    {
        if (player != null)
        {
            player.player.voice.onTalkingChanged -= onTalked;
        }
    }

    private void UpdateMuteVoiceChatLabel()
    {
        muteVoiceChatButton.Text = (player.isVoiceChatLocallyMuted ? PlayerDashboardInformationUI.localization.format("UnmuteVoiceChat_Label") : PlayerDashboardInformationUI.localization.format("MuteVoiceChat_Label"));
    }

    private void UpdateMuteTextChatLabel()
    {
        muteTextChatButton.Text = (player.isTextChatLocallyMuted ? PlayerDashboardInformationUI.localization.format("UnmuteTextChat_Label") : PlayerDashboardInformationUI.localization.format("MuteTextChat_Label"));
    }

    public SleekPlayer(SteamPlayer newPlayer, bool isButton, ESleekPlayerDisplayContext context)
    {
        player = newPlayer;
        this.context = context;
        Texture2D texture = (OptionsSettings.streamer ? null : ((!Provider.isServer) ? Provider.provider.communityService.getIcon(player.playerID.steamID) : Provider.provider.communityService.getIcon(Provider.user)));
        SleekColor backgroundColor = ESleekTint.BACKGROUND;
        SleekColor textColor = ESleekTint.FOREGROUND;
        if (player.isAdmin && !Provider.isServer)
        {
            backgroundColor = SleekColor.BackgroundIfLight(Palette.ADMIN);
            textColor = Palette.ADMIN;
        }
        else if (player.isPro)
        {
            backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
            textColor = Palette.PRO;
        }
        if (isButton)
        {
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.SizeScale_X = 1f;
            sleekButton.SizeScale_Y = 1f;
            sleekButton.TooltipText = player.playerID.playerName;
            sleekButton.FontSize = ESleekFontSize.Medium;
            sleekButton.BackgroundColor = backgroundColor;
            sleekButton.TextColor = textColor;
            sleekButton.OnClicked += onClickedPlayerButton;
            AddChild(sleekButton);
            box = sleekButton;
        }
        else
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.SizeScale_X = 1f;
            sleekBox.SizeScale_Y = 1f;
            sleekBox.TooltipText = player.playerID.playerName;
            sleekBox.FontSize = ESleekFontSize.Medium;
            sleekBox.BackgroundColor = backgroundColor;
            sleekBox.TextColor = textColor;
            AddChild(sleekBox);
            box = sleekBox;
        }
        avatarImage = Glazier.Get().CreateImage();
        avatarImage.PositionOffset_X = 9f;
        avatarImage.PositionOffset_Y = 9f;
        avatarImage.SizeOffset_X = 32f;
        avatarImage.SizeOffset_Y = 32f;
        avatarImage.Texture = texture;
        avatarImage.ShouldDestroyTexture = true;
        box.AddChild(avatarImage);
        if (player.player != null && player.player.skills != null)
        {
            repImage = Glazier.Get().CreateImage();
            repImage.PositionOffset_X = 46f;
            repImage.PositionOffset_Y = 9f;
            repImage.SizeOffset_X = 32f;
            repImage.SizeOffset_Y = 32f;
            repImage.Texture = PlayerTool.getRepTexture(player.player.skills.reputation);
            repImage.TintColor = PlayerTool.getRepColor(player.player.skills.reputation);
            box.AddChild(repImage);
        }
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 83f;
        nameLabel.SizeOffset_X = -113f;
        nameLabel.SizeOffset_Y = 30f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.Text = player.GetLocalDisplayName();
        nameLabel.FontSize = ESleekFontSize.Medium;
        box.AddChild(nameLabel);
        if (player.player != null && player.player.skills != null)
        {
            repLabel = Glazier.Get().CreateLabel();
            repLabel.PositionOffset_X = 83f;
            repLabel.PositionOffset_Y = 20f;
            repLabel.SizeOffset_X = -113f;
            repLabel.SizeOffset_Y = 30f;
            repLabel.SizeScale_X = 1f;
            repLabel.TextColor = repImage.TintColor;
            repLabel.Text = PlayerTool.getRepTitle(player.player.skills.reputation);
            repLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            box.AddChild(repLabel);
        }
        if (context == ESleekPlayerDisplayContext.GROUP_ROSTER)
        {
            nameLabel.PositionOffset_Y = -5f;
            repLabel.PositionOffset_Y = 10f;
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.PositionOffset_X = 83f;
            sleekLabel.PositionOffset_Y = 25f;
            sleekLabel.SizeOffset_X = -113f;
            sleekLabel.SizeOffset_Y = 30f;
            sleekLabel.SizeScale_X = 1f;
            sleekLabel.TextColor = repImage.TintColor;
            sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            box.AddChild(sleekLabel);
            switch (player.player.quests.groupRank)
            {
            case EPlayerGroupRank.MEMBER:
                sleekLabel.Text = PlayerDashboardInformationUI.localization.format("Group_Rank_Member");
                break;
            case EPlayerGroupRank.ADMIN:
                sleekLabel.Text = PlayerDashboardInformationUI.localization.format("Group_Rank_Admin");
                break;
            case EPlayerGroupRank.OWNER:
                sleekLabel.Text = PlayerDashboardInformationUI.localization.format("Group_Rank_Owner");
                break;
            }
        }
        voice = Glazier.Get().CreateImage();
        voice.PositionOffset_X = 15f;
        voice.PositionOffset_Y = 15f;
        voice.SizeOffset_X = 20f;
        voice.SizeOffset_Y = 20f;
        voice.Texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Voice");
        box.AddChild(voice);
        skillset = Glazier.Get().CreateImage();
        skillset.PositionOffset_X = -25f;
        skillset.PositionOffset_Y = 25f;
        skillset.PositionScale_X = 1f;
        skillset.SizeOffset_X = 20f;
        skillset.SizeOffset_Y = 20f;
        skillset.Texture = MenuSurvivorsCharacterUI.icons.load<Texture2D>("Skillset_" + (int)player.skillset);
        skillset.TintColor = ESleekTint.FOREGROUND;
        box.AddChild(skillset);
        if (player.isAdmin && !Provider.isServer)
        {
            nameLabel.TextColor = Palette.ADMIN;
            nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            icon = Glazier.Get().CreateImage();
            icon.PositionOffset_X = -25f;
            icon.PositionOffset_Y = 5f;
            icon.PositionScale_X = 1f;
            icon.SizeOffset_X = 20f;
            icon.SizeOffset_Y = 20f;
            icon.Texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Admin");
            box.AddChild(icon);
        }
        else if (player.isPro)
        {
            nameLabel.TextColor = Palette.PRO;
            nameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            icon = Glazier.Get().CreateImage();
            icon.PositionOffset_X = -25f;
            icon.PositionOffset_Y = 5f;
            icon.PositionScale_X = 1f;
            icon.SizeOffset_X = 20f;
            icon.SizeOffset_Y = 20f;
            icon.Texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Pro");
            box.AddChild(icon);
        }
        switch (context)
        {
        case ESleekPlayerDisplayContext.GROUP_ROSTER:
        {
            int num2 = 0;
            if (!player.player.channel.IsLocalPlayer)
            {
                if (Player.player.quests.hasPermissionToChangeRank)
                {
                    if (player.player.quests.groupRank < EPlayerGroupRank.OWNER)
                    {
                        ISleekButton sleekButton5 = Glazier.Get().CreateButton();
                        sleekButton5.PositionOffset_X = num2;
                        sleekButton5.PositionScale_X = 1f;
                        sleekButton5.SizeOffset_X = 80f;
                        sleekButton5.SizeScale_Y = 1f;
                        sleekButton5.Text = PlayerDashboardInformationUI.localization.format("Group_Promote");
                        sleekButton5.TooltipText = PlayerDashboardInformationUI.localization.format("Group_Promote_Tooltip");
                        sleekButton5.OnClicked += onClickedPromoteButton;
                        box.AddChild(sleekButton5);
                        num2 += 80;
                    }
                    if (player.player.quests.groupRank == EPlayerGroupRank.ADMIN)
                    {
                        ISleekButton sleekButton6 = Glazier.Get().CreateButton();
                        sleekButton6.PositionOffset_X = num2;
                        sleekButton6.PositionScale_X = 1f;
                        sleekButton6.SizeOffset_X = 80f;
                        sleekButton6.SizeScale_Y = 1f;
                        sleekButton6.Text = PlayerDashboardInformationUI.localization.format("Group_Demote");
                        sleekButton6.TooltipText = PlayerDashboardInformationUI.localization.format("Group_Demote_Tooltip");
                        sleekButton6.OnClicked += onClickedDemoteButton;
                        box.AddChild(sleekButton6);
                        num2 += 80;
                    }
                }
                if (Player.player.quests.hasPermissionToKickMembers && player.player.quests.canBeKickedFromGroup)
                {
                    ISleekButton sleekButton7 = Glazier.Get().CreateButton();
                    sleekButton7.PositionOffset_X = num2;
                    sleekButton7.PositionScale_X = 1f;
                    sleekButton7.SizeOffset_X = 50f;
                    sleekButton7.SizeScale_Y = 1f;
                    sleekButton7.Text = PlayerDashboardInformationUI.localization.format("Group_Kick");
                    sleekButton7.TooltipText = PlayerDashboardInformationUI.localization.format("Group_Kick_Tooltip");
                    sleekButton7.OnClicked += onClickedKickButton;
                    box.AddChild(sleekButton7);
                    num2 += 50;
                }
            }
            box.SizeOffset_X = -num2;
            break;
        }
        case ESleekPlayerDisplayContext.PLAYER_LIST:
        {
            int num = 0;
            if (!player.player.channel.IsLocalPlayer)
            {
                muteVoiceChatButton = Glazier.Get().CreateButton();
                muteVoiceChatButton.PositionScale_X = 1f;
                muteVoiceChatButton.SizeOffset_X = 100f;
                muteVoiceChatButton.SizeScale_Y = 0.5f;
                UpdateMuteVoiceChatLabel();
                muteVoiceChatButton.TooltipText = PlayerDashboardInformationUI.localization.format("Mute_Tooltip");
                muteVoiceChatButton.OnClicked += OnMuteVoiceChatClicked;
                box.AddChild(muteVoiceChatButton);
                muteTextChatButton = Glazier.Get().CreateButton();
                muteTextChatButton.PositionScale_X = 1f;
                muteTextChatButton.PositionScale_Y = 0.5f;
                muteTextChatButton.SizeOffset_X = 100f;
                muteTextChatButton.SizeScale_Y = 0.5f;
                UpdateMuteTextChatLabel();
                muteTextChatButton.TooltipText = PlayerDashboardInformationUI.localization.format("Mute_Tooltip");
                muteTextChatButton.OnClicked += OnMuteTextChatClicked;
                box.AddChild(muteTextChatButton);
                num += 100;
            }
            if (!player.player.channel.IsLocalPlayer && !player.isAdmin)
            {
                ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                sleekButton2.PositionOffset_X = num;
                sleekButton2.PositionScale_X = 1f;
                sleekButton2.SizeOffset_X = 50f;
                sleekButton2.SizeScale_Y = 1f;
                sleekButton2.Text = PlayerDashboardInformationUI.localization.format("Vote_Kick");
                sleekButton2.TooltipText = PlayerDashboardInformationUI.localization.format("Vote_Kick_Tooltip");
                sleekButton2.OnClicked += onClickedKickButton;
                box.AddChild(sleekButton2);
                num += 50;
            }
            if (Player.player != null)
            {
                if (!player.player.channel.IsLocalPlayer && Player.player.quests.isMemberOfAGroup && Player.player.quests.hasPermissionToInviteMembers && Player.player.quests.hasSpaceForMoreMembersInGroup && !player.player.quests.isMemberOfAGroup)
                {
                    ISleekButton sleekButton3 = Glazier.Get().CreateButton();
                    sleekButton3.PositionOffset_X = num;
                    sleekButton3.PositionScale_X = 1f;
                    sleekButton3.SizeOffset_X = 60f;
                    sleekButton3.SizeScale_Y = 1f;
                    sleekButton3.Text = PlayerDashboardInformationUI.localization.format("Group_Invite");
                    sleekButton3.TooltipText = PlayerDashboardInformationUI.localization.format("Group_Invite_Tooltip");
                    sleekButton3.OnClicked += onClickedInviteButton;
                    box.AddChild(sleekButton3);
                    num += 60;
                }
                if (Player.player.channel.owner.isAdmin)
                {
                    ISleekButton sleekButton4 = Glazier.Get().CreateButton();
                    sleekButton4.PositionOffset_X = num;
                    sleekButton4.PositionScale_X = 1f;
                    sleekButton4.SizeOffset_X = 50f;
                    sleekButton4.SizeScale_Y = 1f;
                    sleekButton4.Text = PlayerDashboardInformationUI.localization.format("Spy");
                    sleekButton4.TooltipText = PlayerDashboardInformationUI.localization.format("Spy_Tooltip");
                    sleekButton4.OnClicked += onClickedSpyButton;
                    box.AddChild(sleekButton4);
                    num += 50;
                }
            }
            box.SizeOffset_X = -num;
            break;
        }
        }
        if (player != null)
        {
            player.player.voice.onTalkingChanged += onTalked;
            onTalked(player.player.voice.isTalking);
        }
    }
}
