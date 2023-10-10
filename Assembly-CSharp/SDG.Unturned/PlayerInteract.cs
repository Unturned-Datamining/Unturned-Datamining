using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerInteract : PlayerCaller
{
    private static Transform focus;

    private static Transform target;

    private static ItemAsset purchaseAsset;

    private static Interactable _interactable;

    private static Interactable2 _interactable2;

    internal static RaycastHit hit;

    private static float lastInteract;

    private static float salvageHeldTime;

    private static bool isHoldingKey;

    private bool shouldOverrideSalvageTime;

    private float overrideSalvageTimeValue;

    private static readonly ClientInstanceMethod<float> SendSalvageTimeOverride = ClientInstanceMethod<float>.Get(typeof(PlayerInteract), "ReceiveSalvageTimeOverride");

    private static readonly ServerInstanceMethod SendInspectRequest = ServerInstanceMethod.Get(typeof(PlayerInteract), "ReceiveInspectRequest");

    private static readonly ClientInstanceMethod SendPlayInspect = ClientInstanceMethod.Get(typeof(PlayerInteract), "ReceivePlayInspect");

    private Transform highlightedTransform;

    private bool didHaveFocus;

    public static Interactable interactable => _interactable;

    public static Interactable2 interactable2 => _interactable2;

    private float salvageTime
    {
        get
        {
            if (shouldOverrideSalvageTime)
            {
                return overrideSalvageTimeValue;
            }
            if (base.player.equipment.useable is UseableHousingPlanner)
            {
                return 0.5f;
            }
            if (Provider.isServer || base.channel.owner.isAdmin)
            {
                LevelAsset asset = Level.getAsset();
                if (asset == null || asset.enableAdminFasterSalvageDuration)
                {
                    return 1f;
                }
            }
            return 8f;
        }
    }

    private float interactableSalvageTime
    {
        get
        {
            float num = salvageTime;
            if (_interactable2 != null)
            {
                num *= _interactable2.salvageDurationMultiplier;
            }
            return num;
        }
    }

    [Obsolete]
    public void tellSalvageTimeOverride(CSteamID senderId, float overrideValue)
    {
        ReceiveSalvageTimeOverride(overrideValue);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellSalvageTimeOverride")]
    public void ReceiveSalvageTimeOverride(float overrideValue)
    {
        overrideSalvageTimeValue = overrideValue;
        shouldOverrideSalvageTime = overrideSalvageTimeValue > -0.5f;
    }

    public void sendSalvageTimeOverride(float overrideValue)
    {
        SendSalvageTimeOverride.Invoke(GetNetId(), ENetReliability.Reliable, base.channel.GetOwnerTransportConnection(), overrideValue);
    }

    private void hotkey(byte button)
    {
        VehicleManager.swapVehicle(button);
    }

    [Obsolete]
    public void askInspect(CSteamID steamID)
    {
        ReceiveInspectRequest();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 2, legacyName = "askInspect")]
    public void ReceiveInspectRequest()
    {
        if (base.player.equipment.canInspect)
        {
            SendPlayInspect.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
            base.player.equipment.InvokeOnInspectingUseable();
        }
    }

    [Obsolete]
    public void tellInspect(CSteamID steamID)
    {
        ReceivePlayInspect();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "tellInspect")]
    public void ReceivePlayInspect()
    {
        base.player.equipment.inspect();
    }

    private void localInspect()
    {
        if (base.player.equipment.canInspect)
        {
            base.player.equipment.inspect();
            if (!Provider.isServer)
            {
                SendInspectRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
            }
        }
    }

    private void onPurchaseUpdated(HordePurchaseVolume node)
    {
        if (node == null)
        {
            purchaseAsset = null;
        }
        else
        {
            purchaseAsset = Assets.find(EAssetType.ITEM, node.id) as ItemAsset;
        }
    }

    private void OnLifeUpdated(bool isDead)
    {
        salvageHeldTime = 0f;
    }

    private void Update()
    {
        if (!base.channel.IsLocalPlayer)
        {
            return;
        }
        if (base.player.stance.stance != EPlayerStance.DRIVING && base.player.stance.stance != EPlayerStance.SITTING && base.player.life.IsAlive && !base.player.workzone.isBuilding)
        {
            if (Time.realtimeSinceStartup - lastInteract > 0.1f)
            {
                lastInteract = Time.realtimeSinceStartup;
                int num = RayMasks.PLAYER_INTERACT;
                if (base.player.stance.stance == EPlayerStance.CLIMB)
                {
                    num &= -33554433;
                }
                if (base.player.look.isCam)
                {
                    Physics.Raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), out hit, 4f, num);
                }
                else
                {
                    Physics.Raycast(new Ray(MainCamera.instance.transform.position, MainCamera.instance.transform.forward), out hit, (base.player.look.perspective == EPlayerPerspective.THIRD) ? 6 : 4, num);
                }
            }
            Transform transform = ((hit.collider != null) ? hit.collider.transform : null);
            bool flag = transform != null;
            if (transform != focus || flag != didHaveFocus)
            {
                clearHighlight();
                focus = null;
                didHaveFocus = false;
                target = null;
                _interactable = null;
                _interactable2 = null;
                if (transform != null)
                {
                    focus = transform;
                    didHaveFocus = true;
                    _interactable = focus.GetComponentInParent<Interactable>();
                    _interactable2 = focus.GetComponentInParent<Interactable2>();
                    if (_interactable == null && focus.CompareTag("Ladder"))
                    {
                        _interactable = focus.gameObject.AddComponent<InteractableLadder>();
                    }
                    if (interactable != null)
                    {
                        target = interactable.transform.FindChildRecursive("Target");
                        if (interactable.checkInteractable())
                        {
                            if (PlayerUI.window.isEnabled)
                            {
                                if (interactable.checkUseable())
                                {
                                    if (!interactable.checkHighlight(out var color))
                                    {
                                        color = Color.green;
                                    }
                                    InteractableDoorHinge componentInParent = focus.GetComponentInParent<InteractableDoorHinge>();
                                    if (componentInParent != null)
                                    {
                                        setHighlight(componentInParent.door.transform, color);
                                    }
                                    else
                                    {
                                        setHighlight(interactable.transform, color);
                                    }
                                }
                                else
                                {
                                    Color color = Color.red;
                                    InteractableDoorHinge componentInParent2 = focus.GetComponentInParent<InteractableDoorHinge>();
                                    if (componentInParent2 != null)
                                    {
                                        setHighlight(componentInParent2.door.transform, color);
                                    }
                                    else
                                    {
                                        setHighlight(interactable.transform, color);
                                    }
                                }
                            }
                        }
                        else
                        {
                            target = null;
                            _interactable = null;
                        }
                    }
                }
            }
        }
        else
        {
            clearHighlight();
            focus = null;
            didHaveFocus = false;
            target = null;
            _interactable = null;
            _interactable2 = null;
        }
        if (!base.player.life.IsAlive)
        {
            return;
        }
        if (interactable != null)
        {
            if (interactable.checkHint(out var message, out var text, out var color2) && !PlayerUI.window.showCursor)
            {
                if (message == EPlayerMessage.ITEM)
                {
                    PlayerUI.hint((target != null) ? target : focus, message, text, color2, ((InteractableItem)interactable).item, ((InteractableItem)interactable).asset);
                }
                else
                {
                    PlayerUI.hint((target != null) ? target : focus, message, text, color2);
                }
            }
        }
        else if (purchaseAsset != null && base.player.movement.purchaseNode != null && !PlayerUI.window.showCursor)
        {
            PlayerUI.hint(null, EPlayerMessage.PURCHASE, "", Color.white, purchaseAsset.itemName, base.player.movement.purchaseNode.cost);
        }
        else if (focus != null && focus.CompareTag("Enemy"))
        {
            bool num2 = (base.player.pluginWidgetFlags & EPluginWidgetFlags.ShowInteractWithEnemy) == EPluginWidgetFlags.ShowInteractWithEnemy;
            bool flag2 = !PlayerUI.window.showCursor;
            if (num2 && flag2)
            {
                Player player = DamageTool.getPlayer(focus);
                if (player != null && player != base.player)
                {
                    PlayerUI.hint(null, EPlayerMessage.ENEMY, string.Empty, Color.white, player.channel.owner);
                }
            }
        }
        if (interactable2 != null)
        {
            if (interactable2.checkHint(out var message2, out var data) && !PlayerUI.window.showCursor)
            {
                PlayerUI.hint2(message2, isHoldingKey ? (salvageHeldTime / interactableSalvageTime) : 0f, data);
            }
        }
        else
        {
            salvageHeldTime = 0f;
        }
        if ((base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING) && !InputEx.GetKey(KeyCode.LeftShift))
        {
            if (InputEx.GetKeyDown(KeyCode.F1))
            {
                hotkey(0);
            }
            if (InputEx.GetKeyDown(KeyCode.F2))
            {
                hotkey(1);
            }
            if (InputEx.GetKeyDown(KeyCode.F3))
            {
                hotkey(2);
            }
            if (InputEx.GetKeyDown(KeyCode.F4))
            {
                hotkey(3);
            }
            if (InputEx.GetKeyDown(KeyCode.F5))
            {
                hotkey(4);
            }
            if (InputEx.GetKeyDown(KeyCode.F6))
            {
                hotkey(5);
            }
            if (InputEx.GetKeyDown(KeyCode.F7))
            {
                hotkey(6);
            }
            if (InputEx.GetKeyDown(KeyCode.F8))
            {
                hotkey(7);
            }
            if (InputEx.GetKeyDown(KeyCode.F9))
            {
                hotkey(8);
            }
            if (InputEx.GetKeyDown(KeyCode.F10))
            {
                hotkey(9);
            }
        }
        if (InputEx.GetKeyDown(ControlsSettings.interact))
        {
            salvageHeldTime = 0f;
            isHoldingKey = true;
        }
        if (InputEx.GetKeyDown(ControlsSettings.inspect) && ControlsSettings.inspect != ControlsSettings.interact)
        {
            localInspect();
        }
        if (!isHoldingKey)
        {
            return;
        }
        salvageHeldTime += Time.deltaTime;
        if (InputEx.GetKeyUp(ControlsSettings.interact))
        {
            isHoldingKey = false;
            if (PlayerUI.window.showCursor)
            {
                if (base.player.inventory.isStoring && base.player.inventory.shouldInteractCloseStorage)
                {
                    PlayerDashboardUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerBarricadeSignUI.active)
                {
                    PlayerBarricadeSignUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerUI.instance.boomboxUI.active)
                {
                    PlayerUI.instance.boomboxUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerBarricadeLibraryUI.active)
                {
                    PlayerBarricadeLibraryUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerUI.instance.mannequinUI.active)
                {
                    PlayerUI.instance.mannequinUI.close();
                    PlayerLifeUI.open();
                }
                else if (PlayerNPCDialogueUI.active)
                {
                    if (PlayerNPCDialogueUI.IsDialogueAnimating)
                    {
                        PlayerNPCDialogueUI.SkipAnimation();
                        return;
                    }
                    if (PlayerNPCDialogueUI.CanAdvanceToNextPage)
                    {
                        PlayerNPCDialogueUI.AdvancePage();
                        return;
                    }
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
            }
            else if (base.player.stance.stance == EPlayerStance.DRIVING || base.player.stance.stance == EPlayerStance.SITTING)
            {
                VehicleManager.exitVehicle();
            }
            else if (focus != null && interactable != null)
            {
                if (interactable.checkUseable())
                {
                    interactable.use();
                }
            }
            else if (purchaseAsset != null)
            {
                if (base.player.skills.experience >= base.player.movement.purchaseNode.cost)
                {
                    base.player.skills.sendPurchase(base.player.movement.purchaseNode);
                }
            }
            else if (ControlsSettings.inspect == ControlsSettings.interact)
            {
                localInspect();
            }
        }
        else if (salvageHeldTime > interactableSalvageTime)
        {
            isHoldingKey = false;
            if (!PlayerUI.window.showCursor && interactable2 != null)
            {
                interactable2.use();
            }
        }
    }

    internal void InitializePlayer()
    {
        if (base.channel.IsLocalPlayer)
        {
            PlayerMovement movement = base.player.movement;
            movement.onPurchaseUpdated = (PurchaseUpdated)Delegate.Combine(movement.onPurchaseUpdated, new PurchaseUpdated(onPurchaseUpdated));
            PlayerLife life = base.player.life;
            life.onLifeUpdated = (LifeUpdated)Delegate.Combine(life.onLifeUpdated, new LifeUpdated(OnLifeUpdated));
        }
    }

    private void clearHighlight()
    {
        if (highlightedTransform != null)
        {
            HighlighterTool.unhighlight(highlightedTransform);
        }
    }

    private void setHighlight(Transform newHighlightedTransform, Color color)
    {
        highlightedTransform = newHighlightedTransform;
        HighlighterTool.highlight(newHighlightedTransform, color);
    }
}
