using System;
using System.Collections.Generic;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class UseableHousingPlanner : Useable
{
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
            OneShotAudioDefinition oneShotAudioDefinition = Assets.coreMasterBundle.assetBundle.LoadAsset<OneShotAudioDefinition>("Assets/CoreMasterBundle/Sounds/Popup/Popup.asset");
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
            AudioClip audioClip = Assets.coreMasterBundle.assetBundle.LoadAsset<AudioClip>("Assets/CoreMasterBundle/Sounds/Error.wav");
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
        if (base.channel.isOwner && selectedAsset != null && UpdatePendingPlacement())
        {
            SendPlaceHousingItem.Invoke(GetNetId(), ENetReliability.Reliable, selectedAsset.GUID, pendingPlacementPosition, pendingPlacementYaw + customRotationOffset);
            return true;
        }
        return false;
    }

    public override bool startSecondary()
    {
        if (base.channel.isOwner && selectedAsset != null)
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
        if (base.channel.isOwner)
        {
            itemSearch = new List<InventorySearch>();
            floors = new List<InventorySearch>();
            roofs = new List<InventorySearch>();
            walls = new List<InventorySearch>();
            pillars = new List<InventorySearch>();
            itemAmounts = new Dictionary<ushort, int>();
            selectedItemBox = Glazier.Get().CreateBox();
            selectedItemBox.positionOffset_Y = -50;
            selectedItemBox.positionScale_X = 0.7f;
            selectedItemBox.positionScale_Y = 1f;
            selectedItemBox.sizeOffset_Y = 50;
            selectedItemBox.sizeScale_X = 0.3f;
            selectedItemBox.isVisible = false;
            PlayerLifeUI.container.AddChild(selectedItemBox);
            selectedItemNameLabel = Glazier.Get().CreateLabel();
            selectedItemNameLabel.positionOffset_X = 10;
            selectedItemNameLabel.sizeScale_X = 1f;
            selectedItemNameLabel.sizeScale_Y = 1f;
            selectedItemNameLabel.sizeOffset_X = -20;
            selectedItemNameLabel.fontAlignment = TextAnchor.MiddleRight;
            selectedItemNameLabel.fontSize = ESleekFontSize.Large;
            selectedItemBox.AddChild(selectedItemNameLabel);
            selectedItemNameLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            selectedItemQuantityLabel = Glazier.Get().CreateLabel();
            selectedItemQuantityLabel.positionOffset_X = 10;
            selectedItemQuantityLabel.sizeScale_X = 1f;
            selectedItemQuantityLabel.sizeScale_Y = 1f;
            selectedItemQuantityLabel.sizeOffset_X = -20;
            selectedItemQuantityLabel.fontAlignment = TextAnchor.MiddleLeft;
            selectedItemQuantityLabel.fontSize = ESleekFontSize.Large;
            selectedItemBox.AddChild(selectedItemQuantityLabel);
            selectedItemQuantityLabel.shadowStyle = ETextContrastContext.InconspicuousBackdrop;
            Local local = Localization.read("/Player/Useable/PlayerUseableHousingPlanner.dat");
            Bundle bundle = Bundles.getBundle("/Bundles/Textures/Player/Icons/Useable/PlayerUseableHousingPlanner/PlayerUseableHousingPlanner.unity3d");
            Texture texture = bundle.load<Texture>("RadialMenu");
            bundle.unload();
            itemSelectionContainer = Glazier.Get().CreateFrame();
            itemSelectionContainer.sizeScale_X = 1f;
            itemSelectionContainer.sizeScale_Y = 1f;
            itemSelectionContainer.isVisible = false;
            PlayerUI.container.AddChild(itemSelectionContainer);
            ISleekImage sleekImage = Glazier.Get().CreateImage(texture);
            sleekImage.positionScale_X = 0.5f;
            sleekImage.positionScale_Y = 0.5f;
            sleekImage.positionOffset_X = 50;
            sleekImage.positionOffset_Y = -306;
            sleekImage.sizeOffset_X = 256;
            sleekImage.sizeOffset_Y = 256;
            sleekImage.color = SleekColor.BackgroundIfLight(new Color(0f, 0f, 0f, 0.2f));
            itemSelectionContainer.AddChild(sleekImage);
            floorsLabel = Glazier.Get().CreateLabel();
            floorsLabel.positionScale_X = 0.5f;
            floorsLabel.positionScale_Y = 0.5f;
            floorsLabel.positionOffset_X = 50;
            floorsLabel.positionOffset_Y = -306;
            floorsLabel.sizeOffset_X = 256;
            floorsLabel.sizeOffset_Y = 256;
            floorsLabel.fontSize = ESleekFontSize.Large;
            floorsLabel.text = local.format("Floors");
            itemSelectionContainer.AddChild(floorsLabel);
            floorsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            noFloorItemsLabel = Glazier.Get().CreateLabel();
            noFloorItemsLabel.positionScale_X = 0.5f;
            noFloorItemsLabel.positionScale_Y = 0.5f;
            noFloorItemsLabel.positionOffset_X = 50;
            noFloorItemsLabel.positionOffset_Y = -286;
            noFloorItemsLabel.sizeOffset_X = 256;
            noFloorItemsLabel.sizeOffset_Y = 256;
            noFloorItemsLabel.fontSize = ESleekFontSize.Medium;
            noFloorItemsLabel.textColor = ESleekTint.BAD;
            noFloorItemsLabel.text = local.format("NoItems");
            noFloorItemsLabel.isVisible = false;
            itemSelectionContainer.AddChild(noFloorItemsLabel);
            noFloorItemsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            ISleekImage sleekImage2 = Glazier.Get().CreateImage(texture);
            sleekImage2.positionScale_X = 0.5f;
            sleekImage2.positionScale_Y = 0.5f;
            sleekImage2.positionOffset_X = 50;
            sleekImage2.positionOffset_Y = 50;
            sleekImage2.sizeOffset_X = 256;
            sleekImage2.sizeOffset_Y = 256;
            sleekImage2.color = SleekColor.BackgroundIfLight(new Color(0f, 0f, 0f, 0.2f));
            itemSelectionContainer.AddChild(sleekImage2);
            roofsLabel = Glazier.Get().CreateLabel();
            roofsLabel.positionScale_X = 0.5f;
            roofsLabel.positionScale_Y = 0.5f;
            roofsLabel.positionOffset_X = 50;
            roofsLabel.positionOffset_Y = 50;
            roofsLabel.sizeOffset_X = 256;
            roofsLabel.sizeOffset_Y = 256;
            roofsLabel.fontSize = ESleekFontSize.Large;
            roofsLabel.text = local.format("Roofs");
            itemSelectionContainer.AddChild(roofsLabel);
            roofsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            noRoofItemsLabel = Glazier.Get().CreateLabel();
            noRoofItemsLabel.positionScale_X = 0.5f;
            noRoofItemsLabel.positionScale_Y = 0.5f;
            noRoofItemsLabel.positionOffset_X = 50;
            noRoofItemsLabel.positionOffset_Y = 70;
            noRoofItemsLabel.sizeOffset_X = 256;
            noRoofItemsLabel.sizeOffset_Y = 256;
            noRoofItemsLabel.fontSize = ESleekFontSize.Medium;
            noRoofItemsLabel.textColor = ESleekTint.BAD;
            noRoofItemsLabel.text = local.format("NoItems");
            noRoofItemsLabel.isVisible = false;
            itemSelectionContainer.AddChild(noRoofItemsLabel);
            noRoofItemsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            ISleekImage sleekImage3 = Glazier.Get().CreateImage(texture);
            sleekImage3.positionScale_X = 0.5f;
            sleekImage3.positionScale_Y = 0.5f;
            sleekImage3.positionOffset_X = -306;
            sleekImage3.positionOffset_Y = -306;
            sleekImage3.sizeOffset_X = 256;
            sleekImage3.sizeOffset_Y = 256;
            sleekImage3.color = SleekColor.BackgroundIfLight(new Color(0f, 0f, 0f, 0.2f));
            itemSelectionContainer.AddChild(sleekImage3);
            wallsLabel = Glazier.Get().CreateLabel();
            wallsLabel.positionScale_X = 0.5f;
            wallsLabel.positionScale_Y = 0.5f;
            wallsLabel.positionOffset_X = -306;
            wallsLabel.positionOffset_Y = -306;
            wallsLabel.sizeOffset_X = 256;
            wallsLabel.sizeOffset_Y = 256;
            wallsLabel.fontSize = ESleekFontSize.Large;
            wallsLabel.text = local.format("Walls");
            itemSelectionContainer.AddChild(wallsLabel);
            wallsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            noWallItemsLabel = Glazier.Get().CreateLabel();
            noWallItemsLabel.positionScale_X = 0.5f;
            noWallItemsLabel.positionScale_Y = 0.5f;
            noWallItemsLabel.positionOffset_X = -306;
            noWallItemsLabel.positionOffset_Y = -286;
            noWallItemsLabel.sizeOffset_X = 256;
            noWallItemsLabel.sizeOffset_Y = 256;
            noWallItemsLabel.fontSize = ESleekFontSize.Medium;
            noWallItemsLabel.textColor = ESleekTint.BAD;
            noWallItemsLabel.text = local.format("NoItems");
            noWallItemsLabel.isVisible = false;
            itemSelectionContainer.AddChild(noWallItemsLabel);
            noWallItemsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            ISleekImage sleekImage4 = Glazier.Get().CreateImage(texture);
            sleekImage4.positionScale_X = 0.5f;
            sleekImage4.positionScale_Y = 0.5f;
            sleekImage4.positionOffset_X = -306;
            sleekImage4.positionOffset_Y = 50;
            sleekImage4.sizeOffset_X = 256;
            sleekImage4.sizeOffset_Y = 256;
            sleekImage4.color = SleekColor.BackgroundIfLight(new Color(0f, 0f, 0f, 0.2f));
            itemSelectionContainer.AddChild(sleekImage4);
            pillarsLabel = Glazier.Get().CreateLabel();
            pillarsLabel.positionScale_X = 0.5f;
            pillarsLabel.positionScale_Y = 0.5f;
            pillarsLabel.positionOffset_X = -306;
            pillarsLabel.positionOffset_Y = 50;
            pillarsLabel.sizeOffset_X = 256;
            pillarsLabel.sizeOffset_Y = 256;
            pillarsLabel.fontSize = ESleekFontSize.Large;
            pillarsLabel.text = local.format("Pillars");
            itemSelectionContainer.AddChild(pillarsLabel);
            pillarsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            noPillarItemsLabel = Glazier.Get().CreateLabel();
            noPillarItemsLabel.positionScale_X = 0.5f;
            noPillarItemsLabel.positionScale_Y = 0.5f;
            noPillarItemsLabel.positionOffset_X = -306;
            noPillarItemsLabel.positionOffset_Y = 70;
            noPillarItemsLabel.sizeOffset_X = 256;
            noPillarItemsLabel.sizeOffset_Y = 256;
            noPillarItemsLabel.fontSize = ESleekFontSize.Medium;
            noPillarItemsLabel.textColor = ESleekTint.BAD;
            noPillarItemsLabel.text = local.format("NoItems");
            noPillarItemsLabel.isVisible = false;
            itemSelectionContainer.AddChild(noPillarItemsLabel);
            noPillarItemsLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            PlayerUI.message(EPlayerMessage.HOUSING_PLANNER_TUTORIAL, "");
        }
    }

    public override void dequip()
    {
        if (base.channel.isOwner)
        {
            SetItemSelectionMenuOpen(isOpen: false);
            SetSelectedAsset(null);
            PlayerUI.container.RemoveChild(selectedItemBox);
            PlayerUI.container.RemoveChild(itemSelectionContainer);
        }
    }

    public override void tick()
    {
        if (!base.channel.isOwner)
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
        itemSelectionContainer.isVisible = isOpen;
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
        noFloorItemsLabel.isVisible = floors.Count < 1;
        noRoofItemsLabel.isVisible = roofs.Count < 1;
        noWallItemsLabel.isVisible = walls.Count < 1;
        noPillarItemsLabel.isVisible = pillars.Count < 1;
        floorsMenu = new SleekJars(128f, floors, (float)Math.PI * 3f / 4f);
        floorsMenu.positionScale_X = 0.5f;
        floorsMenu.positionScale_Y = 0.5f;
        floorsMenu.positionOffset_X = 50;
        floorsMenu.positionOffset_Y = -306;
        floorsMenu.sizeOffset_X = 256;
        floorsMenu.sizeOffset_Y = 256;
        SleekJars sleekJars = floorsMenu;
        sleekJars.onClickedJar = (ClickedJar)Delegate.Combine(sleekJars.onClickedJar, new ClickedJar(OnSelectedFloorItem));
        itemSelectionContainer.AddChild(floorsMenu);
        roofsMenu = new SleekJars(128f, roofs, 3.926991f);
        roofsMenu.positionScale_X = 0.5f;
        roofsMenu.positionScale_Y = 0.5f;
        roofsMenu.positionOffset_X = 50;
        roofsMenu.positionOffset_Y = 50;
        roofsMenu.sizeOffset_X = 256;
        roofsMenu.sizeOffset_Y = 256;
        SleekJars sleekJars2 = roofsMenu;
        sleekJars2.onClickedJar = (ClickedJar)Delegate.Combine(sleekJars2.onClickedJar, new ClickedJar(OnSelectedRoofItem));
        itemSelectionContainer.AddChild(roofsMenu);
        wallsMenu = new SleekJars(128f, walls, (float)Math.PI / 4f);
        wallsMenu.positionScale_X = 0.5f;
        wallsMenu.positionScale_Y = 0.5f;
        wallsMenu.positionOffset_X = -306;
        wallsMenu.positionOffset_Y = -306;
        wallsMenu.sizeOffset_X = 256;
        wallsMenu.sizeOffset_Y = 256;
        SleekJars sleekJars3 = wallsMenu;
        sleekJars3.onClickedJar = (ClickedJar)Delegate.Combine(sleekJars3.onClickedJar, new ClickedJar(OnSelectedWallItem));
        itemSelectionContainer.AddChild(wallsMenu);
        pillarsMenu = new SleekJars(128f, pillars, 5.4977875f);
        pillarsMenu.positionScale_X = 0.5f;
        pillarsMenu.positionScale_Y = 0.5f;
        pillarsMenu.positionOffset_X = -306;
        pillarsMenu.positionOffset_Y = 50;
        pillarsMenu.sizeOffset_X = 256;
        pillarsMenu.sizeOffset_Y = 256;
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
            selectedItemNameLabel.text = selectedAsset.itemName;
            selectedItemNameLabel.textColor = ItemTool.getRarityColorUI(selectedAsset.rarity);
            if (itemAmounts.TryGetValue(selectedAsset.id, out var value))
            {
                selectedItemQuantityLabel.text = "x" + value;
                selectedItemQuantityLabel.isVisible = true;
            }
            else
            {
                selectedItemQuantityLabel.isVisible = false;
            }
        }
        selectedItemBox.isVisible = selectedAsset != null;
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
                selectedItemQuantityLabel.text = "x" + value2;
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
