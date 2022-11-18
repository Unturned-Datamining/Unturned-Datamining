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

    private ISleekButton muteButton;

    private ESleekPlayerDisplayContext context;

    public SteamPlayer player { get; private set; }

    private void onClickedPlayerButton(ISleekElement button)
    {
        Provider.provider.browserService.open("http://steamcommunity.com/profiles/" + player.playerID.steamID.ToString());
    }

    private void onClickedMuteButton(ISleekElement button)
    {
        player.isMuted = !player.isMuted;
        muteButton.text = (player.isMuted ? PlayerDashboardInformationUI.localization.format("Mute_Off") : PlayerDashboardInformationUI.localization.format("Mute_On"));
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
        voice.isVisible = isTalking;
    }

    public override void OnDestroy()
    {
        if (player != null)
        {
            player.player.voice.onTalkingChanged -= onTalked;
        }
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
            sleekButton.sizeScale_X = 1f;
            sleekButton.sizeScale_Y = 1f;
            sleekButton.tooltipText = player.playerID.playerName;
            sleekButton.fontSize = ESleekFontSize.Medium;
            sleekButton.backgroundColor = backgroundColor;
            sleekButton.textColor = textColor;
            sleekButton.onClickedButton += onClickedPlayerButton;
            AddChild(sleekButton);
            box = sleekButton;
        }
        else
        {
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.sizeScale_X = 1f;
            sleekBox.sizeScale_Y = 1f;
            sleekBox.tooltipText = player.playerID.playerName;
            sleekBox.fontSize = ESleekFontSize.Medium;
            sleekBox.backgroundColor = backgroundColor;
            sleekBox.textColor = textColor;
            AddChild(sleekBox);
            box = sleekBox;
        }
        avatarImage = Glazier.Get().CreateImage();
        avatarImage.positionOffset_X = 9;
        avatarImage.positionOffset_Y = 9;
        avatarImage.sizeOffset_X = 32;
        avatarImage.sizeOffset_Y = 32;
        avatarImage.texture = texture;
        avatarImage.shouldDestroyTexture = true;
        box.AddChild(avatarImage);
        if (player.player != null && player.player.skills != null)
        {
            repImage = Glazier.Get().CreateImage();
            repImage.positionOffset_X = 46;
            repImage.positionOffset_Y = 9;
            repImage.sizeOffset_X = 32;
            repImage.sizeOffset_Y = 32;
            repImage.texture = PlayerTool.getRepTexture(player.player.skills.reputation);
            repImage.color = PlayerTool.getRepColor(player.player.skills.reputation);
            box.AddChild(repImage);
        }
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionOffset_X = 83;
        nameLabel.sizeOffset_X = -113;
        nameLabel.sizeOffset_Y = 30;
        nameLabel.sizeScale_X = 1f;
        nameLabel.text = player.GetLocalDisplayName();
        nameLabel.fontSize = ESleekFontSize.Medium;
        box.AddChild(nameLabel);
        if (player.player != null && player.player.skills != null)
        {
            repLabel = Glazier.Get().CreateLabel();
            repLabel.positionOffset_X = 83;
            repLabel.positionOffset_Y = 20;
            repLabel.sizeOffset_X = -113;
            repLabel.sizeOffset_Y = 30;
            repLabel.sizeScale_X = 1f;
            repLabel.textColor = repImage.color;
            repLabel.text = PlayerTool.getRepTitle(player.player.skills.reputation);
            repLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            box.AddChild(repLabel);
        }
        if (context == ESleekPlayerDisplayContext.GROUP_ROSTER)
        {
            nameLabel.positionOffset_Y = -5;
            repLabel.positionOffset_Y = 10;
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_X = 83;
            sleekLabel.positionOffset_Y = 25;
            sleekLabel.sizeOffset_X = -113;
            sleekLabel.sizeOffset_Y = 30;
            sleekLabel.sizeScale_X = 1f;
            sleekLabel.textColor = repImage.color;
            sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            box.AddChild(sleekLabel);
            switch (player.player.quests.groupRank)
            {
            case EPlayerGroupRank.MEMBER:
                sleekLabel.text = PlayerDashboardInformationUI.localization.format("Group_Rank_Member");
                break;
            case EPlayerGroupRank.ADMIN:
                sleekLabel.text = PlayerDashboardInformationUI.localization.format("Group_Rank_Admin");
                break;
            case EPlayerGroupRank.OWNER:
                sleekLabel.text = PlayerDashboardInformationUI.localization.format("Group_Rank_Owner");
                break;
            }
        }
        voice = Glazier.Get().CreateImage();
        voice.positionOffset_X = 15;
        voice.positionOffset_Y = 15;
        voice.sizeOffset_X = 20;
        voice.sizeOffset_Y = 20;
        voice.texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Voice");
        box.AddChild(voice);
        skillset = Glazier.Get().CreateImage();
        skillset.positionOffset_X = -25;
        skillset.positionOffset_Y = 25;
        skillset.positionScale_X = 1f;
        skillset.sizeOffset_X = 20;
        skillset.sizeOffset_Y = 20;
        skillset.texture = MenuSurvivorsCharacterUI.icons.load<Texture2D>("Skillset_" + (int)player.skillset);
        skillset.color = ESleekTint.FOREGROUND;
        box.AddChild(skillset);
        if (player.isAdmin && !Provider.isServer)
        {
            nameLabel.textColor = Palette.ADMIN;
            nameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            icon = Glazier.Get().CreateImage();
            icon.positionOffset_X = -25;
            icon.positionOffset_Y = 5;
            icon.positionScale_X = 1f;
            icon.sizeOffset_X = 20;
            icon.sizeOffset_Y = 20;
            icon.texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Admin");
            box.AddChild(icon);
        }
        else if (player.isPro)
        {
            nameLabel.textColor = Palette.PRO;
            nameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            icon = Glazier.Get().CreateImage();
            icon.positionOffset_X = -25;
            icon.positionOffset_Y = 5;
            icon.positionScale_X = 1f;
            icon.sizeOffset_X = 20;
            icon.sizeOffset_Y = 20;
            icon.texture = PlayerDashboardInformationUI.icons.load<Texture2D>("Pro");
            box.AddChild(icon);
        }
        switch (context)
        {
        case ESleekPlayerDisplayContext.GROUP_ROSTER:
        {
            int num2 = 0;
            if (!player.player.channel.isOwner)
            {
                if (Player.player.quests.hasPermissionToChangeRank)
                {
                    if (player.player.quests.groupRank < EPlayerGroupRank.OWNER)
                    {
                        ISleekButton sleekButton5 = Glazier.Get().CreateButton();
                        sleekButton5.positionOffset_X = num2;
                        sleekButton5.positionScale_X = 1f;
                        sleekButton5.sizeOffset_X = 80;
                        sleekButton5.sizeScale_Y = 1f;
                        sleekButton5.text = PlayerDashboardInformationUI.localization.format("Group_Promote");
                        sleekButton5.tooltipText = PlayerDashboardInformationUI.localization.format("Group_Promote_Tooltip");
                        sleekButton5.onClickedButton += onClickedPromoteButton;
                        box.AddChild(sleekButton5);
                        num2 += 80;
                    }
                    if (player.player.quests.groupRank == EPlayerGroupRank.ADMIN)
                    {
                        ISleekButton sleekButton6 = Glazier.Get().CreateButton();
                        sleekButton6.positionOffset_X = num2;
                        sleekButton6.positionScale_X = 1f;
                        sleekButton6.sizeOffset_X = 80;
                        sleekButton6.sizeScale_Y = 1f;
                        sleekButton6.text = PlayerDashboardInformationUI.localization.format("Group_Demote");
                        sleekButton6.tooltipText = PlayerDashboardInformationUI.localization.format("Group_Demote_Tooltip");
                        sleekButton6.onClickedButton += onClickedDemoteButton;
                        box.AddChild(sleekButton6);
                        num2 += 80;
                    }
                }
                if (Player.player.quests.hasPermissionToKickMembers && player.player.quests.canBeKickedFromGroup)
                {
                    ISleekButton sleekButton7 = Glazier.Get().CreateButton();
                    sleekButton7.positionOffset_X = num2;
                    sleekButton7.positionScale_X = 1f;
                    sleekButton7.sizeOffset_X = 50;
                    sleekButton7.sizeScale_Y = 1f;
                    sleekButton7.text = PlayerDashboardInformationUI.localization.format("Group_Kick");
                    sleekButton7.tooltipText = PlayerDashboardInformationUI.localization.format("Group_Kick_Tooltip");
                    sleekButton7.onClickedButton += onClickedKickButton;
                    box.AddChild(sleekButton7);
                    num2 += 50;
                }
            }
            box.sizeOffset_X = -num2;
            break;
        }
        case ESleekPlayerDisplayContext.PLAYER_LIST:
        {
            int num = 0;
            muteButton = Glazier.Get().CreateButton();
            muteButton.positionScale_X = 1f;
            muteButton.sizeOffset_X = 60;
            muteButton.sizeScale_Y = 1f;
            muteButton.text = (player.isMuted ? PlayerDashboardInformationUI.localization.format("Mute_Off") : PlayerDashboardInformationUI.localization.format("Mute_On"));
            muteButton.tooltipText = PlayerDashboardInformationUI.localization.format("Mute_Tooltip");
            muteButton.onClickedButton += onClickedMuteButton;
            box.AddChild(muteButton);
            num += 60;
            if (!player.player.channel.isOwner && !player.isAdmin)
            {
                ISleekButton sleekButton2 = Glazier.Get().CreateButton();
                sleekButton2.positionOffset_X = num;
                sleekButton2.positionScale_X = 1f;
                sleekButton2.sizeOffset_X = 50;
                sleekButton2.sizeScale_Y = 1f;
                sleekButton2.text = PlayerDashboardInformationUI.localization.format("Vote_Kick");
                sleekButton2.tooltipText = PlayerDashboardInformationUI.localization.format("Vote_Kick_Tooltip");
                sleekButton2.onClickedButton += onClickedKickButton;
                box.AddChild(sleekButton2);
                num += 50;
            }
            if (Player.player != null)
            {
                if (!player.player.channel.isOwner && Player.player.quests.isMemberOfAGroup && Player.player.quests.hasPermissionToInviteMembers && Player.player.quests.hasSpaceForMoreMembersInGroup && !player.player.quests.isMemberOfAGroup)
                {
                    ISleekButton sleekButton3 = Glazier.Get().CreateButton();
                    sleekButton3.positionOffset_X = num;
                    sleekButton3.positionScale_X = 1f;
                    sleekButton3.sizeOffset_X = 60;
                    sleekButton3.sizeScale_Y = 1f;
                    sleekButton3.text = PlayerDashboardInformationUI.localization.format("Group_Invite");
                    sleekButton3.tooltipText = PlayerDashboardInformationUI.localization.format("Group_Invite_Tooltip");
                    sleekButton3.onClickedButton += onClickedInviteButton;
                    box.AddChild(sleekButton3);
                    num += 60;
                }
                if (Player.player.channel.owner.isAdmin)
                {
                    ISleekButton sleekButton4 = Glazier.Get().CreateButton();
                    sleekButton4.positionOffset_X = num;
                    sleekButton4.positionScale_X = 1f;
                    sleekButton4.sizeOffset_X = 50;
                    sleekButton4.sizeScale_Y = 1f;
                    sleekButton4.text = PlayerDashboardInformationUI.localization.format("Spy");
                    sleekButton4.tooltipText = PlayerDashboardInformationUI.localization.format("Spy_Tooltip");
                    sleekButton4.onClickedButton += onClickedSpyButton;
                    box.AddChild(sleekButton4);
                    num += 50;
                }
            }
            box.sizeOffset_X = -num;
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
