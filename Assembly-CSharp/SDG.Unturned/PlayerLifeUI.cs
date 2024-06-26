using System;
using System.Collections.Generic;
using SDG.Provider;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerLifeUI
{
    private struct CachedHotbarItem
    {
        public ushort id;

        public byte[] state;
    }

    public static Local localization;

    public static Bundle icons;

    private static SleekFullscreenBox _container;

    public static bool active;

    public static bool chatting;

    public static bool gesturing;

    public static InteractableObjectNPC npc;

    public static bool isVoteMessaged;

    public static float lastVoteMessage;

    private static ISleekScrollView chatHistoryBoxV1;

    private static SleekChatEntryV1[] chatHistoryLabelsV1;

    private static SleekChatEntryV1[] chatPreviewLabelsV1;

    private static ISleekScrollView chatScrollViewV2;

    private static Queue<SleekChatEntryV2> chatEntriesV2;

    public static ISleekField chatField;

    private static SleekButtonState chatModeButton;

    private static SleekButtonIcon sendChatButton;

    public static ISleekBox voteBox;

    private static ISleekLabel voteInfoLabel;

    private static ISleekLabel votesNeededLabel;

    private static ISleekLabel voteYesLabel;

    private static ISleekLabel voteNoLabel;

    private static SleekBoxIcon voiceBox;

    private static ISleekLabel trackedQuestTitle;

    private static ISleekImage trackedQuestBar;

    private static ISleekBox levelTextBox;

    private static ISleekBox levelNumberBox;

    public static ISleekBox compassBox;

    private static ISleekElement compassLabelsContainer;

    private static ISleekElement compassMarkersContainer;

    private static List<ISleekImage> compassMarkers;

    private static int compassMarkersVisibleCount;

    private static ISleekLabel[] compassLabels;

    private static ISleekElement hotbarContainer;

    private static SleekItemIcon[] hotbarImages;

    private static ISleekLabel[] hotbarLabels;

    private static CachedHotbarItem[] cachedHotbarValues;

    public static ISleekLabel statTrackerLabel;

    private static ISleekButton[] faceButtons;

    private static ISleekButton surrenderButton;

    private static ISleekButton pointButton;

    private static ISleekButton waveButton;

    private static ISleekButton saluteButton;

    private static ISleekButton restButton;

    private static ISleekButton facepalmButton;

    public static SleekScopeOverlay scopeOverlay;

    public static ISleekImage binocularsOverlay;

    public static Crosshair crosshair;

    private static ISleekBox lifeBox;

    private static ISleekImage healthIcon;

    private static SleekProgress healthProgress;

    private static ISleekImage foodIcon;

    private static SleekProgress foodProgress;

    private static ISleekImage waterIcon;

    private static SleekProgress waterProgress;

    private static ISleekImage virusIcon;

    private static SleekProgress virusProgress;

    private static ISleekImage staminaIcon;

    private static SleekProgress staminaProgress;

    private static ISleekLabel waveLabel;

    private static ISleekLabel scoreLabel;

    private static ISleekImage oxygenIcon;

    private static SleekProgress oxygenProgress;

    private static ISleekBox vehicleBox;

    private static ISleekImage fuelIcon;

    private static SleekProgress fuelProgress;

    private static ISleekLabel vehicleLockedLabel;

    private static ISleekLabel vehicleEngineLabel;

    private static bool vehicleVisibleByDefault;

    private static ISleekBox gasmaskBox;

    private static SleekItemIcon gasmaskIcon;

    private static SleekProgress gasmaskProgress;

    private static ISleekImage speedIcon;

    private static SleekProgress speedProgress;

    private static ISleekImage batteryChargeIcon;

    private static SleekProgress batteryChargeProgress;

    private static ISleekImage hpIcon;

    private static SleekProgress hpProgress;

    private static ISleekElement statusIconsContainer;

    private static SleekBoxIcon bleedingBox;

    private static SleekBoxIcon brokenBox;

    private static SleekBoxIcon temperatureBox;

    private static SleekBoxIcon starvedBox;

    private static SleekBoxIcon dehydratedBox;

    private static SleekBoxIcon infectedBox;

    private static SleekBoxIcon drownedBox;

    private static SleekBoxIcon asphyxiatingBox;

    private static SleekBoxIcon moonBox;

    private static SleekBoxIcon radiationBox;

    private static SleekBoxIcon safeBox;

    private static SleekBoxIcon arrestBox;

    /// <summary>
    /// Reset to -1 when not chatting. If player presses up/down we get index 0 (most recent).
    /// </summary>
    private static int repeatChatIndex = -1;

    private static int cachedHotbarSearch;

    private static int cachedCompassSearch;

    private static bool cachedHasCompass;

    internal static List<HitmarkerInfo> activeHitmarkers;

    private static List<SleekHitmarker> hitmarkersPool;

    private static List<bool> areConditionsMet = new List<bool>(8);

    public static SleekFullscreenBox container => _container;

    private static ISleekLabel getCompassLabelByAngle(int angle)
    {
        return compassLabels[angle / 5];
    }

    public static void open()
    {
        if (!active)
        {
            active = true;
            if (npc != null)
            {
                npc.OnStoppedTalkingWithLocalPlayer();
                npc = null;
            }
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            closeChat();
            closeGestures();
            if (container != null)
            {
                container.AnimateOutOfView(0f, 1f);
            }
        }
    }

    public static void openChat()
    {
        if (chatting)
        {
            return;
        }
        chatting = true;
        chatField.Text = string.Empty;
        chatField.AnimatePositionOffset(100f, chatField.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20f);
        chatModeButton.state = (int)PlayerUI.chat;
        if (chatEntriesV2 != null)
        {
            chatScrollViewV2.VerticalScrollbarVisibility = ESleekScrollbarVisibility.Default;
            chatScrollViewV2.IsRaycastTarget = true;
            foreach (SleekChatEntryV2 item in chatEntriesV2)
            {
                item.forceVisibleWhileBrowsingChatHistory = true;
            }
            chatScrollViewV2.ScrollToBottom();
        }
        else if (chatHistoryBoxV1 != null)
        {
            chatHistoryBoxV1.IsVisible = true;
            chatHistoryBoxV1.ScrollToBottom();
            for (int i = 0; i < chatPreviewLabelsV1.Length; i++)
            {
                chatPreviewLabelsV1[i].IsVisible = false;
            }
        }
    }

    public static void closeChat()
    {
        if (!chatting)
        {
            return;
        }
        chatting = false;
        repeatChatIndex = -1;
        if (chatField != null)
        {
            chatField.Text = string.Empty;
            chatField.AnimatePositionOffset(0f - chatField.SizeOffset_X - 50f, chatField.PositionOffset_Y, ESleekLerp.EXPONENTIAL, 20f);
        }
        if (chatEntriesV2 != null)
        {
            chatScrollViewV2.VerticalScrollbarVisibility = ESleekScrollbarVisibility.Hidden;
            chatScrollViewV2.IsRaycastTarget = false;
            foreach (SleekChatEntryV2 item in chatEntriesV2)
            {
                item.forceVisibleWhileBrowsingChatHistory = false;
            }
            chatScrollViewV2.ScrollToBottom();
        }
        else if (chatHistoryBoxV1 != null)
        {
            chatHistoryBoxV1.IsVisible = false;
            for (int i = 0; i < chatPreviewLabelsV1.Length; i++)
            {
                chatPreviewLabelsV1[i].IsVisible = true;
            }
        }
    }

    public static void SendChatAndClose()
    {
        if (!string.IsNullOrEmpty(chatField.Text))
        {
            ChatManager.sendChat(PlayerUI.chat, chatField.Text);
        }
        closeChat();
    }

    /// <summary>
    /// Fill chat field with previous sent message.
    /// Useful for repeating commands with minor changes.
    /// </summary>
    public static void repeatChat(int delta)
    {
        if (chatField != null)
        {
            int index = Mathf.Max(repeatChatIndex + delta, 0);
            string recentlySentMessage = ChatManager.getRecentlySentMessage(index);
            if (!string.IsNullOrEmpty(recentlySentMessage))
            {
                chatField.Text = recentlySentMessage;
                repeatChatIndex = index;
            }
        }
    }

    private static void OnChatFieldEscaped(ISleekField field)
    {
        if (chatting)
        {
            closeChat();
        }
    }

    private static void OnSwappedChatModeState(SleekButtonState button, int index)
    {
        PlayerUI.chat = (EChatMode)index;
    }

    private static void OnSendChatButtonClicked(ISleekElement button)
    {
        if (chatting)
        {
            SendChatAndClose();
        }
    }

    public static void openGestures()
    {
        if (!gesturing)
        {
            gesturing = true;
            for (int i = 0; i < faceButtons.Length; i++)
            {
                faceButtons[i].IsVisible = true;
            }
            bool isVisible = !Player.player.equipment.HasValidUseable && Player.player.stance.stance != EPlayerStance.PRONE && Player.player.stance.stance != EPlayerStance.DRIVING && Player.player.stance.stance != EPlayerStance.SITTING;
            surrenderButton.IsVisible = isVisible;
            pointButton.IsVisible = isVisible;
            waveButton.IsVisible = isVisible;
            saluteButton.IsVisible = isVisible;
            restButton.IsVisible = isVisible;
            facepalmButton.IsVisible = isVisible;
        }
    }

    public static void closeGestures()
    {
        if (!gesturing)
        {
            return;
        }
        gesturing = false;
        if (faceButtons != null)
        {
            for (int i = 0; i < faceButtons.Length; i++)
            {
                faceButtons[i].IsVisible = false;
            }
            surrenderButton.IsVisible = false;
            pointButton.IsVisible = false;
            waveButton.IsVisible = false;
            saluteButton.IsVisible = false;
            restButton.IsVisible = false;
            facepalmButton.IsVisible = false;
        }
    }

    private static void OnLocalPluginWidgetFlagsChanged(Player player, EPluginWidgetFlags oldFlags)
    {
        EPluginWidgetFlags pluginWidgetFlags = player.pluginWidgetFlags;
        if ((oldFlags & EPluginWidgetFlags.ShowStatusIcons) != (pluginWidgetFlags & EPluginWidgetFlags.ShowStatusIcons))
        {
            updateIcons();
        }
        if ((oldFlags & EPluginWidgetFlags.ShowLifeMeters) != (pluginWidgetFlags & EPluginWidgetFlags.ShowLifeMeters))
        {
            updateLifeBoxVisibility();
        }
        if ((oldFlags & EPluginWidgetFlags.ShowVehicleStatus) != (pluginWidgetFlags & EPluginWidgetFlags.ShowVehicleStatus))
        {
            UpdateVehicleBoxVisibility();
        }
        if (crosshair != null)
        {
            crosshair.SetPluginAllowsCenterDotVisible(pluginWidgetFlags.HasFlag(EPluginWidgetFlags.ShowCenterDot));
        }
    }

    private static void onDamaged(byte damage)
    {
        if (damage > 5)
        {
            PlayerUI.pain(Mathf.Clamp((float)(int)damage / 40f, 0f, 1f));
        }
    }

    private static void updateHotbarItem(ref float offset, ItemJar jar, byte index)
    {
        SleekItemIcon sleekItemIcon = hotbarImages[index];
        ISleekLabel sleekLabel = hotbarLabels[index];
        ushort num = 0;
        byte[] array = null;
        if (jar != null && jar.item != null)
        {
            num = jar.item.id;
            array = jar.item.state;
        }
        if (cachedHotbarValues[index].id != num || cachedHotbarValues[index].state != array)
        {
            cachedHotbarValues[index].id = num;
            cachedHotbarValues[index].state = array;
            ItemAsset itemAsset = Assets.find(EAssetType.ITEM, num) as ItemAsset;
            sleekItemIcon.IsVisible = itemAsset != null;
            sleekLabel.IsVisible = itemAsset != null;
            if (itemAsset != null)
            {
                sleekItemIcon.SizeOffset_X = itemAsset.size_x * 25;
                sleekItemIcon.SizeOffset_Y = itemAsset.size_y * 25;
                sleekItemIcon.Refresh(jar.item.id, jar.item.quality, jar.item.state, itemAsset);
            }
        }
        sleekItemIcon.PositionOffset_X = offset;
        sleekLabel.PositionOffset_X = offset + sleekItemIcon.SizeOffset_X - 55f;
        if (sleekItemIcon.IsVisible)
        {
            offset += sleekItemIcon.SizeOffset_X;
            hotbarContainer.SizeOffset_X = offset;
            offset += 5f;
            hotbarContainer.SizeOffset_Y = Mathf.Max(hotbarContainer.SizeOffset_Y, sleekItemIcon.SizeOffset_Y);
        }
    }

    /// <summary>
    /// Use the latest hotbar items in the UI.
    /// </summary>
    public static void updateHotbar()
    {
        if (hotbarContainer == null || Player.player == null)
        {
            return;
        }
        hotbarContainer.IsVisible = !PlayerUI.messageBox.IsVisible && !PlayerUI.messageBox2.IsVisible && OptionsSettings.showHotbar;
        if (!Player.player.inventory.doesSearchNeedRefresh(ref cachedHotbarSearch))
        {
            return;
        }
        float offset = 0f;
        updateHotbarItem(ref offset, Player.player.inventory.getItem(0, 0), 0);
        updateHotbarItem(ref offset, Player.player.inventory.getItem(1, 0), 1);
        for (byte b = 0; b < Player.player.equipment.hotkeys.Length; b++)
        {
            HotkeyInfo hotkeyInfo = Player.player.equipment.hotkeys[b];
            ItemJar itemJar = null;
            if (hotkeyInfo.id != 0)
            {
                byte index = Player.player.inventory.getIndex(hotkeyInfo.page, hotkeyInfo.x, hotkeyInfo.y);
                itemJar = Player.player.inventory.getItem(hotkeyInfo.page, index);
                if (itemJar != null && itemJar.item.id != hotkeyInfo.id)
                {
                    itemJar = null;
                }
            }
            updateHotbarItem(ref offset, itemJar, (byte)(b + 2));
        }
        hotbarContainer.PositionOffset_X = hotbarContainer.SizeOffset_X / -2f;
        hotbarContainer.PositionOffset_Y = -80f - hotbarContainer.SizeOffset_Y;
    }

    public static void updateStatTracker()
    {
        statTrackerLabel.IsVisible = Player.player.equipment.getUseableStatTrackerValue(out var type, out var kills);
        if (statTrackerLabel.IsVisible)
        {
            statTrackerLabel.TextColor = Provider.provider.economyService.getStatTrackerColor(type);
            statTrackerLabel.Text = localization.format((type == EStatTrackerType.TOTAL) ? "Stat_Tracker_Total_Kills" : "Stat_Tracker_Player_Kills", kills.ToString("D7"));
        }
    }

    private static void onHotkeysUpdated()
    {
        cachedHotbarSearch = -1;
    }

    public static void updateGrayscale()
    {
        GrayscaleEffect component = Player.player.animator.viewmodelCameraTransform.GetComponent<GrayscaleEffect>();
        GrayscaleEffect component2 = MainCamera.instance.GetComponent<GrayscaleEffect>();
        GrayscaleEffect component3 = Player.player.look.characterCamera.GetComponent<GrayscaleEffect>();
        if (Player.player.look.perspective == EPlayerPerspective.FIRST)
        {
            component.enabled = true;
            component2.enabled = false;
        }
        else
        {
            component.enabled = false;
            component2.enabled = true;
        }
        if (LevelLighting.vision == ELightingVision.CIVILIAN)
        {
            component.blend = 1f;
        }
        else if (Player.player.life.health < 50)
        {
            component.blend = (1f - (float)(int)Player.player.life.health / 50f) * (1f - Player.player.skills.mastery(1, 3) * 0.75f);
        }
        else
        {
            component.blend = 0f;
        }
        component2.blend = component.blend;
        component3.blend = component.blend;
    }

    private static void onPerspectiveUpdated(EPlayerPerspective newPerspective)
    {
        updateGrayscale();
    }

    private static void onHealthUpdated(byte newHealth)
    {
        healthProgress.state = (float)(int)newHealth / 100f;
        onPerspectiveUpdated(Player.player.look.perspective);
    }

    private static void onFoodUpdated(byte newFood)
    {
        updateIcons();
        foodProgress.state = (float)(int)newFood / 100f;
    }

    private static void onWaterUpdated(byte newWater)
    {
        updateIcons();
        waterProgress.state = (float)(int)newWater / 100f;
    }

    private static void onVirusUpdated(byte newVirus)
    {
        updateIcons();
        virusProgress.state = (float)(int)newVirus / 100f;
    }

    private static void onStaminaUpdated(byte newStamina)
    {
        staminaProgress.state = (float)(int)newStamina / 100f;
    }

    private static void onOxygenUpdated(byte newOxygen)
    {
        updateIcons();
        oxygenProgress.state = (float)(int)newOxygen / 100f;
    }

    private static void OnIsAsphyxiatingChanged()
    {
        updateIcons();
    }

    private static void updateCompassElement(ISleekElement element, float viewAngle, float elementAngle, out float alpha)
    {
        float num = Mathf.DeltaAngle(viewAngle, elementAngle) / 22.5f;
        element.PositionScale_X = num / 2f + 0.5f;
        element.IsVisible = Mathf.Abs(num) < 1f;
        alpha = 1f - MathfEx.Square(Mathf.Abs(num));
    }

    protected static bool hasCompassInInventory()
    {
        if (!Player.player.inventory.doesSearchNeedRefresh(ref cachedCompassSearch))
        {
            return cachedHasCompass;
        }
        cachedHasCompass = false;
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
                        if (asset != null && asset.enablesCompass)
                        {
                            cachedHasCompass = true;
                            return cachedHasCompass;
                        }
                    }
                }
            }
        }
        return cachedHasCompass;
    }

    public static void updateCompass()
    {
        if (Provider.modeConfigData.Gameplay.Compass || (Level.info != null && Level.info.type == ELevelType.ARENA))
        {
            compassBox.IsVisible = true;
        }
        else
        {
            compassBox.IsVisible = hasCompassInInventory();
        }
        if (!compassBox.IsVisible)
        {
            return;
        }
        Transform transform = MainCamera.instance.transform;
        Vector3 position = transform.position;
        float y = transform.rotation.eulerAngles.y;
        for (int i = 0; i < compassLabels.Length; i++)
        {
            float elementAngle = i * 5;
            ISleekLabel obj = compassLabels[i];
            Color color = obj.TextColor;
            updateCompassElement(obj, y, elementAngle, out color.a);
            obj.TextColor = color;
        }
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
                if (num < compassMarkers.Count)
                {
                    sleekImage = compassMarkers[num];
                }
                else
                {
                    sleekImage = Glazier.Get().CreateImage(icons.load<Texture2D>("Marker"));
                    sleekImage.PositionOffset_X = -10f;
                    sleekImage.PositionOffset_Y = -5f;
                    sleekImage.SizeOffset_X = 20f;
                    sleekImage.SizeOffset_Y = 20f;
                    compassMarkersContainer.AddChild(sleekImage);
                    compassMarkers.Add(sleekImage);
                }
                num++;
                float num2 = Mathf.Atan2(quests.markerPosition.x - position.x, quests.markerPosition.z - position.z);
                num2 *= 57.29578f;
                Color markerColor = client.markerColor;
                updateCompassElement(sleekImage, y, num2, out markerColor.a);
                sleekImage.TintColor = markerColor;
            }
        }
        for (int num3 = compassMarkersVisibleCount - 1; num3 >= num; num3--)
        {
            compassMarkers[num3].IsVisible = false;
        }
        compassMarkersVisibleCount = num;
    }

    private static void updateIcons()
    {
        Player player = Player.player;
        bool flag = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowStatusIcons);
        int num = 0;
        bleedingBox.IsVisible = player.life.isBleeding && flag;
        if (bleedingBox.IsVisible)
        {
            num += 60;
        }
        brokenBox.PositionOffset_X = num;
        brokenBox.IsVisible = player.life.isBroken && flag;
        if (brokenBox.IsVisible)
        {
            num += 60;
        }
        temperatureBox.PositionOffset_X = num;
        temperatureBox.IsVisible = player.life.temperature != EPlayerTemperature.NONE && flag;
        if (temperatureBox.IsVisible)
        {
            num += 60;
        }
        starvedBox.PositionOffset_X = num;
        starvedBox.IsVisible = player.life.food == 0 && flag;
        if (starvedBox.IsVisible)
        {
            num += 60;
        }
        dehydratedBox.PositionOffset_X = num;
        dehydratedBox.IsVisible = player.life.water == 0 && flag;
        if (dehydratedBox.IsVisible)
        {
            num += 60;
        }
        infectedBox.PositionOffset_X = num;
        infectedBox.IsVisible = player.life.virus == 0 && flag;
        if (infectedBox.IsVisible)
        {
            num += 60;
        }
        drownedBox.PositionOffset_X = num;
        drownedBox.IsVisible = player.life.oxygen == 0 && flag;
        if (drownedBox.IsVisible)
        {
            num += 60;
        }
        asphyxiatingBox.PositionOffset_X = num;
        asphyxiatingBox.IsVisible = !drownedBox.IsVisible && player.life.isAsphyxiating && flag;
        if (asphyxiatingBox.IsVisible)
        {
            num += 60;
        }
        moonBox.PositionOffset_X = num;
        moonBox.IsVisible = LightingManager.isFullMoon && flag;
        if (moonBox.IsVisible)
        {
            num += 60;
        }
        radiationBox.PositionOffset_X = num;
        radiationBox.IsVisible = player.movement.isRadiated && flag;
        if (radiationBox.IsVisible)
        {
            num += 60;
        }
        safeBox.PositionOffset_X = num;
        safeBox.IsVisible = player.movement.isSafe && flag;
        if (safeBox.IsVisible)
        {
            num += 60;
        }
        arrestBox.PositionOffset_X = num;
        arrestBox.IsVisible = player.animator.gesture == EPlayerGesture.ARREST_START && flag;
        if (arrestBox.IsVisible)
        {
            num += 60;
        }
        statusIconsContainer.SizeOffset_X = num - 10;
        statusIconsContainer.IsVisible = num > 0;
    }

    private static void updateLifeBoxVisibility()
    {
        Player player = Player.player;
        bool flag = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowHealth);
        bool flag2 = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowFood);
        bool flag3 = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowWater);
        bool flag4 = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowVirus);
        bool flag5 = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowStamina);
        bool flag6 = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowOxygen);
        bool flag7 = false;
        if (Level.info != null)
        {
            if (Level.info.configData != null)
            {
                flag &= Level.info.configData.PlayerUI_HealthVisible;
                flag2 &= Level.info.configData.PlayerUI_FoodVisible;
                flag3 &= Level.info.configData.PlayerUI_WaterVisible;
                flag4 &= Level.info.configData.PlayerUI_VirusVisible;
                flag5 &= Level.info.configData.PlayerUI_StaminaVisible;
                flag6 &= Level.info.configData.PlayerUI_OxygenVisible;
            }
            if (Level.info.type == ELevelType.ARENA)
            {
                levelTextBox.IsVisible = true;
                levelNumberBox.IsVisible = true;
                compassBox.PositionOffset_Y = 60f;
            }
            if (Level.info.type != 0)
            {
                flag2 = false;
                flag3 = false;
                flag4 = false;
                if (Level.info.type == ELevelType.HORDE)
                {
                    flag6 = false;
                    flag7 = true;
                }
            }
        }
        int num = 5;
        healthIcon.IsVisible = flag;
        healthProgress.IsVisible = flag;
        if (flag)
        {
            healthIcon.PositionOffset_Y = num;
            healthProgress.PositionOffset_Y = num + 5;
            num += 30;
        }
        foodIcon.IsVisible = flag2;
        foodProgress.IsVisible = flag2;
        if (flag2)
        {
            foodIcon.PositionOffset_Y = num;
            foodProgress.PositionOffset_Y = num + 5;
            num += 30;
        }
        waterIcon.IsVisible = flag3;
        waterProgress.IsVisible = flag3;
        if (flag3)
        {
            waterIcon.PositionOffset_Y = num;
            waterProgress.PositionOffset_Y = num + 5;
            num += 30;
        }
        virusIcon.IsVisible = flag4;
        virusProgress.IsVisible = flag4;
        if (flag4)
        {
            virusIcon.PositionOffset_Y = num;
            virusProgress.PositionOffset_Y = num + 5;
            num += 30;
        }
        staminaIcon.IsVisible = flag5;
        staminaProgress.IsVisible = flag5;
        if (flag5)
        {
            staminaIcon.PositionOffset_Y = num;
            staminaProgress.PositionOffset_Y = num + 5;
            num += 30;
        }
        waveLabel.IsVisible = flag7;
        scoreLabel.IsVisible = flag7;
        if (flag7)
        {
            waveLabel.PositionOffset_Y = num;
            scoreLabel.PositionOffset_Y = num;
            num += 30;
        }
        oxygenIcon.IsVisible = flag6;
        oxygenProgress.IsVisible = flag6;
        if (flag6)
        {
            oxygenIcon.PositionOffset_Y = num;
            oxygenProgress.PositionOffset_Y = num + 5;
            num += 30;
        }
        lifeBox.SizeOffset_Y = num - 5;
        lifeBox.PositionOffset_Y = 0f - lifeBox.SizeOffset_Y;
        lifeBox.IsVisible = lifeBox.SizeOffset_Y > 0f;
        statusIconsContainer.PositionOffset_Y = lifeBox.PositionOffset_Y - 60f;
    }

    private static void UpdateVehicleBoxVisibility()
    {
        bool flag = vehicleVisibleByDefault;
        flag &= Player.player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowVehicleStatus);
        vehicleBox.IsVisible = flag;
    }

    private static void onBleedingUpdated(bool newBleeding)
    {
        updateIcons();
    }

    private static void onBrokenUpdated(bool newBroken)
    {
        updateIcons();
    }

    private static void onTemperatureUpdated(EPlayerTemperature newTemperature)
    {
        switch (newTemperature)
        {
        case EPlayerTemperature.FREEZING:
            temperatureBox.icon = icons.load<Texture2D>("Freezing");
            break;
        case EPlayerTemperature.COLD:
            temperatureBox.icon = icons.load<Texture2D>("Cold");
            break;
        case EPlayerTemperature.WARM:
            temperatureBox.icon = icons.load<Texture2D>("Warm");
            break;
        case EPlayerTemperature.BURNING:
            temperatureBox.icon = icons.load<Texture2D>("Burning");
            break;
        case EPlayerTemperature.COVERED:
            temperatureBox.icon = icons.load<Texture2D>("Covered");
            break;
        case EPlayerTemperature.ACID:
            temperatureBox.icon = icons.load<Texture2D>("Acid");
            break;
        default:
            temperatureBox.icon = null;
            break;
        }
        updateIcons();
    }

    private static void onMoonUpdated(bool isFullMoon)
    {
        updateIcons();
    }

    private static void onExperienceUpdated(uint newExperience)
    {
        scoreLabel.Text = localization.format("Score", newExperience.ToString());
    }

    private static void onWaveUpdated(bool newWaveReady, int newWaveIndex)
    {
        waveLabel.Text = localization.format("Round", newWaveIndex);
        if (newWaveReady)
        {
            PlayerUI.message(EPlayerMessage.WAVE_ON, "");
        }
        else
        {
            PlayerUI.message(EPlayerMessage.WAVE_OFF, "");
        }
    }

    private static void onSeated(bool isDriver, bool inVehicle, bool wasVehicle, InteractableVehicle oldVehicle, InteractableVehicle newVehicle)
    {
        if (isDriver && inVehicle)
        {
            int num = 5;
            bool flag = newVehicle.usesFuel || newVehicle.asset.isStaminaPowered;
            fuelIcon.IsVisible = flag;
            fuelProgress.IsVisible = flag;
            if (flag)
            {
                fuelIcon.PositionOffset_Y = num;
                fuelProgress.PositionOffset_Y = num + 5;
                num += 30;
            }
            speedIcon.PositionOffset_Y = num;
            speedProgress.PositionOffset_Y = num + 5;
            num += 30;
            hpIcon.IsVisible = newVehicle.usesHealth;
            hpProgress.IsVisible = newVehicle.usesHealth;
            if (newVehicle.usesHealth)
            {
                hpIcon.PositionOffset_Y = num;
                hpProgress.PositionOffset_Y = num + 5;
                num += 30;
            }
            batteryChargeIcon.IsVisible = newVehicle.usesBattery;
            batteryChargeProgress.IsVisible = newVehicle.usesBattery;
            if (newVehicle.usesBattery)
            {
                batteryChargeIcon.PositionOffset_Y = num;
                batteryChargeProgress.PositionOffset_Y = num + 5;
                num += 30;
            }
            vehicleEngineLabel.IsVisible = newVehicle.asset.UsesEngineRpmAndGears;
            if (vehicleEngineLabel.IsVisible)
            {
                vehicleEngineLabel.PositionOffset_Y = num - 5;
                num += 30;
            }
            vehicleBox.SizeOffset_Y = num - 5;
            vehicleBox.PositionOffset_Y = 0f - vehicleBox.SizeOffset_Y;
            if (newVehicle.passengers[Player.player.movement.getSeat()].turret != null)
            {
                vehicleBox.PositionOffset_Y -= 80f;
            }
            vehicleVisibleByDefault = true;
        }
        else
        {
            vehicleVisibleByDefault = false;
        }
        UpdateVehicleBoxVisibility();
    }

    private static void onVehicleUpdated(bool isDriveable, ushort newFuel, ushort maxFuel, float newSpeed, float minSpeed, float maxSpeed, ushort newHealth, ushort maxHealth, ushort newBatteryCharge)
    {
        if (isDriveable)
        {
            fuelProgress.state = (float)(int)newFuel / (float)(int)maxFuel;
            float num = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
            num = ((!(num > 0f)) ? (num / minSpeed) : (num / maxSpeed));
            speedProgress.state = num;
            if (OptionsSettings.metric)
            {
                speedProgress.measure = (int)MeasurementTool.speedToKPH(Mathf.Abs(newSpeed));
            }
            else
            {
                speedProgress.measure = (int)MeasurementTool.KPHToMPH(MeasurementTool.speedToKPH(Mathf.Abs(newSpeed)));
            }
            batteryChargeProgress.state = (float)(int)newBatteryCharge / 10000f;
            hpProgress.state = (float)(int)newHealth / (float)(int)maxHealth;
            InteractableVehicle vehicle = Player.player.movement.getVehicle();
            if (vehicle.asset != null && vehicle.asset.canBeLocked)
            {
                vehicleLockedLabel.Text = localization.format(vehicle.isLocked ? "Vehicle_Locked" : "Vehicle_Unlocked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
                vehicleLockedLabel.IsVisible = true;
            }
            else
            {
                vehicleLockedLabel.IsVisible = false;
            }
            if (vehicleEngineLabel.IsVisible)
            {
                string arg = ((vehicle.gearNumber < 0) ? localization.format("VehicleGear_Reverse") : ((vehicle.gearNumber != 0) ? vehicle.gearNumber.ToString() : localization.format("VehicleGear_Neutral")));
                vehicleEngineLabel.Text = localization.format("VehicleEngineStatus", arg, Mathf.RoundToInt(vehicle.animatedEngineRpm));
            }
        }
        vehicleVisibleByDefault = isDriveable;
        UpdateVehicleBoxVisibility();
    }

    private static void updateGasmask()
    {
        if (Player.player.movement.isRadiated)
        {
            ItemMaskAsset maskAsset = Player.player.clothing.maskAsset;
            if (maskAsset != null && maskAsset.proofRadiation)
            {
                gasmaskIcon.Refresh(maskAsset.id, Player.player.clothing.maskQuality, Player.player.clothing.maskState, maskAsset);
                gasmaskProgress.state = (float)(int)Player.player.clothing.maskQuality / 100f;
                gasmaskProgress.color = ItemTool.getQualityColor((float)(int)Player.player.clothing.maskQuality / 100f);
                gasmaskBox.IsVisible = true;
            }
            else
            {
                gasmaskBox.IsVisible = false;
            }
        }
        else
        {
            gasmaskBox.IsVisible = false;
        }
    }

    private static void onMaskUpdated(ushort newMask, byte newMaskQuality, byte[] newMaskState)
    {
        updateGasmask();
    }

    private static void onSafetyUpdated(bool isSafe)
    {
        updateIcons();
        if (isSafe)
        {
            PlayerUI.message(EPlayerMessage.SAFEZONE_ON, "");
        }
        else
        {
            PlayerUI.message(EPlayerMessage.SAFEZONE_OFF, "");
        }
    }

    private static void onRadiationUpdated(bool isRadiated)
    {
        updateIcons();
        if (isRadiated)
        {
            PlayerUI.message(EPlayerMessage.DEADZONE_ON, "");
        }
        else
        {
            PlayerUI.message(EPlayerMessage.DEADZONE_OFF, "");
        }
        updateGasmask();
    }

    private static void onGestureUpdated(EPlayerGesture gesture)
    {
        updateIcons();
    }

    private static void onTalked(bool isTalking)
    {
        voiceBox.IsVisible = isTalking;
    }

    internal static void UpdateTrackedQuest()
    {
        QuestAsset trackedQuest = Player.player.quests.GetTrackedQuest();
        if (trackedQuest == null)
        {
            trackedQuestTitle.IsVisible = false;
            trackedQuestBar.IsVisible = false;
            return;
        }
        trackedQuestTitle.Text = trackedQuest.questName;
        bool flag = true;
        if (trackedQuest.conditions != null)
        {
            trackedQuestBar.RemoveAllChildren();
            areConditionsMet.Clear();
            INPCCondition[] conditions = trackedQuest.conditions;
            foreach (INPCCondition iNPCCondition in conditions)
            {
                areConditionsMet.Add(iNPCCondition.isConditionMet(Player.player));
            }
            int num = 5;
            for (int j = 0; j < trackedQuest.conditions.Length; j++)
            {
                INPCCondition iNPCCondition2 = trackedQuest.conditions[j];
                if (!areConditionsMet[j] && iNPCCondition2.AreUIRequirementsMet(areConditionsMet))
                {
                    string text = iNPCCondition2.formatCondition(Player.player);
                    if (!string.IsNullOrEmpty(text))
                    {
                        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                        sleekLabel.PositionOffset_X = -300f;
                        sleekLabel.PositionOffset_Y = num;
                        sleekLabel.SizeOffset_X = 500f;
                        sleekLabel.SizeOffset_Y = 30f;
                        sleekLabel.AllowRichText = true;
                        sleekLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
                        sleekLabel.TextAlignment = TextAnchor.MiddleRight;
                        sleekLabel.Text = text;
                        sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
                        trackedQuestBar.AddChild(sleekLabel);
                        num += 20;
                        flag = false;
                    }
                }
            }
        }
        trackedQuestTitle.IsVisible = !flag;
        trackedQuestBar.IsVisible = trackedQuestTitle.IsVisible;
    }

    private static void OnTrackedQuestUpdated(PlayerQuests quests)
    {
        UpdateTrackedQuest();
    }

    private static void OnChatMessageReceived()
    {
        if (chatScrollViewV2 != null && ChatManager.receivedChatHistory.Count > 0)
        {
            if (chatEntriesV2.Count >= Provider.preferenceData.Chat.History_Length)
            {
                SleekChatEntryV2 child = chatEntriesV2.Dequeue();
                chatScrollViewV2.RemoveChild(child);
            }
            SleekChatEntryV2 sleekChatEntryV = new SleekChatEntryV2();
            sleekChatEntryV.shouldFadeOutWithAge = Glazier.Get().SupportsRichTextAlpha;
            sleekChatEntryV.forceVisibleWhileBrowsingChatHistory = chatting;
            sleekChatEntryV.representingChatMessage = ChatManager.receivedChatHistory[0];
            chatScrollViewV2.AddChild(sleekChatEntryV);
            chatEntriesV2.Enqueue(sleekChatEntryV);
            if (!chatting)
            {
                chatScrollViewV2.ScrollToBottom();
            }
        }
        else
        {
            if (chatHistoryBoxV1 == null)
            {
                return;
            }
            int num = 0;
            for (int i = 0; i < ChatManager.receivedChatHistory.Count && i < chatHistoryLabelsV1.Length; i++)
            {
                int index = ChatManager.receivedChatHistory.Count - 1 - i;
                chatHistoryLabelsV1[i].representingChatMessage = ChatManager.receivedChatHistory[index];
                num++;
            }
            int num2 = num * 40;
            int num3 = chatPreviewLabelsV1.Length * 40;
            chatHistoryBoxV1.SizeOffset_Y = Mathf.Min(num2, num3);
            chatHistoryBoxV1.PositionOffset_Y = Mathf.Max(0f, (float)num3 - chatHistoryBoxV1.SizeOffset_Y);
            chatHistoryBoxV1.ContentSizeOffset = new Vector2(0f, num2);
            for (int j = 0; j < chatPreviewLabelsV1.Length; j++)
            {
                int num4 = chatPreviewLabelsV1.Length - 1 - j;
                if (num4 < ChatManager.receivedChatHistory.Count)
                {
                    chatPreviewLabelsV1[j].representingChatMessage = ChatManager.receivedChatHistory[num4];
                }
            }
        }
    }

    private static void onVotingStart(SteamPlayer origin, SteamPlayer target, byte votesNeeded)
    {
        isVoteMessaged = false;
        voteBox.Text = "";
        voteBox.IsVisible = true;
        voteInfoLabel.IsVisible = true;
        votesNeededLabel.IsVisible = true;
        voteYesLabel.IsVisible = true;
        voteNoLabel.IsVisible = true;
        voteInfoLabel.Text = localization.format("Vote_Kick", origin.playerID.characterName, origin.playerID.playerName, target.playerID.characterName, target.playerID.playerName);
        votesNeededLabel.Text = localization.format("Votes_Needed", votesNeeded);
        voteYesLabel.Text = localization.format("Vote_Yes", KeyCode.F1, 0);
        voteNoLabel.Text = localization.format("Vote_No", KeyCode.F2, 0);
    }

    private static void onVotingUpdate(byte voteYes, byte voteNo)
    {
        voteYesLabel.Text = localization.format("Vote_Yes", KeyCode.F1, voteYes);
        voteNoLabel.Text = localization.format("Vote_No", KeyCode.F2, voteNo);
    }

    private static void onVotingStop(EVotingMessage message)
    {
        voteInfoLabel.IsVisible = false;
        votesNeededLabel.IsVisible = false;
        voteYesLabel.IsVisible = false;
        voteNoLabel.IsVisible = false;
        switch (message)
        {
        case EVotingMessage.PASS:
            voteBox.Text = localization.format("Vote_Pass");
            break;
        case EVotingMessage.FAIL:
            voteBox.Text = localization.format("Vote_Fail");
            break;
        }
        isVoteMessaged = true;
        lastVoteMessage = Time.realtimeSinceStartup;
    }

    private static void onVotingMessage(EVotingMessage message)
    {
        voteBox.IsVisible = true;
        voteInfoLabel.IsVisible = false;
        votesNeededLabel.IsVisible = false;
        voteYesLabel.IsVisible = false;
        voteNoLabel.IsVisible = false;
        switch (message)
        {
        case EVotingMessage.OFF:
            voteBox.Text = localization.format("Vote_Off");
            break;
        case EVotingMessage.DELAY:
            voteBox.Text = localization.format("Vote_Delay");
            break;
        case EVotingMessage.PLAYERS:
            voteBox.Text = localization.format("Vote_Players");
            break;
        }
        isVoteMessaged = true;
        lastVoteMessage = Time.realtimeSinceStartup;
    }

    private static void onArenaMessageUpdated(EArenaMessage newArenaMessage)
    {
        switch (newArenaMessage)
        {
        case EArenaMessage.LOBBY:
            levelTextBox.Text = localization.format("Arena_Lobby");
            break;
        case EArenaMessage.WARMUP:
            levelTextBox.Text = localization.format("Arena_Warm_Up");
            break;
        case EArenaMessage.PLAY:
            levelTextBox.Text = localization.format("Arena_Play");
            break;
        case EArenaMessage.LOSE:
            levelTextBox.Text = localization.format("Arena_Lose");
            break;
        case EArenaMessage.INTERMISSION:
            levelTextBox.Text = localization.format("Arena_Intermission");
            break;
        case EArenaMessage.DIED:
        case EArenaMessage.ABANDONED:
        case EArenaMessage.WIN:
            break;
        }
    }

    private static void onArenaPlayerUpdated(ulong[] playerIDs, EArenaMessage newArenaMessage)
    {
        List<SteamPlayer> list = new List<SteamPlayer>();
        for (int i = 0; i < playerIDs.Length; i++)
        {
            SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(playerIDs[i]);
            if (steamPlayer != null)
            {
                list.Add(steamPlayer);
            }
        }
        if (list.Count != 0)
        {
            string text = "";
            for (int j = 0; j < list.Count; j++)
            {
                SteamPlayer steamPlayer2 = list[j];
                text = ((j != 0) ? ((j != list.Count - 1) ? (text + localization.format("List_Joint_0") + steamPlayer2.playerID.characterName) : (text + localization.format("List_Joint_1") + steamPlayer2.playerID.characterName)) : (text + steamPlayer2.playerID.characterName));
            }
            switch (newArenaMessage)
            {
            case EArenaMessage.DIED:
                levelTextBox.Text = localization.format("Arena_Died", text);
                break;
            case EArenaMessage.ABANDONED:
                levelTextBox.Text = localization.format("Arena_Abandoned", text);
                break;
            case EArenaMessage.WIN:
                levelTextBox.Text = localization.format("Arena_Win", text);
                break;
            }
        }
    }

    private static void onLevelNumberUpdated(int newLevelNumber)
    {
        levelNumberBox.Text = newLevelNumber.ToString();
    }

    private static void onClickedFaceButton(ISleekElement button)
    {
        byte b = 0;
        while (b < faceButtons.Length && faceButtons[b] != button)
        {
            b++;
        }
        Player.player.clothing.sendSwapFace(b);
        closeGestures();
    }

    private static void onClickedSurrenderButton(ISleekElement button)
    {
        if (Player.player.animator.gesture == EPlayerGesture.SURRENDER_START)
        {
            Player.player.animator.sendGesture(EPlayerGesture.SURRENDER_STOP, all: true);
        }
        else
        {
            Player.player.animator.sendGesture(EPlayerGesture.SURRENDER_START, all: true);
        }
        closeGestures();
    }

    private static void onClickedPointButton(ISleekElement button)
    {
        Player.player.animator.sendGesture(EPlayerGesture.POINT, all: true);
        closeGestures();
    }

    private static void onClickedWaveButton(ISleekElement button)
    {
        Player.player.animator.sendGesture(EPlayerGesture.WAVE, all: true);
        closeGestures();
    }

    private static void onClickedSaluteButton(ISleekElement button)
    {
        Player.player.animator.sendGesture(EPlayerGesture.SALUTE, all: true);
        closeGestures();
    }

    private static void onClickedRestButton(ISleekElement button)
    {
        if (Player.player.animator.gesture == EPlayerGesture.REST_START)
        {
            Player.player.animator.sendGesture(EPlayerGesture.REST_STOP, all: true);
        }
        else
        {
            Player.player.animator.sendGesture(EPlayerGesture.REST_START, all: true);
        }
        closeGestures();
    }

    private static void onClickedFacepalmButton(ISleekElement button)
    {
        Player.player.animator.sendGesture(EPlayerGesture.FACEPALM, all: true);
        closeGestures();
    }

    private void OnUnitSystemChanged()
    {
        speedProgress.suffix = (OptionsSettings.metric ? " kph" : " mph");
    }

    public void OnDestroy()
    {
        ChatManager.onChatMessageReceived = (ChatMessageReceivedHandler)Delegate.Remove(ChatManager.onChatMessageReceived, new ChatMessageReceivedHandler(OnChatMessageReceived));
        ChatManager.onVotingStart = (VotingStart)Delegate.Remove(ChatManager.onVotingStart, new VotingStart(onVotingStart));
        ChatManager.onVotingUpdate = (VotingUpdate)Delegate.Remove(ChatManager.onVotingUpdate, new VotingUpdate(onVotingUpdate));
        ChatManager.onVotingStop = (VotingStop)Delegate.Remove(ChatManager.onVotingStop, new VotingStop(onVotingStop));
        ChatManager.onVotingMessage = (VotingMessage)Delegate.Remove(ChatManager.onVotingMessage, new VotingMessage(onVotingMessage));
        LevelManager.onArenaMessageUpdated = (ArenaMessageUpdated)Delegate.Remove(LevelManager.onArenaMessageUpdated, new ArenaMessageUpdated(onArenaMessageUpdated));
        LevelManager.onArenaPlayerUpdated = (ArenaPlayerUpdated)Delegate.Remove(LevelManager.onArenaPlayerUpdated, new ArenaPlayerUpdated(onArenaPlayerUpdated));
        LevelManager.onLevelNumberUpdated = (LevelNumberUpdated)Delegate.Remove(LevelManager.onLevelNumberUpdated, new LevelNumberUpdated(onLevelNumberUpdated));
        OptionsSettings.OnUnitSystemChanged -= OnUnitSystemChanged;
        Player.player.life.OnIsAsphyxiatingChanged -= OnIsAsphyxiatingChanged;
    }

    public PlayerLifeUI()
    {
        if (icons != null)
        {
            icons.unload();
        }
        localization = Localization.read("/Player/PlayerLife.dat");
        icons = Bundles.getBundle("/Bundles/Textures/Player/Icons/PlayerLife/PlayerLife.unity3d");
        _container = new SleekFullscreenBox();
        container.PositionOffset_X = 10f;
        container.PositionOffset_Y = 10f;
        container.SizeOffset_X = -20f;
        container.SizeOffset_Y = -20f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = true;
        chatting = false;
        if (Glazier.Get().SupportsAutomaticLayout)
        {
            chatScrollViewV2 = Glazier.Get().CreateScrollView();
            chatScrollViewV2.SizeOffset_X = 630f;
            chatScrollViewV2.SizeOffset_Y = Provider.preferenceData.Chat.Preview_Length * 40;
            chatScrollViewV2.ScaleContentToWidth = true;
            chatScrollViewV2.ContentUseManualLayout = false;
            chatScrollViewV2.AlignContentToBottom = true;
            chatScrollViewV2.VerticalScrollbarVisibility = ESleekScrollbarVisibility.Hidden;
            chatScrollViewV2.IsRaycastTarget = false;
            container.AddChild(chatScrollViewV2);
            chatEntriesV2 = new Queue<SleekChatEntryV2>(Provider.preferenceData.Chat.History_Length);
        }
        else
        {
            chatHistoryBoxV1 = Glazier.Get().CreateScrollView();
            chatHistoryBoxV1.SizeOffset_X = 630f;
            chatHistoryBoxV1.ScaleContentToWidth = true;
            container.AddChild(chatHistoryBoxV1);
            chatHistoryBoxV1.IsVisible = false;
            chatHistoryLabelsV1 = new SleekChatEntryV1[Provider.preferenceData.Chat.History_Length];
            for (int i = 0; i < chatHistoryLabelsV1.Length; i++)
            {
                SleekChatEntryV1 sleekChatEntryV = new SleekChatEntryV1
                {
                    PositionOffset_Y = i * 40,
                    SizeOffset_X = chatHistoryBoxV1.SizeOffset_X - 30f,
                    SizeOffset_Y = 40f,
                    shouldFadeOutWithAge = false
                };
                chatHistoryBoxV1.AddChild(sleekChatEntryV);
                chatHistoryLabelsV1[i] = sleekChatEntryV;
            }
            bool supportsRichTextAlpha = Glazier.Get().SupportsRichTextAlpha;
            chatPreviewLabelsV1 = new SleekChatEntryV1[Provider.preferenceData.Chat.Preview_Length];
            for (int j = 0; j < chatPreviewLabelsV1.Length; j++)
            {
                SleekChatEntryV1 sleekChatEntryV2 = new SleekChatEntryV1
                {
                    PositionOffset_Y = j * 40,
                    SizeOffset_X = chatHistoryBoxV1.SizeOffset_X - 30f,
                    SizeOffset_Y = 40f,
                    shouldFadeOutWithAge = supportsRichTextAlpha
                };
                container.AddChild(sleekChatEntryV2);
                chatPreviewLabelsV1[j] = sleekChatEntryV2;
            }
        }
        chatField = Glazier.Get().CreateStringField();
        chatField.PositionOffset_Y = Provider.preferenceData.Chat.Preview_Length * 40 + 10;
        chatField.SizeOffset_X = 500f;
        chatField.PositionOffset_X = 0f - chatField.SizeOffset_X - 50f;
        chatField.SizeOffset_Y = 30f;
        chatField.TextAlignment = TextAnchor.MiddleLeft;
        chatField.MaxLength = ChatManager.MAX_MESSAGE_LENGTH;
        chatField.OnTextEscaped += OnChatFieldEscaped;
        container.AddChild(chatField);
        chatModeButton = new SleekButtonState();
        chatModeButton.UseContentTooltip = true;
        chatModeButton.setContent(new GUIContent(localization.format("Mode_Global"), localization.format("Mode_Global_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.global))), new GUIContent(localization.format("Mode_Local"), localization.format("Mode_Local_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.local))), new GUIContent(localization.format("Mode_Group"), localization.format("Mode_Group_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.group))));
        chatModeButton.PositionOffset_X = -100f;
        chatModeButton.SizeOffset_X = 100f;
        chatModeButton.SizeOffset_Y = 30f;
        chatModeButton.onSwappedState = OnSwappedChatModeState;
        chatField.AddChild(chatModeButton);
        sendChatButton = new SleekButtonIcon(icons.load<Texture2D>("SendChat"));
        sendChatButton.PositionScale_X = 1f;
        sendChatButton.SizeOffset_X = 30f;
        sendChatButton.SizeOffset_Y = 30f;
        sendChatButton.tooltip = localization.format("SendChat_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Return));
        sendChatButton.iconColor = ESleekTint.FOREGROUND;
        sendChatButton.onClickedButton += OnSendChatButtonClicked;
        chatField.AddChild(sendChatButton);
        voteBox = Glazier.Get().CreateBox();
        voteBox.PositionOffset_X = -430f;
        voteBox.PositionScale_X = 1f;
        voteBox.SizeOffset_X = 430f;
        voteBox.SizeOffset_Y = 90f;
        container.AddChild(voteBox);
        voteBox.IsVisible = false;
        voteInfoLabel = Glazier.Get().CreateLabel();
        voteInfoLabel.SizeOffset_Y = 30f;
        voteInfoLabel.SizeScale_X = 1f;
        voteBox.AddChild(voteInfoLabel);
        votesNeededLabel = Glazier.Get().CreateLabel();
        votesNeededLabel.PositionOffset_Y = 30f;
        votesNeededLabel.SizeOffset_Y = 30f;
        votesNeededLabel.SizeScale_X = 1f;
        voteBox.AddChild(votesNeededLabel);
        voteYesLabel = Glazier.Get().CreateLabel();
        voteYesLabel.PositionOffset_Y = 60f;
        voteYesLabel.SizeOffset_Y = 30f;
        voteYesLabel.SizeScale_X = 0.5f;
        voteBox.AddChild(voteYesLabel);
        voteNoLabel = Glazier.Get().CreateLabel();
        voteNoLabel.PositionOffset_Y = 60f;
        voteNoLabel.PositionScale_X = 0.5f;
        voteNoLabel.SizeOffset_Y = 30f;
        voteNoLabel.SizeScale_X = 0.5f;
        voteBox.AddChild(voteNoLabel);
        voiceBox = new SleekBoxIcon(icons.load<Texture2D>("Voice"));
        voiceBox.PositionOffset_Y = 210f;
        voiceBox.SizeOffset_X = 50f;
        voiceBox.SizeOffset_Y = 50f;
        voiceBox.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(voiceBox);
        voiceBox.IsVisible = false;
        trackedQuestTitle = Glazier.Get().CreateLabel();
        trackedQuestTitle.PositionOffset_X = -500f;
        trackedQuestTitle.PositionOffset_Y = 200f;
        trackedQuestTitle.PositionScale_X = 1f;
        trackedQuestTitle.SizeOffset_X = 500f;
        trackedQuestTitle.SizeOffset_Y = 35f;
        trackedQuestTitle.AllowRichText = true;
        trackedQuestTitle.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
        trackedQuestTitle.FontSize = ESleekFontSize.Medium;
        trackedQuestTitle.TextAlignment = TextAnchor.LowerRight;
        trackedQuestTitle.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        container.AddChild(trackedQuestTitle);
        trackedQuestBar = Glazier.Get().CreateImage();
        trackedQuestBar.PositionOffset_X = -200f;
        trackedQuestBar.PositionOffset_Y = 240f;
        trackedQuestBar.PositionScale_X = 1f;
        trackedQuestBar.SizeOffset_X = 200f;
        trackedQuestBar.SizeOffset_Y = 3f;
        trackedQuestBar.Texture = (Texture2D)GlazierResources.PixelTexture;
        trackedQuestBar.TintColor = ESleekTint.FOREGROUND;
        container.AddChild(trackedQuestBar);
        levelTextBox = Glazier.Get().CreateBox();
        levelTextBox.PositionOffset_X = -180f;
        levelTextBox.PositionScale_X = 0.5f;
        levelTextBox.SizeOffset_X = 300f;
        levelTextBox.SizeOffset_Y = 50f;
        levelTextBox.FontSize = ESleekFontSize.Medium;
        container.AddChild(levelTextBox);
        levelTextBox.IsVisible = false;
        levelNumberBox = Glazier.Get().CreateBox();
        levelNumberBox.PositionOffset_X = 130f;
        levelNumberBox.PositionScale_X = 0.5f;
        levelNumberBox.SizeOffset_X = 50f;
        levelNumberBox.SizeOffset_Y = 50f;
        levelNumberBox.FontSize = ESleekFontSize.Medium;
        container.AddChild(levelNumberBox);
        levelNumberBox.IsVisible = false;
        cachedCompassSearch = -1;
        cachedHasCompass = false;
        compassBox = Glazier.Get().CreateBox();
        compassBox.PositionOffset_X = -180f;
        compassBox.PositionScale_X = 0.5f;
        compassBox.SizeOffset_X = 360f;
        compassBox.SizeOffset_Y = 50f;
        compassBox.FontSize = ESleekFontSize.Medium;
        container.AddChild(compassBox);
        compassBox.IsVisible = false;
        compassLabelsContainer = Glazier.Get().CreateFrame();
        compassLabelsContainer.PositionOffset_X = 10f;
        compassLabelsContainer.PositionOffset_Y = 10f;
        compassLabelsContainer.SizeOffset_X = -20f;
        compassLabelsContainer.SizeOffset_Y = -20f;
        compassLabelsContainer.SizeScale_X = 1f;
        compassLabelsContainer.SizeScale_Y = 1f;
        compassBox.AddChild(compassLabelsContainer);
        compassMarkersContainer = Glazier.Get().CreateFrame();
        compassMarkersContainer.PositionOffset_X = 10f;
        compassMarkersContainer.PositionOffset_Y = 10f;
        compassMarkersContainer.SizeOffset_X = -20f;
        compassMarkersContainer.SizeOffset_Y = -20f;
        compassMarkersContainer.SizeScale_X = 1f;
        compassMarkersContainer.SizeScale_Y = 1f;
        compassBox.AddChild(compassMarkersContainer);
        compassMarkers = new List<ISleekImage>();
        compassMarkersVisibleCount = 0;
        compassLabels = new ISleekLabel[72];
        for (int k = 0; k < compassLabels.Length; k++)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.PositionOffset_X = -25f;
            sleekLabel.SizeOffset_X = 50f;
            sleekLabel.SizeOffset_Y = 30f;
            sleekLabel.Text = (k * 5).ToString();
            sleekLabel.TextColor = new Color(0.75f, 0.75f, 0.75f);
            compassLabelsContainer.AddChild(sleekLabel);
            compassLabels[k] = sleekLabel;
        }
        ISleekLabel compassLabelByAngle = getCompassLabelByAngle(0);
        compassLabelByAngle.FontSize = ESleekFontSize.Large;
        compassLabelByAngle.Text = "N";
        compassLabelByAngle.TextColor = Palette.COLOR_R;
        ISleekLabel compassLabelByAngle2 = getCompassLabelByAngle(45);
        compassLabelByAngle2.FontSize = ESleekFontSize.Medium;
        compassLabelByAngle2.Text = "NE";
        compassLabelByAngle2.TextColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle3 = getCompassLabelByAngle(90);
        compassLabelByAngle3.FontSize = ESleekFontSize.Large;
        compassLabelByAngle3.Text = "E";
        compassLabelByAngle3.TextColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle4 = getCompassLabelByAngle(135);
        compassLabelByAngle4.FontSize = ESleekFontSize.Medium;
        compassLabelByAngle4.Text = "SE";
        compassLabelByAngle4.TextColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle5 = getCompassLabelByAngle(180);
        compassLabelByAngle5.FontSize = ESleekFontSize.Large;
        compassLabelByAngle5.Text = "S";
        compassLabelByAngle5.TextColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle6 = getCompassLabelByAngle(225);
        compassLabelByAngle6.FontSize = ESleekFontSize.Medium;
        compassLabelByAngle6.Text = "SW";
        compassLabelByAngle6.TextColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle7 = getCompassLabelByAngle(270);
        compassLabelByAngle7.FontSize = ESleekFontSize.Large;
        compassLabelByAngle7.Text = "W";
        compassLabelByAngle7.TextColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle8 = getCompassLabelByAngle(315);
        compassLabelByAngle8.FontSize = ESleekFontSize.Medium;
        compassLabelByAngle8.Text = "NW";
        compassLabelByAngle8.TextColor = new Color(1f, 1f, 1f);
        hotbarContainer = Glazier.Get().CreateFrame();
        hotbarContainer.PositionScale_X = 0.5f;
        hotbarContainer.PositionScale_Y = 1f;
        hotbarContainer.PositionOffset_Y = -200f;
        container.AddChild(hotbarContainer);
        hotbarContainer.IsVisible = false;
        cachedHotbarSearch = -1;
        cachedHotbarValues = new CachedHotbarItem[10];
        hotbarImages = new SleekItemIcon[cachedHotbarValues.Length];
        for (int l = 0; l < hotbarImages.Length; l++)
        {
            SleekItemIcon sleekItemIcon = new SleekItemIcon
            {
                color = new Color(1f, 1f, 1f, 0.5f)
            };
            hotbarContainer.AddChild(sleekItemIcon);
            sleekItemIcon.IsVisible = false;
            hotbarImages[l] = sleekItemIcon;
        }
        hotbarLabels = new ISleekLabel[cachedHotbarValues.Length];
        for (int m = 0; m < hotbarLabels.Length; m++)
        {
            ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
            sleekLabel2.PositionOffset_Y = 5f;
            sleekLabel2.SizeOffset_X = 50f;
            sleekLabel2.SizeOffset_Y = 30f;
            sleekLabel2.Text = ControlsSettings.getEquipmentHotkeyText(m);
            sleekLabel2.TextAlignment = TextAnchor.UpperRight;
            sleekLabel2.TextColor = new SleekColor(ESleekTint.FONT, 0.75f);
            sleekLabel2.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            hotbarContainer.AddChild(sleekLabel2);
            sleekLabel2.IsVisible = false;
            hotbarLabels[m] = sleekLabel2;
        }
        statTrackerLabel = Glazier.Get().CreateLabel();
        statTrackerLabel.PositionOffset_X = -100f;
        statTrackerLabel.PositionOffset_Y = -30f;
        statTrackerLabel.PositionScale_X = 0.5f;
        statTrackerLabel.PositionScale_Y = 1f;
        statTrackerLabel.SizeOffset_X = 200f;
        statTrackerLabel.SizeOffset_Y = 30f;
        statTrackerLabel.TextAlignment = TextAnchor.LowerCenter;
        statTrackerLabel.FontStyle = FontStyle.Italic;
        statTrackerLabel.FontSize = ESleekFontSize.Default;
        container.AddChild(statTrackerLabel);
        statTrackerLabel.IsVisible = false;
        scopeOverlay = new SleekScopeOverlay();
        scopeOverlay.SizeScale_X = 1f;
        scopeOverlay.SizeScale_Y = 1f;
        scopeOverlay.IsVisible = false;
        PlayerUI.window.AddChild(scopeOverlay);
        binocularsOverlay = Glazier.Get().CreateImage((Texture2D)Resources.Load("Overlay/Binoculars"));
        binocularsOverlay.SizeScale_X = 1f;
        binocularsOverlay.SizeScale_Y = 1f;
        PlayerUI.window.AddChild(binocularsOverlay);
        binocularsOverlay.IsVisible = false;
        faceButtons = new ISleekButton[Customization.FACES_FREE + Customization.FACES_PRO];
        for (int n = 0; n < faceButtons.Length; n++)
        {
            float num = MathF.PI * 4f * ((float)n / (float)faceButtons.Length);
            float num2 = 210f;
            if (n >= faceButtons.Length / 2)
            {
                num += MathF.PI / (float)(faceButtons.Length / 2);
                num2 += 30f;
            }
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.PositionOffset_X = (int)(Mathf.Cos(num) * num2) - 20;
            sleekButton.PositionOffset_Y = (int)(Mathf.Sin(num) * num2) - 20;
            sleekButton.PositionScale_X = 0.5f;
            sleekButton.PositionScale_Y = 0.5f;
            sleekButton.SizeOffset_X = 40f;
            sleekButton.SizeOffset_Y = 40f;
            container.AddChild(sleekButton);
            sleekButton.IsVisible = false;
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.PositionOffset_X = 10f;
            sleekImage.PositionOffset_Y = 10f;
            sleekImage.SizeOffset_X = 20f;
            sleekImage.SizeOffset_Y = 20f;
            sleekImage.Texture = (Texture2D)GlazierResources.PixelTexture;
            sleekImage.TintColor = Characters.active.skin;
            sleekButton.AddChild(sleekImage);
            ISleekImage sleekImage2 = Glazier.Get().CreateImage();
            sleekImage2.PositionOffset_X = 2f;
            sleekImage2.PositionOffset_Y = 2f;
            sleekImage2.SizeOffset_X = 16f;
            sleekImage2.SizeOffset_Y = 16f;
            sleekImage2.Texture = (Texture2D)Resources.Load("Faces/" + n + "/Texture");
            sleekImage.AddChild(sleekImage2);
            if (n >= Customization.FACES_FREE)
            {
                if (Provider.isPro)
                {
                    sleekButton.OnClicked += onClickedFaceButton;
                }
                else
                {
                    sleekButton.BackgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                    Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
                    ISleekImage sleekImage3 = Glazier.Get().CreateImage();
                    sleekImage3.PositionOffset_X = -10f;
                    sleekImage3.PositionOffset_Y = -10f;
                    sleekImage3.PositionScale_X = 0.5f;
                    sleekImage3.PositionScale_Y = 0.5f;
                    sleekImage3.SizeOffset_X = 20f;
                    sleekImage3.SizeOffset_Y = 20f;
                    sleekImage3.Texture = bundle.load<Texture2D>("Lock_Small");
                    sleekButton.AddChild(sleekImage3);
                    bundle.unload();
                }
            }
            else
            {
                sleekButton.OnClicked += onClickedFaceButton;
            }
            faceButtons[n] = sleekButton;
        }
        surrenderButton = Glazier.Get().CreateButton();
        surrenderButton.PositionOffset_X = -160f;
        surrenderButton.PositionOffset_Y = -15f;
        surrenderButton.PositionScale_X = 0.5f;
        surrenderButton.PositionScale_Y = 0.5f;
        surrenderButton.SizeOffset_X = 150f;
        surrenderButton.SizeOffset_Y = 30f;
        surrenderButton.Text = localization.format("Surrender");
        surrenderButton.OnClicked += onClickedSurrenderButton;
        container.AddChild(surrenderButton);
        surrenderButton.IsVisible = false;
        pointButton = Glazier.Get().CreateButton();
        pointButton.PositionOffset_X = 10f;
        pointButton.PositionOffset_Y = -15f;
        pointButton.PositionScale_X = 0.5f;
        pointButton.PositionScale_Y = 0.5f;
        pointButton.SizeOffset_X = 150f;
        pointButton.SizeOffset_Y = 30f;
        pointButton.Text = localization.format("Point");
        pointButton.OnClicked += onClickedPointButton;
        container.AddChild(pointButton);
        pointButton.IsVisible = false;
        waveButton = Glazier.Get().CreateButton();
        waveButton.PositionOffset_X = -75f;
        waveButton.PositionOffset_Y = -55f;
        waveButton.PositionScale_X = 0.5f;
        waveButton.PositionScale_Y = 0.5f;
        waveButton.SizeOffset_X = 150f;
        waveButton.SizeOffset_Y = 30f;
        waveButton.Text = localization.format("Wave");
        waveButton.OnClicked += onClickedWaveButton;
        container.AddChild(waveButton);
        waveButton.IsVisible = false;
        saluteButton = Glazier.Get().CreateButton();
        saluteButton.PositionOffset_X = -75f;
        saluteButton.PositionOffset_Y = 25f;
        saluteButton.PositionScale_X = 0.5f;
        saluteButton.PositionScale_Y = 0.5f;
        saluteButton.SizeOffset_X = 150f;
        saluteButton.SizeOffset_Y = 30f;
        saluteButton.Text = localization.format("Salute");
        saluteButton.OnClicked += onClickedSaluteButton;
        container.AddChild(saluteButton);
        saluteButton.IsVisible = false;
        restButton = Glazier.Get().CreateButton();
        restButton.PositionOffset_X = -160f;
        restButton.PositionOffset_Y = 65f;
        restButton.PositionScale_X = 0.5f;
        restButton.PositionScale_Y = 0.5f;
        restButton.SizeOffset_X = 150f;
        restButton.SizeOffset_Y = 30f;
        restButton.Text = localization.format("Rest");
        restButton.OnClicked += onClickedRestButton;
        container.AddChild(restButton);
        restButton.IsVisible = false;
        facepalmButton = Glazier.Get().CreateButton();
        facepalmButton.PositionOffset_X = 10f;
        facepalmButton.PositionOffset_Y = -95f;
        facepalmButton.PositionScale_X = 0.5f;
        facepalmButton.PositionScale_Y = 0.5f;
        facepalmButton.SizeOffset_X = 150f;
        facepalmButton.SizeOffset_Y = 30f;
        facepalmButton.Text = localization.format("Facepalm");
        facepalmButton.OnClicked += onClickedFacepalmButton;
        container.AddChild(facepalmButton);
        facepalmButton.IsVisible = false;
        activeHitmarkers = new List<HitmarkerInfo>(16);
        hitmarkersPool = new List<SleekHitmarker>(16);
        for (int num3 = 0; num3 < 16; num3++)
        {
            ReleaseHitmarker(NewHitmarker());
        }
        crosshair = new Crosshair(icons);
        crosshair.SizeScale_X = 1f;
        crosshair.SizeScale_Y = 1f;
        container.AddChild(crosshair);
        crosshair.SetPluginAllowsCenterDotVisible(Player.player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowCenterDot));
        lifeBox = Glazier.Get().CreateBox();
        lifeBox.PositionScale_Y = 1f;
        lifeBox.SizeScale_X = 0.2f;
        container.AddChild(lifeBox);
        statusIconsContainer = Glazier.Get().CreateFrame();
        statusIconsContainer.PositionOffset_Y = -60f;
        statusIconsContainer.PositionScale_Y = 1f;
        statusIconsContainer.SizeScale_X = 0.2f;
        statusIconsContainer.SizeOffset_Y = 50f;
        container.AddChild(statusIconsContainer);
        healthIcon = Glazier.Get().CreateImage();
        healthIcon.PositionOffset_X = 5f;
        healthIcon.SizeOffset_X = 20f;
        healthIcon.SizeOffset_Y = 20f;
        healthIcon.Texture = icons.load<Texture2D>("Health");
        lifeBox.AddChild(healthIcon);
        healthProgress = new SleekProgress("");
        healthProgress.PositionOffset_X = 30f;
        healthProgress.SizeOffset_X = -40f;
        healthProgress.SizeOffset_Y = 10f;
        healthProgress.SizeScale_X = 1f;
        healthProgress.color = Palette.COLOR_R;
        lifeBox.AddChild(healthProgress);
        foodIcon = Glazier.Get().CreateImage();
        foodIcon.PositionOffset_X = 5f;
        foodIcon.SizeOffset_X = 20f;
        foodIcon.SizeOffset_Y = 20f;
        foodIcon.Texture = icons.load<Texture2D>("Food");
        lifeBox.AddChild(foodIcon);
        foodProgress = new SleekProgress("");
        foodProgress.PositionOffset_X = 30f;
        foodProgress.SizeOffset_X = -40f;
        foodProgress.SizeOffset_Y = 10f;
        foodProgress.SizeScale_X = 1f;
        foodProgress.color = Palette.COLOR_O;
        lifeBox.AddChild(foodProgress);
        waterIcon = Glazier.Get().CreateImage();
        waterIcon.PositionOffset_X = 5f;
        waterIcon.SizeOffset_X = 20f;
        waterIcon.SizeOffset_Y = 20f;
        waterIcon.Texture = icons.load<Texture2D>("Water");
        lifeBox.AddChild(waterIcon);
        waterProgress = new SleekProgress("");
        waterProgress.PositionOffset_X = 30f;
        waterProgress.SizeOffset_X = -40f;
        waterProgress.SizeOffset_Y = 10f;
        waterProgress.SizeScale_X = 1f;
        waterProgress.color = Palette.COLOR_B;
        lifeBox.AddChild(waterProgress);
        virusIcon = Glazier.Get().CreateImage();
        virusIcon.PositionOffset_X = 5f;
        virusIcon.SizeOffset_X = 20f;
        virusIcon.SizeOffset_Y = 20f;
        virusIcon.Texture = icons.load<Texture2D>("Virus");
        lifeBox.AddChild(virusIcon);
        virusProgress = new SleekProgress("");
        virusProgress.PositionOffset_X = 30f;
        virusProgress.SizeOffset_X = -40f;
        virusProgress.SizeOffset_Y = 10f;
        virusProgress.SizeScale_X = 1f;
        virusProgress.color = Palette.COLOR_G;
        lifeBox.AddChild(virusProgress);
        staminaIcon = Glazier.Get().CreateImage();
        staminaIcon.PositionOffset_X = 5f;
        staminaIcon.SizeOffset_X = 20f;
        staminaIcon.SizeOffset_Y = 20f;
        staminaIcon.Texture = icons.load<Texture2D>("Stamina");
        lifeBox.AddChild(staminaIcon);
        staminaProgress = new SleekProgress("");
        staminaProgress.PositionOffset_X = 30f;
        staminaProgress.SizeOffset_X = -40f;
        staminaProgress.SizeOffset_Y = 10f;
        staminaProgress.SizeScale_X = 1f;
        staminaProgress.color = Palette.COLOR_Y;
        lifeBox.AddChild(staminaProgress);
        waveLabel = Glazier.Get().CreateLabel();
        waveLabel.SizeOffset_Y = 30f;
        waveLabel.SizeScale_X = 0.5f;
        lifeBox.AddChild(waveLabel);
        scoreLabel = Glazier.Get().CreateLabel();
        scoreLabel.PositionScale_X = 0.5f;
        scoreLabel.SizeOffset_Y = 30f;
        scoreLabel.SizeScale_X = 0.5f;
        lifeBox.AddChild(scoreLabel);
        oxygenIcon = Glazier.Get().CreateImage();
        oxygenIcon.PositionOffset_X = 5f;
        oxygenIcon.SizeOffset_X = 20f;
        oxygenIcon.SizeOffset_Y = 20f;
        oxygenIcon.Texture = icons.load<Texture2D>("Oxygen");
        lifeBox.AddChild(oxygenIcon);
        oxygenProgress = new SleekProgress("");
        oxygenProgress.PositionOffset_X = 30f;
        oxygenProgress.SizeOffset_X = -40f;
        oxygenProgress.SizeOffset_Y = 10f;
        oxygenProgress.SizeScale_X = 1f;
        oxygenProgress.color = Palette.COLOR_W;
        lifeBox.AddChild(oxygenProgress);
        vehicleBox = Glazier.Get().CreateBox();
        vehicleBox.PositionOffset_Y = -120f;
        vehicleBox.PositionScale_X = 0.8f;
        vehicleBox.PositionScale_Y = 1f;
        vehicleBox.SizeOffset_Y = 120f;
        vehicleBox.SizeScale_X = 0.2f;
        container.AddChild(vehicleBox);
        vehicleVisibleByDefault = false;
        fuelIcon = Glazier.Get().CreateImage();
        fuelIcon.PositionOffset_X = 5f;
        fuelIcon.PositionOffset_Y = 5f;
        fuelIcon.SizeOffset_X = 20f;
        fuelIcon.SizeOffset_Y = 20f;
        fuelIcon.Texture = icons.load<Texture2D>("Fuel");
        vehicleBox.AddChild(fuelIcon);
        fuelProgress = new SleekProgress("");
        fuelProgress.PositionOffset_X = 30f;
        fuelProgress.PositionOffset_Y = 10f;
        fuelProgress.SizeOffset_X = -40f;
        fuelProgress.SizeOffset_Y = 10f;
        fuelProgress.SizeScale_X = 1f;
        fuelProgress.color = Palette.COLOR_Y;
        vehicleBox.AddChild(fuelProgress);
        speedIcon = Glazier.Get().CreateImage();
        speedIcon.PositionOffset_X = 5f;
        speedIcon.PositionOffset_Y = 35f;
        speedIcon.SizeOffset_X = 20f;
        speedIcon.SizeOffset_Y = 20f;
        speedIcon.Texture = icons.load<Texture2D>("Speed");
        vehicleBox.AddChild(speedIcon);
        speedProgress = new SleekProgress(OptionsSettings.metric ? " kph" : " mph");
        speedProgress.PositionOffset_X = 30f;
        speedProgress.PositionOffset_Y = 40f;
        speedProgress.SizeOffset_X = -40f;
        speedProgress.SizeOffset_Y = 10f;
        speedProgress.SizeScale_X = 1f;
        speedProgress.color = Palette.COLOR_P;
        vehicleBox.AddChild(speedProgress);
        hpIcon = Glazier.Get().CreateImage();
        hpIcon.PositionOffset_X = 5f;
        hpIcon.PositionOffset_Y = 65f;
        hpIcon.SizeOffset_X = 20f;
        hpIcon.SizeOffset_Y = 20f;
        hpIcon.Texture = icons.load<Texture2D>("Health");
        vehicleBox.AddChild(hpIcon);
        hpProgress = new SleekProgress("");
        hpProgress.PositionOffset_X = 30f;
        hpProgress.PositionOffset_Y = 70f;
        hpProgress.SizeOffset_X = -40f;
        hpProgress.SizeOffset_Y = 10f;
        hpProgress.SizeScale_X = 1f;
        hpProgress.color = Palette.COLOR_R;
        vehicleBox.AddChild(hpProgress);
        batteryChargeIcon = Glazier.Get().CreateImage();
        batteryChargeIcon.PositionOffset_X = 5f;
        batteryChargeIcon.PositionOffset_Y = 95f;
        batteryChargeIcon.SizeOffset_X = 20f;
        batteryChargeIcon.SizeOffset_Y = 20f;
        batteryChargeIcon.Texture = icons.load<Texture2D>("Stamina");
        vehicleBox.AddChild(batteryChargeIcon);
        batteryChargeProgress = new SleekProgress("");
        batteryChargeProgress.PositionOffset_X = 30f;
        batteryChargeProgress.PositionOffset_Y = 100f;
        batteryChargeProgress.SizeOffset_X = -40f;
        batteryChargeProgress.SizeOffset_Y = 10f;
        batteryChargeProgress.SizeScale_X = 1f;
        batteryChargeProgress.color = Palette.COLOR_Y;
        vehicleBox.AddChild(batteryChargeProgress);
        vehicleLockedLabel = Glazier.Get().CreateLabel();
        vehicleLockedLabel.PositionOffset_Y = -25f;
        vehicleLockedLabel.SizeScale_X = 1f;
        vehicleLockedLabel.SizeOffset_Y = 30f;
        vehicleLockedLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
        vehicleBox.AddChild(vehicleLockedLabel);
        vehicleEngineLabel = Glazier.Get().CreateLabel();
        vehicleEngineLabel.SizeScale_X = 1f;
        vehicleEngineLabel.SizeOffset_Y = 30f;
        vehicleBox.AddChild(vehicleEngineLabel);
        gasmaskBox = Glazier.Get().CreateBox();
        gasmaskBox.PositionOffset_X = -200f;
        gasmaskBox.PositionOffset_Y = -60f;
        gasmaskBox.PositionScale_X = 0.5f;
        gasmaskBox.PositionScale_Y = 1f;
        gasmaskBox.SizeOffset_X = 400f;
        gasmaskBox.SizeOffset_Y = 60f;
        container.AddChild(gasmaskBox);
        gasmaskBox.IsVisible = false;
        gasmaskIcon = new SleekItemIcon();
        gasmaskIcon.PositionOffset_X = 5f;
        gasmaskIcon.PositionOffset_Y = 5f;
        gasmaskIcon.SizeOffset_X = 50f;
        gasmaskIcon.SizeOffset_Y = 50f;
        gasmaskBox.AddChild(gasmaskIcon);
        gasmaskProgress = new SleekProgress("");
        gasmaskProgress.PositionOffset_X = 60f;
        gasmaskProgress.PositionOffset_Y = 10f;
        gasmaskProgress.SizeOffset_X = -70f;
        gasmaskProgress.SizeOffset_Y = 40f;
        gasmaskProgress.SizeScale_X = 1f;
        gasmaskBox.AddChild(gasmaskProgress);
        bleedingBox = new SleekBoxIcon(icons.load<Texture2D>("Bleeding"));
        bleedingBox.SizeOffset_X = 50f;
        bleedingBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(bleedingBox);
        bleedingBox.IsVisible = false;
        brokenBox = new SleekBoxIcon(icons.load<Texture2D>("Broken"));
        brokenBox.SizeOffset_X = 50f;
        brokenBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(brokenBox);
        brokenBox.IsVisible = false;
        temperatureBox = new SleekBoxIcon(null);
        temperatureBox.SizeOffset_X = 50f;
        temperatureBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(temperatureBox);
        temperatureBox.IsVisible = false;
        starvedBox = new SleekBoxIcon(icons.load<Texture2D>("Starved"));
        starvedBox.SizeOffset_X = 50f;
        starvedBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(starvedBox);
        starvedBox.IsVisible = false;
        dehydratedBox = new SleekBoxIcon(icons.load<Texture2D>("Dehydrated"));
        dehydratedBox.SizeOffset_X = 50f;
        dehydratedBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(dehydratedBox);
        dehydratedBox.IsVisible = false;
        infectedBox = new SleekBoxIcon(icons.load<Texture2D>("Infected"));
        infectedBox.SizeOffset_X = 50f;
        infectedBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(infectedBox);
        infectedBox.IsVisible = false;
        drownedBox = new SleekBoxIcon(icons.load<Texture2D>("Drowned"));
        drownedBox.SizeOffset_X = 50f;
        drownedBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(drownedBox);
        drownedBox.IsVisible = false;
        asphyxiatingBox = new SleekBoxIcon(icons.load<Texture2D>("AsphyxiatingStatus"));
        asphyxiatingBox.SizeOffset_X = 50f;
        asphyxiatingBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(asphyxiatingBox);
        asphyxiatingBox.IsVisible = false;
        moonBox = new SleekBoxIcon(icons.load<Texture2D>("Moon"));
        moonBox.SizeOffset_X = 50f;
        moonBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(moonBox);
        moonBox.IsVisible = false;
        radiationBox = new SleekBoxIcon(icons.load<Texture2D>("Deadzone"));
        radiationBox.SizeOffset_X = 50f;
        radiationBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(radiationBox);
        radiationBox.IsVisible = false;
        safeBox = new SleekBoxIcon(icons.load<Texture2D>("Safe"));
        safeBox.SizeOffset_X = 50f;
        safeBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(safeBox);
        safeBox.IsVisible = false;
        arrestBox = new SleekBoxIcon(icons.load<Texture2D>("Arrest"));
        arrestBox.SizeOffset_X = 50f;
        arrestBox.SizeOffset_Y = 50f;
        statusIconsContainer.AddChild(arrestBox);
        arrestBox.IsVisible = false;
        updateIcons();
        updateLifeBoxVisibility();
        UpdateVehicleBoxVisibility();
        OptionsSettings.OnUnitSystemChanged += OnUnitSystemChanged;
        Player.player.onLocalPluginWidgetFlagsChanged += OnLocalPluginWidgetFlagsChanged;
        PlayerLife life = Player.player.life;
        life.onDamaged = (Damaged)Delegate.Combine(life.onDamaged, new Damaged(onDamaged));
        Player.player.life.onHealthUpdated = onHealthUpdated;
        Player.player.life.onFoodUpdated = onFoodUpdated;
        Player.player.life.onWaterUpdated = onWaterUpdated;
        Player.player.life.onVirusUpdated = onVirusUpdated;
        Player.player.life.onStaminaUpdated = onStaminaUpdated;
        Player.player.life.onOxygenUpdated = onOxygenUpdated;
        Player.player.life.OnIsAsphyxiatingChanged += OnIsAsphyxiatingChanged;
        Player.player.life.onBleedingUpdated = onBleedingUpdated;
        Player.player.life.onBrokenUpdated = onBrokenUpdated;
        Player.player.life.onTemperatureUpdated = onTemperatureUpdated;
        PlayerLook look = Player.player.look;
        look.onPerspectiveUpdated = (PerspectiveUpdated)Delegate.Combine(look.onPerspectiveUpdated, new PerspectiveUpdated(onPerspectiveUpdated));
        PlayerMovement movement = Player.player.movement;
        movement.onSeated = (Seated)Delegate.Combine(movement.onSeated, new Seated(onSeated));
        PlayerMovement movement2 = Player.player.movement;
        movement2.onVehicleUpdated = (VehicleUpdated)Delegate.Combine(movement2.onVehicleUpdated, new VehicleUpdated(onVehicleUpdated));
        PlayerMovement movement3 = Player.player.movement;
        movement3.onSafetyUpdated = (SafetyUpdated)Delegate.Combine(movement3.onSafetyUpdated, new SafetyUpdated(onSafetyUpdated));
        PlayerMovement movement4 = Player.player.movement;
        movement4.onRadiationUpdated = (RadiationUpdated)Delegate.Combine(movement4.onRadiationUpdated, new RadiationUpdated(onRadiationUpdated));
        PlayerAnimator animator = Player.player.animator;
        animator.onGestureUpdated = (GestureUpdated)Delegate.Combine(animator.onGestureUpdated, new GestureUpdated(onGestureUpdated));
        PlayerEquipment equipment = Player.player.equipment;
        equipment.onHotkeysUpdated = (HotkeysUpdated)Delegate.Combine(equipment.onHotkeysUpdated, new HotkeysUpdated(onHotkeysUpdated));
        Player.player.voice.onTalkingChanged += onTalked;
        Player.player.quests.TrackedQuestUpdated += OnTrackedQuestUpdated;
        PlayerSkills skills = Player.player.skills;
        skills.onExperienceUpdated = (ExperienceUpdated)Delegate.Combine(skills.onExperienceUpdated, new ExperienceUpdated(onExperienceUpdated));
        LightingManager.onMoonUpdated = (MoonUpdated)Delegate.Combine(LightingManager.onMoonUpdated, new MoonUpdated(onMoonUpdated));
        ZombieManager.onWaveUpdated = (WaveUpdated)Delegate.Combine(ZombieManager.onWaveUpdated, new WaveUpdated(onWaveUpdated));
        PlayerClothing clothing = Player.player.clothing;
        clothing.onMaskUpdated = (MaskUpdated)Delegate.Combine(clothing.onMaskUpdated, new MaskUpdated(onMaskUpdated));
        OnChatMessageReceived();
        ChatManager.onChatMessageReceived = (ChatMessageReceivedHandler)Delegate.Combine(ChatManager.onChatMessageReceived, new ChatMessageReceivedHandler(OnChatMessageReceived));
        ChatManager.onVotingStart = (VotingStart)Delegate.Combine(ChatManager.onVotingStart, new VotingStart(onVotingStart));
        ChatManager.onVotingUpdate = (VotingUpdate)Delegate.Combine(ChatManager.onVotingUpdate, new VotingUpdate(onVotingUpdate));
        ChatManager.onVotingStop = (VotingStop)Delegate.Combine(ChatManager.onVotingStop, new VotingStop(onVotingStop));
        ChatManager.onVotingMessage = (VotingMessage)Delegate.Combine(ChatManager.onVotingMessage, new VotingMessage(onVotingMessage));
        LevelManager.onArenaMessageUpdated = (ArenaMessageUpdated)Delegate.Combine(LevelManager.onArenaMessageUpdated, new ArenaMessageUpdated(onArenaMessageUpdated));
        LevelManager.onArenaPlayerUpdated = (ArenaPlayerUpdated)Delegate.Combine(LevelManager.onArenaPlayerUpdated, new ArenaPlayerUpdated(onArenaPlayerUpdated));
        LevelManager.onLevelNumberUpdated = (LevelNumberUpdated)Delegate.Combine(LevelManager.onLevelNumberUpdated, new LevelNumberUpdated(onLevelNumberUpdated));
    }

    private static SleekHitmarker NewHitmarker()
    {
        SleekHitmarker sleekHitmarker = new SleekHitmarker();
        sleekHitmarker.PositionOffset_X = -64f;
        sleekHitmarker.PositionOffset_Y = -64f;
        sleekHitmarker.SizeOffset_X = 128f;
        sleekHitmarker.SizeOffset_Y = 128f;
        PlayerUI.window.AddChild(sleekHitmarker);
        return sleekHitmarker;
    }

    internal static SleekHitmarker ClaimHitmarker()
    {
        if (hitmarkersPool.Count > 0)
        {
            return hitmarkersPool.GetAndRemoveTail();
        }
        return NewHitmarker();
    }

    internal static void ReleaseHitmarker(SleekHitmarker hitmarker)
    {
        hitmarker.IsVisible = false;
        hitmarkersPool.Add(hitmarker);
    }
}
