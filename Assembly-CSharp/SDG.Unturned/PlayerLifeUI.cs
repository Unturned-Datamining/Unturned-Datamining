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

    private static ISleekScrollView chatHistoryBox;

    private static SleekChat[] chatHistoryLabels;

    private static SleekChat[] chatPreviewLabels;

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

    private static int repeatChatIndex;

    private static int cachedHotbarSearch;

    private static int cachedCompassSearch;

    private static bool cachedHasCompass;

    internal static List<HitmarkerInfo> activeHitmarkers;

    private static List<SleekHitmarker> hitmarkersPool;

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
        if (!chatting)
        {
            chatting = true;
            chatField.text = string.Empty;
            chatField.lerpPositionOffset(100, chatField.positionOffset_Y, ESleekLerp.EXPONENTIAL, 20f);
            chatModeButton.state = (int)PlayerUI.chat;
            chatHistoryBox.isVisible = true;
            chatHistoryBox.ScrollToBottom();
            for (int i = 0; i < chatPreviewLabels.Length; i++)
            {
                chatPreviewLabels[i].isVisible = false;
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
        repeatChatIndex = 0;
        if (chatField != null && chatHistoryBox != null && chatPreviewLabels != null)
        {
            chatField.text = string.Empty;
            chatField.lerpPositionOffset(-chatField.sizeOffset_X - 50, chatField.positionOffset_Y, ESleekLerp.EXPONENTIAL, 20f);
            chatHistoryBox.isVisible = false;
            for (int i = 0; i < chatPreviewLabels.Length; i++)
            {
                chatPreviewLabels[i].isVisible = true;
            }
        }
    }

    public static void SendChatAndClose()
    {
        if (!string.IsNullOrEmpty(chatField.text))
        {
            ChatManager.sendChat(PlayerUI.chat, chatField.text);
        }
        closeChat();
    }

    public static void repeatChat(int delta)
    {
        if (chatField != null)
        {
            string recentlySentMessage = ChatManager.getRecentlySentMessage(repeatChatIndex);
            if (!string.IsNullOrEmpty(recentlySentMessage))
            {
                chatField.text = recentlySentMessage;
                repeatChatIndex += delta;
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
                faceButtons[i].isVisible = true;
            }
            bool isVisible = !Player.player.equipment.isSelected && Player.player.stance.stance != EPlayerStance.PRONE && Player.player.stance.stance != EPlayerStance.DRIVING && Player.player.stance.stance != EPlayerStance.SITTING;
            surrenderButton.isVisible = isVisible;
            pointButton.isVisible = isVisible;
            waveButton.isVisible = isVisible;
            saluteButton.isVisible = isVisible;
            restButton.isVisible = isVisible;
            facepalmButton.isVisible = isVisible;
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
                faceButtons[i].isVisible = false;
            }
            surrenderButton.isVisible = false;
            pointButton.isVisible = false;
            waveButton.isVisible = false;
            saluteButton.isVisible = false;
            restButton.isVisible = false;
            facepalmButton.isVisible = false;
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

    private static void updateHotbarItem(ref int offset, ItemJar jar, byte index)
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
            sleekItemIcon.isVisible = itemAsset != null;
            sleekLabel.isVisible = itemAsset != null;
            if (itemAsset != null)
            {
                sleekItemIcon.sizeOffset_X = itemAsset.size_x * 25;
                sleekItemIcon.sizeOffset_Y = itemAsset.size_y * 25;
                sleekItemIcon.Refresh(jar.item.id, jar.item.quality, jar.item.state, itemAsset);
            }
        }
        sleekItemIcon.positionOffset_X = offset;
        sleekLabel.positionOffset_X = offset + sleekItemIcon.sizeOffset_X - 55;
        if (sleekItemIcon.isVisible)
        {
            offset += sleekItemIcon.sizeOffset_X;
            hotbarContainer.sizeOffset_X = offset;
            offset += 5;
            hotbarContainer.sizeOffset_Y = Mathf.Max(hotbarContainer.sizeOffset_Y, sleekItemIcon.sizeOffset_Y);
        }
    }

    public static void updateHotbar()
    {
        if (hotbarContainer == null || Player.player == null)
        {
            return;
        }
        hotbarContainer.isVisible = !PlayerUI.messageBox.isVisible && !PlayerUI.messageBox2.isVisible && OptionsSettings.showHotbar;
        if (!Player.player.inventory.doesSearchNeedRefresh(ref cachedHotbarSearch))
        {
            return;
        }
        int offset = 0;
        updateHotbarItem(ref offset, Player.player.inventory.getItem(0, 0), 0);
        updateHotbarItem(ref offset, Player.player.inventory.getItem(1, 0), 1);
        for (byte b = 0; b < Player.player.equipment.hotkeys.Length; b = (byte)(b + 1))
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
        hotbarContainer.positionOffset_X = hotbarContainer.sizeOffset_X / -2;
        hotbarContainer.positionOffset_Y = -80 - hotbarContainer.sizeOffset_Y;
    }

    public static void updateStatTracker()
    {
        statTrackerLabel.isVisible = Player.player.equipment.getUseableStatTrackerValue(out var type, out var kills);
        if (statTrackerLabel.isVisible)
        {
            statTrackerLabel.textColor = Provider.provider.economyService.getStatTrackerColor(type);
            statTrackerLabel.text = localization.format((type == EStatTrackerType.TOTAL) ? "Stat_Tracker_Total_Kills" : "Stat_Tracker_Player_Kills", kills.ToString("D7"));
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
        element.positionScale_X = num / 2f + 0.5f;
        element.isVisible = Mathf.Abs(num) < 1f;
        alpha = 1f - MathfEx.Square(Mathf.Abs(num));
    }

    protected static bool hasCompassInInventory()
    {
        if (!Player.player.inventory.doesSearchNeedRefresh(ref cachedCompassSearch))
        {
            return cachedHasCompass;
        }
        cachedHasCompass = false;
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
            compassBox.isVisible = true;
        }
        else
        {
            compassBox.isVisible = hasCompassInInventory();
        }
        if (!compassBox.isVisible)
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
            Color color = obj.textColor;
            updateCompassElement(obj, y, elementAngle, out color.a);
            obj.textColor = color;
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
                    sleekImage.positionOffset_X = -10;
                    sleekImage.positionOffset_Y = -5;
                    sleekImage.sizeOffset_X = 20;
                    sleekImage.sizeOffset_Y = 20;
                    compassMarkersContainer.AddChild(sleekImage);
                    compassMarkers.Add(sleekImage);
                }
                num++;
                float num2 = Mathf.Atan2(quests.markerPosition.x - position.x, quests.markerPosition.z - position.z);
                num2 *= 57.29578f;
                Color markerColor = client.markerColor;
                updateCompassElement(sleekImage, y, num2, out markerColor.a);
                sleekImage.color = markerColor;
            }
        }
        for (int num3 = compassMarkersVisibleCount - 1; num3 >= num; num3--)
        {
            compassMarkers[num3].isVisible = false;
        }
        compassMarkersVisibleCount = num;
    }

    private static void updateIcons()
    {
        Player player = Player.player;
        bool flag = player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowStatusIcons);
        int num = 0;
        bleedingBox.isVisible = player.life.isBleeding && flag;
        if (bleedingBox.isVisible)
        {
            num += 60;
        }
        brokenBox.positionOffset_X = num;
        brokenBox.isVisible = player.life.isBroken && flag;
        if (brokenBox.isVisible)
        {
            num += 60;
        }
        temperatureBox.positionOffset_X = num;
        temperatureBox.isVisible = player.life.temperature != EPlayerTemperature.NONE && flag;
        if (temperatureBox.isVisible)
        {
            num += 60;
        }
        starvedBox.positionOffset_X = num;
        starvedBox.isVisible = player.life.food == 0 && flag;
        if (starvedBox.isVisible)
        {
            num += 60;
        }
        dehydratedBox.positionOffset_X = num;
        dehydratedBox.isVisible = player.life.water == 0 && flag;
        if (dehydratedBox.isVisible)
        {
            num += 60;
        }
        infectedBox.positionOffset_X = num;
        infectedBox.isVisible = player.life.virus == 0 && flag;
        if (infectedBox.isVisible)
        {
            num += 60;
        }
        drownedBox.positionOffset_X = num;
        drownedBox.isVisible = player.life.oxygen == 0 && flag;
        if (drownedBox.isVisible)
        {
            num += 60;
        }
        asphyxiatingBox.positionOffset_X = num;
        asphyxiatingBox.isVisible = !drownedBox.isVisible && player.life.isAsphyxiating && flag;
        if (asphyxiatingBox.isVisible)
        {
            num += 60;
        }
        moonBox.positionOffset_X = num;
        moonBox.isVisible = LightingManager.isFullMoon && flag;
        if (moonBox.isVisible)
        {
            num += 60;
        }
        radiationBox.positionOffset_X = num;
        radiationBox.isVisible = player.movement.isRadiated && flag;
        if (radiationBox.isVisible)
        {
            num += 60;
        }
        safeBox.positionOffset_X = num;
        safeBox.isVisible = player.movement.isSafe && flag;
        if (safeBox.isVisible)
        {
            num += 60;
        }
        arrestBox.positionOffset_X = num;
        arrestBox.isVisible = player.animator.gesture == EPlayerGesture.ARREST_START && flag;
        if (arrestBox.isVisible)
        {
            num += 60;
        }
        statusIconsContainer.sizeOffset_X = num - 10;
        statusIconsContainer.isVisible = num > 0;
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
                levelTextBox.isVisible = true;
                levelNumberBox.isVisible = true;
                compassBox.positionOffset_Y = 60;
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
        healthIcon.isVisible = flag;
        healthProgress.isVisible = flag;
        if (flag)
        {
            healthIcon.positionOffset_Y = num;
            healthProgress.positionOffset_Y = num + 5;
            num += 30;
        }
        foodIcon.isVisible = flag2;
        foodProgress.isVisible = flag2;
        if (flag2)
        {
            foodIcon.positionOffset_Y = num;
            foodProgress.positionOffset_Y = num + 5;
            num += 30;
        }
        waterIcon.isVisible = flag3;
        waterProgress.isVisible = flag3;
        if (flag3)
        {
            waterIcon.positionOffset_Y = num;
            waterProgress.positionOffset_Y = num + 5;
            num += 30;
        }
        virusIcon.isVisible = flag4;
        virusProgress.isVisible = flag4;
        if (flag4)
        {
            virusIcon.positionOffset_Y = num;
            virusProgress.positionOffset_Y = num + 5;
            num += 30;
        }
        staminaIcon.isVisible = flag5;
        staminaProgress.isVisible = flag5;
        if (flag5)
        {
            staminaIcon.positionOffset_Y = num;
            staminaProgress.positionOffset_Y = num + 5;
            num += 30;
        }
        waveLabel.isVisible = flag7;
        scoreLabel.isVisible = flag7;
        if (flag7)
        {
            waveLabel.positionOffset_Y = num;
            scoreLabel.positionOffset_Y = num;
            num += 30;
        }
        oxygenIcon.isVisible = flag6;
        oxygenProgress.isVisible = flag6;
        if (flag6)
        {
            oxygenIcon.positionOffset_Y = num;
            oxygenProgress.positionOffset_Y = num + 5;
            num += 30;
        }
        lifeBox.sizeOffset_Y = num - 5;
        lifeBox.positionOffset_Y = -lifeBox.sizeOffset_Y;
        lifeBox.isVisible = lifeBox.sizeOffset_Y > 0;
        statusIconsContainer.positionOffset_Y = lifeBox.positionOffset_Y - 60;
    }

    private static void UpdateVehicleBoxVisibility()
    {
        bool flag = vehicleVisibleByDefault;
        flag &= Player.player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowVehicleStatus);
        vehicleBox.isVisible = flag;
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
        scoreLabel.text = localization.format("Score", newExperience.ToString());
    }

    private static void onWaveUpdated(bool newWaveReady, int newWaveIndex)
    {
        waveLabel.text = localization.format("Round", newWaveIndex);
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
            fuelIcon.isVisible = flag;
            fuelProgress.isVisible = flag;
            if (flag)
            {
                fuelIcon.positionOffset_Y = num;
                fuelProgress.positionOffset_Y = num + 5;
                num += 30;
            }
            speedIcon.positionOffset_Y = num;
            speedProgress.positionOffset_Y = num + 5;
            num += 30;
            hpIcon.isVisible = newVehicle.usesHealth;
            hpProgress.isVisible = newVehicle.usesHealth;
            if (newVehicle.usesHealth)
            {
                hpIcon.positionOffset_Y = num;
                hpProgress.positionOffset_Y = num + 5;
                num += 30;
            }
            batteryChargeIcon.isVisible = newVehicle.usesBattery;
            batteryChargeProgress.isVisible = newVehicle.usesBattery;
            if (newVehicle.usesBattery)
            {
                batteryChargeIcon.positionOffset_Y = num;
                batteryChargeProgress.positionOffset_Y = num + 5;
                num += 30;
            }
            vehicleBox.sizeOffset_Y = num - 5;
            vehicleBox.positionOffset_Y = -vehicleBox.sizeOffset_Y;
            if (newVehicle.passengers[Player.player.movement.getSeat()].turret != null)
            {
                vehicleBox.positionOffset_Y -= 80;
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
                vehicleLockedLabel.text = localization.format(vehicle.isLocked ? "Vehicle_Locked" : "Vehicle_Unlocked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
                vehicleLockedLabel.isVisible = true;
            }
            else
            {
                vehicleLockedLabel.isVisible = false;
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
                gasmaskBox.isVisible = true;
            }
            else
            {
                gasmaskBox.isVisible = false;
            }
        }
        else
        {
            gasmaskBox.isVisible = false;
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
        voiceBox.isVisible = isTalking;
    }

    internal static void UpdateTrackedQuest()
    {
        QuestAsset trackedQuest = Player.player.quests.GetTrackedQuest();
        if (trackedQuest == null)
        {
            trackedQuestTitle.isVisible = false;
            trackedQuestBar.isVisible = false;
            return;
        }
        trackedQuestTitle.text = trackedQuest.questName;
        bool flag = true;
        if (trackedQuest.conditions != null)
        {
            trackedQuestBar.RemoveAllChildren();
            int num = 5;
            for (int i = 0; i < trackedQuest.conditions.Length; i++)
            {
                INPCCondition iNPCCondition = trackedQuest.conditions[i];
                if (iNPCCondition != null && !iNPCCondition.isConditionMet(Player.player))
                {
                    string text = iNPCCondition.formatCondition(Player.player);
                    if (!string.IsNullOrEmpty(text))
                    {
                        ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
                        sleekLabel.positionOffset_X = -300;
                        sleekLabel.positionOffset_Y = num;
                        sleekLabel.sizeOffset_X = 500;
                        sleekLabel.sizeOffset_Y = 30;
                        sleekLabel.enableRichText = true;
                        sleekLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
                        sleekLabel.fontAlignment = TextAnchor.MiddleRight;
                        sleekLabel.text = text;
                        sleekLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
                        trackedQuestBar.AddChild(sleekLabel);
                        num += 20;
                        flag = false;
                    }
                }
            }
        }
        trackedQuestTitle.isVisible = !flag;
        trackedQuestBar.isVisible = trackedQuestTitle.isVisible;
    }

    private static void OnTrackedQuestUpdated(PlayerQuests quests)
    {
        UpdateTrackedQuest();
    }

    private static void onListed()
    {
        int num = 0;
        for (int i = 0; i < ChatManager.receivedChatHistory.Count && i < chatHistoryLabels.Length; i++)
        {
            int index = ChatManager.receivedChatHistory.Count - 1 - i;
            chatHistoryLabels[i].representingChatMessage = ChatManager.receivedChatHistory[index];
            num++;
        }
        int num2 = num * 40;
        int num3 = chatPreviewLabels.Length * 40;
        chatHistoryBox.sizeOffset_Y = Mathf.Min(num2, num3);
        chatHistoryBox.positionOffset_Y = Mathf.Max(0, num3 - chatHistoryBox.sizeOffset_Y);
        chatHistoryBox.contentSizeOffset = new Vector2(0f, num2);
        for (int j = 0; j < chatPreviewLabels.Length; j++)
        {
            int num4 = chatPreviewLabels.Length - 1 - j;
            if (num4 < ChatManager.receivedChatHistory.Count)
            {
                chatPreviewLabels[j].representingChatMessage = ChatManager.receivedChatHistory[num4];
            }
        }
    }

    private static void onVotingStart(SteamPlayer origin, SteamPlayer target, byte votesNeeded)
    {
        isVoteMessaged = false;
        voteBox.text = "";
        voteBox.isVisible = true;
        voteInfoLabel.isVisible = true;
        votesNeededLabel.isVisible = true;
        voteYesLabel.isVisible = true;
        voteNoLabel.isVisible = true;
        voteInfoLabel.text = localization.format("Vote_Kick", origin.playerID.characterName, origin.playerID.playerName, target.playerID.characterName, target.playerID.playerName);
        votesNeededLabel.text = localization.format("Votes_Needed", votesNeeded);
        voteYesLabel.text = localization.format("Vote_Yes", KeyCode.F1, 0);
        voteNoLabel.text = localization.format("Vote_No", KeyCode.F2, 0);
    }

    private static void onVotingUpdate(byte voteYes, byte voteNo)
    {
        voteYesLabel.text = localization.format("Vote_Yes", KeyCode.F1, voteYes);
        voteNoLabel.text = localization.format("Vote_No", KeyCode.F2, voteNo);
    }

    private static void onVotingStop(EVotingMessage message)
    {
        voteInfoLabel.isVisible = false;
        votesNeededLabel.isVisible = false;
        voteYesLabel.isVisible = false;
        voteNoLabel.isVisible = false;
        switch (message)
        {
        case EVotingMessage.PASS:
            voteBox.text = localization.format("Vote_Pass");
            break;
        case EVotingMessage.FAIL:
            voteBox.text = localization.format("Vote_Fail");
            break;
        }
        isVoteMessaged = true;
        lastVoteMessage = Time.realtimeSinceStartup;
    }

    private static void onVotingMessage(EVotingMessage message)
    {
        voteBox.isVisible = true;
        voteInfoLabel.isVisible = false;
        votesNeededLabel.isVisible = false;
        voteYesLabel.isVisible = false;
        voteNoLabel.isVisible = false;
        switch (message)
        {
        case EVotingMessage.OFF:
            voteBox.text = localization.format("Vote_Off");
            break;
        case EVotingMessage.DELAY:
            voteBox.text = localization.format("Vote_Delay");
            break;
        case EVotingMessage.PLAYERS:
            voteBox.text = localization.format("Vote_Players");
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
            levelTextBox.text = localization.format("Arena_Lobby");
            break;
        case EArenaMessage.WARMUP:
            levelTextBox.text = localization.format("Arena_Warm_Up");
            break;
        case EArenaMessage.PLAY:
            levelTextBox.text = localization.format("Arena_Play");
            break;
        case EArenaMessage.LOSE:
            levelTextBox.text = localization.format("Arena_Lose");
            break;
        case EArenaMessage.INTERMISSION:
            levelTextBox.text = localization.format("Arena_Intermission");
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
                levelTextBox.text = localization.format("Arena_Died", text);
                break;
            case EArenaMessage.ABANDONED:
                levelTextBox.text = localization.format("Arena_Abandoned", text);
                break;
            case EArenaMessage.WIN:
                levelTextBox.text = localization.format("Arena_Win", text);
                break;
            }
        }
    }

    private static void onLevelNumberUpdated(int newLevelNumber)
    {
        levelNumberBox.text = newLevelNumber.ToString();
    }

    private static void onClickedFaceButton(ISleekElement button)
    {
        byte b = 0;
        while (b < faceButtons.Length && faceButtons[b] != button)
        {
            b = (byte)(b + 1);
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
        ChatManager.onChatMessageReceived = (ChatMessageReceivedHandler)Delegate.Remove(ChatManager.onChatMessageReceived, new ChatMessageReceivedHandler(onListed));
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
        container.positionOffset_X = 10;
        container.positionOffset_Y = 10;
        container.sizeOffset_X = -20;
        container.sizeOffset_Y = -20;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        PlayerUI.container.AddChild(container);
        active = true;
        chatting = false;
        chatHistoryBox = Glazier.Get().CreateScrollView();
        chatHistoryBox.sizeOffset_X = 630;
        chatHistoryBox.scaleContentToWidth = true;
        container.AddChild(chatHistoryBox);
        chatHistoryBox.isVisible = false;
        chatHistoryLabels = new SleekChat[Provider.preferenceData.Chat.History_Length];
        for (int i = 0; i < chatHistoryLabels.Length; i++)
        {
            SleekChat sleekChat = new SleekChat
            {
                positionOffset_Y = i * 40,
                sizeOffset_X = chatHistoryBox.sizeOffset_X - 30,
                sizeOffset_Y = 40,
                shouldFadeOutWithAge = false
            };
            chatHistoryBox.AddChild(sleekChat);
            chatHistoryLabels[i] = sleekChat;
        }
        bool shouldFadeOutWithAge = Glazier.Get().SupportsRichTextAlpha && Provider.preferenceData.Chat.Enable_Fade_Out;
        chatPreviewLabels = new SleekChat[Provider.preferenceData.Chat.Preview_Length];
        for (int j = 0; j < chatPreviewLabels.Length; j++)
        {
            SleekChat sleekChat2 = new SleekChat
            {
                positionOffset_Y = j * 40,
                sizeOffset_X = chatHistoryBox.sizeOffset_X - 30,
                sizeOffset_Y = 40,
                shouldFadeOutWithAge = shouldFadeOutWithAge
            };
            container.AddChild(sleekChat2);
            chatPreviewLabels[j] = sleekChat2;
        }
        chatField = Glazier.Get().CreateStringField();
        chatField.positionOffset_Y = Provider.preferenceData.Chat.Preview_Length * 40 + 10;
        chatField.sizeOffset_X = chatHistoryBox.sizeOffset_X - 130;
        chatField.positionOffset_X = -chatField.sizeOffset_X - 50;
        chatField.sizeOffset_Y = 30;
        chatField.fontAlignment = TextAnchor.MiddleLeft;
        chatField.maxLength = ChatManager.MAX_MESSAGE_LENGTH;
        chatField.onEscaped += OnChatFieldEscaped;
        container.AddChild(chatField);
        chatModeButton = new SleekButtonState();
        chatModeButton.useContentTooltip = true;
        chatModeButton.setContent(new GUIContent(localization.format("Mode_Global"), localization.format("Mode_Global_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.global))), new GUIContent(localization.format("Mode_Local"), localization.format("Mode_Local_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.local))), new GUIContent(localization.format("Mode_Group"), localization.format("Mode_Group_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.group))));
        chatModeButton.positionOffset_X = -100;
        chatModeButton.sizeOffset_X = 100;
        chatModeButton.sizeOffset_Y = 30;
        chatModeButton.onSwappedState = OnSwappedChatModeState;
        chatField.AddChild(chatModeButton);
        sendChatButton = new SleekButtonIcon(icons.load<Texture2D>("SendChat"));
        sendChatButton.positionScale_X = 1f;
        sendChatButton.sizeOffset_X = 30;
        sendChatButton.sizeOffset_Y = 30;
        sendChatButton.tooltip = localization.format("SendChat_Tooltip", MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Return));
        sendChatButton.iconColor = ESleekTint.FOREGROUND;
        sendChatButton.onClickedButton += OnSendChatButtonClicked;
        chatField.AddChild(sendChatButton);
        voteBox = Glazier.Get().CreateBox();
        voteBox.positionOffset_X = -430;
        voteBox.positionScale_X = 1f;
        voteBox.sizeOffset_X = 430;
        voteBox.sizeOffset_Y = 90;
        container.AddChild(voteBox);
        voteBox.isVisible = false;
        voteInfoLabel = Glazier.Get().CreateLabel();
        voteInfoLabel.sizeOffset_Y = 30;
        voteInfoLabel.sizeScale_X = 1f;
        voteBox.AddChild(voteInfoLabel);
        votesNeededLabel = Glazier.Get().CreateLabel();
        votesNeededLabel.positionOffset_Y = 30;
        votesNeededLabel.sizeOffset_Y = 30;
        votesNeededLabel.sizeScale_X = 1f;
        voteBox.AddChild(votesNeededLabel);
        voteYesLabel = Glazier.Get().CreateLabel();
        voteYesLabel.positionOffset_Y = 60;
        voteYesLabel.sizeOffset_Y = 30;
        voteYesLabel.sizeScale_X = 0.5f;
        voteBox.AddChild(voteYesLabel);
        voteNoLabel = Glazier.Get().CreateLabel();
        voteNoLabel.positionOffset_Y = 60;
        voteNoLabel.positionScale_X = 0.5f;
        voteNoLabel.sizeOffset_Y = 30;
        voteNoLabel.sizeScale_X = 0.5f;
        voteBox.AddChild(voteNoLabel);
        voiceBox = new SleekBoxIcon(icons.load<Texture2D>("Voice"));
        voiceBox.positionOffset_Y = 210;
        voiceBox.sizeOffset_X = 50;
        voiceBox.sizeOffset_Y = 50;
        voiceBox.iconColor = ESleekTint.FOREGROUND;
        container.AddChild(voiceBox);
        voiceBox.isVisible = false;
        trackedQuestTitle = Glazier.Get().CreateLabel();
        trackedQuestTitle.positionOffset_X = -500;
        trackedQuestTitle.positionOffset_Y = 200;
        trackedQuestTitle.positionScale_X = 1f;
        trackedQuestTitle.sizeOffset_X = 500;
        trackedQuestTitle.sizeOffset_Y = 35;
        trackedQuestTitle.enableRichText = true;
        trackedQuestTitle.textColor = ESleekTint.RICH_TEXT_DEFAULT;
        trackedQuestTitle.fontSize = ESleekFontSize.Medium;
        trackedQuestTitle.fontAlignment = TextAnchor.LowerRight;
        trackedQuestTitle.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        container.AddChild(trackedQuestTitle);
        trackedQuestBar = Glazier.Get().CreateImage();
        trackedQuestBar.positionOffset_X = -200;
        trackedQuestBar.positionOffset_Y = 240;
        trackedQuestBar.positionScale_X = 1f;
        trackedQuestBar.sizeOffset_X = 200;
        trackedQuestBar.sizeOffset_Y = 3;
        trackedQuestBar.texture = (Texture2D)GlazierResources.PixelTexture;
        trackedQuestBar.color = ESleekTint.FOREGROUND;
        container.AddChild(trackedQuestBar);
        levelTextBox = Glazier.Get().CreateBox();
        levelTextBox.positionOffset_X = -180;
        levelTextBox.positionScale_X = 0.5f;
        levelTextBox.sizeOffset_X = 300;
        levelTextBox.sizeOffset_Y = 50;
        levelTextBox.fontSize = ESleekFontSize.Medium;
        container.AddChild(levelTextBox);
        levelTextBox.isVisible = false;
        levelNumberBox = Glazier.Get().CreateBox();
        levelNumberBox.positionOffset_X = 130;
        levelNumberBox.positionScale_X = 0.5f;
        levelNumberBox.sizeOffset_X = 50;
        levelNumberBox.sizeOffset_Y = 50;
        levelNumberBox.fontSize = ESleekFontSize.Medium;
        container.AddChild(levelNumberBox);
        levelNumberBox.isVisible = false;
        cachedCompassSearch = -1;
        cachedHasCompass = false;
        compassBox = Glazier.Get().CreateBox();
        compassBox.positionOffset_X = -180;
        compassBox.positionScale_X = 0.5f;
        compassBox.sizeOffset_X = 360;
        compassBox.sizeOffset_Y = 50;
        compassBox.fontSize = ESleekFontSize.Medium;
        container.AddChild(compassBox);
        compassBox.isVisible = false;
        compassLabelsContainer = Glazier.Get().CreateFrame();
        compassLabelsContainer.positionOffset_X = 10;
        compassLabelsContainer.positionOffset_Y = 10;
        compassLabelsContainer.sizeOffset_X = -20;
        compassLabelsContainer.sizeOffset_Y = -20;
        compassLabelsContainer.sizeScale_X = 1f;
        compassLabelsContainer.sizeScale_Y = 1f;
        compassBox.AddChild(compassLabelsContainer);
        compassMarkersContainer = Glazier.Get().CreateFrame();
        compassMarkersContainer.positionOffset_X = 10;
        compassMarkersContainer.positionOffset_Y = 10;
        compassMarkersContainer.sizeOffset_X = -20;
        compassMarkersContainer.sizeOffset_Y = -20;
        compassMarkersContainer.sizeScale_X = 1f;
        compassMarkersContainer.sizeScale_Y = 1f;
        compassBox.AddChild(compassMarkersContainer);
        compassMarkers = new List<ISleekImage>();
        compassMarkersVisibleCount = 0;
        compassLabels = new ISleekLabel[72];
        for (int k = 0; k < compassLabels.Length; k++)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_X = -25;
            sleekLabel.sizeOffset_X = 50;
            sleekLabel.sizeOffset_Y = 30;
            sleekLabel.text = (k * 5).ToString();
            sleekLabel.textColor = new Color(0.75f, 0.75f, 0.75f);
            compassLabelsContainer.AddChild(sleekLabel);
            compassLabels[k] = sleekLabel;
        }
        ISleekLabel compassLabelByAngle = getCompassLabelByAngle(0);
        compassLabelByAngle.fontSize = ESleekFontSize.Large;
        compassLabelByAngle.text = "N";
        compassLabelByAngle.textColor = Palette.COLOR_R;
        ISleekLabel compassLabelByAngle2 = getCompassLabelByAngle(45);
        compassLabelByAngle2.fontSize = ESleekFontSize.Medium;
        compassLabelByAngle2.text = "NE";
        compassLabelByAngle2.textColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle3 = getCompassLabelByAngle(90);
        compassLabelByAngle3.fontSize = ESleekFontSize.Large;
        compassLabelByAngle3.text = "E";
        compassLabelByAngle3.textColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle4 = getCompassLabelByAngle(135);
        compassLabelByAngle4.fontSize = ESleekFontSize.Medium;
        compassLabelByAngle4.text = "SE";
        compassLabelByAngle4.textColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle5 = getCompassLabelByAngle(180);
        compassLabelByAngle5.fontSize = ESleekFontSize.Large;
        compassLabelByAngle5.text = "S";
        compassLabelByAngle5.textColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle6 = getCompassLabelByAngle(225);
        compassLabelByAngle6.fontSize = ESleekFontSize.Medium;
        compassLabelByAngle6.text = "SW";
        compassLabelByAngle6.textColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle7 = getCompassLabelByAngle(270);
        compassLabelByAngle7.fontSize = ESleekFontSize.Large;
        compassLabelByAngle7.text = "W";
        compassLabelByAngle7.textColor = new Color(1f, 1f, 1f);
        ISleekLabel compassLabelByAngle8 = getCompassLabelByAngle(315);
        compassLabelByAngle8.fontSize = ESleekFontSize.Medium;
        compassLabelByAngle8.text = "NW";
        compassLabelByAngle8.textColor = new Color(1f, 1f, 1f);
        hotbarContainer = Glazier.Get().CreateFrame();
        hotbarContainer.positionScale_X = 0.5f;
        hotbarContainer.positionScale_Y = 1f;
        hotbarContainer.positionOffset_Y = -200;
        container.AddChild(hotbarContainer);
        hotbarContainer.isVisible = false;
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
            sleekItemIcon.isVisible = false;
            hotbarImages[l] = sleekItemIcon;
        }
        hotbarLabels = new ISleekLabel[cachedHotbarValues.Length];
        for (int m = 0; m < hotbarLabels.Length; m++)
        {
            ISleekLabel sleekLabel2 = Glazier.Get().CreateLabel();
            sleekLabel2.positionOffset_Y = 5;
            sleekLabel2.sizeOffset_X = 50;
            sleekLabel2.sizeOffset_Y = 30;
            sleekLabel2.text = ControlsSettings.getEquipmentHotkeyText(m);
            sleekLabel2.fontAlignment = TextAnchor.UpperRight;
            sleekLabel2.textColor = new SleekColor(ESleekTint.FONT, 0.75f);
            sleekLabel2.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            hotbarContainer.AddChild(sleekLabel2);
            sleekLabel2.isVisible = false;
            hotbarLabels[m] = sleekLabel2;
        }
        statTrackerLabel = Glazier.Get().CreateLabel();
        statTrackerLabel.positionOffset_X = -100;
        statTrackerLabel.positionOffset_Y = -30;
        statTrackerLabel.positionScale_X = 0.5f;
        statTrackerLabel.positionScale_Y = 1f;
        statTrackerLabel.sizeOffset_X = 200;
        statTrackerLabel.sizeOffset_Y = 30;
        statTrackerLabel.fontAlignment = TextAnchor.LowerCenter;
        statTrackerLabel.fontStyle = FontStyle.Italic;
        statTrackerLabel.fontSize = ESleekFontSize.Default;
        container.AddChild(statTrackerLabel);
        statTrackerLabel.isVisible = false;
        scopeOverlay = new SleekScopeOverlay();
        scopeOverlay.sizeScale_X = 1f;
        scopeOverlay.sizeScale_Y = 1f;
        scopeOverlay.isVisible = false;
        PlayerUI.window.AddChild(scopeOverlay);
        binocularsOverlay = Glazier.Get().CreateImage((Texture2D)Resources.Load("Overlay/Binoculars"));
        binocularsOverlay.sizeScale_X = 1f;
        binocularsOverlay.sizeScale_Y = 1f;
        PlayerUI.window.AddChild(binocularsOverlay);
        binocularsOverlay.isVisible = false;
        faceButtons = new ISleekButton[Customization.FACES_FREE + Customization.FACES_PRO];
        for (int n = 0; n < faceButtons.Length; n++)
        {
            float num = (float)Math.PI * 4f * ((float)n / (float)faceButtons.Length);
            float num2 = 210f;
            if (n >= faceButtons.Length / 2)
            {
                num += (float)Math.PI / (float)(faceButtons.Length / 2);
                num2 += 30f;
            }
            ISleekButton sleekButton = Glazier.Get().CreateButton();
            sleekButton.positionOffset_X = (int)(Mathf.Cos(num) * num2) - 20;
            sleekButton.positionOffset_Y = (int)(Mathf.Sin(num) * num2) - 20;
            sleekButton.positionScale_X = 0.5f;
            sleekButton.positionScale_Y = 0.5f;
            sleekButton.sizeOffset_X = 40;
            sleekButton.sizeOffset_Y = 40;
            container.AddChild(sleekButton);
            sleekButton.isVisible = false;
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.positionOffset_X = 10;
            sleekImage.positionOffset_Y = 10;
            sleekImage.sizeOffset_X = 20;
            sleekImage.sizeOffset_Y = 20;
            sleekImage.texture = (Texture2D)GlazierResources.PixelTexture;
            sleekImage.color = Characters.active.skin;
            sleekButton.AddChild(sleekImage);
            ISleekImage sleekImage2 = Glazier.Get().CreateImage();
            sleekImage2.positionOffset_X = 2;
            sleekImage2.positionOffset_Y = 2;
            sleekImage2.sizeOffset_X = 16;
            sleekImage2.sizeOffset_Y = 16;
            sleekImage2.texture = (Texture2D)Resources.Load("Faces/" + n + "/Texture");
            sleekImage.AddChild(sleekImage2);
            if (n >= Customization.FACES_FREE)
            {
                if (Provider.isPro)
                {
                    sleekButton.onClickedButton += onClickedFaceButton;
                }
                else
                {
                    sleekButton.backgroundColor = SleekColor.BackgroundIfLight(Palette.PRO);
                    Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
                    ISleekImage sleekImage3 = Glazier.Get().CreateImage();
                    sleekImage3.positionOffset_X = -10;
                    sleekImage3.positionOffset_Y = -10;
                    sleekImage3.positionScale_X = 0.5f;
                    sleekImage3.positionScale_Y = 0.5f;
                    sleekImage3.sizeOffset_X = 20;
                    sleekImage3.sizeOffset_Y = 20;
                    sleekImage3.texture = bundle.load<Texture2D>("Lock_Small");
                    sleekButton.AddChild(sleekImage3);
                    bundle.unload();
                }
            }
            else
            {
                sleekButton.onClickedButton += onClickedFaceButton;
            }
            faceButtons[n] = sleekButton;
        }
        surrenderButton = Glazier.Get().CreateButton();
        surrenderButton.positionOffset_X = -160;
        surrenderButton.positionOffset_Y = -15;
        surrenderButton.positionScale_X = 0.5f;
        surrenderButton.positionScale_Y = 0.5f;
        surrenderButton.sizeOffset_X = 150;
        surrenderButton.sizeOffset_Y = 30;
        surrenderButton.text = localization.format("Surrender");
        surrenderButton.onClickedButton += onClickedSurrenderButton;
        container.AddChild(surrenderButton);
        surrenderButton.isVisible = false;
        pointButton = Glazier.Get().CreateButton();
        pointButton.positionOffset_X = 10;
        pointButton.positionOffset_Y = -15;
        pointButton.positionScale_X = 0.5f;
        pointButton.positionScale_Y = 0.5f;
        pointButton.sizeOffset_X = 150;
        pointButton.sizeOffset_Y = 30;
        pointButton.text = localization.format("Point");
        pointButton.onClickedButton += onClickedPointButton;
        container.AddChild(pointButton);
        pointButton.isVisible = false;
        waveButton = Glazier.Get().CreateButton();
        waveButton.positionOffset_X = -75;
        waveButton.positionOffset_Y = -55;
        waveButton.positionScale_X = 0.5f;
        waveButton.positionScale_Y = 0.5f;
        waveButton.sizeOffset_X = 150;
        waveButton.sizeOffset_Y = 30;
        waveButton.text = localization.format("Wave");
        waveButton.onClickedButton += onClickedWaveButton;
        container.AddChild(waveButton);
        waveButton.isVisible = false;
        saluteButton = Glazier.Get().CreateButton();
        saluteButton.positionOffset_X = -75;
        saluteButton.positionOffset_Y = 25;
        saluteButton.positionScale_X = 0.5f;
        saluteButton.positionScale_Y = 0.5f;
        saluteButton.sizeOffset_X = 150;
        saluteButton.sizeOffset_Y = 30;
        saluteButton.text = localization.format("Salute");
        saluteButton.onClickedButton += onClickedSaluteButton;
        container.AddChild(saluteButton);
        saluteButton.isVisible = false;
        restButton = Glazier.Get().CreateButton();
        restButton.positionOffset_X = -160;
        restButton.positionOffset_Y = 65;
        restButton.positionScale_X = 0.5f;
        restButton.positionScale_Y = 0.5f;
        restButton.sizeOffset_X = 150;
        restButton.sizeOffset_Y = 30;
        restButton.text = localization.format("Rest");
        restButton.onClickedButton += onClickedRestButton;
        container.AddChild(restButton);
        restButton.isVisible = false;
        facepalmButton = Glazier.Get().CreateButton();
        facepalmButton.positionOffset_X = 10;
        facepalmButton.positionOffset_Y = -95;
        facepalmButton.positionScale_X = 0.5f;
        facepalmButton.positionScale_Y = 0.5f;
        facepalmButton.sizeOffset_X = 150;
        facepalmButton.sizeOffset_Y = 30;
        facepalmButton.text = localization.format("Facepalm");
        facepalmButton.onClickedButton += onClickedFacepalmButton;
        container.AddChild(facepalmButton);
        facepalmButton.isVisible = false;
        activeHitmarkers = new List<HitmarkerInfo>(16);
        hitmarkersPool = new List<SleekHitmarker>(16);
        for (int num3 = 0; num3 < 16; num3++)
        {
            ReleaseHitmarker(NewHitmarker());
        }
        crosshair = new Crosshair(icons);
        crosshair.sizeScale_X = 1f;
        crosshair.sizeScale_Y = 1f;
        container.AddChild(crosshair);
        crosshair.SetPluginAllowsCenterDotVisible(Player.player.isPluginWidgetFlagActive(EPluginWidgetFlags.ShowCenterDot));
        lifeBox = Glazier.Get().CreateBox();
        lifeBox.positionScale_Y = 1f;
        lifeBox.sizeScale_X = 0.2f;
        container.AddChild(lifeBox);
        statusIconsContainer = Glazier.Get().CreateFrame();
        statusIconsContainer.positionOffset_Y = -60;
        statusIconsContainer.positionScale_Y = 1f;
        statusIconsContainer.sizeScale_X = 0.2f;
        statusIconsContainer.sizeOffset_Y = 50;
        container.AddChild(statusIconsContainer);
        healthIcon = Glazier.Get().CreateImage();
        healthIcon.positionOffset_X = 5;
        healthIcon.sizeOffset_X = 20;
        healthIcon.sizeOffset_Y = 20;
        healthIcon.texture = icons.load<Texture2D>("Health");
        lifeBox.AddChild(healthIcon);
        healthProgress = new SleekProgress("");
        healthProgress.positionOffset_X = 30;
        healthProgress.sizeOffset_X = -40;
        healthProgress.sizeOffset_Y = 10;
        healthProgress.sizeScale_X = 1f;
        healthProgress.color = Palette.COLOR_R;
        lifeBox.AddChild(healthProgress);
        foodIcon = Glazier.Get().CreateImage();
        foodIcon.positionOffset_X = 5;
        foodIcon.sizeOffset_X = 20;
        foodIcon.sizeOffset_Y = 20;
        foodIcon.texture = icons.load<Texture2D>("Food");
        lifeBox.AddChild(foodIcon);
        foodProgress = new SleekProgress("");
        foodProgress.positionOffset_X = 30;
        foodProgress.sizeOffset_X = -40;
        foodProgress.sizeOffset_Y = 10;
        foodProgress.sizeScale_X = 1f;
        foodProgress.color = Palette.COLOR_O;
        lifeBox.AddChild(foodProgress);
        waterIcon = Glazier.Get().CreateImage();
        waterIcon.positionOffset_X = 5;
        waterIcon.sizeOffset_X = 20;
        waterIcon.sizeOffset_Y = 20;
        waterIcon.texture = icons.load<Texture2D>("Water");
        lifeBox.AddChild(waterIcon);
        waterProgress = new SleekProgress("");
        waterProgress.positionOffset_X = 30;
        waterProgress.sizeOffset_X = -40;
        waterProgress.sizeOffset_Y = 10;
        waterProgress.sizeScale_X = 1f;
        waterProgress.color = Palette.COLOR_B;
        lifeBox.AddChild(waterProgress);
        virusIcon = Glazier.Get().CreateImage();
        virusIcon.positionOffset_X = 5;
        virusIcon.sizeOffset_X = 20;
        virusIcon.sizeOffset_Y = 20;
        virusIcon.texture = icons.load<Texture2D>("Virus");
        lifeBox.AddChild(virusIcon);
        virusProgress = new SleekProgress("");
        virusProgress.positionOffset_X = 30;
        virusProgress.sizeOffset_X = -40;
        virusProgress.sizeOffset_Y = 10;
        virusProgress.sizeScale_X = 1f;
        virusProgress.color = Palette.COLOR_G;
        lifeBox.AddChild(virusProgress);
        staminaIcon = Glazier.Get().CreateImage();
        staminaIcon.positionOffset_X = 5;
        staminaIcon.sizeOffset_X = 20;
        staminaIcon.sizeOffset_Y = 20;
        staminaIcon.texture = icons.load<Texture2D>("Stamina");
        lifeBox.AddChild(staminaIcon);
        staminaProgress = new SleekProgress("");
        staminaProgress.positionOffset_X = 30;
        staminaProgress.sizeOffset_X = -40;
        staminaProgress.sizeOffset_Y = 10;
        staminaProgress.sizeScale_X = 1f;
        staminaProgress.color = Palette.COLOR_Y;
        lifeBox.AddChild(staminaProgress);
        waveLabel = Glazier.Get().CreateLabel();
        waveLabel.sizeOffset_Y = 30;
        waveLabel.sizeScale_X = 0.5f;
        lifeBox.AddChild(waveLabel);
        scoreLabel = Glazier.Get().CreateLabel();
        scoreLabel.positionScale_X = 0.5f;
        scoreLabel.sizeOffset_Y = 30;
        scoreLabel.sizeScale_X = 0.5f;
        lifeBox.AddChild(scoreLabel);
        oxygenIcon = Glazier.Get().CreateImage();
        oxygenIcon.positionOffset_X = 5;
        oxygenIcon.sizeOffset_X = 20;
        oxygenIcon.sizeOffset_Y = 20;
        oxygenIcon.texture = icons.load<Texture2D>("Oxygen");
        lifeBox.AddChild(oxygenIcon);
        oxygenProgress = new SleekProgress("");
        oxygenProgress.positionOffset_X = 30;
        oxygenProgress.sizeOffset_X = -40;
        oxygenProgress.sizeOffset_Y = 10;
        oxygenProgress.sizeScale_X = 1f;
        oxygenProgress.color = Palette.COLOR_W;
        lifeBox.AddChild(oxygenProgress);
        vehicleBox = Glazier.Get().CreateBox();
        vehicleBox.positionOffset_Y = -120;
        vehicleBox.positionScale_X = 0.8f;
        vehicleBox.positionScale_Y = 1f;
        vehicleBox.sizeOffset_Y = 120;
        vehicleBox.sizeScale_X = 0.2f;
        container.AddChild(vehicleBox);
        vehicleVisibleByDefault = false;
        fuelIcon = Glazier.Get().CreateImage();
        fuelIcon.positionOffset_X = 5;
        fuelIcon.positionOffset_Y = 5;
        fuelIcon.sizeOffset_X = 20;
        fuelIcon.sizeOffset_Y = 20;
        fuelIcon.texture = icons.load<Texture2D>("Fuel");
        vehicleBox.AddChild(fuelIcon);
        fuelProgress = new SleekProgress("");
        fuelProgress.positionOffset_X = 30;
        fuelProgress.positionOffset_Y = 10;
        fuelProgress.sizeOffset_X = -40;
        fuelProgress.sizeOffset_Y = 10;
        fuelProgress.sizeScale_X = 1f;
        fuelProgress.color = Palette.COLOR_Y;
        vehicleBox.AddChild(fuelProgress);
        speedIcon = Glazier.Get().CreateImage();
        speedIcon.positionOffset_X = 5;
        speedIcon.positionOffset_Y = 35;
        speedIcon.sizeOffset_X = 20;
        speedIcon.sizeOffset_Y = 20;
        speedIcon.texture = icons.load<Texture2D>("Speed");
        vehicleBox.AddChild(speedIcon);
        speedProgress = new SleekProgress(OptionsSettings.metric ? " kph" : " mph");
        speedProgress.positionOffset_X = 30;
        speedProgress.positionOffset_Y = 40;
        speedProgress.sizeOffset_X = -40;
        speedProgress.sizeOffset_Y = 10;
        speedProgress.sizeScale_X = 1f;
        speedProgress.color = Palette.COLOR_P;
        vehicleBox.AddChild(speedProgress);
        hpIcon = Glazier.Get().CreateImage();
        hpIcon.positionOffset_X = 5;
        hpIcon.positionOffset_Y = 65;
        hpIcon.sizeOffset_X = 20;
        hpIcon.sizeOffset_Y = 20;
        hpIcon.texture = icons.load<Texture2D>("Health");
        vehicleBox.AddChild(hpIcon);
        hpProgress = new SleekProgress("");
        hpProgress.positionOffset_X = 30;
        hpProgress.positionOffset_Y = 70;
        hpProgress.sizeOffset_X = -40;
        hpProgress.sizeOffset_Y = 10;
        hpProgress.sizeScale_X = 1f;
        hpProgress.color = Palette.COLOR_R;
        vehicleBox.AddChild(hpProgress);
        batteryChargeIcon = Glazier.Get().CreateImage();
        batteryChargeIcon.positionOffset_X = 5;
        batteryChargeIcon.positionOffset_Y = 95;
        batteryChargeIcon.sizeOffset_X = 20;
        batteryChargeIcon.sizeOffset_Y = 20;
        batteryChargeIcon.texture = icons.load<Texture2D>("Stamina");
        vehicleBox.AddChild(batteryChargeIcon);
        batteryChargeProgress = new SleekProgress("");
        batteryChargeProgress.positionOffset_X = 30;
        batteryChargeProgress.positionOffset_Y = 100;
        batteryChargeProgress.sizeOffset_X = -40;
        batteryChargeProgress.sizeOffset_Y = 10;
        batteryChargeProgress.sizeScale_X = 1f;
        batteryChargeProgress.color = Palette.COLOR_Y;
        vehicleBox.AddChild(batteryChargeProgress);
        vehicleLockedLabel = Glazier.Get().CreateLabel();
        vehicleLockedLabel.positionOffset_Y = -25;
        vehicleLockedLabel.sizeScale_X = 1f;
        vehicleLockedLabel.sizeOffset_Y = 30;
        vehicleLockedLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
        vehicleBox.AddChild(vehicleLockedLabel);
        gasmaskBox = Glazier.Get().CreateBox();
        gasmaskBox.positionOffset_X = -200;
        gasmaskBox.positionOffset_Y = -60;
        gasmaskBox.positionScale_X = 0.5f;
        gasmaskBox.positionScale_Y = 1f;
        gasmaskBox.sizeOffset_X = 400;
        gasmaskBox.sizeOffset_Y = 60;
        container.AddChild(gasmaskBox);
        gasmaskBox.isVisible = false;
        gasmaskIcon = new SleekItemIcon();
        gasmaskIcon.positionOffset_X = 5;
        gasmaskIcon.positionOffset_Y = 5;
        gasmaskIcon.sizeOffset_X = 50;
        gasmaskIcon.sizeOffset_Y = 50;
        gasmaskBox.AddChild(gasmaskIcon);
        gasmaskProgress = new SleekProgress("");
        gasmaskProgress.positionOffset_X = 60;
        gasmaskProgress.positionOffset_Y = 10;
        gasmaskProgress.sizeOffset_X = -70;
        gasmaskProgress.sizeOffset_Y = 40;
        gasmaskProgress.sizeScale_X = 1f;
        gasmaskBox.AddChild(gasmaskProgress);
        bleedingBox = new SleekBoxIcon(icons.load<Texture2D>("Bleeding"));
        bleedingBox.sizeOffset_X = 50;
        bleedingBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(bleedingBox);
        bleedingBox.isVisible = false;
        brokenBox = new SleekBoxIcon(icons.load<Texture2D>("Broken"));
        brokenBox.sizeOffset_X = 50;
        brokenBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(brokenBox);
        brokenBox.isVisible = false;
        temperatureBox = new SleekBoxIcon(null);
        temperatureBox.sizeOffset_X = 50;
        temperatureBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(temperatureBox);
        temperatureBox.isVisible = false;
        starvedBox = new SleekBoxIcon(icons.load<Texture2D>("Starved"));
        starvedBox.sizeOffset_X = 50;
        starvedBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(starvedBox);
        starvedBox.isVisible = false;
        dehydratedBox = new SleekBoxIcon(icons.load<Texture2D>("Dehydrated"));
        dehydratedBox.sizeOffset_X = 50;
        dehydratedBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(dehydratedBox);
        dehydratedBox.isVisible = false;
        infectedBox = new SleekBoxIcon(icons.load<Texture2D>("Infected"));
        infectedBox.sizeOffset_X = 50;
        infectedBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(infectedBox);
        infectedBox.isVisible = false;
        drownedBox = new SleekBoxIcon(icons.load<Texture2D>("Drowned"));
        drownedBox.sizeOffset_X = 50;
        drownedBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(drownedBox);
        drownedBox.isVisible = false;
        asphyxiatingBox = new SleekBoxIcon(icons.load<Texture2D>("AsphyxiatingStatus"));
        asphyxiatingBox.sizeOffset_X = 50;
        asphyxiatingBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(asphyxiatingBox);
        asphyxiatingBox.isVisible = false;
        moonBox = new SleekBoxIcon(icons.load<Texture2D>("Moon"));
        moonBox.sizeOffset_X = 50;
        moonBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(moonBox);
        moonBox.isVisible = false;
        radiationBox = new SleekBoxIcon(icons.load<Texture2D>("Deadzone"));
        radiationBox.sizeOffset_X = 50;
        radiationBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(radiationBox);
        radiationBox.isVisible = false;
        safeBox = new SleekBoxIcon(icons.load<Texture2D>("Safe"));
        safeBox.sizeOffset_X = 50;
        safeBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(safeBox);
        safeBox.isVisible = false;
        arrestBox = new SleekBoxIcon(icons.load<Texture2D>("Arrest"));
        arrestBox.sizeOffset_X = 50;
        arrestBox.sizeOffset_Y = 50;
        statusIconsContainer.AddChild(arrestBox);
        arrestBox.isVisible = false;
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
        onListed();
        ChatManager.onChatMessageReceived = (ChatMessageReceivedHandler)Delegate.Combine(ChatManager.onChatMessageReceived, new ChatMessageReceivedHandler(onListed));
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
        sleekHitmarker.positionOffset_X = -64;
        sleekHitmarker.positionOffset_Y = -64;
        sleekHitmarker.sizeOffset_X = 128;
        sleekHitmarker.sizeOffset_Y = 128;
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
        hitmarker.isVisible = false;
        hitmarkersPool.Add(hitmarker);
    }
}
