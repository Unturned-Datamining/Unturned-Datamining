using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerDashboardInformationUI
{
    private class SleekInviteButton : SleekWrapper
    {
        public CSteamID groupID { get; protected set; }

        private void handleJoinButtonClicked(ISleekElement button)
        {
            Player.player.quests.SendAcceptGroupInvitation(groupID);
        }

        private void handleIgnoreButtonClicked(ISleekElement button)
        {
            Player.player.quests.SendDeclineGroupInvitation(groupID);
        }

        public SleekInviteButton(CSteamID newGroupID)
        {
            groupID = newGroupID;
            GroupInfo groupInfo = GroupManager.getGroupInfo(groupID);
            string text = ((groupInfo != null) ? groupInfo.name : groupID.ToString());
            ISleekBox sleekBox = Glazier.Get().CreateBox();
            sleekBox.sizeOffset_X = -140;
            sleekBox.sizeScale_X = 1f;
            sleekBox.sizeScale_Y = 1f;
            sleekBox.text = text;
            AddChild(sleekBox);
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionScale_X = 1f;
            sleekButton.sizeOffset_X = 60;
            sleekButton.sizeScale_Y = 1f;
            sleekButton.text = localization.format("Group_Join");
            sleekButton.tooltipText = localization.format("Group_Join_Tooltip");
            sleekButton.onClickedButton += handleJoinButtonClicked;
            sleekBox.AddChild(sleekButton);
            ISleekButton sleekButton2 = Glazier.Get().CreateButton();
            sleekButton2.positionOffset_X = 60;
            sleekButton2.positionScale_X = 1f;
            sleekButton2.sizeOffset_X = 80;
            sleekButton2.sizeScale_Y = 1f;
            sleekButton2.text = localization.format("Group_Ignore");
            sleekButton2.tooltipText = localization.format("Group_Ignore_Tooltip");
            sleekButton2.onClickedButton += handleIgnoreButtonClicked;
            sleekBox.AddChild(sleekButton2);
        }
    }

    private enum EInfoTab
    {
        QUESTS,
        GROUPS,
        PLAYERS
    }

    private static readonly List<SteamPlayer> sortedClients = new List<SteamPlayer>();

    public static Local localization;

    public static Bundle icons;

    private static SleekFullscreenBox container;

    public static bool active;

    private static int zoomMultiplier;

    private static int maxZoomMultiplier;

    private static ISleekBox backdropBox;

    private static ISleekElement mapInspect;

    private static ISleekScrollView mapBox;

    private static ISleekImage mapImage;

    private static ISleekElement mapLocationsContainer;

    private static ISleekElement mapArenaContainer;

    private static ISleekElement mapMarkersContainer;

    private static ISleekElement mapRemotePlayersContainer;

    private static List<ISleekImage> markerImages;

    private static List<ISleekImage> arenaTargetPoints;

    private static List<ISleekImage> remotePlayerImages;

    private static ISleekImage localPlayerImage;

    private static ISleekImage arenaAreaCurrentOverlay;

    private static ISleekImage arenaAreaCurrentLeftOverlay;

    private static ISleekImage arenaAreaCurrentRightOverlay;

    private static ISleekImage arenaAreaCurrentUpOverlay;

    private static ISleekImage arenaAreaCurrentDownOverlay;

    private static SleekButtonIcon zoomInButton;

    private static SleekButtonIcon zoomOutButton;

    private static SleekButtonIcon centerButton;

    private static SleekButtonState mapButtonState;

    public static ISleekLabel noLabel;

    private static ISleekElement headerButtonsContainer;

    private static SleekButtonIcon questsButton;

    private static SleekButtonIcon groupsButton;

    private static SleekButtonIcon playersButton;

    private static ISleekScrollView questsBox;

    private static ISleekScrollView groupsBox;

    private static ISleekElement playersBox;

    private static SleekButtonState playerSortButton;

    private static SleekList<SteamPlayer> playersList;

    private static ISleekFloat64Field radioFrequencyField;

    private static ISleekField groupNameField;

    private static bool hasChart;

    private static bool hasGPS;

    private static EInfoTab tab;

    private static Texture2D mapTexture;

    private static Texture2D chartTexture;

    private static Texture2D staticTexture;

    private static List<PlayerQuest> displayedQuests = new List<PlayerQuest>();

    private const string playerListSortKey = "PlayerListSortMode";

    private static void synchronizeMapVisibility(int view)
    {
        if (view == 0)
        {
            if (chartTexture != null && !PlayerUI.isBlindfolded && hasChart)
            {
                mapImage.texture = chartTexture;
                noLabel.isVisible = false;
            }
            else
            {
                mapImage.texture = staticTexture;
                noLabel.text = localization.format("No_Chart");
                noLabel.isVisible = true;
            }
        }
        else if (mapTexture != null && !PlayerUI.isBlindfolded && hasGPS)
        {
            mapImage.texture = mapTexture;
            noLabel.isVisible = false;
        }
        else
        {
            mapImage.texture = staticTexture;
            noLabel.text = localization.format("No_GPS");
            noLabel.isVisible = true;
        }
        bool flag = !noLabel.isVisible;
        mapLocationsContainer.isVisible = flag;
        bool flag2 = flag && Provider.modeConfigData.Gameplay.Group_Map;
        mapMarkersContainer.isVisible = flag2;
        mapArenaContainer.isVisible = flag2 && LevelManager.levelType == ELevelType.ARENA;
        mapRemotePlayersContainer.isVisible = flag2;
        localPlayerImage.isVisible = flag2;
    }

    private static void updateMarkers()
    {
        int num = 0;
        foreach (SteamPlayer client in Provider.clients)
        {
            if (client.model == null)
            {
                continue;
            }
            PlayerQuests quests = client.player.quests;
            if ((!(client.playerID.steamID != Provider.client) || quests.isMemberOfSameGroupAs(Player.player)) && quests.isMarkerPlaced)
            {
                ISleekImage sleekImage;
                if (num < markerImages.Count)
                {
                    sleekImage = markerImages[num];
                    sleekImage.isVisible = true;
                }
                else
                {
                    sleekImage = Glazier.Get().CreateImage(icons.load<Texture2D>("Marker"));
                    sleekImage.positionOffset_X = -10;
                    sleekImage.positionOffset_Y = -10;
                    sleekImage.sizeOffset_X = 20;
                    sleekImage.sizeOffset_Y = 20;
                    sleekImage.addLabel(string.Empty, ESleekSide.RIGHT);
                    mapMarkersContainer.AddChild(sleekImage);
                    markerImages.Add(sleekImage);
                }
                num++;
                Vector2 vector = ProjectWorldPositionToMap(quests.markerPosition);
                sleekImage.positionScale_X = vector.x;
                sleekImage.positionScale_Y = vector.y;
                sleekImage.color = client.markerColor;
                string text = quests.markerTextOverride;
                if (string.IsNullOrEmpty(text))
                {
                    text = ((!string.IsNullOrEmpty(client.playerID.nickName)) ? client.playerID.nickName : client.playerID.characterName);
                }
                sleekImage.updateLabel(text);
            }
        }
        for (int num2 = markerImages.Count - 1; num2 >= num; num2--)
        {
            markerImages[num2].isVisible = false;
        }
    }

    private static void updateArenaCircle()
    {
        int num = 0;
        if ((double)Mathf.Abs(LevelManager.arenaTargetRadius - 0.5f) > 0.01)
        {
            num = Mathf.RoundToInt(Mathf.Lerp(10f, 64f, LevelManager.arenaTargetRadius / 2000f));
            num *= zoomMultiplier;
            if (num > 1)
            {
                float num2 = Time.time / 100f;
                num2 -= Mathf.Floor(num2);
                for (int i = 0; i < num; i++)
                {
                    float f = ((float)i / (float)num + num2) * (float)Math.PI * 2f;
                    float num3 = Mathf.Cos(f);
                    float num4 = Mathf.Sin(f);
                    Vector2 vector = ProjectWorldPositionToMap(LevelManager.arenaTargetCenter + new Vector3(num3 * LevelManager.arenaTargetRadius, 0f, num4 * LevelManager.arenaTargetRadius));
                    ISleekImage sleekImage;
                    if (i < arenaTargetPoints.Count)
                    {
                        sleekImage = arenaTargetPoints[i];
                        sleekImage.isVisible = true;
                    }
                    else
                    {
                        sleekImage = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
                        sleekImage.sizeOffset_X = 2;
                        sleekImage.sizeOffset_Y = 2;
                        sleekImage.color = new Color(1f, 1f, 0f, 1f);
                        arenaTargetPoints.Add(sleekImage);
                        mapArenaContainer.AddChild(sleekImage);
                    }
                    sleekImage.positionScale_X = vector.x;
                    sleekImage.positionScale_Y = vector.y;
                }
            }
        }
        for (int num5 = arenaTargetPoints.Count - 1; num5 >= num; num5--)
        {
            arenaTargetPoints[num5].isVisible = false;
        }
        Vector2 vector2 = ProjectWorldPositionToMap(LevelManager.arenaCurrentCenter);
        float num6 = (float)(int)Level.size - (float)(int)Level.border * 2f;
        float num7 = LevelManager.arenaCurrentRadius / num6;
        float num8 = num7 * 2f;
        arenaAreaCurrentOverlay.positionScale_X = vector2.x - num7;
        arenaAreaCurrentOverlay.positionScale_Y = vector2.y - num7;
        arenaAreaCurrentOverlay.sizeScale_X = num8;
        arenaAreaCurrentOverlay.sizeScale_Y = num8;
        arenaAreaCurrentLeftOverlay.positionScale_Y = arenaAreaCurrentOverlay.positionScale_Y;
        arenaAreaCurrentLeftOverlay.sizeScale_X = arenaAreaCurrentOverlay.positionScale_X;
        arenaAreaCurrentLeftOverlay.sizeScale_Y = arenaAreaCurrentOverlay.sizeScale_Y;
        arenaAreaCurrentRightOverlay.positionScale_X = arenaAreaCurrentOverlay.positionScale_X + arenaAreaCurrentOverlay.sizeScale_X;
        arenaAreaCurrentRightOverlay.positionScale_Y = arenaAreaCurrentOverlay.positionScale_Y;
        arenaAreaCurrentRightOverlay.sizeScale_X = 1f - arenaAreaCurrentOverlay.positionScale_X - arenaAreaCurrentOverlay.sizeScale_X;
        arenaAreaCurrentRightOverlay.sizeScale_Y = arenaAreaCurrentOverlay.sizeScale_Y;
        arenaAreaCurrentUpOverlay.sizeScale_Y = arenaAreaCurrentOverlay.positionScale_Y;
        arenaAreaCurrentDownOverlay.positionScale_Y = arenaAreaCurrentOverlay.positionScale_Y + arenaAreaCurrentOverlay.sizeScale_Y;
        arenaAreaCurrentDownOverlay.sizeScale_Y = 1f - arenaAreaCurrentOverlay.positionScale_Y - arenaAreaCurrentOverlay.sizeScale_Y;
    }

    private static void updateRemotePlayerAvatars()
    {
        int num = 0;
        bool areSpecStatsVisible = Player.player.look.areSpecStatsVisible;
        foreach (SteamPlayer client in Provider.clients)
        {
            if (client.model == null || client.playerID.steamID == Provider.client)
            {
                continue;
            }
            bool flag = client.player.quests.isMemberOfSameGroupAs(Player.player);
            if (areSpecStatsVisible || flag)
            {
                ISleekImage sleekImage;
                if (num < remotePlayerImages.Count)
                {
                    sleekImage = remotePlayerImages[num];
                    sleekImage.isVisible = true;
                }
                else
                {
                    sleekImage = Glazier.Get().CreateImage();
                    sleekImage.positionOffset_X = -10;
                    sleekImage.positionOffset_Y = -10;
                    sleekImage.sizeOffset_X = 20;
                    sleekImage.sizeOffset_Y = 20;
                    sleekImage.addLabel(string.Empty, ESleekSide.RIGHT);
                    mapRemotePlayersContainer.AddChild(sleekImage);
                    remotePlayerImages.Add(sleekImage);
                }
                num++;
                Vector2 vector = ProjectWorldPositionToMap(client.player.transform.position);
                sleekImage.positionScale_X = vector.x;
                sleekImage.positionScale_Y = vector.y;
                if (OptionsSettings.streamer)
                {
                    sleekImage.texture = null;
                }
                else
                {
                    sleekImage.texture = Provider.provider.communityService.getIcon(client.playerID.steamID, shouldCache: true);
                }
                if (flag && !string.IsNullOrEmpty(client.playerID.nickName))
                {
                    sleekImage.updateLabel(client.playerID.nickName);
                }
                else
                {
                    sleekImage.updateLabel(client.playerID.characterName);
                }
            }
        }
        for (int num2 = remotePlayerImages.Count - 1; num2 >= num; num2--)
        {
            remotePlayerImages[num2].isVisible = false;
        }
    }

    public static void updateDynamicMap()
    {
        if (mapMarkersContainer.isVisible)
        {
            updateMarkers();
        }
        if (mapArenaContainer.isVisible)
        {
            updateArenaCircle();
        }
        if (mapRemotePlayersContainer.isVisible)
        {
            updateRemotePlayerAvatars();
        }
        if (localPlayerImage.isVisible && Player.player != null)
        {
            Vector2 vector = ProjectWorldPositionToMap(Player.player.transform.position);
            localPlayerImage.positionScale_X = vector.x;
            localPlayerImage.positionScale_Y = vector.y;
            localPlayerImage.angle = ProjectWorldRotationToMap(Player.player.transform.rotation.eulerAngles.y);
        }
    }

    protected static void searchForMapsInInventory(ref bool enableChart, ref bool enableMap)
    {
        if (enableChart & enableMap)
        {
            return;
        }
        for (byte b = 0; b < PlayerInventory.PAGES - 2; b = (byte)(b + 1))
        {
            Items items = Player.player.inventory.items[b];
            if (items != null)
            {
                foreach (ItemJar item in items.items)
                {
                    if (item != null)
                    {
                        ItemMapAsset asset = item.GetAsset<ItemMapAsset>();
                        if (asset != null)
                        {
                            enableChart |= asset.enablesChart;
                            enableMap |= asset.enablesMap;
                        }
                        if (enableChart & enableMap)
                        {
                            return;
                        }
                    }
                }
            }
        }
    }

    public static void open()
    {
        if (!active)
        {
            active = true;
            hasChart = Provider.modeConfigData.Gameplay.Chart || Level.info.type != ELevelType.SURVIVAL;
            hasGPS = Provider.modeConfigData.Gameplay.Satellite || Level.info.type != ELevelType.SURVIVAL;
            searchForMapsInInventory(ref hasChart, ref hasGPS);
            if (hasChart && !hasGPS)
            {
                mapButtonState.state = 0;
            }
            if (hasGPS && !hasChart)
            {
                mapButtonState.state = 1;
            }
            synchronizeMapVisibility(mapButtonState.state);
            updateDynamicMap();
            questsButton.text = localization.format("Quests", Player.player.quests.countValidQuests());
            if (OptionsSettings.streamer)
            {
                playersButton.text = localization.format("Streamer");
            }
            else
            {
                playersButton.text = localization.format("Players", Provider.clients.Count, Provider.maxPlayers);
            }
            switch (tab)
            {
            case EInfoTab.GROUPS:
                openGroups();
                break;
            case EInfoTab.QUESTS:
                openQuests();
                break;
            case EInfoTab.PLAYERS:
                openPlayers();
                break;
            }
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            container.AnimateOutOfView(0f, 1f);
        }
    }

    public static void openQuests()
    {
        tab = EInfoTab.QUESTS;
        questsBox.RemoveAllChildren();
        displayedQuests.Clear();
        int num = 0;
        foreach (PlayerQuest quests in Player.player.quests.questsList)
        {
            if (quests != null && quests.asset != null)
            {
                displayedQuests.Add(quests);
                bool flag = quests.asset.areConditionsMet(Player.player);
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.positionOffset_Y = num;
                sleekButton.sizeOffset_Y = 50;
                sleekButton.sizeScale_X = 1f;
                sleekButton.onClickedButton += onClickedQuestButton;
                questsBox.AddChild(sleekButton);
                ISleekImage sleekImage = Glazier.Get().CreateImage(icons.load<Texture2D>(flag ? "Complete" : "Incomplete"));
                sleekImage.positionOffset_X = 5;
                sleekImage.positionOffset_Y = 5;
                sleekImage.sizeOffset_X = 40;
                sleekImage.sizeOffset_Y = 40;
                sleekButton.AddChild(sleekImage);
                ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                sleekLabel.positionOffset_X = 50;
                sleekLabel.sizeOffset_X = -55;
                sleekLabel.sizeScale_X = 1f;
                sleekLabel.sizeScale_Y = 1f;
                sleekLabel.fontAlignment = TextAnchor.MiddleLeft;
                sleekLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
                sleekLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
                sleekLabel.enableRichText = true;
                sleekLabel.fontSize = ESleekFontSize.Medium;
                sleekLabel.text = quests.asset.questName;
                sleekButton.AddChild(sleekLabel);
                num += sleekButton.sizeOffset_Y + 10;
            }
        }
        questsBox.contentSizeOffset = new Vector2(0f, num - 10);
        updateTabs();
    }

    private static void onClickedTuneButton(ISleekElement button)
    {
        uint num = (uint)(radioFrequencyField.state * 1000.0);
        if (num < 300000)
        {
            num = 300000u;
        }
        else if (num > 900000)
        {
            num = 900000u;
        }
        radioFrequencyField.state = (double)num / 1000.0;
        Player.player.quests.sendSetRadioFrequency(num);
    }

    private static void onClickedResetButton(ISleekElement button)
    {
        radioFrequencyField.state = (double)PlayerQuests.DEFAULT_RADIO_FREQUENCY / 1000.0;
        onClickedTuneButton(button);
    }

    private static void onClickedRenameButton(ISleekElement button)
    {
        Player.player.quests.sendRenameGroup(groupNameField.text);
    }

    private static void onClickedMainGroupButton(ISleekElement button)
    {
        Player.player.quests.SendAcceptGroupInvitation(Characters.active.group);
    }

    private static void onClickedLeaveGroupButton(ISleekElement button)
    {
        Player.player.quests.sendLeaveGroup();
    }

    private static void onClickedDeleteGroupButton(SleekButtonIconConfirm button)
    {
        Player.player.quests.sendDeleteGroup();
    }

    private static void onClickedCreateGroupButton(ISleekElement button)
    {
        Player.player.quests.sendCreateGroup();
    }

    private static void refreshGroups()
    {
        if (!active)
        {
            return;
        }
        groupsBox.RemoveAllChildren();
        int num = 0;
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.positionOffset_Y = num;
        sleekBox.sizeOffset_X = 125;
        sleekBox.sizeOffset_Y = 30;
        sleekBox.text = localization.format("Radio_Frequency_Label");
        groupsBox.AddChild(sleekBox);
        radioFrequencyField = Glazier.Get().CreateFloat64Field();
        radioFrequencyField.positionOffset_X = 125;
        radioFrequencyField.sizeOffset_X = -225;
        radioFrequencyField.positionOffset_Y = num;
        radioFrequencyField.sizeOffset_Y = 30;
        radioFrequencyField.sizeScale_X = 1f;
        radioFrequencyField.state = (double)Player.player.quests.radioFrequency / 1000.0;
        groupsBox.AddChild(radioFrequencyField);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.positionOffset_X = -100;
        sleekButton.positionScale_X = 1f;
        sleekButton.sizeOffset_X = 50;
        sleekButton.sizeOffset_Y = 30;
        sleekButton.text = localization.format("Radio_Frequency_Tune");
        sleekButton.tooltipText = localization.format("Radio_Frequency_Tune_Tooltip");
        sleekButton.onClickedButton += onClickedTuneButton;
        groupsBox.AddChild(sleekButton);
        ISleekButton sleekButton2 = Glazier.Get().CreateButton();
        sleekButton2.positionOffset_X = -50;
        sleekButton2.positionScale_X = 1f;
        sleekButton2.sizeOffset_X = 50;
        sleekButton2.sizeOffset_Y = 30;
        sleekButton2.text = localization.format("Radio_Frequency_Reset");
        sleekButton2.tooltipText = localization.format("Radio_Frequency_Reset_Tooltip");
        sleekButton2.onClickedButton += onClickedResetButton;
        groupsBox.AddChild(sleekButton2);
        num += 30;
        PlayerQuests quests = Player.player.quests;
        if (quests.isMemberOfAGroup)
        {
            if (Characters.active.group == quests.groupID)
            {
                SteamGroup cachedGroup = Provider.provider.communityService.getCachedGroup(Characters.active.group);
                if (cachedGroup != null)
                {
                    SleekBoxIcon sleekBoxIcon = new SleekBoxIcon(cachedGroup.icon, 40);
                    sleekBoxIcon.positionOffset_Y = num;
                    sleekBoxIcon.sizeOffset_Y = 50;
                    sleekBoxIcon.sizeScale_X = 1f;
                    sleekBoxIcon.text = cachedGroup.name;
                    groupsBox.AddChild(sleekBoxIcon);
                    num += 50;
                }
            }
            else
            {
                GroupInfo groupInfo = GroupManager.getGroupInfo(quests.groupID);
                string text = ((groupInfo != null) ? groupInfo.name : quests.groupID.ToString());
                if (quests.groupRank == EPlayerGroupRank.OWNER)
                {
                    groupNameField = Glazier.Get().CreateStringField();
                    groupNameField.positionOffset_Y = num;
                    groupNameField.maxLength = 32;
                    groupNameField.text = text;
                    groupNameField.sizeOffset_X = -100;
                    groupNameField.sizeOffset_Y = 30;
                    groupNameField.sizeScale_X = 1f;
                    groupsBox.AddChild(groupNameField);
                    ISleekButton sleekButton3 = Glazier.Get().CreateButton();
                    sleekButton3.positionScale_X = 1f;
                    sleekButton3.positionOffset_X = -100;
                    sleekButton3.positionOffset_Y = num;
                    sleekButton3.sizeOffset_X = 100;
                    sleekButton3.sizeOffset_Y = 30;
                    sleekButton3.text = localization.format("Group_Rename");
                    sleekButton3.tooltipText = localization.format("Group_Rename_Tooltip");
                    sleekButton3.onClickedButton += onClickedRenameButton;
                    groupsBox.AddChild(sleekButton3);
                }
                else
                {
                    ISleekBox sleekBox2 = Glazier.Get().CreateBox();
                    sleekBox2.positionOffset_Y = num;
                    sleekBox2.sizeOffset_Y = 30;
                    sleekBox2.sizeScale_X = 1f;
                    sleekBox2.text = text;
                    groupsBox.AddChild(sleekBox2);
                }
                num += 30;
                if (quests.useMaxGroupMembersLimit)
                {
                    ISleekBox sleekBox3 = Glazier.Get().CreateBox();
                    sleekBox3.positionOffset_Y = num;
                    sleekBox3.sizeOffset_Y = 30;
                    sleekBox3.sizeScale_X = 1f;
                    sleekBox3.text = localization.format("Group_Members", groupInfo.members, Provider.modeConfigData.Gameplay.Max_Group_Members);
                    groupsBox.AddChild(sleekBox3);
                    num += 30;
                }
            }
            if (quests.hasPermissionToLeaveGroup)
            {
                SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
                sleekButtonIcon.positionOffset_Y = num;
                sleekButtonIcon.sizeOffset_Y = 30;
                sleekButtonIcon.sizeScale_X = 1f;
                sleekButtonIcon.text = localization.format("Group_Leave");
                sleekButtonIcon.tooltip = localization.format("Group_Leave_Tooltip");
                sleekButtonIcon.onClickedButton += onClickedLeaveGroupButton;
                groupsBox.AddChild(sleekButtonIcon);
                num += 30;
            }
            if (quests.hasPermissionToDeleteGroup)
            {
                SleekButtonIconConfirm sleekButtonIconConfirm = new SleekButtonIconConfirm(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"), localization.format("Group_Delete_Confirm"), localization.format("Group_Delete_Confirm_Tooltip"), localization.format("Group_Delete_Deny"), localization.format("Group_Delete_Deny_Tooltip"));
                sleekButtonIconConfirm.positionOffset_Y = num;
                sleekButtonIconConfirm.sizeOffset_Y = 30;
                sleekButtonIconConfirm.sizeScale_X = 1f;
                sleekButtonIconConfirm.text = localization.format("Group_Delete");
                sleekButtonIconConfirm.tooltip = localization.format("Group_Delete_Tooltip");
                sleekButtonIconConfirm.onConfirmed = (Confirm)Delegate.Combine(sleekButtonIconConfirm.onConfirmed, new Confirm(onClickedDeleteGroupButton));
                groupsBox.AddChild(sleekButtonIconConfirm);
                num += 30;
            }
            foreach (SteamPlayer client in Provider.clients)
            {
                if (!(client.player == null) && client.player.quests.isMemberOfSameGroupAs(Player.player))
                {
                    SleekPlayer sleekPlayer = new SleekPlayer(client, isButton: true, SleekPlayer.ESleekPlayerDisplayContext.GROUP_ROSTER);
                    sleekPlayer.positionOffset_Y = num;
                    sleekPlayer.sizeOffset_Y = 50;
                    sleekPlayer.sizeScale_X = 1f;
                    groupsBox.AddChild(sleekPlayer);
                    num += 50;
                }
            }
        }
        else
        {
            if (Characters.active.group != CSteamID.Nil && Provider.modeConfigData.Gameplay.Allow_Static_Groups)
            {
                SteamGroup cachedGroup2 = Provider.provider.communityService.getCachedGroup(Characters.active.group);
                if (cachedGroup2 != null)
                {
                    SleekButtonIcon sleekButtonIcon2 = new SleekButtonIcon(cachedGroup2.icon, 40);
                    sleekButtonIcon2.positionOffset_Y = num;
                    sleekButtonIcon2.sizeOffset_Y = 50;
                    sleekButtonIcon2.sizeScale_X = 1f;
                    sleekButtonIcon2.text = cachedGroup2.name;
                    sleekButtonIcon2.onClickedButton += onClickedMainGroupButton;
                    groupsBox.AddChild(sleekButtonIcon2);
                    num += 50;
                }
            }
            foreach (CSteamID groupInvite in quests.groupInvites)
            {
                SleekInviteButton sleekInviteButton = new SleekInviteButton(groupInvite);
                sleekInviteButton.positionOffset_Y = num;
                sleekInviteButton.sizeOffset_Y = 30;
                sleekInviteButton.sizeScale_X = 1f;
                groupsBox.AddChild(sleekInviteButton);
                num += 30;
            }
            if (Player.player.quests.hasPermissionToCreateGroup)
            {
                SleekButtonIcon sleekButtonIcon3 = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
                sleekButtonIcon3.positionOffset_Y = num;
                sleekButtonIcon3.sizeOffset_Y = 30;
                sleekButtonIcon3.sizeScale_X = 1f;
                sleekButtonIcon3.text = localization.format("Group_Create");
                sleekButtonIcon3.tooltip = localization.format("Group_Create_Tooltip");
                sleekButtonIcon3.onClickedButton += onClickedCreateGroupButton;
                groupsBox.AddChild(sleekButtonIcon3);
                num += 30;
            }
        }
        groupsBox.contentSizeOffset = new Vector2(0f, num);
    }

    private static void handleGroupUpdated(PlayerQuests sender)
    {
        refreshGroups();
    }

    private static void handleGroupInfoReady(GroupInfo group)
    {
        refreshGroups();
    }

    public static void openGroups()
    {
        tab = EInfoTab.GROUPS;
        refreshGroups();
        updateTabs();
    }

    public static void openPlayers()
    {
        tab = EInfoTab.PLAYERS;
        SortAndRebuildPlayers();
        updateTabs();
    }

    private static void SortAndRebuildPlayers()
    {
        sortedClients.Clear();
        sortedClients.AddRange(Provider.clients);
        if (playerSortButton.state == 0)
        {
            playersList.onCreateElement = OnCreatePlayerEntry;
            sortedClients.Sort((SteamPlayer lhs, SteamPlayer rhs) => lhs.GetLocalDisplayName().CompareTo(rhs.GetLocalDisplayName()));
        }
        else
        {
            playersList.onCreateElement = OnCreatePlayerEntryWithGrouping;
            sortedClients.Sort(delegate(SteamPlayer lhs, SteamPlayer rhs)
            {
                int num = lhs.player.quests.groupID.CompareTo(rhs.player.quests.groupID);
                return (num != 0) ? num : lhs.GetLocalDisplayName().CompareTo(rhs.GetLocalDisplayName());
            });
        }
        playersList.ForceRebuildElements();
    }

    private static void updateTabs()
    {
        questsBox.isVisible = tab == EInfoTab.QUESTS;
        groupsBox.isVisible = tab == EInfoTab.GROUPS;
        playersBox.isVisible = tab == EInfoTab.PLAYERS;
    }

    private static void updateZoom()
    {
        mapBox.contentScaleFactor = zoomMultiplier;
    }

    public static void focusPoint(Vector3 point)
    {
        Vector2 normalizedStateCenter = ProjectWorldPositionToMap(point);
        mapBox.normalizedStateCenter = normalizedStateCenter;
    }

    private static float ProjectWorldRotationToMap(float yaw)
    {
        CartographyVolume mainVolume = VolumeManager<CartographyVolume, CartographyVolumeManager>.Get().GetMainVolume();
        if (mainVolume != null)
        {
            return yaw - mainVolume.transform.eulerAngles.y;
        }
        return yaw;
    }

    private static Vector2 ProjectWorldPositionToMap(Vector3 worldPosition)
    {
        CartographyVolume mainVolume = VolumeManager<CartographyVolume, CartographyVolumeManager>.Get().GetMainVolume();
        if (mainVolume != null)
        {
            Vector3 vector = mainVolume.transform.InverseTransformPoint(worldPosition);
            return new Vector2(vector.x + 0.5f, 0.5f - vector.z);
        }
        float num = (float)(int)Level.size - (float)(int)Level.border * 2f;
        return new Vector2(worldPosition.x / num + 0.5f, 0.5f - worldPosition.z / num);
    }

    private static Vector3 DeprojectMapToWorld(Vector2 mapPosition)
    {
        CartographyVolume mainVolume = VolumeManager<CartographyVolume, CartographyVolumeManager>.Get().GetMainVolume();
        if (mainVolume != null)
        {
            Vector3 position = new Vector3(mapPosition.x - 0.5f, 0f, 0.5f - mapPosition.y);
            return mainVolume.transform.TransformPoint(position).GetHorizontal();
        }
        float num = (float)(int)Level.size - (float)(int)Level.border * 2f;
        return new Vector3((mapPosition.x - 0.5f) * num, 0f, (0.5f - mapPosition.y) * num);
    }

    private static void onRightClickedMap()
    {
        Vector2 normalizedCursorPosition = mapImage.GetNormalizedCursorPosition();
        Vector3 newMarkerPosition = DeprojectMapToWorld(normalizedCursorPosition);
        PlayerQuests quests = Player.player.quests;
        bool newIsMarkerPlaced = !quests.isMarkerPlaced || Vector2.Distance(ProjectWorldPositionToMap(quests.markerPosition), normalizedCursorPosition) * mapBox.contentSizeOffset.x > 15f;
        quests.sendSetMarker(newIsMarkerPlaced, newMarkerPosition);
    }

    private static void onClickedZoomInButton(ISleekElement button)
    {
        if (zoomMultiplier < maxZoomMultiplier)
        {
            zoomMultiplier++;
            Vector2 normalizedStateCenter = mapBox.normalizedStateCenter;
            updateZoom();
            mapBox.normalizedStateCenter = normalizedStateCenter;
        }
    }

    private static void onClickedZoomOutButton(ISleekElement button)
    {
        if (zoomMultiplier > 1)
        {
            zoomMultiplier--;
            Vector2 normalizedStateCenter = mapBox.normalizedStateCenter;
            updateZoom();
            mapBox.normalizedStateCenter = normalizedStateCenter;
        }
    }

    private static void onClickedCenterButton(ISleekElement button)
    {
        focusPoint(Player.player.transform.position);
    }

    private static void onSwappedMapState(SleekButtonState button, int index)
    {
        synchronizeMapVisibility(index);
        updateDynamicMap();
    }

    private static void onClickedQuestButton(ISleekElement button)
    {
        int num = questsBox.FindIndexOfChild(button);
        if (num < 0 || num >= displayedQuests.Count)
        {
            UnturnedLog.warn("Cannot find clicked quest");
            return;
        }
        PlayerQuest playerQuest = displayedQuests[num];
        PlayerDashboardUI.close();
        PlayerNPCQuestUI.open(playerQuest.asset, null, null, null, EQuestViewMode.DETAILS);
    }

    private static void onClickedQuestsButton(ISleekElement button)
    {
        openQuests();
    }

    private static void onClickedGroupsButton(ISleekElement button)
    {
        openGroups();
    }

    private static void onClickedPlayersButton(ISleekElement button)
    {
        openPlayers();
    }

    private static void handleIsBlindfoldedChanged()
    {
        if (active)
        {
            synchronizeMapVisibility(mapButtonState.state);
            updateDynamicMap();
        }
    }

    private static void onPlayerTeleported(Player player, Vector3 point)
    {
        focusPoint(point);
    }

    private void createLocationNameLabels()
    {
        Local local = Level.info?.getLocalization();
        foreach (LocationDevkitNode allNode in LocationDevkitNodeSystem.Get().GetAllNodes())
        {
            string text = allNode.locationName;
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }
            string key = text.Replace(' ', '_');
            if (local != null && local.has(key))
            {
                text = local.format(key);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }
            }
            Vector2 vector = ProjectWorldPositionToMap(allNode.transform.position);
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_X = -200;
            sleekLabel.positionOffset_Y = -30;
            sleekLabel.positionScale_X = vector.x;
            sleekLabel.positionScale_Y = vector.y;
            sleekLabel.sizeOffset_X = 400;
            sleekLabel.sizeOffset_Y = 60;
            sleekLabel.text = text;
            sleekLabel.textColor = ESleekTint.FONT;
            sleekLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            mapLocationsContainer.AddChild(sleekLabel);
        }
    }

    private void OnSwappedPlayerSortState(SleekButtonState state, int index)
    {
        ConvenientSavedata.get().write("PlayerListSortMode", index);
        SortAndRebuildPlayers();
    }

    private static ISleekElement OnCreatePlayerEntry(SteamPlayer player)
    {
        return new SleekPlayer(player, isButton: true, SleekPlayer.ESleekPlayerDisplayContext.PLAYER_LIST);
    }

    private static ISleekElement OnCreatePlayerEntryWithGrouping(SteamPlayer player)
    {
        SleekPlayer sleekPlayer = new SleekPlayer(player, isButton: true, SleekPlayer.ESleekPlayerDisplayContext.PLAYER_LIST);
        int num = playersList.IndexOfCreateElementItem + 1;
        if (num < sortedClients.Count && player.player.quests.isMemberOfSameGroupAs(sortedClients[num].player))
        {
            ISleekImage sleekImage = Glazier.Get().CreateImage(icons.load<Texture2D>("Group"));
            sleekImage.positionOffset_X = 21;
            sleekImage.positionOffset_Y = 47;
            sleekImage.sizeOffset_X = 8;
            sleekImage.sizeOffset_Y = 16;
            sleekImage.color = ESleekTint.FOREGROUND;
            sleekPlayer.AddChild(sleekImage);
        }
        return sleekPlayer;
    }

    public void OnDestroy()
    {
        PlayerUI.isBlindfoldedChanged -= handleIsBlindfoldedChanged;
        if (Player.player != null)
        {
            Player player = Player.player;
            player.onPlayerTeleported = (PlayerTeleported)Delegate.Remove(player.onPlayerTeleported, new PlayerTeleported(onPlayerTeleported));
        }
        PlayerQuests.groupUpdated = (GroupUpdatedHandler)Delegate.Remove(PlayerQuests.groupUpdated, new GroupUpdatedHandler(handleGroupUpdated));
        GroupManager.groupInfoReady -= handleGroupInfoReady;
    }

    public PlayerDashboardInformationUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Player/PlayerDashboardInformation.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerDashboardInformation/PlayerDashboardInformation.unity3d");
        container = new SleekFullscreenBox();
        container.positionScale_Y = 1f;
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        zoomMultiplier = 1;
        tab = EInfoTab.PLAYERS;
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.positionOffset_Y = 60;
        backdropBox.sizeOffset_Y = -60;
        backdropBox.sizeScale_X = 1f;
        backdropBox.sizeScale_Y = 1f;
        backdropBox.backgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        container.AddChild(backdropBox);
        mapInspect = Glazier.Get().CreateFrame();
        mapInspect.positionOffset_X = 10;
        mapInspect.positionOffset_Y = 10;
        mapInspect.sizeOffset_X = -15;
        mapInspect.sizeOffset_Y = -20;
        mapInspect.sizeScale_X = 0.6f;
        mapInspect.sizeScale_Y = 1f;
        backdropBox.AddChild(mapInspect);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.sizeOffset_Y = -40;
        sleekConstraintFrame.sizeScale_X = 1f;
        sleekConstraintFrame.sizeScale_Y = 1f;
        sleekConstraintFrame.constraint = ESleekConstraint.FitInParent;
        mapInspect.AddChild(sleekConstraintFrame);
        CartographyVolume mainVolume = VolumeManager<CartographyVolume, CartographyVolumeManager>.Get().GetMainVolume();
        if (mainVolume != null)
        {
            Vector3 size = mainVolume.CalculateLocalBounds().size;
            sleekConstraintFrame.AspectRatio = size.x / size.z;
            maxZoomMultiplier = Mathf.CeilToInt(Mathf.Max(size.x, size.z) / 1024f) + 1;
        }
        else
        {
            maxZoomMultiplier = (int)Level.size / 1024 + 1;
        }
        mapBox = Glazier.Get().CreateScrollView();
        mapBox.sizeScale_X = 1f;
        mapBox.sizeScale_Y = 1f;
        mapBox.handleScrollWheel = false;
        mapBox.scaleContentToWidth = true;
        mapBox.scaleContentToHeight = true;
        sleekConstraintFrame.AddChild(mapBox);
        mapImage = Glazier.Get().CreateImage();
        mapImage.sizeScale_X = 1f;
        mapImage.sizeScale_Y = 1f;
        mapImage.onImageRightClicked += onRightClickedMap;
        mapBox.AddChild(mapImage);
        mapLocationsContainer = Glazier.Get().CreateFrame();
        mapLocationsContainer.sizeScale_X = 1f;
        mapLocationsContainer.sizeScale_Y = 1f;
        mapImage.AddChild(mapLocationsContainer);
        createLocationNameLabels();
        mapArenaContainer = Glazier.Get().CreateFrame();
        mapArenaContainer.sizeScale_X = 1f;
        mapArenaContainer.sizeScale_Y = 1f;
        mapImage.AddChild(mapArenaContainer);
        mapMarkersContainer = Glazier.Get().CreateFrame();
        mapMarkersContainer.sizeScale_X = 1f;
        mapMarkersContainer.sizeScale_Y = 1f;
        mapImage.AddChild(mapMarkersContainer);
        mapRemotePlayersContainer = Glazier.Get().CreateFrame();
        mapRemotePlayersContainer.sizeScale_X = 1f;
        mapRemotePlayersContainer.sizeScale_Y = 1f;
        mapImage.AddChild(mapRemotePlayersContainer);
        arenaTargetPoints = new List<ISleekImage>();
        markerImages = new List<ISleekImage>();
        remotePlayerImages = new List<ISleekImage>();
        localPlayerImage = Glazier.Get().CreateImage();
        localPlayerImage.positionOffset_X = -10;
        localPlayerImage.positionOffset_Y = -10;
        localPlayerImage.sizeOffset_X = 20;
        localPlayerImage.sizeOffset_Y = 20;
        localPlayerImage.isAngled = true;
        localPlayerImage.texture = icons.load<Texture2D>("Player");
        localPlayerImage.color = ESleekTint.FOREGROUND;
        if (string.IsNullOrEmpty(Characters.active.nick))
        {
            localPlayerImage.addLabel(Characters.active.name, ESleekSide.RIGHT);
        }
        else
        {
            localPlayerImage.addLabel(Characters.active.nick, ESleekSide.RIGHT);
        }
        mapImage.AddChild(localPlayerImage);
        arenaAreaCurrentOverlay = Glazier.Get().CreateImage(icons.load<Texture2D>("Arena_Area"));
        mapArenaContainer.AddChild(arenaAreaCurrentOverlay);
        arenaAreaCurrentLeftOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        arenaAreaCurrentLeftOverlay.sizeOffset_X = 1;
        mapArenaContainer.AddChild(arenaAreaCurrentLeftOverlay);
        arenaAreaCurrentRightOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        arenaAreaCurrentRightOverlay.positionOffset_X = -1;
        arenaAreaCurrentRightOverlay.sizeOffset_X = 1;
        mapArenaContainer.AddChild(arenaAreaCurrentRightOverlay);
        arenaAreaCurrentUpOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        arenaAreaCurrentUpOverlay.sizeOffset_Y = 1;
        arenaAreaCurrentUpOverlay.sizeScale_X = 1f;
        mapArenaContainer.AddChild(arenaAreaCurrentUpOverlay);
        arenaAreaCurrentDownOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        arenaAreaCurrentDownOverlay.positionOffset_Y = -1;
        arenaAreaCurrentDownOverlay.sizeOffset_Y = 1;
        arenaAreaCurrentDownOverlay.sizeScale_X = 1f;
        mapArenaContainer.AddChild(arenaAreaCurrentDownOverlay);
        noLabel = Glazier.Get().CreateLabel();
        noLabel.sizeOffset_Y = -40;
        noLabel.sizeScale_X = 1f;
        noLabel.sizeScale_Y = 1f;
        noLabel.textColor = Color.black;
        noLabel.fontSize = ESleekFontSize.Large;
        noLabel.fontStyle = FontStyle.Bold;
        mapInspect.AddChild(noLabel);
        noLabel.isVisible = false;
        updateZoom();
        zoomInButton = new SleekButtonIcon(icons.load<Texture2D>("Zoom_In"));
        zoomInButton.positionOffset_Y = -30;
        zoomInButton.positionScale_Y = 1f;
        zoomInButton.sizeOffset_X = -5;
        zoomInButton.sizeOffset_Y = 30;
        zoomInButton.sizeScale_X = 0.25f;
        zoomInButton.text = localization.format("Zoom_In_Button");
        zoomInButton.tooltip = localization.format("Zoom_In_Button_Tooltip");
        zoomInButton.iconColor = ESleekTint.FOREGROUND;
        zoomInButton.onClickedButton += onClickedZoomInButton;
        mapInspect.AddChild(zoomInButton);
        zoomOutButton = new SleekButtonIcon(icons.load<Texture2D>("Zoom_Out"));
        zoomOutButton.positionOffset_X = 5;
        zoomOutButton.positionOffset_Y = -30;
        zoomOutButton.positionScale_X = 0.25f;
        zoomOutButton.positionScale_Y = 1f;
        zoomOutButton.sizeOffset_X = -10;
        zoomOutButton.sizeOffset_Y = 30;
        zoomOutButton.sizeScale_X = 0.25f;
        zoomOutButton.text = localization.format("Zoom_Out_Button");
        zoomOutButton.tooltip = localization.format("Zoom_Out_Button_Tooltip");
        zoomOutButton.iconColor = ESleekTint.FOREGROUND;
        zoomOutButton.onClickedButton += onClickedZoomOutButton;
        mapInspect.AddChild(zoomOutButton);
        centerButton = new SleekButtonIcon(icons.load<Texture2D>("Center"));
        centerButton.positionOffset_X = 5;
        centerButton.positionOffset_Y = -30;
        centerButton.positionScale_X = 0.5f;
        centerButton.positionScale_Y = 1f;
        centerButton.sizeOffset_X = -10;
        centerButton.sizeOffset_Y = 30;
        centerButton.sizeScale_X = 0.25f;
        centerButton.text = localization.format("Center_Button");
        centerButton.tooltip = localization.format("Center_Button_Tooltip");
        centerButton.iconColor = ESleekTint.FOREGROUND;
        centerButton.onClickedButton += onClickedCenterButton;
        mapInspect.AddChild(centerButton);
        mapButtonState = new SleekButtonState(new GUIContent(localization.format("Chart")), new GUIContent(localization.format("Satellite")));
        mapButtonState.positionOffset_X = 5;
        mapButtonState.positionOffset_Y = -30;
        mapButtonState.positionScale_X = 0.75f;
        mapButtonState.positionScale_Y = 1f;
        mapButtonState.sizeOffset_X = -5;
        mapButtonState.sizeOffset_Y = 30;
        mapButtonState.sizeScale_X = 0.25f;
        mapButtonState.onSwappedState = onSwappedMapState;
        mapInspect.AddChild(mapButtonState);
        headerButtonsContainer = Glazier.Get().CreateFrame();
        headerButtonsContainer.positionOffset_X = 5;
        headerButtonsContainer.positionOffset_Y = 10;
        headerButtonsContainer.positionScale_X = 0.6f;
        headerButtonsContainer.sizeOffset_X = -15;
        headerButtonsContainer.sizeOffset_Y = 50;
        headerButtonsContainer.sizeScale_X = 0.4f;
        backdropBox.AddChild(headerButtonsContainer);
        questsButton = new SleekButtonIcon(icons.load<Texture2D>("Quests"));
        questsButton.sizeOffset_X = -5;
        questsButton.sizeScale_X = 0.333f;
        questsButton.sizeScale_Y = 1f;
        questsButton.fontSize = ESleekFontSize.Medium;
        questsButton.tooltip = localization.format("Quests_Tooltip");
        questsButton.onClickedButton += onClickedQuestsButton;
        headerButtonsContainer.AddChild(questsButton);
        groupsButton = new SleekButtonIcon(icons.load<Texture2D>("Groups"));
        groupsButton.positionOffset_X = 5;
        groupsButton.positionScale_X = 0.333f;
        groupsButton.sizeOffset_X = -10;
        groupsButton.sizeScale_X = 0.334f;
        groupsButton.sizeScale_Y = 1f;
        groupsButton.fontSize = ESleekFontSize.Medium;
        groupsButton.text = localization.format("Groups");
        groupsButton.tooltip = localization.format("Groups_Tooltip");
        groupsButton.onClickedButton += onClickedGroupsButton;
        headerButtonsContainer.AddChild(groupsButton);
        playersButton = new SleekButtonIcon(icons.load<Texture2D>("Players"));
        playersButton.positionOffset_X = 5;
        playersButton.positionScale_X = 0.667f;
        playersButton.sizeOffset_X = -5;
        playersButton.sizeScale_X = 0.333f;
        playersButton.sizeScale_Y = 1f;
        playersButton.fontSize = ESleekFontSize.Medium;
        playersButton.tooltip = localization.format("Players_Tooltip");
        playersButton.onClickedButton += onClickedPlayersButton;
        headerButtonsContainer.AddChild(playersButton);
        questsBox = Glazier.Get().CreateScrollView();
        questsBox.positionOffset_X = 5;
        questsBox.positionOffset_Y = 70;
        questsBox.positionScale_X = 0.6f;
        questsBox.sizeOffset_X = -15;
        questsBox.sizeOffset_Y = -80;
        questsBox.sizeScale_X = 0.4f;
        questsBox.sizeScale_Y = 1f;
        questsBox.scaleContentToWidth = true;
        backdropBox.AddChild(questsBox);
        questsBox.isVisible = false;
        groupsBox = Glazier.Get().CreateScrollView();
        groupsBox.positionOffset_X = 5;
        groupsBox.positionOffset_Y = 70;
        groupsBox.positionScale_X = 0.6f;
        groupsBox.sizeOffset_X = -15;
        groupsBox.sizeOffset_Y = -80;
        groupsBox.sizeScale_X = 0.4f;
        groupsBox.sizeScale_Y = 1f;
        groupsBox.scaleContentToWidth = true;
        backdropBox.AddChild(groupsBox);
        groupsBox.isVisible = false;
        playersBox = Glazier.Get().CreateFrame();
        playersBox.positionOffset_X = 5;
        playersBox.positionOffset_Y = 70;
        playersBox.positionScale_X = 0.6f;
        playersBox.sizeOffset_X = -15;
        playersBox.sizeOffset_Y = -80;
        playersBox.sizeScale_X = 0.4f;
        playersBox.sizeScale_Y = 1f;
        backdropBox.AddChild(playersBox);
        playersBox.isVisible = true;
        playerSortButton = new SleekButtonState(new GUIContent(localization.format("SortPlayers_Name")), new GUIContent(localization.format("SortPlayers_Group")));
        playerSortButton.sizeScale_X = 1f;
        playerSortButton.sizeOffset_Y = 30;
        playerSortButton.onSwappedState = OnSwappedPlayerSortState;
        playersBox.AddChild(playerSortButton);
        playersList = new SleekList<SteamPlayer>();
        playersList.sizeScale_X = 1f;
        playersList.sizeScale_Y = 1f;
        playersList.itemHeight = 50;
        playersList.itemPadding = 10;
        playersBox.AddChild(playersList);
        sortedClients.Clear();
        playersList.SetData(sortedClients);
        if (Provider.modeConfigData.Gameplay.Group_Player_List)
        {
            playersList.positionOffset_Y = 30;
            playersList.sizeOffset_Y = -30;
            if (ConvenientSavedata.get().read("PlayerListSortMode", out long value))
            {
                playerSortButton.state = MathfEx.ClampLongToInt(value, 0, 1);
            }
        }
        else
        {
            playerSortButton.isVisible = false;
        }
        PlayerUI.isBlindfoldedChanged += handleIsBlindfoldedChanged;
        Player player = Player.player;
        player.onPlayerTeleported = (PlayerTeleported)Delegate.Combine(player.onPlayerTeleported, new PlayerTeleported(onPlayerTeleported));
        PlayerQuests.groupUpdated = (GroupUpdatedHandler)Delegate.Combine(PlayerQuests.groupUpdated, new GroupUpdatedHandler(handleGroupUpdated));
        GroupManager.groupInfoReady += handleGroupInfoReady;
        onPlayerTeleported(Player.player, Player.player.transform.position);
        string text = ((Level.info != null) ? (Level.info.path + "/Chart.png") : null);
        if (text != null && ReadWrite.fileExists(text, useCloud: false, usePath: false))
        {
            chartTexture = ReadWrite.readTextureFromFile(text);
        }
        else
        {
            chartTexture = null;
        }
        string text2 = ((Level.info != null) ? (Level.info.path + "/Map.png") : null);
        if (text2 != null && ReadWrite.fileExists(text2, useCloud: false, usePath: false))
        {
            mapTexture = ReadWrite.readTextureFromFile(text2);
        }
        else
        {
            mapTexture = null;
        }
        staticTexture = Resources.Load<Texture2D>("Level/Map");
    }
}
