using System;
using System.Collections.Generic;
using SDG.Framework.Water;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerUI : MonoBehaviour
{
    public static readonly float HIT_TIME = 0.33f;

    public static SleekWindow window;

    public static ISleekElement container;

    private static ISleekImage colorOverlayImage;

    private static SleekPlayer messagePlayer;

    public static ISleekBox messageBox;

    private static ISleekLabel messageLabel;

    private static SleekProgress messageProgress_0;

    private static SleekProgress messageProgress_1;

    private static SleekProgress messageProgress_2;

    private static ISleekImage messageIcon_0;

    private static ISleekImage messageIcon_1;

    private static ISleekImage messageIcon_2;

    private static ISleekImage messageQualityImage;

    private static ISleekLabel messageAmountLabel;

    public static ISleekBox messageBox2;

    private static ISleekLabel messageLabel2;

    private static SleekProgress messageProgress2_0;

    private static SleekProgress messageProgress2_1;

    private static ISleekImage messageIcon2;

    private static float painAlpha;

    private static Color stunColor;

    private static float stunAlpha;

    private static bool _isBlindfolded;

    private static bool inputWantsCustomModal;

    private static bool usingCustomModal;

    public static bool isLocked;

    private BlurEffect menuBlurFX;

    private AudioReverbZone hallucinationReverbZone;

    private static float hallucinationTimer;

    private static float messageDisappearTime;

    private static bool isMessaged;

    private static bool lastHinted;

    private static bool isHinted;

    private static bool lastHinted2;

    private static bool isHinted2;

    private static bool wantsWindowEnabled;

    private static bool isWindowEnabledByColorOverlay;

    public static EChatMode chat;

    private static StaticResourceRef<AudioClip> hitCriticalSound = new StaticResourceRef<AudioClip>("Sounds/General/Hit");

    internal static PlayerUI instance;

    internal PlayerGroupUI groupUI;

    private PlayerDashboardUI dashboardUI;

    private PlayerPauseUI pauseUI;

    private PlayerLifeUI lifeUI;

    internal PlayerBarricadeStereoUI boomboxUI;

    internal PlayerBarricadeMannequinUI mannequinUI;

    internal PlayerBrowserRequestUI browserRequestUI;

    public static bool isBlindfolded
    {
        get
        {
            return _isBlindfolded;
        }
        set
        {
            if (isBlindfolded != value)
            {
                _isBlindfolded = value;
                PlayerUI.isBlindfoldedChanged();
                UpdateWindowEnabled();
            }
        }
    }

    /// <summary>
    /// Many places checked that the cursor and chat were closed to see if a menu could be opened. Moved here to
    /// also consider that useable might have a menu open.
    /// </summary>
    private bool canOpenMenus
    {
        get
        {
            if (Player.player != null && Player.player.equipment.isUseableShowingMenu)
            {
                return false;
            }
            if (!window.showCursor)
            {
                return !PlayerLifeUI.chatting;
            }
            return false;
        }
    }

    public static event IsBlindfoldedChangedHandler isBlindfoldedChanged;

    public static void stun(Color color, float amount)
    {
        stunColor = color;
        stunAlpha = amount * 5f;
        MainCamera.instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Sounds/General/Stun"), amount);
        if (!isWindowEnabledByColorOverlay)
        {
            isWindowEnabledByColorOverlay = true;
            UpdateWindowEnabled();
        }
    }

    public static void pain(float amount)
    {
        painAlpha = amount * 0.75f;
        if (!isWindowEnabledByColorOverlay)
        {
            isWindowEnabledByColorOverlay = true;
            UpdateWindowEnabled();
        }
    }

    public static void hitmark(Vector3 point, bool worldspace, EPlayerHit newHit)
    {
        if (wantsWindowEnabled && Provider.modeConfigData.Gameplay.Hitmarkers)
        {
            HitmarkerInfo item = default(HitmarkerInfo);
            item.worldPosition = point;
            item.shouldFollowWorldPosition = worldspace || OptionsSettings.ShouldHitmarkersFollowWorldPosition;
            item.sleekElement = PlayerLifeUI.ClaimHitmarker();
            item.sleekElement.SetStyle(newHit);
            if (OptionsSettings.hitmarkerStyle == EHitmarkerStyle.Animated)
            {
                item.sleekElement.PlayAnimation();
            }
            else
            {
                item.sleekElement.ApplyClassicPositions();
            }
            PlayerLifeUI.activeHitmarkers.Add(item);
            if (newHit == EPlayerHit.CRITICAL)
            {
                MainCamera.instance.GetComponent<AudioSource>().PlayOneShot(hitCriticalSound, 0.5f);
            }
        }
    }

    public static void enableDot()
    {
        PlayerLifeUI.crosshair.SetGameWantsCenterDotVisible(isVisible: true);
    }

    public static void disableDot()
    {
        PlayerLifeUI.crosshair.SetGameWantsCenterDotVisible(isVisible: false);
    }

    public static void updateScope(bool isScoped)
    {
        PlayerLifeUI.scopeOverlay.IsVisible = isScoped;
        container.IsVisible = !isScoped;
        UpdateWindowEnabled();
    }

    public static void updateBinoculars(bool isBinoculars)
    {
        PlayerLifeUI.binocularsOverlay.IsVisible = isBinoculars;
        container.IsVisible = !isBinoculars;
        UpdateWindowEnabled();
    }

    private static void UpdateWindowEnabled()
    {
        window.isEnabled = wantsWindowEnabled || PlayerLifeUI.scopeOverlay.IsVisible || PlayerLifeUI.binocularsOverlay.IsVisible || isBlindfolded || isWindowEnabledByColorOverlay;
    }

    public static void enableCrosshair()
    {
        if (Provider.modeConfigData.Gameplay.Crosshair)
        {
            PlayerLifeUI.crosshair.SetDirectionalArrowsVisible(isVisible: true);
        }
    }

    public static void disableCrosshair()
    {
        if (Provider.modeConfigData.Gameplay.Crosshair)
        {
            PlayerLifeUI.crosshair.SetDirectionalArrowsVisible(isVisible: false);
        }
    }

    /// <summary>
    /// Hints/messages are the pop-up texts below the interaction prompt, e.g. "reload" or "full moon rises". 
    /// Got a complaint that the item placement obstructed hint was shown if placing multiple signs.
    /// </summary>
    private static bool ShouldIgnoreHintAndMessageRequests()
    {
        if (!PlayerBarricadeSignUI.active)
        {
            if (instance.boomboxUI != null)
            {
                return instance.boomboxUI.active;
            }
            return false;
        }
        return true;
    }

    public static void hint(Transform transform, EPlayerMessage message)
    {
        hint(transform, message, "", Color.white);
    }

    public static void hint(Transform transform, EPlayerMessage message, string text, Color color, params object[] objects)
    {
        if (messageBox == null || PlayerLifeUI.localization == null || ShouldIgnoreHintAndMessageRequests())
        {
            return;
        }
        lastHinted = true;
        isHinted = true;
        if (message == EPlayerMessage.ENEMY)
        {
            if (objects.Length == 1)
            {
                SteamPlayer steamPlayer = (SteamPlayer)objects[0];
                if (messagePlayer != null && messagePlayer.player != steamPlayer)
                {
                    container.RemoveChild(messagePlayer);
                    messagePlayer = null;
                }
                if (messagePlayer == null)
                {
                    messagePlayer = new SleekPlayer(steamPlayer, isButton: false, SleekPlayer.ESleekPlayerDisplayContext.NONE);
                    messagePlayer.PositionOffset_X = -150f;
                    messagePlayer.PositionOffset_Y = -130f;
                    messagePlayer.PositionScale_X = 0.5f;
                    messagePlayer.PositionScale_Y = 1f;
                    messagePlayer.SizeOffset_X = 300f;
                    messagePlayer.SizeOffset_Y = 50f;
                    container.AddChild(messagePlayer);
                }
            }
            messageBox.IsVisible = false;
            if (messagePlayer != null)
            {
                messagePlayer.IsVisible = true;
            }
            return;
        }
        messageBox.IsVisible = true;
        if (messagePlayer != null)
        {
            messagePlayer.IsVisible = false;
        }
        messageIcon_0.PositionOffset_Y = 45f;
        messageProgress_0.PositionOffset_Y = 50f;
        messageIcon_1.PositionOffset_Y = 75f;
        messageProgress_1.PositionOffset_Y = 80f;
        messageIcon_2.PositionOffset_Y = 105f;
        messageProgress_2.PositionOffset_Y = 110f;
        switch (message)
        {
        case EPlayerMessage.VEHICLE_ENTER:
        {
            InteractableVehicle interactableVehicle = (InteractableVehicle)PlayerInteract.interactable;
            int num2 = 45;
            bool flag = interactableVehicle.usesFuel || interactableVehicle.asset.isStaminaPowered;
            messageIcon_0.IsVisible = flag;
            messageProgress_0.IsVisible = flag;
            if (flag)
            {
                messageIcon_0.PositionOffset_Y = num2;
                messageProgress_0.PositionOffset_Y = num2 + 5;
                num2 += 30;
            }
            messageIcon_1.IsVisible = interactableVehicle.usesHealth;
            messageProgress_1.IsVisible = interactableVehicle.usesHealth;
            if (interactableVehicle.usesHealth)
            {
                messageIcon_1.PositionOffset_Y = num2;
                messageProgress_1.PositionOffset_Y = num2 + 5;
                num2 += 30;
            }
            messageIcon_2.IsVisible = interactableVehicle.usesBattery;
            messageProgress_2.IsVisible = interactableVehicle.usesBattery;
            if (interactableVehicle.usesBattery)
            {
                messageIcon_2.PositionOffset_Y = num2;
                messageProgress_2.PositionOffset_Y = num2 + 5;
                num2 += 30;
            }
            messageBox.SizeOffset_Y = num2 - 5;
            if (flag)
            {
                interactableVehicle.getDisplayFuel(out var currentFuel, out var maxFuel);
                messageProgress_0.state = (float)(int)currentFuel / (float)(int)maxFuel;
                messageProgress_0.color = Palette.COLOR_Y;
                messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
            }
            if (interactableVehicle.usesHealth)
            {
                messageProgress_1.state = (float)(int)interactableVehicle.health / (float)(int)interactableVehicle.asset.health;
                messageProgress_1.color = Palette.COLOR_R;
                messageIcon_1.Texture = PlayerLifeUI.icons.load<Texture2D>("Health");
            }
            if (interactableVehicle.usesBattery)
            {
                messageProgress_2.state = (float)(int)interactableVehicle.batteryCharge / 10000f;
                messageProgress_2.color = Palette.COLOR_Y;
                messageIcon_2.Texture = PlayerLifeUI.icons.load<Texture2D>("Stamina");
            }
            messageQualityImage.IsVisible = false;
            messageAmountLabel.IsVisible = false;
            break;
        }
        case EPlayerMessage.GROW:
        case EPlayerMessage.GENERATOR_ON:
        case EPlayerMessage.GENERATOR_OFF:
        case EPlayerMessage.VOLUME_WATER:
        case EPlayerMessage.VOLUME_FUEL:
            messageBox.SizeOffset_Y = 70f;
            messageProgress_0.IsVisible = true;
            messageIcon_0.IsVisible = true;
            messageProgress_1.IsVisible = false;
            messageIcon_1.IsVisible = false;
            messageProgress_2.IsVisible = false;
            messageIcon_2.IsVisible = false;
            switch (message)
            {
            case EPlayerMessage.GENERATOR_ON:
            case EPlayerMessage.GENERATOR_OFF:
            {
                InteractableGenerator interactableGenerator = (InteractableGenerator)PlayerInteract.interactable;
                messageProgress_0.state = (float)(int)interactableGenerator.fuel / (float)(int)interactableGenerator.capacity;
                messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
                break;
            }
            case EPlayerMessage.GROW:
            {
                InteractableFarm interactableFarm = (InteractableFarm)PlayerInteract.interactable;
                float num = 0f;
                if (interactableFarm.planted != 0 && Provider.time > interactableFarm.planted)
                {
                    num = (float)Provider.time - (float)interactableFarm.planted;
                }
                messageProgress_0.state = num / (float)interactableFarm.growth;
                messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Grow");
                break;
            }
            case EPlayerMessage.VOLUME_WATER:
                if (PlayerInteract.interactable is InteractableObjectResource)
                {
                    InteractableObjectResource interactableObjectResource2 = (InteractableObjectResource)PlayerInteract.interactable;
                    messageProgress_0.state = (float)(int)interactableObjectResource2.amount / (float)(int)interactableObjectResource2.capacity;
                }
                else if (PlayerInteract.interactable is InteractableTank)
                {
                    InteractableTank interactableTank2 = (InteractableTank)PlayerInteract.interactable;
                    messageProgress_0.state = (float)(int)interactableTank2.amount / (float)(int)interactableTank2.capacity;
                }
                else if (PlayerInteract.interactable is InteractableRainBarrel)
                {
                    InteractableRainBarrel interactableRainBarrel = (InteractableRainBarrel)PlayerInteract.interactable;
                    messageProgress_0.state = (interactableRainBarrel.isFull ? 1f : 0f);
                    text = ((!interactableRainBarrel.isFull) ? PlayerLifeUI.localization.format("Empty") : PlayerLifeUI.localization.format("Full"));
                }
                messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Water");
                break;
            case EPlayerMessage.VOLUME_FUEL:
                if (PlayerInteract.interactable is InteractableObjectResource)
                {
                    InteractableObjectResource interactableObjectResource = (InteractableObjectResource)PlayerInteract.interactable;
                    messageProgress_0.state = (float)(int)interactableObjectResource.amount / (float)(int)interactableObjectResource.capacity;
                }
                else if (PlayerInteract.interactable is InteractableTank)
                {
                    InteractableTank interactableTank = (InteractableTank)PlayerInteract.interactable;
                    messageProgress_0.state = (float)(int)interactableTank.amount / (float)(int)interactableTank.capacity;
                }
                else if (PlayerInteract.interactable is InteractableOil)
                {
                    InteractableOil interactableOil = (InteractableOil)PlayerInteract.interactable;
                    messageProgress_0.state = (float)(int)interactableOil.fuel / (float)(int)interactableOil.capacity;
                }
                messageIcon_0.Texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
                break;
            }
            switch (message)
            {
            case EPlayerMessage.GROW:
                messageProgress_0.color = Palette.COLOR_G;
                break;
            case EPlayerMessage.VOLUME_WATER:
                messageProgress_0.color = Palette.COLOR_B;
                break;
            default:
                messageProgress_0.color = Palette.COLOR_Y;
                break;
            }
            messageQualityImage.IsVisible = false;
            messageAmountLabel.IsVisible = false;
            break;
        case EPlayerMessage.ITEM:
            messageBox.SizeOffset_Y = 70f;
            if (objects.Length == 2)
            {
                if (((ItemAsset)objects[1]).showQuality)
                {
                    messageQualityImage.TintColor = ItemTool.getQualityColor((float)(int)((Item)objects[0]).quality / 100f);
                    messageAmountLabel.Text = ((Item)objects[0]).quality + "%";
                    messageAmountLabel.TextColor = messageQualityImage.TintColor;
                    messageQualityImage.IsVisible = true;
                    messageAmountLabel.IsVisible = true;
                }
                else if (((ItemAsset)objects[1]).amount > 1)
                {
                    messageAmountLabel.Text = "x" + ((Item)objects[0]).amount;
                    messageAmountLabel.TextColor = ESleekTint.FONT;
                    messageQualityImage.IsVisible = false;
                    messageAmountLabel.IsVisible = true;
                }
                else
                {
                    messageQualityImage.IsVisible = false;
                    messageAmountLabel.IsVisible = false;
                }
            }
            messageProgress_0.IsVisible = false;
            messageIcon_0.IsVisible = false;
            messageProgress_1.IsVisible = false;
            messageIcon_1.IsVisible = false;
            messageProgress_2.IsVisible = false;
            messageIcon_2.IsVisible = false;
            break;
        default:
            messageBox.SizeOffset_Y = 50f;
            messageQualityImage.IsVisible = false;
            messageAmountLabel.IsVisible = false;
            messageProgress_0.IsVisible = false;
            messageIcon_0.IsVisible = false;
            messageProgress_1.IsVisible = false;
            messageIcon_1.IsVisible = false;
            messageProgress_2.IsVisible = false;
            messageIcon_2.IsVisible = false;
            break;
        }
        bool flag2 = message == EPlayerMessage.ITEM || message == EPlayerMessage.VEHICLE_ENTER;
        if (flag2)
        {
            messageBox.BackgroundColor = SleekColor.BackgroundIfLight(color);
        }
        else
        {
            messageBox.BackgroundColor = ESleekTint.BACKGROUND;
        }
        messageLabel.AllowRichText = message == EPlayerMessage.CONDITION || message == EPlayerMessage.TALK || message == EPlayerMessage.INTERACT;
        if (messageLabel.AllowRichText)
        {
            messageLabel.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
            messageLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        }
        else if (flag2)
        {
            messageLabel.TextColor = color;
            messageLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        }
        else
        {
            messageLabel.TextColor = ESleekTint.FONT;
            messageLabel.TextContrastContext = ETextContrastContext.Default;
        }
        messageBox.SizeOffset_X = 200f;
        switch (message)
        {
        case EPlayerMessage.ITEM:
            messageBox.SizeOffset_X = 300f;
            messageLabel.Text = PlayerLifeUI.localization.format("Item", text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.VEHICLE_ENTER:
        {
            messageBox.SizeOffset_X = 300f;
            InteractableVehicle interactableVehicle2 = (InteractableVehicle)PlayerInteract.interactable;
            messageLabel.Text = PlayerLifeUI.localization.format(interactableVehicle2.isLocked ? "Vehicle_Enter_Locked" : "Vehicle_Enter_Unlocked", text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        }
        case EPlayerMessage.DOOR_OPEN:
            messageLabel.Text = PlayerLifeUI.localization.format("Door_Open", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.DOOR_CLOSE:
            messageLabel.Text = PlayerLifeUI.localization.format("Door_Close", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.LOCKED:
            messageLabel.Text = PlayerLifeUI.localization.format("Locked");
            break;
        case EPlayerMessage.BLOCKED:
            messageLabel.Text = PlayerLifeUI.localization.format("Blocked");
            break;
        case EPlayerMessage.PLACEMENT_OBSTRUCTED_BY:
            messageLabel.Text = PlayerLifeUI.localization.format("PlacementObstructedBy", text);
            break;
        case EPlayerMessage.PLACEMENT_OBSTRUCTED_BY_GROUND:
            messageLabel.Text = PlayerLifeUI.localization.format("PlacementObstructedByGround");
            break;
        case EPlayerMessage.FREEFORM_BUILDABLE_NOT_ALLOWED:
            messageLabel.Text = PlayerLifeUI.localization.format("FreeformBuildableNotAllowed");
            break;
        case EPlayerMessage.PILLAR:
            messageLabel.Text = PlayerLifeUI.localization.format("Pillar");
            break;
        case EPlayerMessage.POST:
            messageLabel.Text = PlayerLifeUI.localization.format("Post");
            break;
        case EPlayerMessage.ROOF:
            messageLabel.Text = PlayerLifeUI.localization.format("Roof");
            break;
        case EPlayerMessage.WALL:
            messageLabel.Text = PlayerLifeUI.localization.format("Wall");
            break;
        case EPlayerMessage.CORNER:
            messageLabel.Text = PlayerLifeUI.localization.format("Corner");
            break;
        case EPlayerMessage.GROUND:
            messageLabel.Text = PlayerLifeUI.localization.format("Ground");
            break;
        case EPlayerMessage.DOORWAY:
            messageLabel.Text = PlayerLifeUI.localization.format("Doorway");
            break;
        case EPlayerMessage.WINDOW:
            messageLabel.Text = PlayerLifeUI.localization.format("Window");
            break;
        case EPlayerMessage.GARAGE:
            messageLabel.Text = PlayerLifeUI.localization.format("Garage");
            break;
        case EPlayerMessage.BED_ON:
            messageLabel.Text = PlayerLifeUI.localization.format("Bed_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), text);
            break;
        case EPlayerMessage.BED_OFF:
            messageLabel.Text = PlayerLifeUI.localization.format("Bed_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), text);
            break;
        case EPlayerMessage.BED_CLAIMED:
            messageLabel.Text = PlayerLifeUI.localization.format("Bed_Claimed");
            break;
        case EPlayerMessage.BOUNDS:
            messageLabel.Text = PlayerLifeUI.localization.format("Bounds");
            break;
        case EPlayerMessage.STORAGE:
            messageLabel.Text = PlayerLifeUI.localization.format("Storage", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.FARM:
            messageLabel.Text = PlayerLifeUI.localization.format("Farm", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.GROW:
            messageLabel.Text = PlayerLifeUI.localization.format("Grow");
            break;
        case EPlayerMessage.SOIL:
            messageLabel.Text = PlayerLifeUI.localization.format("Soil");
            break;
        case EPlayerMessage.FIRE_ON:
            messageLabel.Text = PlayerLifeUI.localization.format("Fire_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.FIRE_OFF:
            messageLabel.Text = PlayerLifeUI.localization.format("Fire_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.FORAGE:
            messageLabel.Text = PlayerLifeUI.localization.format("Forage", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.GENERATOR_ON:
            messageLabel.Text = PlayerLifeUI.localization.format("Generator_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.GENERATOR_OFF:
            messageLabel.Text = PlayerLifeUI.localization.format("Generator_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.SPOT_ON:
            messageLabel.Text = PlayerLifeUI.localization.format("Spot_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.SPOT_OFF:
            messageLabel.Text = PlayerLifeUI.localization.format("Spot_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.PURCHASE:
            if (objects.Length == 2)
            {
                messageLabel.Text = PlayerLifeUI.localization.format("Purchase", objects[0], objects[1], MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            }
            break;
        case EPlayerMessage.POWER:
            messageLabel.Text = PlayerLifeUI.localization.format("Power");
            break;
        case EPlayerMessage.USE:
            messageLabel.Text = PlayerLifeUI.localization.format("Use", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.TUTORIAL_MOVE:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Move", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.left), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.right), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.up), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.down));
            break;
        case EPlayerMessage.TUTORIAL_LOOK:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Look");
            break;
        case EPlayerMessage.TUTORIAL_JUMP:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Jump", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.jump));
            break;
        case EPlayerMessage.TUTORIAL_PERSPECTIVE:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Perspective", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.perspective));
            break;
        case EPlayerMessage.TUTORIAL_RUN:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Run", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.sprint));
            break;
        case EPlayerMessage.TUTORIAL_INVENTORY:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Inventory", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.TUTORIAL_SURVIVAL:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Survival", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.inventory), MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Mouse1));
            break;
        case EPlayerMessage.TUTORIAL_GUN:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Gun", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
            break;
        case EPlayerMessage.TUTORIAL_LADDER:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Ladder");
            break;
        case EPlayerMessage.TUTORIAL_CRAFT:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Craft", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.attach), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crafting));
            break;
        case EPlayerMessage.TUTORIAL_SKILLS:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Skills", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.skills));
            break;
        case EPlayerMessage.TUTORIAL_SWIM:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Swim", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.jump));
            break;
        case EPlayerMessage.TUTORIAL_MEDICAL:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Medical", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
            break;
        case EPlayerMessage.TUTORIAL_VEHICLE:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Vehicle", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.TUTORIAL_CROUCH:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Crouch", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crouch));
            break;
        case EPlayerMessage.TUTORIAL_PRONE:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Prone", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.prone));
            break;
        case EPlayerMessage.TUTORIAL_EDUCATED:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Educated", MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Escape));
            break;
        case EPlayerMessage.TUTORIAL_HARVEST:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Harvest", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.TUTORIAL_FISH:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Fish", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
            break;
        case EPlayerMessage.TUTORIAL_BUILD:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Build");
            break;
        case EPlayerMessage.TUTORIAL_HORN:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Horn", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
            break;
        case EPlayerMessage.TUTORIAL_LIGHTS:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Lights", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary));
            break;
        case EPlayerMessage.TUTORIAL_SIRENS:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Sirens", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
            break;
        case EPlayerMessage.TUTORIAL_FARM:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Farm");
            break;
        case EPlayerMessage.TUTORIAL_POWER:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Power");
            break;
        case EPlayerMessage.TUTORIAL_FIRE:
            messageBox.SizeOffset_X = 600f;
            messageLabel.Text = PlayerLifeUI.localization.format("Tutorial_Fire", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crafting));
            break;
        case EPlayerMessage.CLAIM:
            messageLabel.Text = PlayerLifeUI.localization.format("Claim");
            break;
        case EPlayerMessage.UNDERWATER:
            messageLabel.Text = PlayerLifeUI.localization.format("Underwater");
            break;
        case EPlayerMessage.NAV:
            messageLabel.Text = PlayerLifeUI.localization.format("Nav");
            break;
        case EPlayerMessage.SPAWN:
            messageLabel.Text = PlayerLifeUI.localization.format("Spawn");
            break;
        case EPlayerMessage.MOBILE:
            messageLabel.Text = PlayerLifeUI.localization.format("Mobile");
            break;
        case EPlayerMessage.BUILD_ON_OCCUPIED_VEHICLE:
            messageLabel.Text = PlayerLifeUI.localization.format("Build_On_Occupied_Vehicle");
            break;
        case EPlayerMessage.NOT_ALLOWED_HERE:
            messageLabel.Text = PlayerLifeUI.localization.format("Not_Allowed_Here");
            break;
        case EPlayerMessage.CANNOT_BUILD_ON_VEHICLE:
            messageLabel.Text = PlayerLifeUI.localization.format("Cannot_Build_On_Vehicle");
            break;
        case EPlayerMessage.TOO_FAR_FROM_HULL:
            messageLabel.Text = PlayerLifeUI.localization.format("Too_Far_From_Hull");
            break;
        case EPlayerMessage.CANNOT_BUILD_WHILE_SEATED:
            messageLabel.Text = PlayerLifeUI.localization.format("Cannot_Build_While_Seated");
            break;
        case EPlayerMessage.OIL:
            messageLabel.Text = PlayerLifeUI.localization.format("Oil");
            break;
        case EPlayerMessage.VOLUME_WATER:
            messageLabel.Text = PlayerLifeUI.localization.format("Volume_Water", text);
            break;
        case EPlayerMessage.VOLUME_FUEL:
            messageLabel.Text = PlayerLifeUI.localization.format("Volume_Fuel");
            break;
        case EPlayerMessage.TRAPDOOR:
            messageLabel.Text = PlayerLifeUI.localization.format("Trapdoor");
            break;
        case EPlayerMessage.TALK:
        {
            InteractableObjectNPC interactableObjectNPC = PlayerInteract.interactable as InteractableObjectNPC;
            string arg = ((interactableObjectNPC != null && interactableObjectNPC.npcAsset != null) ? interactableObjectNPC.npcAsset.GetNameShownToPlayer(Player.player) : "null");
            messageLabel.Text = PlayerLifeUI.localization.format("Talk", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), arg);
            break;
        }
        case EPlayerMessage.CONDITION:
            messageLabel.Text = text;
            break;
        case EPlayerMessage.INTERACT:
            messageLabel.Text = string.Format(text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.SAFEZONE:
            messageLabel.Text = PlayerLifeUI.localization.format("Safezone");
            break;
        case EPlayerMessage.CLIMB:
            messageLabel.Text = PlayerLifeUI.localization.format("Climb", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        }
        messageBox.PositionOffset_X = (0f - messageBox.SizeOffset_X) / 2f;
        if (transform != null && MainCamera.instance != null)
        {
            messageBox.PositionOffset_Y = 10f;
            Vector3 vector = MainCamera.instance.WorldToViewportPoint(transform.position);
            Vector2 vector2 = container.ViewportToNormalizedPosition(vector);
            messageBox.PositionScale_X = vector2.x;
            messageBox.PositionScale_Y = vector2.y;
        }
        else
        {
            if (messageBox2.IsVisible)
            {
                messageBox.PositionOffset_Y = -80f - messageBox.SizeOffset_Y - 10f - messageBox2.SizeOffset_Y;
            }
            else
            {
                messageBox.PositionOffset_Y = -80f - messageBox.SizeOffset_Y;
            }
            messageBox.PositionScale_X = 0.5f;
            messageBox.PositionScale_Y = 1f;
        }
    }

    public static void hint2(EPlayerMessage message, float progress, float data)
    {
        if (messageBox2 != null && PlayerLifeUI.localization != null && !ShouldIgnoreHintAndMessageRequests() && !isMessaged)
        {
            messageBox2.IsVisible = true;
            lastHinted2 = true;
            isHinted2 = true;
            if (message == EPlayerMessage.SALVAGE)
            {
                messageBox2.SizeOffset_Y = 100f;
                messageBox2.PositionOffset_Y = -80f - messageBox2.SizeOffset_Y;
                messageIcon2.IsVisible = true;
                messageProgress2_0.IsVisible = true;
                messageProgress2_1.IsVisible = true;
                messageIcon2.Texture = PlayerLifeUI.icons.load<Texture2D>("Health");
                messageLabel2.AllowRichText = false;
                messageLabel2.TextColor = ESleekTint.FONT;
                messageLabel2.TextContrastContext = ETextContrastContext.Default;
                messageLabel2.Text = PlayerLifeUI.localization.format("Salvage", ControlsSettings.interact);
                messageProgress2_0.state = progress;
                messageProgress2_0.color = Palette.COLOR_P;
                messageProgress2_1.state = data;
                messageProgress2_1.color = Palette.COLOR_R;
            }
        }
    }

    public static void message(EPlayerMessage message, string text, float duration = 2f)
    {
        if (messageBox2 == null || PlayerLifeUI.localization == null || (!OptionsSettings.hints && message != EPlayerMessage.EXPERIENCE && message != EPlayerMessage.MOON_ON && message != EPlayerMessage.MOON_OFF && message != EPlayerMessage.SAFEZONE_ON && message != EPlayerMessage.SAFEZONE_OFF && message != EPlayerMessage.WAVE_ON && message != EPlayerMessage.MOON_OFF && message != EPlayerMessage.DEADZONE_ON && message != EPlayerMessage.DEADZONE_OFF && message != EPlayerMessage.REPUTATION && message != EPlayerMessage.NPC_CUSTOM && message != EPlayerMessage.NOT_PAINTABLE))
        {
            return;
        }
        if (message == EPlayerMessage.NONE)
        {
            messageBox2.IsVisible = false;
            messageDisappearTime = 0f;
            isMessaged = false;
        }
        else if (!ShouldIgnoreHintAndMessageRequests() && ((message != EPlayerMessage.EXPERIENCE && message != EPlayerMessage.REPUTATION) || (!PlayerNPCDialogueUI.active && !PlayerNPCQuestUI.active && !PlayerNPCVendorUI.active)))
        {
            messageBox2.PositionOffset_X = -200f;
            messageBox2.SizeOffset_X = 400f;
            messageBox2.SizeOffset_Y = 50f;
            messageBox2.PositionOffset_Y = -80f - messageBox2.SizeOffset_Y;
            messageBox2.IsVisible = true;
            messageIcon2.IsVisible = false;
            messageProgress2_0.IsVisible = false;
            messageProgress2_1.IsVisible = false;
            messageDisappearTime = Time.realtimeSinceStartup + duration;
            isMessaged = true;
            messageLabel2.AllowRichText = message == EPlayerMessage.NPC_CUSTOM;
            if (messageLabel2.AllowRichText)
            {
                messageLabel2.TextColor = ESleekTint.RICH_TEXT_DEFAULT;
                messageLabel2.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            }
            else
            {
                messageLabel2.TextColor = ESleekTint.FONT;
                messageLabel2.TextContrastContext = ETextContrastContext.Default;
            }
            if (message == EPlayerMessage.SPACE)
            {
                messageLabel2.Text = PlayerLifeUI.localization.format("Space");
            }
            switch (message)
            {
            case EPlayerMessage.RELOAD:
                messageLabel2.Text = PlayerLifeUI.localization.format("Reload", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.reload));
                break;
            case EPlayerMessage.SAFETY:
                messageLabel2.Text = PlayerLifeUI.localization.format("Safety", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.firemode));
                break;
            case EPlayerMessage.VEHICLE_EXIT:
                messageLabel2.Text = PlayerLifeUI.localization.format("Vehicle_Exit", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
                break;
            case EPlayerMessage.VEHICLE_SWAP:
                messageLabel2.Text = PlayerLifeUI.localization.format("Vehicle_Swap", Player.player.movement.getVehicle().passengers.Length);
                break;
            case EPlayerMessage.LIGHT:
                messageLabel2.Text = PlayerLifeUI.localization.format("Light", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
                break;
            case EPlayerMessage.LASER:
                messageLabel2.Text = PlayerLifeUI.localization.format("Laser", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
                break;
            case EPlayerMessage.HOUSING_PLANNER_TUTORIAL:
                messageLabel2.Text = PlayerLifeUI.localization.format("HousingPlannerTutorial", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.attach));
                break;
            case EPlayerMessage.RANGEFINDER:
                messageLabel2.Text = PlayerLifeUI.localization.format("Rangefinder", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
                break;
            case EPlayerMessage.EXPERIENCE:
                messageLabel2.Text = PlayerLifeUI.localization.format("Experience", text);
                break;
            case EPlayerMessage.EMPTY:
                messageLabel2.Text = PlayerLifeUI.localization.format("Empty");
                break;
            case EPlayerMessage.FULL:
                messageLabel2.Text = PlayerLifeUI.localization.format("Full");
                break;
            case EPlayerMessage.MOON_ON:
                messageLabel2.Text = PlayerLifeUI.localization.format("Moon_On");
                break;
            case EPlayerMessage.MOON_OFF:
                messageLabel2.Text = PlayerLifeUI.localization.format("Moon_Off");
                break;
            case EPlayerMessage.SAFEZONE_ON:
                messageLabel2.Text = PlayerLifeUI.localization.format("Safezone_On");
                break;
            case EPlayerMessage.SAFEZONE_OFF:
                messageLabel2.Text = PlayerLifeUI.localization.format("Safezone_Off");
                break;
            case EPlayerMessage.WAVE_ON:
                messageLabel2.Text = PlayerLifeUI.localization.format("Wave_On");
                break;
            case EPlayerMessage.WAVE_OFF:
                messageLabel2.Text = PlayerLifeUI.localization.format("Wave_Off");
                break;
            case EPlayerMessage.DEADZONE_ON:
                messageLabel2.Text = PlayerLifeUI.localization.format("Deadzone_On");
                break;
            case EPlayerMessage.DEADZONE_OFF:
                messageLabel2.Text = PlayerLifeUI.localization.format("Deadzone_Off");
                break;
            case EPlayerMessage.BUSY:
                messageLabel2.Text = PlayerLifeUI.localization.format("Busy");
                break;
            case EPlayerMessage.FUEL:
                messageLabel2.Text = PlayerLifeUI.localization.format("Fuel", text);
                break;
            case EPlayerMessage.CLEAN:
                messageLabel2.Text = PlayerLifeUI.localization.format("Clean");
                break;
            case EPlayerMessage.SALTY:
                messageLabel2.Text = PlayerLifeUI.localization.format("Salty");
                break;
            case EPlayerMessage.DIRTY:
                messageLabel2.Text = PlayerLifeUI.localization.format("Dirty");
                break;
            case EPlayerMessage.REPUTATION:
                messageLabel2.Text = PlayerLifeUI.localization.format("Reputation", text);
                break;
            case EPlayerMessage.BAYONET:
                messageLabel2.Text = PlayerLifeUI.localization.format("Bayonet", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
                break;
            case EPlayerMessage.VEHICLE_LOCKED:
                messageLabel2.Text = PlayerLifeUI.localization.format("Vehicle_Locked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
                break;
            case EPlayerMessage.VEHICLE_UNLOCKED:
                messageLabel2.Text = PlayerLifeUI.localization.format("Vehicle_Unlocked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
                break;
            case EPlayerMessage.NOT_PAINTABLE:
                messageLabel2.Text = PlayerLifeUI.localization.format("NotPaintable");
                break;
            case EPlayerMessage.NPC_CUSTOM:
                messageBox2.PositionOffset_X = -300f;
                messageBox2.SizeOffset_X = 600f;
                RichTextUtil.replaceNewlineMarkup(ref text);
                messageLabel2.Text = text;
                break;
            }
        }
    }

    private void tickIsHallucinating(float deltaTime)
    {
        hallucinationTimer += deltaTime;
        UnturnedPostProcess.instance.tickIsHallucinating(deltaTime, hallucinationTimer);
    }

    private void setIsHallucinating(bool isHallucinating)
    {
        if (isHallucinating && (double)UnityEngine.Random.value < 0.5)
        {
            float value = UnityEngine.Random.value;
            if ((double)value < 0.25)
            {
                hallucinationReverbZone.reverbPreset = AudioReverbPreset.Drugged;
            }
            else if ((double)value < 0.5)
            {
                hallucinationReverbZone.reverbPreset = AudioReverbPreset.Psychotic;
            }
            else if ((double)value < 0.75)
            {
                hallucinationReverbZone.reverbPreset = AudioReverbPreset.Arena;
            }
            else
            {
                hallucinationReverbZone.reverbPreset = AudioReverbPreset.SewerPipe;
            }
            hallucinationReverbZone.enabled = true;
        }
        else
        {
            hallucinationReverbZone.enabled = false;
        }
        UnturnedPostProcess.instance.setIsHallucinating(isHallucinating);
        if (!isHallucinating)
        {
            hallucinationTimer = 0f;
        }
    }

    private void onVisionUpdated(bool isHallucinating)
    {
        setIsHallucinating(isHallucinating);
    }

    private void onLifeUpdated(bool isDead)
    {
        isLocked = false;
        inputWantsCustomModal = false;
        usingCustomModal = false;
        MenuConfigurationOptionsUI.close();
        MenuConfigurationDisplayUI.close();
        MenuConfigurationGraphicsUI.close();
        MenuConfigurationControlsUI.close();
        PlayerPauseUI.audioMenu.close();
        PlayerPauseUI.close();
        PlayerDashboardUI.close();
        PlayerBarricadeSignUI.close();
        boomboxUI.close();
        PlayerBarricadeLibraryUI.close();
        mannequinUI.close();
        browserRequestUI.close();
        PlayerNPCDialogueUI.close();
        PlayerNPCQuestUI.close();
        PlayerNPCVendorUI.close();
        PlayerWorkzoneUI.close();
        if (isDead)
        {
            PlayerLifeUI.close();
            PlayerDeathUI.open(fromDeath: true);
        }
        else
        {
            PlayerDeathUI.close();
            PlayerLifeUI.open();
        }
    }

    private void onGlassesUpdated(ushort newGlasses, byte newGlassesQuality, byte[] newGlassesState)
    {
        isBlindfolded = Player.player.clothing.glassesAsset != null && Player.player.clothing.glassesAsset.isBlindfold;
    }

    private void onMoonUpdated(bool isFullMoon)
    {
        if (isFullMoon)
        {
            message(EPlayerMessage.MOON_ON, "");
        }
        else
        {
            message(EPlayerMessage.MOON_OFF, "");
        }
    }

    private void OnEnable()
    {
        instance = this;
        base.useGUILayout = false;
    }

    internal void Player_OnGUI()
    {
        if (window != null)
        {
            Glazier.Get().Root = window;
        }
    }

    private void OnGUI()
    {
        if (window == null)
        {
            return;
        }
        if (Event.current.isKey && Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.UpArrow)
            {
                if (PlayerLifeUI.chatting)
                {
                    PlayerLifeUI.repeatChat(1);
                }
            }
            else if (Event.current.keyCode == KeyCode.DownArrow)
            {
                if (PlayerLifeUI.chatting)
                {
                    PlayerLifeUI.repeatChat(-1);
                }
            }
            else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
            {
                if (PlayerLifeUI.chatting)
                {
                    PlayerLifeUI.SendChatAndClose();
                }
                else if (PlayerLifeUI.active && canOpenMenus)
                {
                    PlayerLifeUI.openChat();
                }
            }
            else if (Event.current.keyCode == ControlsSettings.global)
            {
                if (PlayerLifeUI.active && canOpenMenus)
                {
                    chat = EChatMode.GLOBAL;
                    PlayerLifeUI.openChat();
                }
            }
            else if (Event.current.keyCode == ControlsSettings.local)
            {
                if (PlayerLifeUI.active && canOpenMenus)
                {
                    chat = EChatMode.LOCAL;
                    PlayerLifeUI.openChat();
                }
            }
            else if (Event.current.keyCode == ControlsSettings.group && PlayerLifeUI.active && canOpenMenus)
            {
                chat = EChatMode.GROUP;
                PlayerLifeUI.openChat();
            }
        }
        if (PlayerLifeUI.chatting)
        {
            PlayerLifeUI.chatField.FocusControl();
        }
        MenuConfigurationControlsUI.bindOnGUI();
    }

    private void escapeMenu()
    {
        if (MenuConfigurationOptionsUI.active)
        {
            MenuConfigurationOptionsUI.close();
            PlayerPauseUI.open();
            return;
        }
        if (MenuConfigurationDisplayUI.active)
        {
            MenuConfigurationDisplayUI.close();
            PlayerPauseUI.open();
            return;
        }
        if (MenuConfigurationGraphicsUI.active)
        {
            MenuConfigurationGraphicsUI.close();
            PlayerPauseUI.open();
            return;
        }
        if (MenuConfigurationControlsUI.active)
        {
            MenuConfigurationControlsUI.close();
            PlayerPauseUI.open();
            return;
        }
        if (PlayerPauseUI.audioMenu.active)
        {
            PlayerPauseUI.audioMenu.close();
            PlayerPauseUI.open();
            return;
        }
        if (PlayerPauseUI.active)
        {
            PlayerPauseUI.closeAndGotoAppropriateHUD();
            return;
        }
        if (PlayerDashboardUI.active && PlayerDashboardInventoryUI.active)
        {
            if (PlayerDashboardInventoryUI.isDragging)
            {
                PlayerDashboardInventoryUI.stopDrag();
                return;
            }
            if (PlayerDashboardInventoryUI.selectedPage != byte.MaxValue)
            {
                PlayerDashboardInventoryUI.closeSelection();
                return;
            }
        }
        bool flag = true;
        if (PlayerDashboardUI.active)
        {
            PlayerDashboardUI.close();
        }
        else if (PlayerBarricadeSignUI.active)
        {
            PlayerBarricadeSignUI.close();
        }
        else if (boomboxUI.active)
        {
            boomboxUI.close();
        }
        else if (PlayerBarricadeLibraryUI.active)
        {
            PlayerBarricadeLibraryUI.close();
        }
        else if (mannequinUI.active)
        {
            mannequinUI.close();
        }
        else if (browserRequestUI.isActive)
        {
            browserRequestUI.close();
        }
        else if (PlayerNPCDialogueUI.active)
        {
            PlayerNPCDialogueUI.close();
        }
        else if (PlayerWorkzoneUI.active)
        {
            PlayerWorkzoneUI.close();
        }
        else
        {
            flag = false;
        }
        if (flag)
        {
            if (Player.player.life.isDead)
            {
                PlayerDeathUI.open(fromDeath: false);
            }
            else
            {
                PlayerLifeUI.open();
            }
        }
        else if (PlayerNPCQuestUI.active)
        {
            PlayerNPCQuestUI.closeNicely();
        }
        else if (PlayerNPCVendorUI.active)
        {
            PlayerNPCVendorUI.closeNicely();
        }
        else if (!Player.player.equipment.isUseableShowingMenu)
        {
            PlayerDeathUI.close();
            PlayerLifeUI.close();
            PlayerPauseUI.open();
        }
    }

    /// <summary>
    /// Adjust screen positioning and visibility of player name widgets to match their world-space counterparts.
    /// </summary>
    private void updateGroupLabels()
    {
        if (Player.player == null || MainCamera.instance == null || groupUI.groups == null || groupUI.groups.Count != Provider.clients.Count)
        {
            return;
        }
        Camera camera = MainCamera.instance;
        bool areSpecStatsVisible = Player.player.look.areSpecStatsVisible;
        for (int i = 0; i < groupUI.groups.Count; i++)
        {
            ISleekLabel sleekLabel = groupUI.groups[i];
            SteamPlayer steamPlayer = Provider.clients[i];
            if (sleekLabel == null || steamPlayer == null || steamPlayer.model == null)
            {
                continue;
            }
            bool flag;
            if (areSpecStatsVisible)
            {
                flag = true;
            }
            else if (Provider.modeConfigData.Gameplay.Group_HUD)
            {
                bool num = steamPlayer.playerID.steamID != Provider.client;
                bool flag2 = steamPlayer.player.quests.isMemberOfSameGroupAs(Player.player);
                flag = num && flag2;
            }
            else
            {
                flag = false;
            }
            if (!flag)
            {
                sleekLabel.IsVisible = false;
                continue;
            }
            if ((steamPlayer.model.position - camera.transform.position).sqrMagnitude > 262144f)
            {
                sleekLabel.IsVisible = false;
                continue;
            }
            Vector3 vector = camera.WorldToViewportPoint(steamPlayer.model.position + Vector3.up * 3f);
            if (vector.z <= 0f)
            {
                sleekLabel.IsVisible = false;
                continue;
            }
            Vector2 vector2 = groupUI.ViewportToNormalizedPosition(vector);
            sleekLabel.PositionScale_X = vector2.x;
            sleekLabel.PositionScale_Y = vector2.y;
            float alpha;
            if (areSpecStatsVisible)
            {
                alpha = 1f;
            }
            else if (!OptionsSettings.shouldNametagFadeOut)
            {
                alpha = 0.75f;
            }
            else
            {
                float magnitude = new Vector2(vector2.x - 0.5f, vector2.y - 0.5f).magnitude;
                float t = Mathf.InverseLerp(0.05f, 0.1f, magnitude);
                alpha = Mathf.Lerp(0.1f, 0.75f, t);
            }
            sleekLabel.TextColor = new SleekColor(ESleekTint.FONT, alpha);
            if (!sleekLabel.IsVisible)
            {
                if (steamPlayer.isMemberOfSameGroupAs(Player.player) && !string.IsNullOrEmpty(steamPlayer.playerID.nickName))
                {
                    sleekLabel.Text = steamPlayer.playerID.nickName;
                }
                else
                {
                    sleekLabel.Text = steamPlayer.playerID.characterName;
                }
            }
            sleekLabel.IsVisible = true;
        }
    }

    /// <summary>
    /// Update hitmarker visibility, and their world-space positions if user enabled that.
    /// </summary>
    private void updateHitmarkers()
    {
        if (PlayerLifeUI.activeHitmarkers == null || MainCamera.instance == null)
        {
            return;
        }
        float deltaTime = Time.deltaTime;
        for (int num = PlayerLifeUI.activeHitmarkers.Count - 1; num >= 0; num--)
        {
            HitmarkerInfo value = PlayerLifeUI.activeHitmarkers[num];
            if (value.aliveTime > HIT_TIME)
            {
                PlayerLifeUI.ReleaseHitmarker(value.sleekElement);
                PlayerLifeUI.activeHitmarkers.RemoveAtFast(num);
            }
            else
            {
                value.aliveTime += deltaTime;
                PlayerLifeUI.activeHitmarkers[num] = value;
                Vector2 vector2;
                bool isVisible;
                if (value.shouldFollowWorldPosition)
                {
                    Vector3 vector = MainCamera.instance.WorldToViewportPoint(value.worldPosition);
                    vector2 = window.ViewportToNormalizedPosition(vector);
                    isVisible = vector.z > 0f;
                }
                else
                {
                    vector2 = new Vector3(0.5f, 0.5f);
                    isVisible = true;
                }
                value.sleekElement.PositionScale_X = vector2.x;
                value.sleekElement.PositionScale_Y = vector2.y;
                value.sleekElement.IsVisible = isVisible;
            }
        }
    }

    /// <summary>
    /// Disable hints and messages if no longer applicable.
    /// </summary>
    private void updateHintsAndMessages()
    {
        if (isHinted)
        {
            if (!lastHinted)
            {
                isHinted = false;
                if (messageBox != null)
                {
                    messageBox.IsVisible = false;
                }
                if (messagePlayer != null)
                {
                    messagePlayer.IsVisible = false;
                }
            }
            lastHinted = false;
        }
        if (isMessaged)
        {
            if (Time.realtimeSinceStartup > messageDisappearTime)
            {
                isMessaged = false;
                if (!isHinted2 && messageBox2 != null)
                {
                    messageBox2.IsVisible = false;
                }
            }
        }
        else
        {
            if (!isHinted2)
            {
                return;
            }
            if (!lastHinted2)
            {
                isHinted2 = false;
                if (messageBox2 != null)
                {
                    messageBox2.IsVisible = false;
                }
            }
            lastHinted2 = false;
        }
    }

    /// <summary>
    /// Disable vote popup if enough time has passed.
    /// </summary>
    private void updateVoteDisplay()
    {
        if (PlayerLifeUI.isVoteMessaged && Time.realtimeSinceStartup - PlayerLifeUI.lastVoteMessage > 2f)
        {
            PlayerLifeUI.isVoteMessaged = false;
            if (PlayerLifeUI.voteBox != null)
            {
                PlayerLifeUI.voteBox.IsVisible = false;
            }
        }
    }

    /// <summary>
    /// Pause the game if playing singleplayer and menu is open.
    /// </summary>
    private void updatePauseTimeScale()
    {
        if (Provider.isServer && (MenuConfigurationOptionsUI.active || MenuConfigurationDisplayUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationControlsUI.active || PlayerPauseUI.audioMenu.active || PlayerPauseUI.active))
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
        else
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    private void tickDeathTimers()
    {
        if (!PlayerDeathUI.active)
        {
            return;
        }
        if (PlayerDeathUI.homeButton != null)
        {
            if (!Provider.isServer && Provider.isPvP)
            {
                if (Time.realtimeSinceStartup - Player.player.life.lastDeath < (float)Provider.modeConfigData.Gameplay.Timer_Home)
                {
                    PlayerDeathUI.homeButton.text = PlayerDeathUI.localization.format("Home_Button_Timer", Mathf.Ceil((float)Provider.modeConfigData.Gameplay.Timer_Home - (Time.realtimeSinceStartup - Player.player.life.lastDeath)));
                }
                else
                {
                    PlayerDeathUI.homeButton.text = PlayerDeathUI.localization.format("Home_Button");
                }
            }
            else if (Time.realtimeSinceStartup - Player.player.life.lastRespawn < (float)Provider.modeConfigData.Gameplay.Timer_Respawn)
            {
                PlayerDeathUI.homeButton.text = PlayerDeathUI.localization.format("Home_Button_Timer", Mathf.Ceil((float)Provider.modeConfigData.Gameplay.Timer_Respawn - (Time.realtimeSinceStartup - Player.player.life.lastRespawn)));
            }
            else
            {
                PlayerDeathUI.homeButton.text = PlayerDeathUI.localization.format("Home_Button");
            }
        }
        if (PlayerDeathUI.respawnButton != null)
        {
            if (Time.realtimeSinceStartup - Player.player.life.lastRespawn < (float)Provider.modeConfigData.Gameplay.Timer_Respawn)
            {
                PlayerDeathUI.respawnButton.text = PlayerDeathUI.localization.format("Respawn_Button_Timer", Mathf.Ceil((float)Provider.modeConfigData.Gameplay.Timer_Respawn - (Time.realtimeSinceStartup - Player.player.life.lastRespawn)));
            }
            else
            {
                PlayerDeathUI.respawnButton.text = PlayerDeathUI.localization.format("Respawn_Button");
            }
        }
    }

    private void tickExitTimer()
    {
        if (!PlayerPauseUI.active)
        {
            return;
        }
        if (PlayerPauseUI.exitButton != null)
        {
            if (PlayerPauseUI.shouldExitButtonRespectTimer && Time.realtimeSinceStartup - PlayerPauseUI.lastLeave < (float)Provider.modeConfigData.Gameplay.Timer_Exit)
            {
                PlayerPauseUI.exitButton.text = PlayerPauseUI.localization.format("Exit_Button_Timer", Mathf.Ceil((float)Provider.modeConfigData.Gameplay.Timer_Exit - (Time.realtimeSinceStartup - PlayerPauseUI.lastLeave)));
            }
            else
            {
                PlayerPauseUI.exitButton.text = PlayerPauseUI.localization.format("Exit_Button_Text");
            }
        }
        if (PlayerPauseUI.quitButton != null)
        {
            if (PlayerPauseUI.shouldExitButtonRespectTimer && Time.realtimeSinceStartup - PlayerPauseUI.lastLeave < (float)Provider.modeConfigData.Gameplay.Timer_Exit)
            {
                PlayerPauseUI.quitButton.text = PlayerPauseUI.localization.format("Quit_Button_Timer", Mathf.Ceil((float)Provider.modeConfigData.Gameplay.Timer_Exit - (Time.realtimeSinceStartup - PlayerPauseUI.lastLeave)));
            }
            else
            {
                PlayerPauseUI.quitButton.text = PlayerPauseUI.localization.format("Quit_Button");
            }
        }
    }

    private void tickInput()
    {
        inputWantsCustomModal = false;
        if (MenuConfigurationControlsUI.binding != byte.MaxValue)
        {
            return;
        }
        if ((InputEx.GetKeyDown(ControlsSettings.left) || InputEx.GetKeyDown(ControlsSettings.up) || InputEx.GetKeyDown(ControlsSettings.right) || InputEx.GetKeyDown(ControlsSettings.down)) && PlayerDashboardUI.active)
        {
            PlayerDashboardUI.close();
            if (Player.player.life.IsAlive)
            {
                PlayerLifeUI.open();
            }
        }
        if (PlayerLifeUI.chatting && Input.GetKeyDown(KeyCode.Escape))
        {
            PlayerLifeUI.closeChat();
        }
        else if (InputEx.ConsumeKeyDown(KeyCode.Escape))
        {
            escapeMenu();
        }
        if (Player.player.life.IsAlive)
        {
            if (InputEx.ConsumeKeyDown(ControlsSettings.dashboard))
            {
                if (PlayerDashboardUI.active)
                {
                    PlayerDashboardUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerBarricadeSignUI.active)
                {
                    PlayerBarricadeSignUI.close();
                    PlayerLifeUI.open();
                }
                else if (boomboxUI.active)
                {
                    boomboxUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerBarricadeLibraryUI.active)
                {
                    PlayerBarricadeLibraryUI.close();
                    PlayerLifeUI.open();
                }
                else if (mannequinUI.active)
                {
                    mannequinUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerNPCDialogueUI.active)
                {
                    PlayerNPCDialogueUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerNPCQuestUI.active)
                {
                    PlayerNPCQuestUI.closeNicely();
                }
                else if (PlayerNPCVendorUI.active)
                {
                    PlayerNPCVendorUI.closeNicely();
                }
                else if (canOpenMenus)
                {
                    PlayerLifeUI.close();
                    PlayerPauseUI.close();
                    PlayerDashboardUI.open();
                }
            }
            if (InputEx.ConsumeKeyDown(ControlsSettings.inventory))
            {
                if (PlayerDashboardUI.active && PlayerDashboardInventoryUI.active)
                {
                    PlayerDashboardUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerDashboardUI.active)
                {
                    PlayerDashboardCraftingUI.close();
                    PlayerDashboardSkillsUI.close();
                    PlayerDashboardInformationUI.close();
                    PlayerDashboardInventoryUI.open();
                }
                else if (canOpenMenus)
                {
                    PlayerLifeUI.close();
                    PlayerPauseUI.close();
                    PlayerDashboardInventoryUI.active = true;
                    PlayerDashboardCraftingUI.active = false;
                    PlayerDashboardSkillsUI.active = false;
                    PlayerDashboardInformationUI.active = false;
                    PlayerDashboardUI.open();
                }
            }
            if (InputEx.ConsumeKeyDown(ControlsSettings.crafting) && Level.info != null && Level.info.type != ELevelType.HORDE && Level.info.configData.Allow_Crafting)
            {
                if (PlayerDashboardUI.active && PlayerDashboardCraftingUI.active)
                {
                    PlayerDashboardUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerDashboardUI.active)
                {
                    PlayerDashboardInventoryUI.close();
                    PlayerDashboardSkillsUI.close();
                    PlayerDashboardInformationUI.close();
                    PlayerDashboardCraftingUI.open();
                }
                else if (canOpenMenus)
                {
                    PlayerLifeUI.close();
                    PlayerPauseUI.close();
                    PlayerDashboardInventoryUI.active = false;
                    PlayerDashboardCraftingUI.active = true;
                    PlayerDashboardSkillsUI.active = false;
                    PlayerDashboardInformationUI.active = false;
                    PlayerDashboardUI.open();
                }
            }
            if (InputEx.ConsumeKeyDown(ControlsSettings.skills) && Level.info != null && Level.info.type != ELevelType.HORDE && Level.info.configData.Allow_Skills)
            {
                if (PlayerDashboardUI.active && PlayerDashboardSkillsUI.active)
                {
                    PlayerDashboardUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerDashboardUI.active)
                {
                    PlayerDashboardInventoryUI.close();
                    PlayerDashboardCraftingUI.close();
                    PlayerDashboardInformationUI.close();
                    PlayerDashboardSkillsUI.open();
                }
                else if (canOpenMenus)
                {
                    PlayerLifeUI.close();
                    PlayerPauseUI.close();
                    PlayerDashboardInventoryUI.active = false;
                    PlayerDashboardCraftingUI.active = false;
                    PlayerDashboardSkillsUI.active = true;
                    PlayerDashboardInformationUI.active = false;
                    PlayerDashboardUI.open();
                }
            }
            if ((InputEx.ConsumeKeyDown(ControlsSettings.map) || InputEx.ConsumeKeyDown(ControlsSettings.quests) || InputEx.ConsumeKeyDown(ControlsSettings.players)) && Level.info != null && Level.info.configData.Allow_Information)
            {
                if (PlayerDashboardUI.active && PlayerDashboardInformationUI.active)
                {
                    PlayerDashboardUI.close();
                    PlayerLifeUI.open();
                }
                else
                {
                    if (InputEx.GetKeyDown(ControlsSettings.quests))
                    {
                        PlayerDashboardInformationUI.openQuests();
                    }
                    else if (InputEx.GetKeyDown(ControlsSettings.players))
                    {
                        PlayerDashboardInformationUI.openPlayers();
                    }
                    if (PlayerDashboardUI.active)
                    {
                        PlayerDashboardInventoryUI.close();
                        PlayerDashboardCraftingUI.close();
                        PlayerDashboardSkillsUI.close();
                        PlayerDashboardInformationUI.open();
                    }
                    else if (canOpenMenus)
                    {
                        PlayerLifeUI.close();
                        PlayerPauseUI.close();
                        PlayerDashboardInventoryUI.active = false;
                        PlayerDashboardCraftingUI.active = false;
                        PlayerDashboardSkillsUI.active = false;
                        PlayerDashboardInformationUI.active = true;
                        PlayerDashboardUI.open();
                    }
                }
            }
            if (InputEx.ConsumeKeyDown(ControlsSettings.gesture))
            {
                if (PlayerLifeUI.active && canOpenMenus)
                {
                    PlayerLifeUI.openGestures();
                }
            }
            else if (InputEx.GetKeyUp(ControlsSettings.gesture) && PlayerLifeUI.active)
            {
                PlayerLifeUI.closeGestures();
            }
        }
        if (window != null)
        {
            if (InputEx.GetKeyDown(ControlsSettings.screenshot))
            {
                Provider.RequestScreenshot();
            }
            if (InputEx.GetKeyDown(ControlsSettings.hud))
            {
                wantsWindowEnabled = !wantsWindowEnabled;
                window.drawCursorWhileDisabled = false;
                UpdateWindowEnabled();
            }
            InputEx.GetKeyDown(ControlsSettings.terminal);
        }
        if (InputEx.GetKeyDown(ControlsSettings.refreshAssets) && Provider.isServer)
        {
            Assets.RequestReloadAllAssets();
        }
        if (InputEx.GetKeyDown(ControlsSettings.clipboardDebug))
        {
            string text = string.Empty;
            for (int i = 0; i < Player.player.quests.flagsList.Count; i++)
            {
                if (i > 0)
                {
                    text += "\n";
                }
                text += $"{Player.player.quests.flagsList[i].id,5} {Player.player.quests.flagsList[i].value,5}";
            }
            GUIUtility.systemCopyBuffer = text;
        }
        inputWantsCustomModal = InputEx.GetKey(ControlsSettings.CustomModal);
    }

    private void tickMenuBlur()
    {
        if (!(menuBlurFX == null))
        {
            EPluginWidgetFlags pluginWidgetFlags = Player.player.pluginWidgetFlags;
            bool flag = (pluginWidgetFlags & EPluginWidgetFlags.ForceBlur) == EPluginWidgetFlags.ForceBlur || ((pluginWidgetFlags & EPluginWidgetFlags.NoBlur) != EPluginWidgetFlags.NoBlur && ((window.showCursor && !usingCustomModal && !MenuConfigurationGraphicsUI.active && !PlayerNPCDialogueUI.active && !PlayerNPCQuestUI.active && !PlayerNPCVendorUI.active && !PlayerWorkzoneUI.active) || (WaterUtility.isPointUnderwater(MainCamera.instance.transform.position) && (Player.player.clothing.glassesAsset == null || !Player.player.clothing.glassesAsset.proofWater)) || (Player.player.look.isScopeActive && GraphicsSettings.scopeQuality != 0 && Player.player.look.perspective == EPlayerPerspective.FIRST && Player.player.equipment.useable != null && ((UseableGun)Player.player.equipment.useable).isAiming)));
            if (menuBlurFX.enabled != flag)
            {
                menuBlurFX.enabled = flag;
            }
        }
    }

    private void UpdateOverlayColor()
    {
        float num;
        Color black;
        if (isBlindfolded)
        {
            black = Color.black;
            num = 1f;
        }
        else
        {
            black = stunColor;
            num = stunAlpha;
        }
        black = Color.Lerp(black, Palette.COLOR_R, painAlpha + (1f - num));
        black.a = Mathf.Max(num, painAlpha);
        colorOverlayImage.TintColor = black;
        if (isWindowEnabledByColorOverlay && stunAlpha < 0.001f && painAlpha < 0.001f)
        {
            isWindowEnabledByColorOverlay = false;
            UpdateWindowEnabled();
        }
    }

    private void Update()
    {
        if (window != null)
        {
            MenuConfigurationControlsUI.bindUpdate();
            PlayerDashboardInventoryUI.updateDraggedItem();
            PlayerDashboardInventoryUI.updateNearbyDrops();
            updateGroupLabels();
            PlayerLifeUI.updateCompass();
            PlayerLifeUI.updateHotbar();
            PlayerLifeUI.updateStatTracker();
            PlayerNPCVendorUI.MaybeRefresh();
            UpdateOverlayColor();
            painAlpha = Mathf.Max(0f, painAlpha - Time.deltaTime);
            stunAlpha = Mathf.Max(0f, stunAlpha - Time.deltaTime);
            updateHitmarkers();
            updateHintsAndMessages();
            updateVoteDisplay();
            updatePauseTimeScale();
            tickDeathTimers();
            tickExitTimer();
            if (PlayerNPCDialogueUI.active)
            {
                PlayerNPCDialogueUI.UpdateAnimation();
            }
            if (PlayerDashboardInformationUI.active)
            {
                PlayerDashboardInformationUI.updateDynamicMap();
            }
            tickInput();
            bool flag = Player.player.inPluginModal || PlayerPauseUI.active || MenuConfigurationOptionsUI.active || MenuConfigurationDisplayUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationControlsUI.active || PlayerPauseUI.audioMenu.active || PlayerDashboardUI.active || PlayerDeathUI.active || PlayerLifeUI.chatting || PlayerLifeUI.gesturing || PlayerBarricadeSignUI.active || boomboxUI.active || PlayerBarricadeLibraryUI.active || mannequinUI.active || browserRequestUI.isActive || PlayerNPCDialogueUI.active || PlayerNPCQuestUI.active || PlayerNPCVendorUI.active || (PlayerWorkzoneUI.active && !InputEx.GetKey(ControlsSettings.secondary)) || isLocked;
            usingCustomModal = !flag & inputWantsCustomModal;
            flag |= inputWantsCustomModal;
            window.showCursor = flag;
            tickMenuBlur();
            if (Player.player.life.vision > 0)
            {
                tickIsHallucinating(Time.deltaTime);
            }
        }
    }

    internal void InitializePlayer()
    {
        isLocked = false;
        inputWantsCustomModal = false;
        usingCustomModal = false;
        chat = EChatMode.GLOBAL;
        window = new SleekWindow();
        if (Player.player.channel.owner.playerID.BypassIntegrityChecks)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.SizeOffset_X = 200f;
            sleekLabel.SizeOffset_Y = 30f;
            sleekLabel.PositionOffset_X = -100f;
            sleekLabel.PositionOffset_Y = -15f;
            sleekLabel.PositionScale_X = 0.5f;
            sleekLabel.PositionScale_Y = 0.2f;
            sleekLabel.TextColor = ESleekTint.BAD;
            sleekLabel.Text = "Bypassing integrity checks";
            sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            window.AddChild(sleekLabel);
        }
        colorOverlayImage = Glazier.Get().CreateImage();
        colorOverlayImage.SizeScale_X = 1f;
        colorOverlayImage.SizeScale_Y = 1f;
        colorOverlayImage.Texture = (Texture2D)GlazierResources.PixelTexture;
        colorOverlayImage.TintColor = new Color(0f, 0f, 0f, 0f);
        window.AddChild(colorOverlayImage);
        container = Glazier.Get().CreateFrame();
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        window.AddChild(container);
        wantsWindowEnabled = true;
        isWindowEnabledByColorOverlay = false;
        OptionsSettings.apply();
        GraphicsSettings.apply("loaded player");
        groupUI = new PlayerGroupUI();
        groupUI.SizeScale_X = 1f;
        groupUI.SizeScale_Y = 1f;
        container.AddChild(groupUI);
        dashboardUI = new PlayerDashboardUI();
        pauseUI = new PlayerPauseUI();
        lifeUI = new PlayerLifeUI();
        new PlayerDeathUI();
        new PlayerBarricadeSignUI();
        boomboxUI = new PlayerBarricadeStereoUI();
        container.AddChild(boomboxUI);
        new PlayerBarricadeLibraryUI();
        mannequinUI = new PlayerBarricadeMannequinUI();
        container.AddChild(mannequinUI);
        browserRequestUI = new PlayerBrowserRequestUI();
        container.AddChild(browserRequestUI);
        new PlayerNPCDialogueUI();
        new PlayerNPCQuestUI();
        new PlayerNPCVendorUI();
        new PlayerWorkzoneUI();
        PlayerLifeUI.UpdateTrackedQuest();
        messagePlayer = null;
        messageBox = Glazier.Get().CreateBox();
        messageBox.PositionOffset_X = -200f;
        messageBox.PositionScale_X = 0.5f;
        messageBox.PositionScale_Y = 1f;
        messageBox.SizeOffset_X = 400f;
        container.AddChild(messageBox);
        messageBox.IsVisible = false;
        messageLabel = Glazier.Get().CreateLabel();
        messageLabel.PositionOffset_X = 5f;
        messageLabel.PositionOffset_Y = 5f;
        messageLabel.SizeOffset_X = -10f;
        messageLabel.SizeOffset_Y = 40f;
        messageLabel.SizeScale_X = 1f;
        messageLabel.FontSize = ESleekFontSize.Medium;
        messageBox.AddChild(messageLabel);
        messageIcon_0 = Glazier.Get().CreateImage();
        messageIcon_0.PositionOffset_X = 5f;
        messageIcon_0.PositionOffset_Y = 45f;
        messageIcon_0.SizeOffset_X = 20f;
        messageIcon_0.SizeOffset_Y = 20f;
        messageBox.AddChild(messageIcon_0);
        messageIcon_0.IsVisible = false;
        messageIcon_1 = Glazier.Get().CreateImage();
        messageIcon_1.PositionOffset_X = 5f;
        messageIcon_1.PositionOffset_Y = 75f;
        messageIcon_1.SizeOffset_X = 20f;
        messageIcon_1.SizeOffset_Y = 20f;
        messageBox.AddChild(messageIcon_1);
        messageIcon_1.IsVisible = false;
        messageIcon_2 = Glazier.Get().CreateImage();
        messageIcon_2.PositionOffset_X = 5f;
        messageIcon_2.PositionOffset_Y = 105f;
        messageIcon_2.SizeOffset_X = 20f;
        messageIcon_2.SizeOffset_Y = 20f;
        messageBox.AddChild(messageIcon_2);
        messageIcon_2.IsVisible = false;
        messageProgress_0 = new SleekProgress("");
        messageProgress_0.PositionOffset_X = 30f;
        messageProgress_0.PositionOffset_Y = 50f;
        messageProgress_0.SizeOffset_X = -40f;
        messageProgress_0.SizeOffset_Y = 10f;
        messageProgress_0.SizeScale_X = 1f;
        messageBox.AddChild(messageProgress_0);
        messageProgress_0.IsVisible = false;
        messageProgress_1 = new SleekProgress("");
        messageProgress_1.PositionOffset_X = 30f;
        messageProgress_1.PositionOffset_Y = 80f;
        messageProgress_1.SizeOffset_X = -40f;
        messageProgress_1.SizeOffset_Y = 10f;
        messageProgress_1.SizeScale_X = 1f;
        messageBox.AddChild(messageProgress_1);
        messageProgress_1.IsVisible = false;
        messageProgress_2 = new SleekProgress("");
        messageProgress_2.PositionOffset_X = 30f;
        messageProgress_2.PositionOffset_Y = 110f;
        messageProgress_2.SizeOffset_X = -40f;
        messageProgress_2.SizeOffset_Y = 10f;
        messageProgress_2.SizeScale_X = 1f;
        messageBox.AddChild(messageProgress_2);
        messageProgress_2.IsVisible = false;
        messageQualityImage = Glazier.Get().CreateImage(PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_0"));
        messageQualityImage.PositionOffset_X = -30f;
        messageQualityImage.PositionOffset_Y = -30f;
        messageQualityImage.PositionScale_X = 1f;
        messageQualityImage.PositionScale_Y = 1f;
        messageQualityImage.SizeOffset_X = 20f;
        messageQualityImage.SizeOffset_Y = 20f;
        messageBox.AddChild(messageQualityImage);
        messageQualityImage.IsVisible = false;
        messageAmountLabel = Glazier.Get().CreateLabel();
        messageAmountLabel.PositionOffset_X = 10f;
        messageAmountLabel.PositionOffset_Y = -40f;
        messageAmountLabel.PositionScale_Y = 1f;
        messageAmountLabel.SizeOffset_X = -20f;
        messageAmountLabel.SizeOffset_Y = 30f;
        messageAmountLabel.SizeScale_X = 1f;
        messageAmountLabel.TextAlignment = TextAnchor.LowerLeft;
        messageAmountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
        messageBox.AddChild(messageAmountLabel);
        messageAmountLabel.IsVisible = false;
        messageBox2 = Glazier.Get().CreateBox();
        messageBox2.PositionOffset_X = -200f;
        messageBox2.PositionScale_X = 0.5f;
        messageBox2.PositionScale_Y = 1f;
        messageBox2.SizeOffset_X = 400f;
        container.AddChild(messageBox2);
        messageBox2.IsVisible = false;
        messageLabel2 = Glazier.Get().CreateLabel();
        messageLabel2.PositionOffset_X = 5f;
        messageLabel2.PositionOffset_Y = 5f;
        messageLabel2.SizeOffset_X = -10f;
        messageLabel2.SizeOffset_Y = 40f;
        messageLabel2.SizeScale_X = 1f;
        messageLabel2.FontSize = ESleekFontSize.Medium;
        messageBox2.AddChild(messageLabel2);
        messageIcon2 = Glazier.Get().CreateImage();
        messageIcon2.PositionOffset_X = 5f;
        messageIcon2.PositionOffset_Y = 75f;
        messageIcon2.SizeOffset_X = 20f;
        messageIcon2.SizeOffset_Y = 20f;
        messageBox2.AddChild(messageIcon2);
        messageIcon2.IsVisible = false;
        messageProgress2_0 = new SleekProgress("");
        messageProgress2_0.PositionOffset_X = 5f;
        messageProgress2_0.PositionOffset_Y = 50f;
        messageProgress2_0.SizeOffset_X = -10f;
        messageProgress2_0.SizeOffset_Y = 10f;
        messageProgress2_0.SizeScale_X = 1f;
        messageBox2.AddChild(messageProgress2_0);
        messageProgress2_1 = new SleekProgress("");
        messageProgress2_1.PositionOffset_X = 30f;
        messageProgress2_1.PositionOffset_Y = 80f;
        messageProgress2_1.SizeOffset_X = -40f;
        messageProgress2_1.SizeOffset_Y = 10f;
        messageProgress2_1.SizeScale_X = 1f;
        messageBox2.AddChild(messageProgress2_1);
        painAlpha = 0f;
        stunAlpha = 0f;
        isBlindfolded = false;
        PlayerLife life = Player.player.life;
        life.onVisionUpdated = (VisionUpdated)Delegate.Combine(life.onVisionUpdated, new VisionUpdated(onVisionUpdated));
        PlayerLife life2 = Player.player.life;
        life2.onLifeUpdated = (LifeUpdated)Delegate.Combine(life2.onLifeUpdated, new LifeUpdated(onLifeUpdated));
        onLifeUpdated(Player.player.life.isDead);
        PlayerClothing clothing = Player.player.clothing;
        clothing.onGlassesUpdated = (GlassesUpdated)Delegate.Combine(clothing.onGlassesUpdated, new GlassesUpdated(onGlassesUpdated));
        LightingManager.onMoonUpdated = (MoonUpdated)Delegate.Combine(LightingManager.onMoonUpdated, new MoonUpdated(onMoonUpdated));
        menuBlurFX = GetComponent<BlurEffect>();
        hallucinationReverbZone = GetComponent<AudioReverbZone>();
    }

    private void OnDestroy()
    {
        if (window != null)
        {
            if (dashboardUI != null)
            {
                dashboardUI.OnDestroy();
            }
            if (pauseUI != null)
            {
                pauseUI.OnDestroy();
            }
            if (lifeUI != null)
            {
                lifeUI.OnDestroy();
            }
            if (!Provider.isApplicationQuitting)
            {
                window.InternalDestroy();
            }
            window = null;
            setIsHallucinating(isHallucinating: false);
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (OptionsSettings.pauseWhenUnfocused && window != null && !focus)
        {
            escapeMenu();
            if (!PlayerPauseUI.active)
            {
                escapeMenu();
            }
        }
    }
}
