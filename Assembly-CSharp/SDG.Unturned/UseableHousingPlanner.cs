using System;
using System.Collections.Generic;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class UseableHousingPlanner : Useable
{
    private static MasterBundleReference<OneShotAudioDefinition> popupAudioRef = new MasterBundleReference<OneShotAudioDefinition>("core.masterbundle", "Sounds/Popup/Popup.asset");

    private static MasterBundleReference<AudioClip> errorClipRef = new MasterBundleReference<AudioClip>("core.masterbundle", "Sounds/Error.wav");

    private static readonly ClientInstanceMethod<bool> SendPlaceHousingItemResult = ClientInstanceMethod<bool>.Get(typeof(UseableHousingPlanner), "ReceivePlaceHousingItemResult");

    private static readonly ServerInstanceMethod<Guid, Vector3, float> SendPlaceHousingItem = ServerInstanceMethod<Guid, Vector3, float>.Get(typeof(UseableHousingPlanner), "ReceivePlaceHousingItem");

    private Transform placementPreviewTransform;

    private bool isPlacementPreviewValid;

    private Vector3 pendingPlacementPosition;

    private float pendingPlacementYaw;

    private float animatedRotationOffset;

    private float customRotationOffset;

    private float foundationPositionOffset;

    private ItemStructureAsset selectedAsset;

    private bool isItemSelectionMenuOpen;

    private ISleekElement itemSelectionContainer;

    private SleekJars floorsMenu;

    private SleekJars roofsMenu;

    private SleekJars wallsMenu;

    private SleekJars pillarsMenu;

    private ISleekLabel floorsLabel;

    private ISleekLabel noFloorItemsLabel;

    private ISleekLabel roofsLabel;

    private ISleekLabel noRoofItemsLabel;

    private ISleekLabel wallsLabel;

    private ISleekLabel noWallItemsLabel;

    private ISleekLabel pillarsLabel;

    private ISleekLabel noPillarItemsLabel;

    private ISleekBox selectedItemBox;

    private ISleekLabel selectedItemNameLabel;

    private ISleekLabel selectedItemQuantityLabel;

    private List<InventorySearch> itemSearch;

    private List<InventorySearch> floors;

    private List<InventorySearch> roofs;

    private List<InventorySearch> walls;

    private List<InventorySearch> pillars;

    private Dictionary<ushort, int> itemAmounts;

    private int cachedSearchIndex = -1;

    private const float MENU_RADIUS = 128f;

    private const int MENU_SIZE = 256;

    private const int MENU_PADDING = 50;

    private const float RADIAL_BACKDROP_ALPHA = 0.2f;

    public override bool isUseableShowingMenu => isItemSelectionMenuOpen;

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceivePlaceHousingItemResult(bool success)
    {
        if (success)
        {
            OneShotAudioDefinition oneShotAudioDefinition = popupAudioRef.loadAsset();
            if (oneShotAudioDefinition == null)
            {
                UnturnedLog.warn("Missing built-in housing planner success audio");
            }
            else
            {
                base.player.playSound(oneShotAudioDefinition.GetRandomClip(), 0.5f * oneShotAudioDefinition.volumeMultiplier, UnityEngine.Random.Range(oneShotAudioDefinition.minPitch, oneShotAudioDefinition.maxPitch), 0f);
            }
        }
        else
        {
            AudioClip audioClip = errorClipRef.loadAsset();
            if (audioClip == null)
            {
                UnturnedLog.warn("Missing built-in housing planner error audio");
            }
            else
            {
                base.player.playSound(audioClip, 0.5f, 1f, 0.025f);
            }
        }
    }

    private bool ReceivePlaceHousingItemInternal(in ServerInvocationContext context, Guid assetGuid, Vector3 position, float yaw)
    {
        if ((position - base.player.look.aim.position).sqrMagnitude > 256f)
        {
            return false;
        }
        if (!UseableHousingUtils.IsPendingPositionValid(base.player, position))
        {
            return false;
        }
        if (!(Assets.find(assetGuid) is ItemStructureAsset itemStructureAsset))
        {
            return false;
        }
        InventorySearch inventorySearch = base.player.inventory.has(itemStructureAsset.id);
        if (inventorySearch == null)
        {
            return false;
        }
        string obstructionHint = string.Empty;
        if (UseableHousingUtils.ValidatePendingPlacement(itemStructureAsset, ref position, yaw, ref obstructionHint) != 0)
        {
            return false;
        }
        bool num = StructureManager.dropStructure(new Structure(itemStructureAsset, itemStructureAsset.health), position, 0f, yaw, 0f, base.channel.owner.playerID.steamID.m_SteamID, base.player.quests.groupID.m_SteamID);
        if (num)
        {
            base.player.sendStat(EPlayerStat.FOUND_BUILDABLES);
            inventorySearch.deleteAmount(base.player, 1u);
        }
        return num;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 5)]
    public void ReceivePlaceHousingItem(in ServerInvocationContext context, Guid assetGuid, Vector3 position, float yaw)
    {
        bool arg = ReceivePlaceHousingItemInternal(in context, assetGuid, position, yaw);
        SendPlaceHousingItemResult.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GetOwnerTransportConnection(), arg);
    }

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (base.channel.IsLocalPlayer && selectedAsset != null && UpdatePendingPlacement())
        {
            SendPlaceHousingItem.Invoke(GetNetId(), ENetReliability.Reliable, selectedAsset.GUID, pendingPlacementPosition, pendingPlacementYaw + customRotationOffset);
            return true;
        }
        return false;
    }

    public override bool startSecondary()
    {
        if (base.channel.IsLocalPlayer && selectedAsset != null)
        {
            if (selectedAsset.construct == EConstruct.FLOOR_POLY || selectedAsset.construct == EConstruct.ROOF_POLY)
            {
                return false;
            }
            float num = ((selectedAsset.construct != 0 && selectedAsset.construct != EConstruct.ROOF) ? ((selectedAsset.construct != EConstruct.RAMPART && selectedAsset.construct != EConstruct.WALL) ? 30f : 180f) : 90f);
            if (InputEx.GetKey(KeyCode.LeftShift))
            {
                num *= -1f;
            }
            customRotationOffset += num;
            return true;
        }
        return false;
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        if (base.channel.IsLocalPlayer)
        {
            itemSearch = new List<InventorySearch>();
            floors = new List<InventorySearch>();
            roofs = new List<InventorySearch>();
            walls = new List<InventorySearch>();
            pillars = new List<InventorySearch>();
            itemAmounts = new Dictionary<ushort, int>();
            selectedItemBox = Glazier.Get().CreateBox();
            selectedItemBox.PositionOffset_Y = -50f;
            selectedItemBox.PositionScale_X = 0.7f;
            selectedItemBox.PositionScale_Y = 1f;
            selectedItemBox.SizeOffset_Y = 50f;
            selectedItemBox.SizeScale_X = 0.3f;
            selectedItemBox.IsVisible = false;
            PlayerLifeUI.container.AddChild(selectedItemBox);
            selectedItemNameLabel = Glazier.Get().CreateLabel();
            selectedItemNameLabel.PositionOffset_X = 10f;
            selectedItemNameLabel.SizeScale_X = 1f;
            selectedItemNameLabel.SizeScale_Y = 1f;
            selectedItemNameLabel.SizeOffset_X = -20f;
            selectedItemNameLabel.TextAlignment = TextAnchor.MiddleRight;
            selectedItemNameLabel.FontSize = ESleekFontSize.Large;
            selectedItemBox.AddChild(selectedItemNameLabel);
            selectedItemNameLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            selectedItemQuantityLabel = Glazier.Get().CreateLabel();
            selectedItemQuantityLabel.PositionOffset_X = 10f;
            selectedItemQuantityLabel.SizeScale_X = 1f;
            selectedItemQuantityLabel.SizeScale_Y = 1f;
            selectedItemQuantityLabel.SizeOffset_X = -20f;
            selectedItemQuantityLabel.TextAlignment = TextAnchor.MiddleLeft;
            selectedItemQuantityLabel.FontSize = ESleekFontSize.Large;
            selectedItemBox.AddChild(selectedItemQuantityLabel);
            selectedItemQuantityLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            Local local = Localization.read("/Player/Useable/PlayerUseableHousingPlanner.dat");
            Bundle bundle = Bundles.getBundle("/Bundles/Textures/Player/Icons/Useable/PlayerUseableHousingPlanner/PlayerUseableHousingPlanner.unity3d");
            Texture texture = bundle.load<Texture>("RadialMenu");
            bundle.unload();
            itemSelectionContainer = Glazier.Get().CreateFrame();
            itemSelectionContainer.SizeScale_X = 1f;
            itemSelectionContainer.SizeScale_Y = 1f;
            itemSelectionContainer.IsVisible = false;
            PlayerUI.container.AddChild(itemSelectionContainer);
            ISleekImage sleekImage = Glazier.Get().CreateImage(texture);
            sleekImage.PositionScale_X = 0.5f;
            sleekImage.PositionScale_Y = 0.5f;
            sleekImage.PositionOffset_X = 50f;
            sleekImage.PositionOffset_Y = -306f;
            sleekImage.SizeOffset_X = 256f;
            sleekImage.SizeOffset_Y = 256f;
            sleekImage.TintColor = SleekColor.BackgroundIfLight(new Color(0f, 0f, 0f, 0.2f));
            itemSelectionContainer.AddChild(sleekImage);
            floorsLabel = Glazier.Get().CreateLabel();
            floorsLabel.PositionScale_X = 0.5f;
            floorsLabel.PositionScale_Y = 0.5f;
            floorsLabel.PositionOffset_X = 50f;
            floorsLabel.PositionOffset_Y = -306f;
            floorsLabel.SizeOffset_X = 256f;
            floorsLabel.SizeOffset_Y = 256f;
            floorsLabel.FontSize = ESleekFontSize.Large;
            floorsLabel.Text = local.format("Floors");
            itemSelectionContainer.AddChild(floorsLabel);
            floorsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            noFloorItemsLabel = Glazier.Get().CreateLabel();
            noFloorItemsLabel.PositionScale_X = 0.5f;
            noFloorItemsLabel.PositionScale_Y = 0.5f;
            noFloorItemsLabel.PositionOffset_X = 50f;
            noFloorItemsLabel.PositionOffset_Y = -286f;
            noFloorItemsLabel.SizeOffset_X = 256f;
            noFloorItemsLabel.SizeOffset_Y = 256f;
            noFloorItemsLabel.FontSize = ESleekFontSize.Medium;
            noFloorItemsLabel.TextColor = ESleekTint.BAD;
            noFloorItemsLabel.Text = local.format("NoItems");
            noFloorItemsLabel.IsVisible = false;
            itemSelectionContainer.AddChild(noFloorItemsLabel);
            noFloorItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            ISleekImage sleekImage2 = Glazier.Get().CreateImage(texture);
            sleekImage2.PositionScale_X = 0.5f;
            sleekImage2.PositionScale_Y = 0.5f;
            sleekImage2.PositionOffset_X = 50f;
            sleekImage2.PositionOffset_Y = 50f;
            sleekImage2.SizeOffset_X = 256f;
            sleekImage2.SizeOffset_Y = 256f;
            sleekImage2.TintColor = SleekColor.BackgroundIfLight(new Color(0f, 0f, 0f, 0.2f));
            itemSelectionContainer.AddChild(sleekImage2);
            roofsLabel = Glazier.Get().CreateLabel();
            roofsLabel.PositionScale_X = 0.5f;
            roofsLabel.PositionScale_Y = 0.5f;
            roofsLabel.PositionOffset_X = 50f;
            roofsLabel.PositionOffset_Y = 50f;
            roofsLabel.SizeOffset_X = 256f;
            roofsLabel.SizeOffset_Y = 256f;
            roofsLabel.FontSize = ESleekFontSize.Large;
            roofsLabel.Text = local.format("Roofs");
            itemSelectionContainer.AddChild(roofsLabel);
            roofsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            noRoofItemsLabel = Glazier.Get().CreateLabel();
            noRoofItemsLabel.PositionScale_X = 0.5f;
            noRoofItemsLabel.PositionScale_Y = 0.5f;
            noRoofItemsLabel.PositionOffset_X = 50f;
            noRoofItemsLabel.PositionOffset_Y = 70f;
            noRoofItemsLabel.SizeOffset_X = 256f;
            noRoofItemsLabel.SizeOffset_Y = 256f;
            noRoofItemsLabel.FontSize = ESleekFontSize.Medium;
            noRoofItemsLabel.TextColor = ESleekTint.BAD;
            noRoofItemsLabel.Text = local.format("NoItems");
            noRoofItemsLabel.IsVisible = false;
            itemSelectionContainer.AddChild(noRoofItemsLabel);
            noRoofItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            ISleekImage sleekImage3 = Glazier.Get().CreateImage(texture);
            sleekImage3.PositionScale_X = 0.5f;
            sleekImage3.PositionScale_Y = 0.5f;
            sleekImage3.PositionOffset_X = -306f;
            sleekImage3.PositionOffset_Y = -306f;
            sleekImage3.SizeOffset_X = 256f;
            sleekImage3.SizeOffset_Y = 256f;
            sleekImage3.TintColor = SleekColor.BackgroundIfLight(new Color(0f, 0f, 0f, 0.2f));
            itemSelectionContainer.AddChild(sleekImage3);
            wallsLabel = Glazier.Get().CreateLabel();
            wallsLabel.PositionScale_X = 0.5f;
            wallsLabel.PositionScale_Y = 0.5f;
            wallsLabel.PositionOffset_X = -306f;
            wallsLabel.PositionOffset_Y = -306f;
            wallsLabel.SizeOffset_X = 256f;
            wallsLabel.SizeOffset_Y = 256f;
            wallsLabel.FontSize = ESleekFontSize.Large;
            wallsLabel.Text = local.format("Walls");
            itemSelectionContainer.AddChild(wallsLabel);
            wallsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            noWallItemsLabel = Glazier.Get().CreateLabel();
            noWallItemsLabel.PositionScale_X = 0.5f;
            noWallItemsLabel.PositionScale_Y = 0.5f;
            noWallItemsLabel.PositionOffset_X = -306f;
            noWallItemsLabel.PositionOffset_Y = -286f;
            noWallItemsLabel.SizeOffset_X = 256f;
            noWallItemsLabel.SizeOffset_Y = 256f;
            noWallItemsLabel.FontSize = ESleekFontSize.Medium;
            noWallItemsLabel.TextColor = ESleekTint.BAD;
            noWallItemsLabel.Text = local.format("NoItems");
            noWallItemsLabel.IsVisible = false;
            itemSelectionContainer.AddChild(noWallItemsLabel);
            noWallItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            ISleekImage sleekImage4 = Glazier.Get().CreateImage(texture);
            sleekImage4.PositionScale_X = 0.5f;
            sleekImage4.PositionScale_Y = 0.5f;
            sleekImage4.PositionOffset_X = -306f;
            sleekImage4.PositionOffset_Y = 50f;
            sleekImage4.SizeOffset_X = 256f;
            sleekImage4.SizeOffset_Y = 256f;
            sleekImage4.TintColor = SleekColor.BackgroundIfLight(new Color(0f, 0f, 0f, 0.2f));
            itemSelectionContainer.AddChild(sleekImage4);
            pillarsLabel = Glazier.Get().CreateLabel();
            pillarsLabel.PositionScale_X = 0.5f;
            pillarsLabel.PositionScale_Y = 0.5f;
            pillarsLabel.PositionOffset_X = -306f;
            pillarsLabel.PositionOffset_Y = 50f;
            pillarsLabel.SizeOffset_X = 256f;
            pillarsLabel.SizeOffset_Y = 256f;
            pillarsLabel.FontSize = ESleekFontSize.Large;
            pillarsLabel.Text = local.format("Pillars");
            itemSelectionContainer.AddChild(pillarsLabel);
            pillarsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            noPillarItemsLabel = Glazier.Get().CreateLabel();
            noPillarItemsLabel.PositionScale_X = 0.5f;
            noPillarItemsLabel.PositionScale_Y = 0.5f;
            noPillarItemsLabel.PositionOffset_X = -306f;
            noPillarItemsLabel.PositionOffset_Y = 70f;
            noPillarItemsLabel.SizeOffset_X = 256f;
            noPillarItemsLabel.SizeOffset_Y = 256f;
            noPillarItemsLabel.FontSize = ESleekFontSize.Medium;
            noPillarItemsLabel.TextColor = ESleekTint.BAD;
            noPillarItemsLabel.Text = local.format("NoItems");
            noPillarItemsLabel.IsVisible = false;
            itemSelectionContainer.AddChild(noPillarItemsLabel);
            noPillarItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            PlayerUI.message(EPlayerMessage.HOUSING_PLANNER_TUTORIAL, "");
        }
    }

    public override void dequip()
    {
        if (base.channel.IsLocalPlayer)
        {
            SetItemSelectionMenuOpen(isOpen: false);
            SetSelectedAsset(null);
            PlayerUI.container.RemoveChild(selectedItemBox);
            PlayerUI.container.RemoveChild(itemSelectionContainer);
        }
    }

    public override void tick()
    {
        if (!base.channel.IsLocalPlayer)
        {
            return;
        }
        if (base.player.inventory.doesSearchNeedRefresh(ref cachedSearchIndex))
        {
            RefreshAvailableItems();
        }
        if (InputEx.GetKeyUp(ControlsSettings.attach))
        {
            SetItemSelectionMenuOpen(isOpen: false);
        }
        else if (!PlayerUI.window.showCursor && InputEx.ConsumeKeyDown(ControlsSettings.attach))
        {
            SetItemSelectionMenuOpen(isOpen: true);
        }
        if (placementPreviewTransform != null)
        {
            bool flag = UpdatePendingPlacement();
            if (isPlacementPreviewValid != flag)
            {
                isPlacementPreviewValid = flag;
                HighlighterTool.help(placementPreviewTransform, isPlacementPreviewValid);
            }
            float num = (Glazier.Get().ShouldGameProcessInput ? Input.GetAxis("mouse_z") : 0f);
            foundationPositionOffset = Mathf.Clamp(foundationPositionOffset + num * 0.05f, -1f, 1f);
            animatedRotationOffset = Mathf.Lerp(animatedRotationOffset, customRotationOffset, 8f * Time.deltaTime);
            placementPreviewTransform.position = pendingPlacementPosition;
            placementPreviewTransform.rotation = Quaternion.Euler(-90f, pendingPlacementYaw + animatedRotationOffset, 0f);
        }
    }

    private void SetItemSelectionMenuOpen(bool isOpen)
    {
        if (isItemSelectionMenuOpen == isOpen)
        {
            return;
        }
        isItemSelectionMenuOpen = isOpen;
        PlayerUI.isLocked = isOpen;
        if (floorsMenu != null)
        {
            itemSelectionContainer.RemoveChild(floorsMenu);
            floorsMenu = null;
        }
        if (roofsMenu != null)
        {
            itemSelectionContainer.RemoveChild(roofsMenu);
            roofsMenu = null;
        }
        if (wallsMenu != null)
        {
            itemSelectionContainer.RemoveChild(wallsMenu);
            wallsMenu = null;
        }
        if (pillarsMenu != null)
        {
            itemSelectionContainer.RemoveChild(pillarsMenu);
            pillarsMenu = null;
        }
        if (isOpen)
        {
            PlayerLifeUI.close();
        }
        else
        {
            PlayerLifeUI.open();
        }
        itemSelectionContainer.IsVisible = isOpen;
        if (!isOpen)
        {
            return;
        }
        floors.Clear();
        roofs.Clear();
        walls.Clear();
        pillars.Clear();
        foreach (InventorySearch item in itemSearch)
        {
            switch (item.GetAsset<ItemStructureAsset>().construct)
            {
            case EConstruct.FLOOR:
            case EConstruct.FLOOR_POLY:
                floors.Add(item);
                break;
            case EConstruct.ROOF:
            case EConstruct.ROOF_POLY:
                roofs.Add(item);
                break;
            case EConstruct.WALL:
            case EConstruct.RAMPART:
                walls.Add(item);
                break;
            case EConstruct.PILLAR:
            case EConstruct.POST:
                pillars.Add(item);
                break;
            }
        }
        floors.Sort(CompareItemNames);
        roofs.Sort(CompareItemNames);
        walls.Sort(CompareItemNames);
        pillars.Sort(CompareItemNames);
        noFloorItemsLabel.IsVisible = floors.Count < 1;
        noRoofItemsLabel.IsVisible = roofs.Count < 1;
        noWallItemsLabel.IsVisible = walls.Count < 1;
        noPillarItemsLabel.IsVisible = pillars.Count < 1;
        floorsMenu = new SleekJars(128f, floors, MathF.PI * 3f / 4f);
        floorsMenu.PositionScale_X = 0.5f;
        floorsMenu.PositionScale_Y = 0.5f;
        floorsMenu.PositionOffset_X = 50f;
        floorsMenu.PositionOffset_Y = -306f;
        floorsMenu.SizeOffset_X = 256f;
        floorsMenu.SizeOffset_Y = 256f;
        SleekJars sleekJars = floorsMenu;
        sleekJars.onClickedJar = (ClickedJar)Delegate.Combine(sleekJars.onClickedJar, new ClickedJar(OnSelectedFloorItem));
        itemSelectionContainer.AddChild(floorsMenu);
        roofsMenu = new SleekJars(128f, roofs, 3.926991f);
        roofsMenu.PositionScale_X = 0.5f;
        roofsMenu.PositionScale_Y = 0.5f;
        roofsMenu.PositionOffset_X = 50f;
        roofsMenu.PositionOffset_Y = 50f;
        roofsMenu.SizeOffset_X = 256f;
        roofsMenu.SizeOffset_Y = 256f;
        SleekJars sleekJars2 = roofsMenu;
        sleekJars2.onClickedJar = (ClickedJar)Delegate.Combine(sleekJars2.onClickedJar, new ClickedJar(OnSelectedRoofItem));
        itemSelectionContainer.AddChild(roofsMenu);
        wallsMenu = new SleekJars(128f, walls, MathF.PI / 4f);
        wallsMenu.PositionScale_X = 0.5f;
        wallsMenu.PositionScale_Y = 0.5f;
        wallsMenu.PositionOffset_X = -306f;
        wallsMenu.PositionOffset_Y = -306f;
        wallsMenu.SizeOffset_X = 256f;
        wallsMenu.SizeOffset_Y = 256f;
        SleekJars sleekJars3 = wallsMenu;
        sleekJars3.onClickedJar = (ClickedJar)Delegate.Combine(sleekJars3.onClickedJar, new ClickedJar(OnSelectedWallItem));
        itemSelectionContainer.AddChild(wallsMenu);
        pillarsMenu = new SleekJars(128f, pillars, 5.4977875f);
        pillarsMenu.PositionScale_X = 0.5f;
        pillarsMenu.PositionScale_Y = 0.5f;
        pillarsMenu.PositionOffset_X = -306f;
        pillarsMenu.PositionOffset_Y = 50f;
        pillarsMenu.SizeOffset_X = 256f;
        pillarsMenu.SizeOffset_Y = 256f;
        SleekJars sleekJars4 = pillarsMenu;
        sleekJars4.onClickedJar = (ClickedJar)Delegate.Combine(sleekJars4.onClickedJar, new ClickedJar(OnSelectedPillarItem));
        itemSelectionContainer.AddChild(pillarsMenu);
    }

    private void OnSelectedFloorItem(SleekJars jars, int index)
    {
        ItemStructureAsset asset = floors[index].GetAsset<ItemStructureAsset>();
        SetSelectedAsset(asset);
    }

    private void OnSelectedRoofItem(SleekJars jars, int index)
    {
        ItemStructureAsset asset = roofs[index].GetAsset<ItemStructureAsset>();
        SetSelectedAsset(asset);
    }

    private void OnSelectedWallItem(SleekJars jars, int index)
    {
        ItemStructureAsset asset = walls[index].GetAsset<ItemStructureAsset>();
        SetSelectedAsset(asset);
    }

    private void OnSelectedPillarItem(SleekJars jars, int index)
    {
        ItemStructureAsset asset = pillars[index].GetAsset<ItemStructureAsset>();
        SetSelectedAsset(asset);
    }

    private void SetSelectedAsset(ItemStructureAsset selectedAsset)
    {
        this.selectedAsset = selectedAsset;
        if (placementPreviewTransform != null)
        {
            UnityEngine.Object.Destroy(placementPreviewTransform.gameObject);
            placementPreviewTransform = null;
        }
        isPlacementPreviewValid = false;
        foundationPositionOffset = 0f;
        customRotationOffset = 0f;
        animatedRotationOffset = 0f;
        if (selectedAsset != null)
        {
            placementPreviewTransform = UseableHousingUtils.InstantiatePlacementPreview(selectedAsset);
            selectedItemNameLabel.Text = selectedAsset.itemName;
            selectedItemNameLabel.TextColor = ItemTool.getRarityColorUI(selectedAsset.rarity);
            if (itemAmounts.TryGetValue(selectedAsset.id, out var value))
            {
                selectedItemQuantityLabel.Text = "x" + value;
                selectedItemQuantityLabel.IsVisible = true;
            }
            else
            {
                selectedItemQuantityLabel.IsVisible = false;
            }
        }
        selectedItemBox.IsVisible = selectedAsset != null;
    }

    private bool UpdatePendingPlacement()
    {
        if (!UseableHousingUtils.FindPlacement(selectedAsset, base.player, customRotationOffset, foundationPositionOffset, out pendingPlacementPosition, out pendingPlacementYaw))
        {
            return false;
        }
        if (!UseableHousingUtils.IsPendingPositionValid(base.player, pendingPlacementPosition))
        {
            return false;
        }
        return true;
    }

    private void RefreshAvailableItems()
    {
        itemSearch.Clear();
        itemAmounts.Clear();
        base.player.inventory.search(itemSearch, EItemType.STRUCTURE);
        for (int num = itemSearch.Count - 1; num >= 0; num--)
        {
            InventorySearch inventorySearch = itemSearch[num];
            if (itemAmounts.TryGetValue(inventorySearch.jar.item.id, out var value))
            {
                itemSearch.RemoveAtFast(num);
            }
            itemAmounts[inventorySearch.jar.item.id] = value + inventorySearch.jar.item.amount;
        }
        if (selectedAsset != null)
        {
            if (itemAmounts.TryGetValue(selectedAsset.id, out var value2))
            {
                selectedItemQuantityLabel.Text = "x" + value2;
            }
            else
            {
                SetSelectedAsset(null);
            }
        }
    }

    private int CompareItemNames(InventorySearch lhs, InventorySearch rhs)
    {
        ItemAsset asset = lhs.GetAsset();
        ItemAsset asset2 = rhs.GetAsset();
        if (asset != null && asset2 != null)
        {
            return asset.itemName.CompareTo(asset2.itemName);
        }
        return 0;
    }
}
