using System;
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

    private static bool isFlashbanged;

    public static EChatMode chat;

    private static StaticResourceRef<Texture2D> hitEntityTexture = new StaticResourceRef<Texture2D>("Bundles/Textures/Player/Icons/PlayerLife/Hit_Entity");

    private static StaticResourceRef<Texture2D> hitCriticalTexture = new StaticResourceRef<Texture2D>("Bundles/Textures/Player/Icons/PlayerLife/Hit_Critical");

    private static StaticResourceRef<Texture2D> hitBuildTexture = new StaticResourceRef<Texture2D>("Bundles/Textures/Player/Icons/PlayerLife/Hit_Build");

    private static StaticResourceRef<Texture2D> hitGhostTexture = new StaticResourceRef<Texture2D>("Bundles/Textures/Player/Icons/PlayerLife/Hit_Ghost");

    private static StaticResourceRef<AudioClip> hitCriticalSound = new StaticResourceRef<AudioClip>("Sounds/General/Hit");

    internal static PlayerUI instance;

    internal PlayerGroupUI groupUI;

    private PlayerDashboardUI dashboardUI;

    private PlayerLifeUI lifeUI;

    internal PlayerBarricadeStereoUI boomboxUI;

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
        if (!isFlashbanged)
        {
            isFlashbanged = true;
            UpdateWindowEnabled();
        }
    }

    public static void pain(float amount)
    {
        painAlpha = amount * 0.75f;
    }

    public static void hitmark(int index, Vector3 point, bool worldspace, EPlayerHit newHit)
    {
        if (wantsWindowEnabled && index >= 0 && index < PlayerLifeUI.hitmarkers.Length && Provider.modeConfigData.Gameplay.Hitmarkers)
        {
            HitmarkerInfo hitmarkerInfo = PlayerLifeUI.hitmarkers[index];
            hitmarkerInfo.lastHit = Time.realtimeSinceStartup;
            hitmarkerInfo.hit = newHit;
            hitmarkerInfo.point = point;
            hitmarkerInfo.worldspace = worldspace || OptionsSettings.hitmarker;
            if (newHit == EPlayerHit.CRITICAL)
            {
                MainCamera.instance.GetComponent<AudioSource>().PlayOneShot(hitCriticalSound, 0.5f);
            }
            Texture2D texture;
            Color color;
            switch (newHit)
            {
            default:
                return;
            case EPlayerHit.ENTITIY:
                texture = hitEntityTexture;
                color = OptionsSettings.hitmarkerColor;
                break;
            case EPlayerHit.CRITICAL:
                texture = hitCriticalTexture;
                color = OptionsSettings.criticalHitmarkerColor;
                break;
            case EPlayerHit.BUILD:
                texture = hitBuildTexture;
                color = OptionsSettings.hitmarkerColor;
                break;
            case EPlayerHit.GHOST:
                texture = hitGhostTexture;
                color = OptionsSettings.hitmarkerColor;
                break;
            case EPlayerHit.NONE:
                return;
            }
            hitmarkerInfo.image.texture = texture;
            hitmarkerInfo.image.color = color;
        }
    }

    public static void enableDot()
    {
        PlayerLifeUI.dotImage.isVisible = true;
    }

    public static void disableDot()
    {
        PlayerLifeUI.dotImage.isVisible = false;
    }

    public static void updateScope(bool isScoped)
    {
        PlayerLifeUI.scopeOverlay.isVisible = isScoped;
        container.isVisible = !isScoped;
        UpdateWindowEnabled();
    }

    public static void updateBinoculars(bool isBinoculars)
    {
        PlayerLifeUI.binocularsOverlay.isVisible = isBinoculars;
        container.isVisible = !isBinoculars;
        UpdateWindowEnabled();
    }

    private static void UpdateWindowEnabled()
    {
        window.isEnabled = wantsWindowEnabled || PlayerLifeUI.scopeOverlay.isVisible || PlayerLifeUI.binocularsOverlay.isVisible || isBlindfolded || isFlashbanged;
    }

    public static void resetCrosshair()
    {
        if (Provider.modeConfigData.Gameplay.Crosshair)
        {
            PlayerLifeUI.crosshairLeftImage.positionOffset_X = -4;
            PlayerLifeUI.crosshairLeftImage.positionOffset_Y = -4;
            PlayerLifeUI.crosshairRightImage.positionOffset_X = -4;
            PlayerLifeUI.crosshairRightImage.positionOffset_Y = -4;
            PlayerLifeUI.crosshairDownImage.positionOffset_X = -4;
            PlayerLifeUI.crosshairDownImage.positionOffset_Y = -4;
            PlayerLifeUI.crosshairUpImage.positionOffset_X = -4;
            PlayerLifeUI.crosshairUpImage.positionOffset_Y = -4;
        }
    }

    public static void updateCrosshair(float spread)
    {
        if (Provider.modeConfigData.Gameplay.Crosshair)
        {
            PlayerLifeUI.crosshairLeftImage.lerpPositionOffset((int)((0f - spread) * 400f) - 4, -4, ESleekLerp.EXPONENTIAL, 10f);
            PlayerLifeUI.crosshairRightImage.lerpPositionOffset((int)(spread * 400f) - 4, -4, ESleekLerp.EXPONENTIAL, 10f);
            PlayerLifeUI.crosshairDownImage.lerpPositionOffset(-4, (int)(spread * 400f) - 4, ESleekLerp.EXPONENTIAL, 10f);
            PlayerLifeUI.crosshairUpImage.lerpPositionOffset(-4, (int)((0f - spread) * 400f) - 4, ESleekLerp.EXPONENTIAL, 10f);
        }
    }

    public static void enableCrosshair()
    {
        if (Provider.modeConfigData.Gameplay.Crosshair)
        {
            PlayerLifeUI.crosshairLeftImage.isVisible = true;
            PlayerLifeUI.crosshairRightImage.isVisible = true;
            PlayerLifeUI.crosshairDownImage.isVisible = true;
            PlayerLifeUI.crosshairUpImage.isVisible = true;
        }
    }

    public static void disableCrosshair()
    {
        if (Provider.modeConfigData.Gameplay.Crosshair)
        {
            PlayerLifeUI.crosshairLeftImage.isVisible = false;
            PlayerLifeUI.crosshairRightImage.isVisible = false;
            PlayerLifeUI.crosshairDownImage.isVisible = false;
            PlayerLifeUI.crosshairUpImage.isVisible = false;
        }
    }

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
                    messagePlayer.positionOffset_X = -150;
                    messagePlayer.positionOffset_Y = -130;
                    messagePlayer.positionScale_X = 0.5f;
                    messagePlayer.positionScale_Y = 1f;
                    messagePlayer.sizeOffset_X = 300;
                    messagePlayer.sizeOffset_Y = 50;
                    container.AddChild(messagePlayer);
                }
            }
            messageBox.isVisible = false;
            if (messagePlayer != null)
            {
                messagePlayer.isVisible = true;
            }
            return;
        }
        messageBox.isVisible = true;
        if (messagePlayer != null)
        {
            messagePlayer.isVisible = false;
        }
        messageIcon_0.positionOffset_Y = 45;
        messageProgress_0.positionOffset_Y = 50;
        messageIcon_1.positionOffset_Y = 75;
        messageProgress_1.positionOffset_Y = 80;
        messageIcon_2.positionOffset_Y = 105;
        messageProgress_2.positionOffset_Y = 110;
        switch (message)
        {
        case EPlayerMessage.VEHICLE_ENTER:
        {
            InteractableVehicle interactableVehicle = (InteractableVehicle)PlayerInteract.interactable;
            int num2 = 45;
            bool flag = interactableVehicle.usesFuel || interactableVehicle.asset.isStaminaPowered;
            messageIcon_0.isVisible = flag;
            messageProgress_0.isVisible = flag;
            if (flag)
            {
                messageIcon_0.positionOffset_Y = num2;
                messageProgress_0.positionOffset_Y = num2 + 5;
                num2 += 30;
            }
            messageIcon_1.isVisible = interactableVehicle.usesHealth;
            messageProgress_1.isVisible = interactableVehicle.usesHealth;
            if (interactableVehicle.usesHealth)
            {
                messageIcon_1.positionOffset_Y = num2;
                messageProgress_1.positionOffset_Y = num2 + 5;
                num2 += 30;
            }
            messageIcon_2.isVisible = interactableVehicle.usesBattery;
            messageProgress_2.isVisible = interactableVehicle.usesBattery;
            if (interactableVehicle.usesBattery)
            {
                messageIcon_2.positionOffset_Y = num2;
                messageProgress_2.positionOffset_Y = num2 + 5;
                num2 += 30;
            }
            messageBox.sizeOffset_Y = num2 - 5;
            if (flag)
            {
                interactableVehicle.getDisplayFuel(out var currentFuel, out var maxFuel);
                messageProgress_0.state = (float)(int)currentFuel / (float)(int)maxFuel;
                messageProgress_0.color = Palette.COLOR_Y;
                messageIcon_0.texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
            }
            if (interactableVehicle.usesHealth)
            {
                messageProgress_1.state = (float)(int)interactableVehicle.health / (float)(int)interactableVehicle.asset.health;
                messageProgress_1.color = Palette.COLOR_R;
                messageIcon_1.texture = PlayerLifeUI.icons.load<Texture2D>("Health");
            }
            if (interactableVehicle.usesBattery)
            {
                messageProgress_2.state = (float)(int)interactableVehicle.batteryCharge / 10000f;
                messageProgress_2.color = Palette.COLOR_Y;
                messageIcon_2.texture = PlayerLifeUI.icons.load<Texture2D>("Stamina");
            }
            messageQualityImage.isVisible = false;
            messageAmountLabel.isVisible = false;
            break;
        }
        case EPlayerMessage.GROW:
        case EPlayerMessage.GENERATOR_ON:
        case EPlayerMessage.GENERATOR_OFF:
        case EPlayerMessage.VOLUME_WATER:
        case EPlayerMessage.VOLUME_FUEL:
            messageBox.sizeOffset_Y = 70;
            messageProgress_0.isVisible = true;
            messageIcon_0.isVisible = true;
            messageProgress_1.isVisible = false;
            messageIcon_1.isVisible = false;
            messageProgress_2.isVisible = false;
            messageIcon_2.isVisible = false;
            switch (message)
            {
            case EPlayerMessage.GENERATOR_ON:
            case EPlayerMessage.GENERATOR_OFF:
            {
                InteractableGenerator interactableGenerator = (InteractableGenerator)PlayerInteract.interactable;
                messageProgress_0.state = (float)(int)interactableGenerator.fuel / (float)(int)interactableGenerator.capacity;
                messageIcon_0.texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
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
                messageIcon_0.texture = PlayerLifeUI.icons.load<Texture2D>("Grow");
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
                messageIcon_0.texture = PlayerLifeUI.icons.load<Texture2D>("Water");
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
                messageIcon_0.texture = PlayerLifeUI.icons.load<Texture2D>("Fuel");
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
            messageQualityImage.isVisible = false;
            messageAmountLabel.isVisible = false;
            break;
        case EPlayerMessage.ITEM:
            messageBox.sizeOffset_Y = 70;
            if (objects.Length == 2)
            {
                if (((ItemAsset)objects[1]).showQuality)
                {
                    messageQualityImage.color = ItemTool.getQualityColor((float)(int)((Item)objects[0]).quality / 100f);
                    messageAmountLabel.text = ((Item)objects[0]).quality + "%";
                    messageAmountLabel.textColor = messageQualityImage.color;
                    messageQualityImage.isVisible = true;
                    messageAmountLabel.isVisible = true;
                }
                else if (((ItemAsset)objects[1]).amount > 1)
                {
                    messageAmountLabel.text = "x" + ((Item)objects[0]).amount;
                    messageAmountLabel.textColor = ESleekTint.FONT;
                    messageQualityImage.isVisible = false;
                    messageAmountLabel.isVisible = true;
                }
                else
                {
                    messageQualityImage.isVisible = false;
                    messageAmountLabel.isVisible = false;
                }
            }
            messageProgress_0.isVisible = false;
            messageIcon_0.isVisible = false;
            messageProgress_1.isVisible = false;
            messageIcon_1.isVisible = false;
            messageProgress_2.isVisible = false;
            messageIcon_2.isVisible = false;
            break;
        default:
            messageBox.sizeOffset_Y = 50;
            messageQualityImage.isVisible = false;
            messageAmountLabel.isVisible = false;
            messageProgress_0.isVisible = false;
            messageIcon_0.isVisible = false;
            messageProgress_1.isVisible = false;
            messageIcon_1.isVisible = false;
            messageProgress_2.isVisible = false;
            messageIcon_2.isVisible = false;
            break;
        }
        bool flag2 = message == EPlayerMessage.ITEM || message == EPlayerMessage.VEHICLE_ENTER;
        if (flag2)
        {
            messageBox.backgroundColor = SleekColor.BackgroundIfLight(color);
        }
        else
        {
            messageBox.backgroundColor = ESleekTint.BACKGROUND;
        }
        messageLabel.enableRichText = message == EPlayerMessage.CONDITION || message == EPlayerMessage.TALK || message == EPlayerMessage.INTERACT;
        if (messageLabel.enableRichText)
        {
            messageLabel.textColor = ESleekTint.RICH_TEXT_DEFAULT;
            messageLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        }
        else if (flag2)
        {
            messageLabel.textColor = color;
            messageLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        }
        else
        {
            messageLabel.textColor = ESleekTint.FONT;
            messageLabel.shadowStyle = ETextContrastContext.Default;
        }
        messageBox.sizeOffset_X = 200;
        switch (message)
        {
        case EPlayerMessage.ITEM:
            messageBox.sizeOffset_X = 300;
            messageLabel.text = PlayerLifeUI.localization.format("Item", text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.VEHICLE_ENTER:
        {
            messageBox.sizeOffset_X = 300;
            InteractableVehicle interactableVehicle2 = (InteractableVehicle)PlayerInteract.interactable;
            messageLabel.text = PlayerLifeUI.localization.format(interactableVehicle2.isLocked ? "Vehicle_Enter_Locked" : "Vehicle_Enter_Unlocked", text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        }
        case EPlayerMessage.DOOR_OPEN:
            messageLabel.text = PlayerLifeUI.localization.format("Door_Open", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.DOOR_CLOSE:
            messageLabel.text = PlayerLifeUI.localization.format("Door_Close", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.LOCKED:
            messageLabel.text = PlayerLifeUI.localization.format("Locked");
            break;
        case EPlayerMessage.BLOCKED:
            messageLabel.text = PlayerLifeUI.localization.format("Blocked");
            break;
        case EPlayerMessage.PLACEMENT_OBSTRUCTED_BY:
            messageLabel.text = PlayerLifeUI.localization.format("PlacementObstructedBy", text);
            break;
        case EPlayerMessage.PILLAR:
            messageLabel.text = PlayerLifeUI.localization.format("Pillar");
            break;
        case EPlayerMessage.POST:
            messageLabel.text = PlayerLifeUI.localization.format("Post");
            break;
        case EPlayerMessage.ROOF:
            messageLabel.text = PlayerLifeUI.localization.format("Roof");
            break;
        case EPlayerMessage.WALL:
            messageLabel.text = PlayerLifeUI.localization.format("Wall");
            break;
        case EPlayerMessage.CORNER:
            messageLabel.text = PlayerLifeUI.localization.format("Corner");
            break;
        case EPlayerMessage.GROUND:
            messageLabel.text = PlayerLifeUI.localization.format("Ground");
            break;
        case EPlayerMessage.DOORWAY:
            messageLabel.text = PlayerLifeUI.localization.format("Doorway");
            break;
        case EPlayerMessage.WINDOW:
            messageLabel.text = PlayerLifeUI.localization.format("Window");
            break;
        case EPlayerMessage.GARAGE:
            messageLabel.text = PlayerLifeUI.localization.format("Garage");
            break;
        case EPlayerMessage.BED_ON:
            messageLabel.text = PlayerLifeUI.localization.format("Bed_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), text);
            break;
        case EPlayerMessage.BED_OFF:
            messageLabel.text = PlayerLifeUI.localization.format("Bed_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), text);
            break;
        case EPlayerMessage.BED_CLAIMED:
            messageLabel.text = PlayerLifeUI.localization.format("Bed_Claimed");
            break;
        case EPlayerMessage.BOUNDS:
            messageLabel.text = PlayerLifeUI.localization.format("Bounds");
            break;
        case EPlayerMessage.STORAGE:
            messageLabel.text = PlayerLifeUI.localization.format("Storage", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.FARM:
            messageLabel.text = PlayerLifeUI.localization.format("Farm", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.GROW:
            messageLabel.text = PlayerLifeUI.localization.format("Grow");
            break;
        case EPlayerMessage.SOIL:
            messageLabel.text = PlayerLifeUI.localization.format("Soil");
            break;
        case EPlayerMessage.FIRE_ON:
            messageLabel.text = PlayerLifeUI.localization.format("Fire_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.FIRE_OFF:
            messageLabel.text = PlayerLifeUI.localization.format("Fire_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.FORAGE:
            messageLabel.text = PlayerLifeUI.localization.format("Forage", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.GENERATOR_ON:
            messageLabel.text = PlayerLifeUI.localization.format("Generator_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.GENERATOR_OFF:
            messageLabel.text = PlayerLifeUI.localization.format("Generator_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.SPOT_ON:
            messageLabel.text = PlayerLifeUI.localization.format("Spot_On", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.SPOT_OFF:
            messageLabel.text = PlayerLifeUI.localization.format("Spot_Off", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.PURCHASE:
            if (objects.Length == 2)
            {
                messageLabel.text = PlayerLifeUI.localization.format("Purchase", objects[0], objects[1], MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            }
            break;
        case EPlayerMessage.POWER:
            messageLabel.text = PlayerLifeUI.localization.format("Power");
            break;
        case EPlayerMessage.USE:
            messageLabel.text = PlayerLifeUI.localization.format("Use", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.TUTORIAL_MOVE:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Move", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.left), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.right), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.up), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.down));
            break;
        case EPlayerMessage.TUTORIAL_LOOK:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Look");
            break;
        case EPlayerMessage.TUTORIAL_JUMP:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Jump", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.jump));
            break;
        case EPlayerMessage.TUTORIAL_PERSPECTIVE:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Perspective", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.perspective));
            break;
        case EPlayerMessage.TUTORIAL_RUN:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Run", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.sprint));
            break;
        case EPlayerMessage.TUTORIAL_INVENTORY:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Inventory", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.TUTORIAL_SURVIVAL:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Survival", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.inventory), MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Mouse1));
            break;
        case EPlayerMessage.TUTORIAL_GUN:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Gun", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
            break;
        case EPlayerMessage.TUTORIAL_LADDER:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Ladder");
            break;
        case EPlayerMessage.TUTORIAL_CRAFT:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Craft", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.attach), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crafting));
            break;
        case EPlayerMessage.TUTORIAL_SKILLS:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Skills", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.skills));
            break;
        case EPlayerMessage.TUTORIAL_SWIM:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Swim", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.jump));
            break;
        case EPlayerMessage.TUTORIAL_MEDICAL:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Medical", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
            break;
        case EPlayerMessage.TUTORIAL_VEHICLE:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Vehicle", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary), MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.TUTORIAL_CROUCH:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Crouch", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crouch));
            break;
        case EPlayerMessage.TUTORIAL_PRONE:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Prone", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.prone));
            break;
        case EPlayerMessage.TUTORIAL_EDUCATED:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Educated", MenuConfigurationControlsUI.getKeyCodeText(KeyCode.Escape));
            break;
        case EPlayerMessage.TUTORIAL_HARVEST:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Harvest", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.TUTORIAL_FISH:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Fish", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
            break;
        case EPlayerMessage.TUTORIAL_BUILD:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Build");
            break;
        case EPlayerMessage.TUTORIAL_HORN:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Horn", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.primary));
            break;
        case EPlayerMessage.TUTORIAL_LIGHTS:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Lights", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.secondary));
            break;
        case EPlayerMessage.TUTORIAL_SIRENS:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Sirens", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.other));
            break;
        case EPlayerMessage.TUTORIAL_FARM:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Farm");
            break;
        case EPlayerMessage.TUTORIAL_POWER:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Power");
            break;
        case EPlayerMessage.TUTORIAL_FIRE:
            messageBox.sizeOffset_X = 600;
            messageLabel.text = PlayerLifeUI.localization.format("Tutorial_Fire", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.crafting));
            break;
        case EPlayerMessage.CLAIM:
            messageLabel.text = PlayerLifeUI.localization.format("Claim");
            break;
        case EPlayerMessage.UNDERWATER:
            messageLabel.text = PlayerLifeUI.localization.format("Underwater");
            break;
        case EPlayerMessage.NAV:
            messageLabel.text = PlayerLifeUI.localization.format("Nav");
            break;
        case EPlayerMessage.SPAWN:
            messageLabel.text = PlayerLifeUI.localization.format("Spawn");
            break;
        case EPlayerMessage.MOBILE:
            messageLabel.text = PlayerLifeUI.localization.format("Mobile");
            break;
        case EPlayerMessage.BUILD_ON_OCCUPIED_VEHICLE:
            messageLabel.text = PlayerLifeUI.localization.format("Build_On_Occupied_Vehicle");
            break;
        case EPlayerMessage.NOT_ALLOWED_HERE:
            messageLabel.text = PlayerLifeUI.localization.format("Not_Allowed_Here");
            break;
        case EPlayerMessage.CANNOT_BUILD_ON_VEHICLE:
            messageLabel.text = PlayerLifeUI.localization.format("Cannot_Build_On_Vehicle");
            break;
        case EPlayerMessage.TOO_FAR_FROM_HULL:
            messageLabel.text = PlayerLifeUI.localization.format("Too_Far_From_Hull");
            break;
        case EPlayerMessage.CANNOT_BUILD_WHILE_SEATED:
            messageLabel.text = PlayerLifeUI.localization.format("Cannot_Build_While_Seated");
            break;
        case EPlayerMessage.OIL:
            messageLabel.text = PlayerLifeUI.localization.format("Oil");
            break;
        case EPlayerMessage.VOLUME_WATER:
            messageLabel.text = PlayerLifeUI.localization.format("Volume_Water", text);
            break;
        case EPlayerMessage.VOLUME_FUEL:
            messageLabel.text = PlayerLifeUI.localization.format("Volume_Fuel");
            break;
        case EPlayerMessage.TRAPDOOR:
            messageLabel.text = PlayerLifeUI.localization.format("Trapdoor");
            break;
        case EPlayerMessage.TALK:
        {
            InteractableObjectNPC interactableObjectNPC = PlayerInteract.interactable as InteractableObjectNPC;
            string arg = ((interactableObjectNPC != null && interactableObjectNPC.npcAsset != null) ? interactableObjectNPC.npcAsset.npcName : "null");
            messageLabel.text = PlayerLifeUI.localization.format("Talk", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact), arg);
            break;
        }
        case EPlayerMessage.CONDITION:
            messageLabel.text = text;
            break;
        case EPlayerMessage.INTERACT:
            messageLabel.text = string.Format(text, MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        case EPlayerMessage.SAFEZONE:
            messageLabel.text = PlayerLifeUI.localization.format("Safezone");
            break;
        case EPlayerMessage.CLIMB:
            messageLabel.text = PlayerLifeUI.localization.format("Climb", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
            break;
        }
        messageBox.positionOffset_X = -messageBox.sizeOffset_X / 2;
        if (transform != null && MainCamera.instance != null)
        {
            messageBox.positionOffset_Y = 10;
            Vector3 vector = MainCamera.instance.WorldToViewportPoint(transform.position);
            Vector2 vector2 = container.ViewportToNormalizedPosition(vector);
            messageBox.positionScale_X = vector2.x;
            messageBox.positionScale_Y = vector2.y;
        }
        else
        {
            if (messageBox2.isVisible)
            {
                messageBox.positionOffset_Y = -80 - messageBox.sizeOffset_Y - 10 - messageBox2.sizeOffset_Y;
            }
            else
            {
                messageBox.positionOffset_Y = -80 - messageBox.sizeOffset_Y;
            }
            messageBox.positionScale_X = 0.5f;
            messageBox.positionScale_Y = 1f;
        }
    }

    public static void hint2(EPlayerMessage message, float progress, float data)
    {
        if (messageBox2 != null && PlayerLifeUI.localization != null && !ShouldIgnoreHintAndMessageRequests() && !isMessaged)
        {
            messageBox2.isVisible = true;
            lastHinted2 = true;
            isHinted2 = true;
            if (message == EPlayerMessage.SALVAGE)
            {
                messageBox2.sizeOffset_Y = 100;
                messageBox2.positionOffset_Y = -80 - messageBox2.sizeOffset_Y;
                messageIcon2.isVisible = true;
                messageProgress2_0.isVisible = true;
                messageProgress2_1.isVisible = true;
                messageIcon2.texture = PlayerLifeUI.icons.load<Texture2D>("Health");
                messageLabel2.text = PlayerLifeUI.localization.format("Salvage", ControlsSettings.interact);
                messageProgress2_0.state = progress;
                messageProgress2_0.color = Palette.COLOR_P;
                messageProgress2_1.state = data;
                messageProgress2_1.color = Palette.COLOR_R;
            }
        }
    }

    public static void message(EPlayerMessage message, string text, float duration = 2f)
    {
        if (messageBox2 == null || PlayerLifeUI.localization == null || (!OptionsSettings.hints && message != EPlayerMessage.EXPERIENCE && message != EPlayerMessage.MOON_ON && message != EPlayerMessage.MOON_OFF && message != EPlayerMessage.SAFEZONE_ON && message != EPlayerMessage.SAFEZONE_OFF && message != EPlayerMessage.WAVE_ON && message != EPlayerMessage.MOON_OFF && message != EPlayerMessage.DEADZONE_ON && message != EPlayerMessage.DEADZONE_OFF && message != EPlayerMessage.REPUTATION && message != EPlayerMessage.NPC_CUSTOM))
        {
            return;
        }
        if (message == EPlayerMessage.NONE)
        {
            messageBox2.isVisible = false;
            messageDisappearTime = 0f;
            isMessaged = false;
        }
        else if (!ShouldIgnoreHintAndMessageRequests() && ((message != EPlayerMessage.EXPERIENCE && message != EPlayerMessage.REPUTATION) || (!PlayerNPCDialogueUI.active && !PlayerNPCQuestUI.active && !PlayerNPCVendorUI.active)))
        {
            messageBox2.positionOffset_X = -200;
            messageBox2.sizeOffset_X = 400;
            messageBox2.sizeOffset_Y = 50;
            messageBox2.positionOffset_Y = -80 - messageBox2.sizeOffset_Y;
            messageBox2.isVisible = true;
            messageIcon2.isVisible = false;
            messageProgress2_0.isVisible = false;
            messageProgress2_1.isVisible = false;
            messageDisappearTime = Time.realtimeSinceStartup + duration;
            isMessaged = true;
            if (message == EPlayerMessage.SPACE)
            {
                messageLabel2.text = PlayerLifeUI.localization.format("Space");
            }
            switch (message)
            {
            case EPlayerMessage.RELOAD:
                messageLabel2.text = PlayerLifeUI.localization.format("Reload", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.reload));
                break;
            case EPlayerMessage.SAFETY:
                messageLabel2.text = PlayerLifeUI.localization.format("Safety", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.firemode));
                break;
            case EPlayerMessage.VEHICLE_EXIT:
                messageLabel2.text = PlayerLifeUI.localization.format("Vehicle_Exit", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.interact));
                break;
            case EPlayerMessage.VEHICLE_SWAP:
                messageLabel2.text = PlayerLifeUI.localization.format("Vehicle_Swap", Player.player.movement.getVehicle().passengers.Length);
                break;
            case EPlayerMessage.LIGHT:
                messageLabel2.text = PlayerLifeUI.localization.format("Light", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
                break;
            case EPlayerMessage.LASER:
                messageLabel2.text = PlayerLifeUI.localization.format("Laser", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
                break;
            case EPlayerMessage.HOUSING_PLANNER_TUTORIAL:
                messageLabel2.text = PlayerLifeUI.localization.format("HousingPlannerTutorial", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.attach));
                break;
            case EPlayerMessage.RANGEFINDER:
                messageLabel2.text = PlayerLifeUI.localization.format("Rangefinder", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
                break;
            case EPlayerMessage.EXPERIENCE:
                messageLabel2.text = PlayerLifeUI.localization.format("Experience", text);
                break;
            case EPlayerMessage.EMPTY:
                messageLabel2.text = PlayerLifeUI.localization.format("Empty");
                break;
            case EPlayerMessage.FULL:
                messageLabel2.text = PlayerLifeUI.localization.format("Full");
                break;
            case EPlayerMessage.MOON_ON:
                messageLabel2.text = PlayerLifeUI.localization.format("Moon_On");
                break;
            case EPlayerMessage.MOON_OFF:
                messageLabel2.text = PlayerLifeUI.localization.format("Moon_Off");
                break;
            case EPlayerMessage.SAFEZONE_ON:
                messageLabel2.text = PlayerLifeUI.localization.format("Safezone_On");
                break;
            case EPlayerMessage.SAFEZONE_OFF:
                messageLabel2.text = PlayerLifeUI.localization.format("Safezone_Off");
                break;
            case EPlayerMessage.WAVE_ON:
                messageLabel2.text = PlayerLifeUI.localization.format("Wave_On");
                break;
            case EPlayerMessage.WAVE_OFF:
                messageLabel2.text = PlayerLifeUI.localization.format("Wave_Off");
                break;
            case EPlayerMessage.DEADZONE_ON:
                messageLabel2.text = PlayerLifeUI.localization.format("Deadzone_On");
                break;
            case EPlayerMessage.DEADZONE_OFF:
                messageLabel2.text = PlayerLifeUI.localization.format("Deadzone_Off");
                break;
            case EPlayerMessage.BUSY:
                messageLabel2.text = PlayerLifeUI.localization.format("Busy");
                break;
            case EPlayerMessage.FUEL:
                messageLabel2.text = PlayerLifeUI.localization.format("Fuel", text);
                break;
            case EPlayerMessage.CLEAN:
                messageLabel2.text = PlayerLifeUI.localization.format("Clean");
                break;
            case EPlayerMessage.SALTY:
                messageLabel2.text = PlayerLifeUI.localization.format("Salty");
                break;
            case EPlayerMessage.DIRTY:
                messageLabel2.text = PlayerLifeUI.localization.format("Dirty");
                break;
            case EPlayerMessage.REPUTATION:
                messageLabel2.text = PlayerLifeUI.localization.format("Reputation", text);
                break;
            case EPlayerMessage.BAYONET:
                messageLabel2.text = PlayerLifeUI.localization.format("Bayonet", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.tactical));
                break;
            case EPlayerMessage.VEHICLE_LOCKED:
                messageLabel2.text = PlayerLifeUI.localization.format("Vehicle_Locked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
                break;
            case EPlayerMessage.VEHICLE_UNLOCKED:
                messageLabel2.text = PlayerLifeUI.localization.format("Vehicle_Unlocked", MenuConfigurationControlsUI.getKeyCodeText(ControlsSettings.locker));
                break;
            case EPlayerMessage.NPC_CUSTOM:
                messageBox2.positionOffset_X = -300;
                messageBox2.sizeOffset_X = 600;
                messageLabel2.text = text;
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
        PlayerPauseUI.close();
        PlayerDashboardUI.close();
        PlayerBarricadeSignUI.close();
        boomboxUI.close();
        PlayerBarricadeLibraryUI.close();
        PlayerBarricadeMannequinUI.close();
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
            else if (Event.current.keyCode == KeyCode.Return)
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
        else if (PlayerBarricadeMannequinUI.active)
        {
            PlayerBarricadeMannequinUI.close();
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

    private void updateGroupLabels()
    {
        if (Player.player == null || MainCamera.instance == null || groupUI.groups == null || groupUI.groups.Count != Provider.clients.Count)
        {
            return;
        }
        Camera camera = MainCamera.instance;
        for (int i = 0; i < groupUI.groups.Count; i++)
        {
            ISleekLabel sleekLabel = groupUI.groups[i];
            SteamPlayer steamPlayer = Provider.clients[i];
            if (sleekLabel == null || steamPlayer == null || steamPlayer.model == null)
            {
                continue;
            }
            bool flag;
            if (Player.player.look.areSpecStatsVisible)
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
                sleekLabel.isVisible = false;
                continue;
            }
            if ((steamPlayer.model.position - camera.transform.position).sqrMagnitude > 262144f)
            {
                sleekLabel.isVisible = false;
                continue;
            }
            Vector3 vector = camera.WorldToViewportPoint(steamPlayer.model.position + Vector3.up * 3f);
            if (vector.z <= 0f)
            {
                sleekLabel.isVisible = false;
                continue;
            }
            Vector2 vector2 = groupUI.ViewportToNormalizedPosition(vector);
            sleekLabel.positionScale_X = vector2.x;
            sleekLabel.positionScale_Y = vector2.y;
            if (!sleekLabel.isVisible)
            {
                if (steamPlayer.isMemberOfSameGroupAs(Player.player) && !string.IsNullOrEmpty(steamPlayer.playerID.nickName))
                {
                    sleekLabel.text = steamPlayer.playerID.nickName;
                }
                else
                {
                    sleekLabel.text = steamPlayer.playerID.characterName;
                }
            }
            sleekLabel.isVisible = true;
        }
    }

    private void updateHitmarkers()
    {
        if (PlayerLifeUI.hitmarkers == null || MainCamera.instance == null)
        {
            return;
        }
        for (int i = 0; i < PlayerLifeUI.hitmarkers.Length; i++)
        {
            HitmarkerInfo hitmarkerInfo = PlayerLifeUI.hitmarkers[i];
            if (hitmarkerInfo != null && hitmarkerInfo.hit != 0)
            {
                bool flag = Time.realtimeSinceStartup - hitmarkerInfo.lastHit < HIT_TIME;
                Vector2 vector2;
                if (hitmarkerInfo.worldspace)
                {
                    Vector3 vector = MainCamera.instance.WorldToViewportPoint(hitmarkerInfo.point);
                    flag &= vector.z > 0f;
                    vector2 = window.ViewportToNormalizedPosition(vector);
                }
                else
                {
                    vector2 = new Vector3(0.5f, 0.5f);
                }
                hitmarkerInfo.image.isVisible = flag;
                hitmarkerInfo.image.positionScale_X = vector2.x;
                hitmarkerInfo.image.positionScale_Y = vector2.y;
                if (!flag)
                {
                    hitmarkerInfo.hit = EPlayerHit.NONE;
                }
            }
        }
    }

    private void updateHintsAndMessages()
    {
        if (isHinted)
        {
            if (!lastHinted)
            {
                isHinted = false;
                if (messageBox != null)
                {
                    messageBox.isVisible = false;
                }
                if (messagePlayer != null)
                {
                    messagePlayer.isVisible = false;
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
                    messageBox2.isVisible = false;
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
                    messageBox2.isVisible = false;
                }
            }
            lastHinted2 = false;
        }
    }

    private void updateVoteDisplay()
    {
        if (PlayerLifeUI.isVoteMessaged && Time.realtimeSinceStartup - PlayerLifeUI.lastVoteMessage > 2f)
        {
            PlayerLifeUI.isVoteMessaged = false;
            if (PlayerLifeUI.voteBox != null)
            {
                PlayerLifeUI.voteBox.isVisible = false;
            }
        }
    }

    private void updatePauseTimeScale()
    {
        if (Provider.isServer && (MenuConfigurationOptionsUI.active || MenuConfigurationDisplayUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationControlsUI.active || PlayerPauseUI.active))
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
            if (!Player.player.life.isDead)
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
        if (!Player.player.life.isDead)
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
                else if (PlayerBarricadeMannequinUI.active)
                {
                    PlayerBarricadeMannequinUI.close();
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
            Assets.refresh();
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
                text += string.Format("{0, 5} {1, 5}", Player.player.quests.flagsList[i].id, Player.player.quests.flagsList[i].value);
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
        colorOverlayImage.color = black;
        if (isFlashbanged && stunAlpha < 0.001f)
        {
            isFlashbanged = false;
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
            painAlpha = Mathf.Lerp(painAlpha, 0f, 2f * Time.deltaTime);
            stunAlpha = Mathf.Max(0f, stunAlpha - Time.deltaTime);
            updateHitmarkers();
            updateHintsAndMessages();
            updateVoteDisplay();
            updatePauseTimeScale();
            tickDeathTimers();
            tickExitTimer();
            if (PlayerNPCDialogueUI.active)
            {
                PlayerNPCDialogueUI.updateText();
            }
            if (PlayerDashboardInformationUI.active)
            {
                PlayerDashboardInformationUI.updateDynamicMap();
            }
            tickInput();
            bool flag = Player.player.inPluginModal || PlayerPauseUI.active || MenuConfigurationOptionsUI.active || MenuConfigurationDisplayUI.active || MenuConfigurationGraphicsUI.active || MenuConfigurationControlsUI.active || PlayerDashboardUI.active || PlayerDeathUI.active || PlayerLifeUI.chatting || PlayerLifeUI.gesturing || PlayerBarricadeSignUI.active || boomboxUI.active || PlayerBarricadeLibraryUI.active || PlayerBarricadeMannequinUI.active || browserRequestUI.isActive || PlayerNPCDialogueUI.active || PlayerNPCQuestUI.active || PlayerNPCVendorUI.active || (PlayerWorkzoneUI.active && !InputEx.GetKey(ControlsSettings.secondary)) || isLocked;
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
            sleekLabel.sizeOffset_X = 200;
            sleekLabel.sizeOffset_Y = 30;
            sleekLabel.positionOffset_X = -100;
            sleekLabel.positionOffset_Y = -15;
            sleekLabel.positionScale_X = 0.5f;
            sleekLabel.positionScale_Y = 0.2f;
            sleekLabel.textColor = ESleekTint.BAD;
            sleekLabel.text = "Bypassing integrity checks";
            sleekLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            window.AddChild(sleekLabel);
        }
        colorOverlayImage = Glazier.Get().CreateImage();
        colorOverlayImage.sizeScale_X = 1f;
        colorOverlayImage.sizeScale_Y = 1f;
        colorOverlayImage.texture = (Texture2D)GlazierResources.PixelTexture;
        colorOverlayImage.color = new Color(0f, 0f, 0f, 0f);
        window.AddChild(colorOverlayImage);
        container = Glazier.Get().CreateFrame();
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        window.AddChild(container);
        wantsWindowEnabled = true;
        isFlashbanged = false;
        OptionsSettings.apply();
        GraphicsSettings.apply("loaded player");
        groupUI = new PlayerGroupUI();
        groupUI.sizeScale_X = 1f;
        groupUI.sizeScale_Y = 1f;
        container.AddChild(groupUI);
        dashboardUI = new PlayerDashboardUI();
        new PlayerPauseUI();
        lifeUI = new PlayerLifeUI();
        new PlayerDeathUI();
        new PlayerBarricadeSignUI();
        boomboxUI = new PlayerBarricadeStereoUI();
        container.AddChild(boomboxUI);
        new PlayerBarricadeLibraryUI();
        new PlayerBarricadeMannequinUI();
        browserRequestUI = new PlayerBrowserRequestUI();
        container.AddChild(browserRequestUI);
        new PlayerNPCDialogueUI();
        new PlayerNPCQuestUI();
        new PlayerNPCVendorUI();
        new PlayerWorkzoneUI();
        PlayerLifeUI.UpdateTrackedQuest();
        messagePlayer = null;
        messageBox = Glazier.Get().CreateBox();
        messageBox.positionOffset_X = -200;
        messageBox.positionScale_X = 0.5f;
        messageBox.positionScale_Y = 1f;
        messageBox.sizeOffset_X = 400;
        container.AddChild(messageBox);
        messageBox.isVisible = false;
        messageLabel = Glazier.Get().CreateLabel();
        messageLabel.positionOffset_X = 5;
        messageLabel.positionOffset_Y = 5;
        messageLabel.sizeOffset_X = -10;
        messageLabel.sizeOffset_Y = 40;
        messageLabel.sizeScale_X = 1f;
        messageLabel.fontSize = ESleekFontSize.Medium;
        messageBox.AddChild(messageLabel);
        messageIcon_0 = Glazier.Get().CreateImage();
        messageIcon_0.positionOffset_X = 5;
        messageIcon_0.positionOffset_Y = 45;
        messageIcon_0.sizeOffset_X = 20;
        messageIcon_0.sizeOffset_Y = 20;
        messageBox.AddChild(messageIcon_0);
        messageIcon_0.isVisible = false;
        messageIcon_1 = Glazier.Get().CreateImage();
        messageIcon_1.positionOffset_X = 5;
        messageIcon_1.positionOffset_Y = 75;
        messageIcon_1.sizeOffset_X = 20;
        messageIcon_1.sizeOffset_Y = 20;
        messageBox.AddChild(messageIcon_1);
        messageIcon_1.isVisible = false;
        messageIcon_2 = Glazier.Get().CreateImage();
        messageIcon_2.positionOffset_X = 5;
        messageIcon_2.positionOffset_Y = 105;
        messageIcon_2.sizeOffset_X = 20;
        messageIcon_2.sizeOffset_Y = 20;
        messageBox.AddChild(messageIcon_2);
        messageIcon_2.isVisible = false;
        messageProgress_0 = new SleekProgress("");
        messageProgress_0.positionOffset_X = 30;
        messageProgress_0.positionOffset_Y = 50;
        messageProgress_0.sizeOffset_X = -40;
        messageProgress_0.sizeOffset_Y = 10;
        messageProgress_0.sizeScale_X = 1f;
        messageBox.AddChild(messageProgress_0);
        messageProgress_0.isVisible = false;
        messageProgress_1 = new SleekProgress("");
        messageProgress_1.positionOffset_X = 30;
        messageProgress_1.positionOffset_Y = 80;
        messageProgress_1.sizeOffset_X = -40;
        messageProgress_1.sizeOffset_Y = 10;
        messageProgress_1.sizeScale_X = 1f;
        messageBox.AddChild(messageProgress_1);
        messageProgress_1.isVisible = false;
        messageProgress_2 = new SleekProgress("");
        messageProgress_2.positionOffset_X = 30;
        messageProgress_2.positionOffset_Y = 110;
        messageProgress_2.sizeOffset_X = -40;
        messageProgress_2.sizeOffset_Y = 10;
        messageProgress_2.sizeScale_X = 1f;
        messageBox.AddChild(messageProgress_2);
        messageProgress_2.isVisible = false;
        messageQualityImage = Glazier.Get().CreateImage(PlayerDashboardInventoryUI.icons.load<Texture2D>("Quality_0"));
        messageQualityImage.positionOffset_X = -30;
        messageQualityImage.positionOffset_Y = -30;
        messageQualityImage.positionScale_X = 1f;
        messageQualityImage.positionScale_Y = 1f;
        messageQualityImage.sizeOffset_X = 20;
        messageQualityImage.sizeOffset_Y = 20;
        messageBox.AddChild(messageQualityImage);
        messageQualityImage.isVisible = false;
        messageAmountLabel = Glazier.Get().CreateLabel();
        messageAmountLabel.positionOffset_X = 10;
        messageAmountLabel.positionOffset_Y = -40;
        messageAmountLabel.positionScale_Y = 1f;
        messageAmountLabel.sizeOffset_X = -20;
        messageAmountLabel.sizeOffset_Y = 30;
        messageAmountLabel.sizeScale_X = 1f;
        messageAmountLabel.fontAlignment = TextAnchor.LowerLeft;
        messageAmountLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
        messageBox.AddChild(messageAmountLabel);
        messageAmountLabel.isVisible = false;
        messageBox2 = Glazier.Get().CreateBox();
        messageBox2.positionOffset_X = -200;
        messageBox2.positionScale_X = 0.5f;
        messageBox2.positionScale_Y = 1f;
        messageBox2.sizeOffset_X = 400;
        container.AddChild(messageBox2);
        messageBox2.isVisible = false;
        messageLabel2 = Glazier.Get().CreateLabel();
        messageLabel2.positionOffset_X = 5;
        messageLabel2.positionOffset_Y = 5;
        messageLabel2.sizeOffset_X = -10;
        messageLabel2.sizeOffset_Y = 40;
        messageLabel2.sizeScale_X = 1f;
        messageLabel2.fontSize = ESleekFontSize.Medium;
        messageBox2.AddChild(messageLabel2);
        messageIcon2 = Glazier.Get().CreateImage();
        messageIcon2.positionOffset_X = 5;
        messageIcon2.positionOffset_Y = 75;
        messageIcon2.sizeOffset_X = 20;
        messageIcon2.sizeOffset_Y = 20;
        messageBox2.AddChild(messageIcon2);
        messageIcon2.isVisible = false;
        messageProgress2_0 = new SleekProgress("");
        messageProgress2_0.positionOffset_X = 5;
        messageProgress2_0.positionOffset_Y = 50;
        messageProgress2_0.sizeOffset_X = -10;
        messageProgress2_0.sizeOffset_Y = 10;
        messageProgress2_0.sizeScale_X = 1f;
        messageBox2.AddChild(messageProgress2_0);
        messageProgress2_1 = new SleekProgress("");
        messageProgress2_1.positionOffset_X = 30;
        messageProgress2_1.positionOffset_Y = 80;
        messageProgress2_1.sizeOffset_X = -40;
        messageProgress2_1.sizeOffset_Y = 10;
        messageProgress2_1.sizeScale_X = 1f;
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
            if (lifeUI != null)
            {
                lifeUI.OnDestroy();
            }
            if (!Provider.isApplicationQuitting)
            {
                window.destroy();
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
