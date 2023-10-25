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
            sleekBox.SizeOffset_X = -140f;
            sleekBox.SizeScale_X = 1f;
            sleekBox.SizeScale_Y = 1f;
            sleekBox.Text = text;
            AddChild(sleekBox);
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionScale_X = 1f;
            sleekButton.SizeOffset_X = 60f;
            sleekButton.SizeScale_Y = 1f;
            sleekButton.Text = localization.format("Group_Join");
            sleekButton.TooltipText = localization.format("Group_Join_Tooltip");
            sleekButton.OnClicked += handleJoinButtonClicked;
            sleekBox.AddChild(sleekButton);
            ISleekButton sleekButton2 = Glazier.Get().CreateButton();
            sleekButton2.PositionOffset_X = 60f;
            sleekButton2.PositionScale_X = 1f;
            sleekButton2.SizeOffset_X = 80f;
            sleekButton2.SizeScale_Y = 1f;
            sleekButton2.Text = localization.format("Group_Ignore");
            sleekButton2.TooltipText = localization.format("Group_Ignore_Tooltip");
            sleekButton2.OnClicked += handleIgnoreButtonClicked;
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

    /// <summary>
    /// Labels for named locations.
    /// </summary>
    private static ISleekElement mapLocationsContainer;

    /// <summary>
    /// Contains arena outer circle and inner target points.
    /// </summary>
    private static ISleekElement mapArenaContainer;

    private static ISleekElement mapMarkersContainer;

    private static ISleekElement mapRemotePlayersContainer;

    private static List<ISleekImage> markerImages;

    private static List<ISleekImage> arenaTargetPoints;

    /// <summary>
    /// Player avatars.
    /// </summary>
    private static List<ISleekImage> remotePlayerImages;

    /// <summary>
    /// Arrow oriented with the local player.
    /// </summary>
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
                mapImage.Texture = chartTexture;
                noLabel.IsVisible = false;
            }
            else
            {
                mapImage.Texture = staticTexture;
                noLabel.Text = localization.format("No_Chart");
                noLabel.IsVisible = true;
            }
        }
        else if (mapTexture != null && !PlayerUI.isBlindfolded && hasGPS)
        {
            mapImage.Texture = mapTexture;
            noLabel.IsVisible = false;
        }
        else
        {
            mapImage.Texture = staticTexture;
            noLabel.Text = localization.format("No_GPS");
            noLabel.IsVisible = true;
        }
        bool flag = !noLabel.IsVisible;
        mapLocationsContainer.IsVisible = flag;
        bool flag2 = flag && Provider.modeConfigData.Gameplay.Group_Map;
        mapMarkersContainer.IsVisible = flag2;
        mapArenaContainer.IsVisible = flag2 && LevelManager.levelType == ELevelType.ARENA;
        mapRemotePlayersContainer.IsVisible = flag2;
        localPlayerImage.IsVisible = flag2;
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
                    sleekImage.IsVisible = true;
                }
                else
                {
                    sleekImage = Glazier.Get().CreateImage(icons.load<Texture2D>("Marker"));
                    sleekImage.PositionOffset_X = -10f;
                    sleekImage.PositionOffset_Y = -10f;
                    sleekImage.SizeOffset_X = 20f;
                    sleekImage.SizeOffset_Y = 20f;
                    sleekImage.AddLabel(string.Empty, ESleekSide.RIGHT);
                    mapMarkersContainer.AddChild(sleekImage);
                    markerImages.Add(sleekImage);
                }
                num++;
                Vector2 vector = ProjectWorldPositionToMap(quests.markerPosition);
                sleekImage.PositionScale_X = vector.x;
                sleekImage.PositionScale_Y = vector.y;
                sleekImage.TintColor = client.markerColor;
                string text = quests.markerTextOverride;
                if (string.IsNullOrEmpty(text))
                {
                    text = ((!string.IsNullOrEmpty(client.playerID.nickName)) ? client.playerID.nickName : client.playerID.characterName);
                }
                sleekImage.UpdateLabel(text);
            }
        }
        for (int num2 = markerImages.Count - 1; num2 >= num; num2--)
        {
            markerImages[num2].IsVisible = false;
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
                    float f = ((float)i / (float)num + num2) * MathF.PI * 2f;
                    float num3 = Mathf.Cos(f);
                    float num4 = Mathf.Sin(f);
                    Vector2 vector = ProjectWorldPositionToMap(LevelManager.arenaTargetCenter + new Vector3(num3 * LevelManager.arenaTargetRadius, 0f, num4 * LevelManager.arenaTargetRadius));
                    ISleekImage sleekImage;
                    if (i < arenaTargetPoints.Count)
                    {
                        sleekImage = arenaTargetPoints[i];
                        sleekImage.IsVisible = true;
                    }
                    else
                    {
                        sleekImage = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
                        sleekImage.SizeOffset_X = 2f;
                        sleekImage.SizeOffset_Y = 2f;
                        sleekImage.TintColor = new Color(1f, 1f, 0f, 1f);
                        arenaTargetPoints.Add(sleekImage);
                        mapArenaContainer.AddChild(sleekImage);
                    }
                    sleekImage.PositionScale_X = vector.x;
                    sleekImage.PositionScale_Y = vector.y;
                }
            }
        }
        for (int num5 = arenaTargetPoints.Count - 1; num5 >= num; num5--)
        {
            arenaTargetPoints[num5].IsVisible = false;
        }
        Vector2 vector2 = ProjectWorldPositionToMap(LevelManager.arenaCurrentCenter);
        float num6 = (float)(int)Level.size - (float)(int)Level.border * 2f;
        float num7 = LevelManager.arenaCurrentRadius / num6;
        float num8 = num7 * 2f;
        arenaAreaCurrentOverlay.PositionScale_X = vector2.x - num7;
        arenaAreaCurrentOverlay.PositionScale_Y = vector2.y - num7;
        arenaAreaCurrentOverlay.SizeScale_X = num8;
        arenaAreaCurrentOverlay.SizeScale_Y = num8;
        arenaAreaCurrentLeftOverlay.PositionScale_Y = arenaAreaCurrentOverlay.PositionScale_Y;
        arenaAreaCurrentLeftOverlay.SizeScale_X = arenaAreaCurrentOverlay.PositionScale_X;
        arenaAreaCurrentLeftOverlay.SizeScale_Y = arenaAreaCurrentOverlay.SizeScale_Y;
        arenaAreaCurrentRightOverlay.PositionScale_X = arenaAreaCurrentOverlay.PositionScale_X + arenaAreaCurrentOverlay.SizeScale_X;
        arenaAreaCurrentRightOverlay.PositionScale_Y = arenaAreaCurrentOverlay.PositionScale_Y;
        arenaAreaCurrentRightOverlay.SizeScale_X = 1f - arenaAreaCurrentOverlay.PositionScale_X - arenaAreaCurrentOverlay.SizeScale_X;
        arenaAreaCurrentRightOverlay.SizeScale_Y = arenaAreaCurrentOverlay.SizeScale_Y;
        arenaAreaCurrentUpOverlay.SizeScale_Y = arenaAreaCurrentOverlay.PositionScale_Y;
        arenaAreaCurrentDownOverlay.PositionScale_Y = arenaAreaCurrentOverlay.PositionScale_Y + arenaAreaCurrentOverlay.SizeScale_Y;
        arenaAreaCurrentDownOverlay.SizeScale_Y = 1f - arenaAreaCurrentOverlay.PositionScale_Y - arenaAreaCurrentOverlay.SizeScale_Y;
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
                    sleekImage.IsVisible = true;
                }
                else
                {
                    sleekImage = Glazier.Get().CreateImage();
                    sleekImage.PositionOffset_X = -10f;
                    sleekImage.PositionOffset_Y = -10f;
                    sleekImage.SizeOffset_X = 20f;
                    sleekImage.SizeOffset_Y = 20f;
                    sleekImage.AddLabel(string.Empty, ESleekSide.RIGHT);
                    mapRemotePlayersContainer.AddChild(sleekImage);
                    remotePlayerImages.Add(sleekImage);
                }
                num++;
                Vector2 vector = ProjectWorldPositionToMap(client.player.transform.position);
                sleekImage.PositionScale_X = vector.x;
                sleekImage.PositionScale_Y = vector.y;
                if (OptionsSettings.streamer)
                {
                    sleekImage.Texture = null;
                }
                else
                {
                    sleekImage.Texture = Provider.provider.communityService.getIcon(client.playerID.steamID, shouldCache: true);
                }
                if (flag && !string.IsNullOrEmpty(client.playerID.nickName))
                {
                    sleekImage.UpdateLabel(client.playerID.nickName);
                }
                else
                {
                    sleekImage.UpdateLabel(client.playerID.characterName);
                }
            }
        }
        for (int num2 = remotePlayerImages.Count - 1; num2 >= num; num2--)
        {
            remotePlayerImages[num2].IsVisible = false;
        }
    }

    public static void updateDynamicMap()
    {
        if (mapMarkersContainer.IsVisible)
        {
            updateMarkers();
        }
        if (mapArenaContainer.IsVisible)
        {
            updateArenaCircle();
        }
        if (mapRemotePlayersContainer.IsVisible)
        {
            updateRemotePlayerAvatars();
        }
        if (localPlayerImage.IsVisible && Player.player != null)
        {
            Vector2 vector = ProjectWorldPositionToMap(Player.player.transform.position);
            localPlayerImage.PositionScale_X = vector.x;
            localPlayerImage.PositionScale_Y = vector.y;
            localPlayerImage.RotationAngle = ProjectWorldRotationToMap(Player.player.transform.rotation.eulerAngles.y);
        }
    }

    protected static void searchForMapsInInventory(ref bool enableChart, ref bool enableMap)
    {
        if (enableChart & enableMap)
        {
            return;
        }
        for (byte b = 0; b < PlayerInventory.PAGES - 2; b++)
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
        float num = 0f;
        foreach (PlayerQuest quests in Player.player.quests.questsList)
        {
            if (quests != null && quests.asset != null)
            {
                displayedQuests.Add(quests);
                bool flag = quests.asset.areConditionsMet(Player.player);
                ISleekButton sleekButton = Glazier.Get().CreateButton();
                sleekButton.PositionOffset_Y = num;
                sleekButton.SizeOffset_Y = 50f;
                sleekButton.SizeScale_X = 1f;
                sleekButton.OnClicked += onClickedQuestButton;
                questsBox.AddChild(sleekButton);
                ISleekImage sleekImage = Glazier.Get().CreateImage(icons.load<Texture2D>(flag ? "Complete" : "Incomplete"));
                sleekImage.PositionOffset_X = 5f;
                sleekImage.PositionOffset_Y = 5f;
                sleekImage.SizeOffset_X = 40f;
                sleekImage.SizeOffset_Y = 40f;
                sleekButton.AddChild(sleekImage);
                ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                sleekLabel.PositionOffset_X = 50f;
                sleekLabel.SizeOffset_X = -55f;
                sleekLabel.SizeScale_X = 1f;
                sleekLabel.SizeScale_Y = 1f;
                sleekLabel.TextAlignment = TextAnchor.MiddleLeft;
                sleekLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
                sleekLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
                sleekLabel.AllowRichText = true;
                sleekLabel.FontSize = ESleekFontSize.Medium;
                sleekLabel.Text = quests.asset.questName;
                sleekButton.AddChild(sleekLabel);
                num += sleekButton.SizeOffset_Y + 10f;
            }
        }
        questsBox.ContentSizeOffset = new Vector2(0f, num - 10f);
        updateTabs();
    }

    private static void onClickedTuneButton(ISleekElement button)
    {
        uint num = (uint)(radioFrequencyField.Value * 1000.0);
        if (num < 300000)
        {
            num = 300000u;
        }
        else if (num > 900000)
        {
            num = 900000u;
        }
        radioFrequencyField.Value = (double)num / 1000.0;
        Player.player.quests.sendSetRadioFrequency(num);
    }

    private static void onClickedResetButton(ISleekElement button)
    {
        radioFrequencyField.Value = (double)PlayerQuests.DEFAULT_RADIO_FREQUENCY / 1000.0;
        onClickedTuneButton(button);
    }

    private static void onClickedRenameButton(ISleekElement button)
    {
        Player.player.quests.sendRenameGroup(groupNameField.Text);
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
        sleekBox.PositionOffset_Y = num;
        sleekBox.SizeOffset_X = 125f;
        sleekBox.SizeOffset_Y = 30f;
        sleekBox.Text = localization.format("Radio_Frequency_Label");
        groupsBox.AddChild(sleekBox);
        radioFrequencyField = Glazier.Get().CreateFloat64Field();
        radioFrequencyField.PositionOffset_X = 125f;
        radioFrequencyField.SizeOffset_X = -225f;
        radioFrequencyField.PositionOffset_Y = num;
        radioFrequencyField.SizeOffset_Y = 30f;
        radioFrequencyField.SizeScale_X = 1f;
        radioFrequencyField.Value = (double)Player.player.quests.radioFrequency / 1000.0;
        groupsBox.AddChild(radioFrequencyField);
        ISleekButton sleekButton = Glazier.Get().CreateButton();
        sleekButton.PositionOffset_X = -100f;
        sleekButton.PositionScale_X = 1f;
        sleekButton.SizeOffset_X = 50f;
        sleekButton.SizeOffset_Y = 30f;
        sleekButton.Text = localization.format("Radio_Frequency_Tune");
        sleekButton.TooltipText = localization.format("Radio_Frequency_Tune_Tooltip");
        sleekButton.OnClicked += onClickedTuneButton;
        groupsBox.AddChild(sleekButton);
        ISleekButton sleekButton2 = Glazier.Get().CreateButton();
        sleekButton2.PositionOffset_X = -50f;
        sleekButton2.PositionScale_X = 1f;
        sleekButton2.SizeOffset_X = 50f;
        sleekButton2.SizeOffset_Y = 30f;
        sleekButton2.Text = localization.format("Radio_Frequency_Reset");
        sleekButton2.TooltipText = localization.format("Radio_Frequency_Reset_Tooltip");
        sleekButton2.OnClicked += onClickedResetButton;
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
                    sleekBoxIcon.PositionOffset_Y = num;
                    sleekBoxIcon.SizeOffset_Y = 50f;
                    sleekBoxIcon.SizeScale_X = 1f;
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
                    groupNameField.PositionOffset_Y = num;
                    groupNameField.MaxLength = 32;
                    groupNameField.Text = text;
                    groupNameField.SizeOffset_X = -100f;
                    groupNameField.SizeOffset_Y = 30f;
                    groupNameField.SizeScale_X = 1f;
                    groupsBox.AddChild(groupNameField);
                    ISleekButton sleekButton3 = Glazier.Get().CreateButton();
                    sleekButton3.PositionScale_X = 1f;
                    sleekButton3.PositionOffset_X = -100f;
                    sleekButton3.PositionOffset_Y = num;
                    sleekButton3.SizeOffset_X = 100f;
                    sleekButton3.SizeOffset_Y = 30f;
                    sleekButton3.Text = localization.format("Group_Rename");
                    sleekButton3.TooltipText = localization.format("Group_Rename_Tooltip");
                    sleekButton3.OnClicked += onClickedRenameButton;
                    groupsBox.AddChild(sleekButton3);
                }
                else
                {
                    ISleekBox sleekBox2 = Glazier.Get().CreateBox();
                    sleekBox2.PositionOffset_Y = num;
                    sleekBox2.SizeOffset_Y = 30f;
                    sleekBox2.SizeScale_X = 1f;
                    sleekBox2.Text = text;
                    groupsBox.AddChild(sleekBox2);
                }
                num += 30;
                if (quests.useMaxGroupMembersLimit)
                {
                    ISleekBox sleekBox3 = Glazier.Get().CreateBox();
                    sleekBox3.PositionOffset_Y = num;
                    sleekBox3.SizeOffset_Y = 30f;
                    sleekBox3.SizeScale_X = 1f;
                    sleekBox3.Text = localization.format("Group_Members", groupInfo.members, Provider.modeConfigData.Gameplay.Max_Group_Members);
                    groupsBox.AddChild(sleekBox3);
                    num += 30;
                }
            }
            if (quests.hasPermissionToLeaveGroup)
            {
                SleekButtonIcon sleekButtonIcon = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"));
                sleekButtonIcon.PositionOffset_Y = num;
                sleekButtonIcon.SizeOffset_Y = 30f;
                sleekButtonIcon.SizeScale_X = 1f;
                sleekButtonIcon.text = localization.format("Group_Leave");
                sleekButtonIcon.tooltip = localization.format("Group_Leave_Tooltip");
                sleekButtonIcon.onClickedButton += onClickedLeaveGroupButton;
                groupsBox.AddChild(sleekButtonIcon);
                num += 30;
            }
            if (quests.hasPermissionToDeleteGroup)
            {
                SleekButtonIconConfirm sleekButtonIconConfirm = new SleekButtonIconConfirm(MenuWorkshopEditorUI.icons.load<Texture2D>("Remove"), localization.format("Group_Delete_Confirm"), localization.format("Group_Delete_Confirm_Tooltip"), localization.format("Group_Delete_Deny"), localization.format("Group_Delete_Deny_Tooltip"));
                sleekButtonIconConfirm.PositionOffset_Y = num;
                sleekButtonIconConfirm.SizeOffset_Y = 30f;
                sleekButtonIconConfirm.SizeScale_X = 1f;
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
                    sleekPlayer.PositionOffset_Y = num;
                    sleekPlayer.SizeOffset_Y = 50f;
                    sleekPlayer.SizeScale_X = 1f;
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
                    sleekButtonIcon2.PositionOffset_Y = num;
                    sleekButtonIcon2.SizeOffset_Y = 50f;
                    sleekButtonIcon2.SizeScale_X = 1f;
                    sleekButtonIcon2.text = cachedGroup2.name;
                    sleekButtonIcon2.onClickedButton += onClickedMainGroupButton;
                    groupsBox.AddChild(sleekButtonIcon2);
                    num += 50;
                }
            }
            foreach (CSteamID groupInvite in quests.groupInvites)
            {
                SleekInviteButton sleekInviteButton = new SleekInviteButton(groupInvite);
                sleekInviteButton.PositionOffset_Y = num;
                sleekInviteButton.SizeOffset_Y = 30f;
                sleekInviteButton.SizeScale_X = 1f;
                groupsBox.AddChild(sleekInviteButton);
                num += 30;
            }
            if (Player.player.quests.hasPermissionToCreateGroup)
            {
                SleekButtonIcon sleekButtonIcon3 = new SleekButtonIcon(MenuWorkshopEditorUI.icons.load<Texture2D>("Add"));
                sleekButtonIcon3.PositionOffset_Y = num;
                sleekButtonIcon3.SizeOffset_Y = 30f;
                sleekButtonIcon3.SizeScale_X = 1f;
                sleekButtonIcon3.text = localization.format("Group_Create");
                sleekButtonIcon3.tooltip = localization.format("Group_Create_Tooltip");
                sleekButtonIcon3.onClickedButton += onClickedCreateGroupButton;
                groupsBox.AddChild(sleekButtonIcon3);
                num += 30;
            }
        }
        groupsBox.ContentSizeOffset = new Vector2(0f, num);
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
        questsBox.IsVisible = tab == EInfoTab.QUESTS;
        groupsBox.IsVisible = tab == EInfoTab.GROUPS;
        playersBox.IsVisible = tab == EInfoTab.PLAYERS;
    }

    private static void updateZoom()
    {
        mapBox.ContentScaleFactor = zoomMultiplier;
    }

    public static void focusPoint(Vector3 point)
    {
        Vector2 normalizedStateCenter = ProjectWorldPositionToMap(point);
        mapBox.NormalizedStateCenter = normalizedStateCenter;
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

    /// <summary>
    /// Convert level-space 3D position into normalized 2D position.
    /// </summary>
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

    /// <summary>
    /// Convert normalized 2D position into level-space 3D position.
    /// </summary>
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
        bool newIsMarkerPlaced = !quests.isMarkerPlaced || Vector2.Distance(ProjectWorldPositionToMap(quests.markerPosition), normalizedCursorPosition) * mapBox.ContentSizeOffset.x > 15f;
        quests.sendSetMarker(newIsMarkerPlaced, newMarkerPosition);
    }

    private static void onClickedZoomInButton(ISleekElement button)
    {
        if (zoomMultiplier < maxZoomMultiplier)
        {
            zoomMultiplier++;
            Vector2 normalizedStateCenter = mapBox.NormalizedStateCenter;
            updateZoom();
            mapBox.NormalizedStateCenter = normalizedStateCenter;
        }
    }

    private static void onClickedZoomOutButton(ISleekElement button)
    {
        if (zoomMultiplier > 1)
        {
            zoomMultiplier--;
            Vector2 normalizedStateCenter = mapBox.NormalizedStateCenter;
            updateZoom();
            mapBox.NormalizedStateCenter = normalizedStateCenter;
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
        PlayerNPCQuestUI.open(playerQuest.asset, null, null, EQuestViewMode.DETAILS);
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
            if (!allNode.isVisibleOnMap)
            {
                continue;
            }
            string text = allNode.locationName;
            if (!string.IsNullOrWhiteSpace(text))
            {
                string key = text.Replace(' ', '_');
                if (local != null && local.has(key))
                {
                    text = local.format(key);
                }
                Vector2 vector = ProjectWorldPositionToMap(allNode.transform.position);
                ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                sleekLabel.PositionOffset_X = -200f;
                sleekLabel.PositionOffset_Y = -30f;
                sleekLabel.PositionScale_X = vector.x;
                sleekLabel.PositionScale_Y = vector.y;
                sleekLabel.SizeOffset_X = 400f;
                sleekLabel.SizeOffset_Y = 60f;
                sleekLabel.Text = text;
                sleekLabel.TextColor = ESleekTint.FONT;
                sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
                mapLocationsContainer.AddChild(sleekLabel);
            }
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
            sleekImage.PositionOffset_X = 21f;
            sleekImage.PositionOffset_Y = 47f;
            sleekImage.SizeOffset_X = 8f;
            sleekImage.SizeOffset_Y = 16f;
            sleekImage.TintColor = ESleekTint.FOREGROUND;
            sleekPlayer.AddChild(sleekImage);
        }
        return sleekPlayer;
    }

    /// <summary>
    /// Temporary to unbind events because this class is static for now. (sigh)
    /// </summary>
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
        container.PositionScale_Y = 1f;
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = false;
        zoomMultiplier = 1;
        tab = EInfoTab.PLAYERS;
        backdropBox = Glazier.Get().CreateBox();
        backdropBox.PositionOffset_Y = 60f;
        backdropBox.SizeOffset_Y = -60f;
        backdropBox.SizeScale_X = 1f;
        backdropBox.SizeScale_Y = 1f;
        backdropBox.BackgroundColor = new SleekColor(ESleekTint.BACKGROUND, 0.5f);
        container.AddChild(backdropBox);
        mapInspect = Glazier.Get().CreateFrame();
        mapInspect.PositionOffset_X = 10f;
        mapInspect.PositionOffset_Y = 10f;
        mapInspect.SizeOffset_X = -15f;
        mapInspect.SizeOffset_Y = -20f;
        mapInspect.SizeScale_X = 0.6f;
        mapInspect.SizeScale_Y = 1f;
        backdropBox.AddChild(mapInspect);
        ISleekConstraintFrame sleekConstraintFrame = Glazier.Get().CreateConstraintFrame();
        sleekConstraintFrame.SizeOffset_Y = -40f;
        sleekConstraintFrame.SizeScale_X = 1f;
        sleekConstraintFrame.SizeScale_Y = 1f;
        sleekConstraintFrame.Constraint = ESleekConstraint.FitInParent;
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
            maxZoomMultiplier = Level.size / 1024 + 1;
        }
        mapBox = Glazier.Get().CreateScrollView();
        mapBox.SizeScale_X = 1f;
        mapBox.SizeScale_Y = 1f;
        mapBox.HandleScrollWheel = false;
        mapBox.ScaleContentToWidth = true;
        mapBox.ScaleContentToHeight = true;
        sleekConstraintFrame.AddChild(mapBox);
        mapImage = Glazier.Get().CreateImage();
        mapImage.SizeScale_X = 1f;
        mapImage.SizeScale_Y = 1f;
        mapImage.OnRightClicked += onRightClickedMap;
        mapBox.AddChild(mapImage);
        mapLocationsContainer = Glazier.Get().CreateFrame();
        mapLocationsContainer.SizeScale_X = 1f;
        mapLocationsContainer.SizeScale_Y = 1f;
        mapImage.AddChild(mapLocationsContainer);
        createLocationNameLabels();
        mapArenaContainer = Glazier.Get().CreateFrame();
        mapArenaContainer.SizeScale_X = 1f;
        mapArenaContainer.SizeScale_Y = 1f;
        mapImage.AddChild(mapArenaContainer);
        mapMarkersContainer = Glazier.Get().CreateFrame();
        mapMarkersContainer.SizeScale_X = 1f;
        mapMarkersContainer.SizeScale_Y = 1f;
        mapImage.AddChild(mapMarkersContainer);
        mapRemotePlayersContainer = Glazier.Get().CreateFrame();
        mapRemotePlayersContainer.SizeScale_X = 1f;
        mapRemotePlayersContainer.SizeScale_Y = 1f;
        mapImage.AddChild(mapRemotePlayersContainer);
        arenaTargetPoints = new List<ISleekImage>();
        markerImages = new List<ISleekImage>();
        remotePlayerImages = new List<ISleekImage>();
        localPlayerImage = Glazier.Get().CreateImage();
        localPlayerImage.PositionOffset_X = -10f;
        localPlayerImage.PositionOffset_Y = -10f;
        localPlayerImage.SizeOffset_X = 20f;
        localPlayerImage.SizeOffset_Y = 20f;
        localPlayerImage.CanRotate = true;
        localPlayerImage.Texture = icons.load<Texture2D>("Player");
        localPlayerImage.TintColor = ESleekTint.FOREGROUND;
        if (string.IsNullOrEmpty(Characters.active.nick))
        {
            localPlayerImage.AddLabel(Characters.active.name, ESleekSide.RIGHT);
        }
        else
        {
            localPlayerImage.AddLabel(Characters.active.nick, ESleekSide.RIGHT);
        }
        mapImage.AddChild(localPlayerImage);
        arenaAreaCurrentOverlay = Glazier.Get().CreateImage(icons.load<Texture2D>("Arena_Area"));
        mapArenaContainer.AddChild(arenaAreaCurrentOverlay);
        arenaAreaCurrentLeftOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        arenaAreaCurrentLeftOverlay.SizeOffset_X = 1f;
        mapArenaContainer.AddChild(arenaAreaCurrentLeftOverlay);
        arenaAreaCurrentRightOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        arenaAreaCurrentRightOverlay.PositionOffset_X = -1f;
        arenaAreaCurrentRightOverlay.SizeOffset_X = 1f;
        mapArenaContainer.AddChild(arenaAreaCurrentRightOverlay);
        arenaAreaCurrentUpOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        arenaAreaCurrentUpOverlay.SizeOffset_Y = 1f;
        arenaAreaCurrentUpOverlay.SizeScale_X = 1f;
        mapArenaContainer.AddChild(arenaAreaCurrentUpOverlay);
        arenaAreaCurrentDownOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        arenaAreaCurrentDownOverlay.PositionOffset_Y = -1f;
        arenaAreaCurrentDownOverlay.SizeOffset_Y = 1f;
        arenaAreaCurrentDownOverlay.SizeScale_X = 1f;
        mapArenaContainer.AddChild(arenaAreaCurrentDownOverlay);
        noLabel = Glazier.Get().CreateLabel();
        noLabel.SizeOffset_Y = -40f;
        noLabel.SizeScale_X = 1f;
        noLabel.SizeScale_Y = 1f;
        noLabel.TextColor = Color.black;
        noLabel.FontSize = ESleekFontSize.Large;
        noLabel.FontStyle = FontStyle.Bold;
        mapInspect.AddChild(noLabel);
        noLabel.IsVisible = false;
        updateZoom();
        zoomInButton = new SleekButtonIcon(icons.load<Texture2D>("Zoom_In"));
        zoomInButton.PositionOffset_Y = -30f;
        zoomInButton.PositionScale_Y = 1f;
        zoomInButton.SizeOffset_X = -5f;
        zoomInButton.SizeOffset_Y = 30f;
        zoomInButton.SizeScale_X = 0.25f;
        zoomInButton.text = localization.format("Zoom_In_Button");
        zoomInButton.tooltip = localization.format("Zoom_In_Button_Tooltip");
        zoomInButton.iconColor = ESleekTint.FOREGROUND;
        zoomInButton.onClickedButton += onClickedZoomInButton;
        mapInspect.AddChild(zoomInButton);
        zoomOutButton = new SleekButtonIcon(icons.load<Texture2D>("Zoom_Out"));
        zoomOutButton.PositionOffset_X = 5f;
        zoomOutButton.PositionOffset_Y = -30f;
        zoomOutButton.PositionScale_X = 0.25f;
        zoomOutButton.PositionScale_Y = 1f;
        zoomOutButton.SizeOffset_X = -10f;
        zoomOutButton.SizeOffset_Y = 30f;
        zoomOutButton.SizeScale_X = 0.25f;
        zoomOutButton.text = localization.format("Zoom_Out_Button");
        zoomOutButton.tooltip = localization.format("Zoom_Out_Button_Tooltip");
        zoomOutButton.iconColor = ESleekTint.FOREGROUND;
        zoomOutButton.onClickedButton += onClickedZoomOutButton;
        mapInspect.AddChild(zoomOutButton);
        centerButton = new SleekButtonIcon(icons.load<Texture2D>("Center"));
        centerButton.PositionOffset_X = 5f;
        centerButton.PositionOffset_Y = -30f;
        centerButton.PositionScale_X = 0.5f;
        centerButton.PositionScale_Y = 1f;
        centerButton.SizeOffset_X = -10f;
        centerButton.SizeOffset_Y = 30f;
        centerButton.SizeScale_X = 0.25f;
        centerButton.text = localization.format("Center_Button");
        centerButton.tooltip = localization.format("Center_Button_Tooltip");
        centerButton.iconColor = ESleekTint.FOREGROUND;
        centerButton.onClickedButton += onClickedCenterButton;
        mapInspect.AddChild(centerButton);
        mapButtonState = new SleekButtonState(new GUIContent(localization.format("Chart")), new GUIContent(localization.format("Satellite")));
        mapButtonState.PositionOffset_X = 5f;
        mapButtonState.PositionOffset_Y = -30f;
        mapButtonState.PositionScale_X = 0.75f;
        mapButtonState.PositionScale_Y = 1f;
        mapButtonState.SizeOffset_X = -5f;
        mapButtonState.SizeOffset_Y = 30f;
        mapButtonState.SizeScale_X = 0.25f;
        mapButtonState.onSwappedState = onSwappedMapState;
        mapInspect.AddChild(mapButtonState);
        headerButtonsContainer = Glazier.Get().CreateFrame();
        headerButtonsContainer.PositionOffset_X = 5f;
        headerButtonsContainer.PositionOffset_Y = 10f;
        headerButtonsContainer.PositionScale_X = 0.6f;
        headerButtonsContainer.SizeOffset_X = -15f;
        headerButtonsContainer.SizeOffset_Y = 50f;
        headerButtonsContainer.SizeScale_X = 0.4f;
        backdropBox.AddChild(headerButtonsContainer);
        questsButton = new SleekButtonIcon(icons.load<Texture2D>("Quests"));
        questsButton.SizeOffset_X = -5f;
        questsButton.SizeScale_X = 0.333f;
        questsButton.SizeScale_Y = 1f;
        questsButton.fontSize = ESleekFontSize.Medium;
        questsButton.tooltip = localization.format("Quests_Tooltip");
        questsButton.onClickedButton += onClickedQuestsButton;
        headerButtonsContainer.AddChild(questsButton);
        groupsButton = new SleekButtonIcon(icons.load<Texture2D>("Groups"));
        groupsButton.PositionOffset_X = 5f;
        groupsButton.PositionScale_X = 0.333f;
        groupsButton.SizeOffset_X = -10f;
        groupsButton.SizeScale_X = 0.334f;
        groupsButton.SizeScale_Y = 1f;
        groupsButton.fontSize = ESleekFontSize.Medium;
        groupsButton.text = localization.format("Groups");
        groupsButton.tooltip = localization.format("Groups_Tooltip");
        groupsButton.onClickedButton += onClickedGroupsButton;
        headerButtonsContainer.AddChild(groupsButton);
        playersButton = new SleekButtonIcon(icons.load<Texture2D>("Players"));
        playersButton.PositionOffset_X = 5f;
        playersButton.PositionScale_X = 0.667f;
        playersButton.SizeOffset_X = -5f;
        playersButton.SizeScale_X = 0.333f;
        playersButton.SizeScale_Y = 1f;
        playersButton.fontSize = ESleekFontSize.Medium;
        playersButton.tooltip = localization.format("Players_Tooltip");
        playersButton.onClickedButton += onClickedPlayersButton;
        headerButtonsContainer.AddChild(playersButton);
        questsBox = Glazier.Get().CreateScrollView();
        questsBox.PositionOffset_X = 5f;
        questsBox.PositionOffset_Y = 70f;
        questsBox.PositionScale_X = 0.6f;
        questsBox.SizeOffset_X = -15f;
        questsBox.SizeOffset_Y = -80f;
        questsBox.SizeScale_X = 0.4f;
        questsBox.SizeScale_Y = 1f;
        questsBox.ScaleContentToWidth = true;
        backdropBox.AddChild(questsBox);
        questsBox.IsVisible = false;
        groupsBox = Glazier.Get().CreateScrollView();
        groupsBox.PositionOffset_X = 5f;
        groupsBox.PositionOffset_Y = 70f;
        groupsBox.PositionScale_X = 0.6f;
        groupsBox.SizeOffset_X = -15f;
        groupsBox.SizeOffset_Y = -80f;
        groupsBox.SizeScale_X = 0.4f;
        groupsBox.SizeScale_Y = 1f;
        groupsBox.ScaleContentToWidth = true;
        backdropBox.AddChild(groupsBox);
        groupsBox.IsVisible = false;
        playersBox = Glazier.Get().CreateFrame();
        playersBox.PositionOffset_X = 5f;
        playersBox.PositionOffset_Y = 70f;
        playersBox.PositionScale_X = 0.6f;
        playersBox.SizeOffset_X = -15f;
        playersBox.SizeOffset_Y = -80f;
        playersBox.SizeScale_X = 0.4f;
        playersBox.SizeScale_Y = 1f;
        backdropBox.AddChild(playersBox);
        playersBox.IsVisible = true;
        playerSortButton = new SleekButtonState(new GUIContent(localization.format("SortPlayers_Name")), new GUIContent(localization.format("SortPlayers_Group")));
        playerSortButton.SizeScale_X = 1f;
        playerSortButton.SizeOffset_Y = 30f;
        playerSortButton.onSwappedState = OnSwappedPlayerSortState;
        playersBox.AddChild(playerSortButton);
        playersList = new SleekList<SteamPlayer>();
        playersList.SizeScale_X = 1f;
        playersList.SizeScale_Y = 1f;
        playersList.itemHeight = 50;
        playersList.itemPadding = 10;
        playersBox.AddChild(playersList);
        sortedClients.Clear();
        playersList.SetData(sortedClients);
        if (Provider.modeConfigData.Gameplay.Group_Player_List)
        {
            playersList.PositionOffset_Y = 30f;
            playersList.SizeOffset_Y = -30f;
            if (ConvenientSavedata.get().read("PlayerListSortMode", out long value))
            {
                playerSortButton.state = MathfEx.ClampLongToInt(value, 0, 1);
            }
        }
        else
        {
            playerSortButton.IsVisible = false;
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
