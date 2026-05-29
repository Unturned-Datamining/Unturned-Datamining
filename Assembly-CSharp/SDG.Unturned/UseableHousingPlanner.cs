using System;
using System.Collections.Generic;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class UseableHousingPlanner : Useable
{
    private struct RelevantBlueprint
    {
        public Blueprint blueprint;

        public int structureOutputIndex;

        public RelevantBlueprint(Blueprint blueprint, int structureOutputIndex)
        {
            this.blueprint = blueprint;
            this.structureOutputIndex = structureOutputIndex;
        }
    }

    private struct CraftableBlueprint
    {
        public BlueprintStatus status;

        public int structureOutputIndex;

        public CraftableBlueprint(BlueprintStatus status, int structureOutputIndex)
        {
            this.status = status;
            this.structureOutputIndex = structureOutputIndex;
        }
    }

    private struct ItemOption
    {
        public ItemStructureAsset asset;

        public CraftableBlueprint craftable;

        public ItemOption(ItemStructureAsset asset, CraftableBlueprint craftable)
        {
            this.asset = asset;
            this.craftable = craftable;
        }
    }

    private class SleekHousingPlannerOption : SleekWrapper
    {
        private UseableHousingPlanner useable;

        private ItemOption option;

        private ISleekButton button;

        private SleekItemIcon icon;

        private ISleekLabel amountLabel;

        public SleekHousingPlannerOption(UseableHousingPlanner useable, ItemOption option)
        {
            this.useable = useable;
            this.option = option;
            ItemStructureAsset asset = option.asset;
            base.SizeOffset_X = asset.size_x * 50;
            base.SizeOffset_Y = asset.size_y * 50;
            Color rarityColorUI = ItemTool.getRarityColorUI(asset.rarity);
            button = Glazier.Get().CreateButton();
            button.SizeScale_X = 1f;
            button.SizeScale_Y = 1f;
            button.BackgroundColor = SleekColor.BackgroundIfLight(rarityColorUI);
            button.TextColor = rarityColorUI;
            button.TooltipText = asset.itemName;
            button.OnClicked += OnClicked;
            AddChild(button);
            icon = new SleekItemIcon();
            icon.SizeScale_X = 1f;
            icon.SizeScale_Y = 1f;
            icon.Refresh(asset, Mathf.RoundToInt(base.SizeOffset_X), Mathf.RoundToInt(base.SizeOffset_Y));
            AddChild(icon);
            amountLabel = Glazier.Get().CreateLabel();
            amountLabel.PositionScale_Y = 1f;
            amountLabel.SizeOffset_Y = 30f;
            amountLabel.SizeScale_X = 1f;
            if (asset.size_x == 1 || asset.size_y == 1)
            {
                amountLabel.PositionOffset_X = 0f;
                amountLabel.PositionOffset_Y = -30f;
                amountLabel.SizeOffset_X = 0f;
                amountLabel.FontSize = ESleekFontSize.Small;
            }
            else
            {
                amountLabel.PositionOffset_X = 5f;
                amountLabel.PositionOffset_Y = -35f;
                amountLabel.SizeOffset_X = -10f;
                amountLabel.FontSize = ESleekFontSize.Default;
            }
            useable.itemAmounts.TryGetValue(asset.id, out var value);
            int num = option.craftable.status?.EstimateOutputMaxAmount(option.craftable.structureOutputIndex) ?? 0;
            amountLabel.Text = $"{value}+{num}";
            amountLabel.TextAlignment = TextAnchor.LowerLeft;
            amountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            AddChild(amountLabel);
        }

        private void OnClicked(ISleekElement button)
        {
            useable.SetSelectedOption(option);
        }
    }

    private static MasterBundleReference<OneShotAudioDefinition> popupAudioRef = new MasterBundleReference<OneShotAudioDefinition>("core.masterbundle", "Sounds/Popup/Popup.asset");

    private static MasterBundleReference<AudioClip> errorClipRef = new MasterBundleReference<AudioClip>("core.masterbundle", "Sounds/Error.wav");

    private static readonly ClientInstanceMethod<bool> SendPlaceHousingItemResult = ClientInstanceMethod<bool>.Get(typeof(UseableHousingPlanner), "ReceivePlaceHousingItemResult");

    private static readonly ServerInstanceMethod<Guid, Vector3, float, Guid, byte> SendPlaceHousingItem = ServerInstanceMethod<Guid, Vector3, float, Guid, byte>.Get(typeof(UseableHousingPlanner), "ReceivePlaceHousingItem");

    /// <summary>
    /// Stripped-down version of structure prefab for previewing where the structure will be spawned.
    /// </summary>
    private Transform placementPreviewTransform;

    /// <summary>
    /// Whether preview object is currently highlighted positively.
    /// </summary>
    private bool isPlacementPreviewValid;

    /// <summary>
    /// Position the item should be spawned at.
    /// </summary>
    private Vector3 pendingPlacementPosition;

    /// <summary>
    /// Rotation the item should be spawned at.
    /// </summary>
    private float pendingPlacementYaw;

    /// <summary>
    /// Interpolated toward customRotationOffset.
    /// </summary>
    private float animatedRotationOffset;

    /// <summary>
    /// Allows players to flip walls.
    /// </summary>
    private float customRotationOffset;

    /// <summary>
    /// Vertical offset using scroll wheel.
    /// </summary>
    private float foundationPositionOffset;

    private ItemOption selectedOption;

    private bool isItemSelectionMenuOpen;

    private ISleekElement itemSelectionContainer;

    private SleekCircularContainer floorsMenu;

    private SleekCircularContainer roofsMenu;

    private SleekCircularContainer wallsMenu;

    private SleekCircularContainer pillarsMenu;

    private ISleekLabel floorsLabel;

    private ISleekLabel noFloorItemsLabel;

    private ISleekLabel roofsLabel;

    private ISleekLabel noRoofItemsLabel;

    private ISleekLabel wallsLabel;

    private ISleekLabel noWallItemsLabel;

    private ISleekLabel pillarsLabel;

    private ISleekLabel noPillarItemsLabel;

    /// <summary>
    /// Box in the HUD with selected item name and quantity.
    /// </summary>
    private ISleekBox selectedItemBox;

    private ISleekLabel selectedItemNameLabel;

    private ISleekLabel selectedItemAvailableAmountLabel;

    private ISleekLabel selectedItemCraftableAmountLabel;

    /// <summary>
    /// Blueprints which create a structure item.
    /// </summary>
    private List<RelevantBlueprint> relevantBlueprints;

    /// <summary>
    /// One craftable blueprint per potential structure item.
    /// </summary>
    private Dictionary<ItemStructureAsset, CraftableBlueprint> craftableBlueprints;

    /// <summary>
    /// Recycled blueprint statuses.
    /// </summary>
    private Stack<BlueprintStatus> blueprintStatusPool;

    private List<PlayerInventorySearchResultV2> itemSearchResults;

    private List<ItemOption> floors;

    private List<ItemOption> roofs;

    private List<ItemOption> walls;

    private List<ItemOption> pillars;

    private Dictionary<ushort, int> itemAmounts;

    private int cachedSearchIndex = -1;

    private int cachedAssetListChangeCounter = -1;

    private const float MENU_RADIUS = 128f;

    private const int MENU_SIZE = 256;

    private const int MENU_PADDING = 50;

    private const float RADIAL_BACKDROP_ALPHA = 0.2f;

    private static Local localization;

    private const bool bypassWorkstationRequirements = true;

    public override bool isUseableShowingMenu => isItemSelectionMenuOpen;

    private bool HasSelection => selectedOption.asset != null;

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

    private bool ReceivePlaceHousingItemInternal(in ServerInvocationContext context, Guid assetGuid, Vector3 position, float yaw, Guid blueprintGuid, byte blueprintIndex)
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
        if (!base.player.inventory.FindFirstItemByAsset(itemStructureAsset, out var result))
        {
            Asset asset = Assets.find(blueprintGuid);
            if (asset == null)
            {
                return false;
            }
            if (!(asset is IBlueprintOwner blueprintOwner))
            {
                return false;
            }
            Blueprint blueprintByIndex = blueprintOwner.GetBlueprintByIndex(blueprintIndex);
            if (blueprintByIndex == null)
            {
                return false;
            }
            if (!blueprintByIndex.DoesOutputCreateItem(itemStructureAsset))
            {
                return false;
            }
            if (!base.player.crafting.HandleCraftRequestInternal(in context, blueprintByIndex, asManyAsPossible: false, playEffect: false, bypassWorkstationRequirements: true))
            {
                return false;
            }
            if (!base.player.inventory.FindFirstItemByAsset(itemStructureAsset, out result))
            {
                return false;
            }
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
            result.DeleteAmount(base.player, 1u);
        }
        return num;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
    public void ReceivePlaceHousingItem(in ServerInvocationContext context, Guid assetGuid, Vector3 position, float yaw, Guid blueprintGuid, byte blueprintIndex)
    {
        bool arg = ReceivePlaceHousingItemInternal(in context, assetGuid, position, yaw, blueprintGuid, blueprintIndex);
        SendPlaceHousingItemResult.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GetOwnerTransportConnection(), arg);
    }

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (base.channel.IsLocalPlayer && HasSelection && UpdatePendingPlacement())
        {
            itemAmounts.TryGetValue(selectedOption.asset.id, out var value);
            Guid arg = default(Guid);
            byte arg2 = 0;
            if (value < 1 && selectedOption.craftable.status?.blueprint != null)
            {
                Asset ownerAsset = selectedOption.craftable.status.blueprint.GetOwnerAsset();
                if (ownerAsset != null)
                {
                    arg = ownerAsset.GUID;
                    arg2 = selectedOption.craftable.status.blueprint.Index;
                }
            }
            SendPlaceHousingItem.Invoke(GetNetId(), ENetReliability.Reliable, selectedOption.asset.GUID, pendingPlacementPosition, pendingPlacementYaw + customRotationOffset, arg, arg2);
            return true;
        }
        return false;
    }

    public override bool startSecondary()
    {
        if (base.channel.IsLocalPlayer && HasSelection)
        {
            float num;
            switch (selectedOption.asset.construct)
            {
            case EConstruct.FLOOR_POLY:
            case EConstruct.ROOF_POLY:
                return false;
            case EConstruct.FLOOR:
            case EConstruct.ROOF:
                num = 90f;
                break;
            case EConstruct.WALL:
            case EConstruct.RAMPART:
                num = 180f;
                break;
            default:
                num = 30f;
                break;
            }
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
            relevantBlueprints = new List<RelevantBlueprint>();
            craftableBlueprints = new Dictionary<ItemStructureAsset, CraftableBlueprint>();
            blueprintStatusPool = new Stack<BlueprintStatus>();
            itemSearchResults = new List<PlayerInventorySearchResultV2>();
            floors = new List<ItemOption>();
            roofs = new List<ItemOption>();
            walls = new List<ItemOption>();
            pillars = new List<ItemOption>();
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
            selectedItemAvailableAmountLabel = Glazier.Get().CreateLabel();
            selectedItemAvailableAmountLabel.PositionOffset_X = 10f;
            selectedItemAvailableAmountLabel.SizeScale_X = 1f;
            selectedItemAvailableAmountLabel.SizeOffset_X = -20f;
            selectedItemAvailableAmountLabel.SizeOffset_Y = 30f;
            selectedItemAvailableAmountLabel.TextAlignment = TextAnchor.MiddleLeft;
            selectedItemAvailableAmountLabel.FontSize = ESleekFontSize.Medium;
            selectedItemBox.AddChild(selectedItemAvailableAmountLabel);
            selectedItemAvailableAmountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            selectedItemCraftableAmountLabel = Glazier.Get().CreateLabel();
            selectedItemCraftableAmountLabel.PositionOffset_X = 10f;
            selectedItemCraftableAmountLabel.PositionOffset_Y = 20f;
            selectedItemCraftableAmountLabel.SizeScale_X = 1f;
            selectedItemCraftableAmountLabel.SizeOffset_X = -20f;
            selectedItemCraftableAmountLabel.SizeOffset_Y = 30f;
            selectedItemCraftableAmountLabel.TextAlignment = TextAnchor.MiddleLeft;
            selectedItemCraftableAmountLabel.FontSize = ESleekFontSize.Medium;
            selectedItemBox.AddChild(selectedItemCraftableAmountLabel);
            selectedItemCraftableAmountLabel.TextContrastContext = ETextContrastContext.InconspicuousBackdrop;
            localization = Localization.read("/Player/Useable/PlayerUseableHousingPlanner.dat");
            Texture texture = Bundles.getIconsBundle("UI/Player/Icons/Useable/PlayerUseableHousingPlanner").load<Texture>("RadialMenu");
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
            floorsLabel.Text = localization.format("Floors");
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
            noFloorItemsLabel.Text = localization.format("NoItems");
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
            roofsLabel.Text = localization.format("Roofs");
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
            noRoofItemsLabel.Text = localization.format("NoItems");
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
            wallsLabel.Text = localization.format("Walls");
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
            noWallItemsLabel.Text = localization.format("NoItems");
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
            pillarsLabel.Text = localization.format("Pillars");
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
            noPillarItemsLabel.Text = localization.format("NoItems");
            noPillarItemsLabel.IsVisible = false;
            itemSelectionContainer.AddChild(noPillarItemsLabel);
            noPillarItemsLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            floorsMenu = new SleekCircularContainer(128f, MathF.PI * 3f / 4f);
            floorsMenu.PositionScale_X = 0.5f;
            floorsMenu.PositionScale_Y = 0.5f;
            floorsMenu.PositionOffset_X = 50f;
            floorsMenu.PositionOffset_Y = -306f;
            floorsMenu.SizeOffset_X = 256f;
            floorsMenu.SizeOffset_Y = 256f;
            itemSelectionContainer.AddChild(floorsMenu);
            roofsMenu = new SleekCircularContainer(128f, 3.926991f);
            roofsMenu.PositionScale_X = 0.5f;
            roofsMenu.PositionScale_Y = 0.5f;
            roofsMenu.PositionOffset_X = 50f;
            roofsMenu.PositionOffset_Y = 50f;
            roofsMenu.SizeOffset_X = 256f;
            roofsMenu.SizeOffset_Y = 256f;
            itemSelectionContainer.AddChild(roofsMenu);
            wallsMenu = new SleekCircularContainer(128f, MathF.PI / 4f);
            wallsMenu.PositionScale_X = 0.5f;
            wallsMenu.PositionScale_Y = 0.5f;
            wallsMenu.PositionOffset_X = -306f;
            wallsMenu.PositionOffset_Y = -306f;
            wallsMenu.SizeOffset_X = 256f;
            wallsMenu.SizeOffset_Y = 256f;
            itemSelectionContainer.AddChild(wallsMenu);
            pillarsMenu = new SleekCircularContainer(128f, 5.4977875f);
            pillarsMenu.PositionScale_X = 0.5f;
            pillarsMenu.PositionScale_Y = 0.5f;
            pillarsMenu.PositionOffset_X = -306f;
            pillarsMenu.PositionOffset_Y = 50f;
            pillarsMenu.SizeOffset_X = 256f;
            pillarsMenu.SizeOffset_Y = 256f;
            itemSelectionContainer.AddChild(pillarsMenu);
            PlayerUI.message(EPlayerMessage.HOUSING_PLANNER_TUTORIAL, "");
        }
    }

    public override void dequip()
    {
        if (base.channel.IsLocalPlayer)
        {
            SetItemSelectionMenuOpen(isOpen: false);
            DestroyPlacementPreview();
            PlayerLifeUI.container.RemoveChild(selectedItemBox);
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
            RefreshAvailableItemsAndSelectedBlueprint();
        }
        if (Assets.HasCurrentAssetMappingChanged(ref cachedAssetListChangeCounter))
        {
            RefreshRelevantBlueprints();
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
        RefreshAllCraftableBlueprints();
        floors.Clear();
        roofs.Clear();
        walls.Clear();
        pillars.Clear();
        foreach (PlayerInventorySearchResultV2 itemSearchResult in itemSearchResults)
        {
            ItemStructureAsset asset = itemSearchResult.GetAsset<ItemStructureAsset>();
            craftableBlueprints.TryGetValue(asset, out var value);
            switch (asset.construct)
            {
            case EConstruct.FLOOR:
            case EConstruct.FLOOR_POLY:
                floors.Add(new ItemOption(asset, value));
                break;
            case EConstruct.ROOF:
            case EConstruct.ROOF_POLY:
                roofs.Add(new ItemOption(asset, value));
                break;
            case EConstruct.WALL:
            case EConstruct.RAMPART:
                walls.Add(new ItemOption(asset, value));
                break;
            case EConstruct.PILLAR:
            case EConstruct.POST:
                pillars.Add(new ItemOption(asset, value));
                break;
            }
        }
        foreach (KeyValuePair<ItemStructureAsset, CraftableBlueprint> craftableBlueprint in craftableBlueprints)
        {
            ItemStructureAsset key = craftableBlueprint.Key;
            if (!itemAmounts.ContainsKey(key.id))
            {
                switch (key.construct)
                {
                case EConstruct.FLOOR:
                case EConstruct.FLOOR_POLY:
                    floors.Add(new ItemOption(key, craftableBlueprint.Value));
                    break;
                case EConstruct.ROOF:
                case EConstruct.ROOF_POLY:
                    roofs.Add(new ItemOption(key, craftableBlueprint.Value));
                    break;
                case EConstruct.WALL:
                case EConstruct.RAMPART:
                    walls.Add(new ItemOption(key, craftableBlueprint.Value));
                    break;
                case EConstruct.PILLAR:
                case EConstruct.POST:
                    pillars.Add(new ItemOption(key, craftableBlueprint.Value));
                    break;
                }
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
        PopulateCircularMenu(floorsMenu, floors);
        PopulateCircularMenu(roofsMenu, roofs);
        PopulateCircularMenu(wallsMenu, walls);
        PopulateCircularMenu(pillarsMenu, pillars);
    }

    private void PopulateCircularMenu(SleekCircularContainer container, List<ItemOption> options)
    {
        container.RemoveAllChildren();
        foreach (ItemOption option in options)
        {
            SleekHousingPlannerOption sleek = new SleekHousingPlannerOption(this, option);
            container.AddChild(sleek);
        }
        container.UpdateLayout();
    }

    private void DestroyPlacementPreview()
    {
        if (placementPreviewTransform != null)
        {
            UnityEngine.Object.Destroy(placementPreviewTransform.gameObject);
            placementPreviewTransform = null;
        }
    }

    private void ClearSelectedOption()
    {
        SetSelectedOption(default(ItemOption));
    }

    private void SetSelectedOption(ItemOption selectedOption)
    {
        this.selectedOption = selectedOption;
        DestroyPlacementPreview();
        isPlacementPreviewValid = false;
        foundationPositionOffset = 0f;
        customRotationOffset = 0f;
        animatedRotationOffset = 0f;
        if (HasSelection)
        {
            placementPreviewTransform = UseableHousingUtils.InstantiatePlacementPreview(selectedOption.asset);
            selectedItemNameLabel.Text = selectedOption.asset.itemName;
            selectedItemNameLabel.TextColor = ItemTool.getRarityColorUI(selectedOption.asset.rarity);
            itemAmounts.TryGetValue(selectedOption.asset.id, out var value);
            int num = selectedOption.craftable.status?.EstimateOutputMaxAmount(selectedOption.craftable.structureOutputIndex) ?? 0;
            selectedItemAvailableAmountLabel.Text = localization.format("AvailableAmount", value);
            selectedItemCraftableAmountLabel.Text = localization.format("CraftableAmount", num);
        }
        selectedItemBox.IsVisible = HasSelection;
    }

    private bool UpdatePendingPlacement()
    {
        if (!UseableHousingUtils.FindPlacement(selectedOption.asset, base.player, customRotationOffset, foundationPositionOffset, out pendingPlacementPosition, out pendingPlacementYaw))
        {
            return false;
        }
        if (!UseableHousingUtils.IsPendingPositionValid(base.player, pendingPlacementPosition))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Search loaded assets for blueprints that output a single structure item and are
    /// available on the current map.
    /// </summary>
    private void RefreshRelevantBlueprints()
    {
        if (!Level.IsCraftingAllowedByLevel)
        {
            return;
        }
        PlayerCrafting crafting = base.player.crafting;
        relevantBlueprints.Clear();
        foreach (IBlueprintOwner blueprintOwner in PlayerDashboardCraftingUI.GetBlueprintOwners())
        {
            foreach (Blueprint blueprint in blueprintOwner.GetBlueprints())
            {
                if (blueprint.outputs == null || blueprint.outputs.Length < 1)
                {
                    continue;
                }
                int num = -1;
                for (int i = 0; i < blueprint.outputs.Length; i++)
                {
                    if (blueprint.outputs[i].FindItemAsset<ItemStructureAsset>() != null)
                    {
                        num = i;
                        break;
                    }
                }
                if (num >= 0 && !crafting.IsBlueprintPermanentlyDisabled(blueprint))
                {
                    relevantBlueprints.Add(new RelevantBlueprint(blueprint, num));
                }
            }
        }
    }

    /// <summary>
    /// Update status of all relevant blueprints.
    /// </summary>
    private void RefreshAllCraftableBlueprints()
    {
        foreach (CraftableBlueprint value in craftableBlueprints.Values)
        {
            blueprintStatusPool.Push(value.status);
        }
        craftableBlueprints.Clear();
        PlayerCrafting crafting = base.player.crafting;
        foreach (RelevantBlueprint relevantBlueprint in relevantBlueprints)
        {
            Blueprint blueprint = relevantBlueprint.blueprint;
            ItemStructureAsset itemStructureAsset = blueprint.outputs[relevantBlueprint.structureOutputIndex].FindItemAsset<ItemStructureAsset>();
            if (itemStructureAsset == null || craftableBlueprints.ContainsKey(itemStructureAsset))
            {
                continue;
            }
            BlueprintStatus blueprintStatus = CreateBlueprintStatus();
            blueprintStatus.blueprint = blueprint;
            UpdateBlueprintStatusParameters updateBlueprintStatusParameters = default(UpdateBlueprintStatusParameters);
            updateBlueprintStatusParameters.status = blueprintStatus;
            updateBlueprintStatusParameters.shouldExitEarly = true;
            UpdateBlueprintStatusParameters p = updateBlueprintStatusParameters;
            crafting.UpdateBlueprintStaticStatus(in p, bypassWorkstationRequirements: true);
            if (!blueprintStatus.IsCraftable)
            {
                blueprintStatusPool.Push(blueprintStatus);
                continue;
            }
            crafting.UpdateBlueprintDynamicStatus(in p);
            if (!blueprintStatus.IsCraftable)
            {
                blueprintStatusPool.Push(blueprintStatus);
            }
            else
            {
                craftableBlueprints.Add(itemStructureAsset, new CraftableBlueprint(blueprintStatus, relevantBlueprint.structureOutputIndex));
            }
        }
        if (HasSelection)
        {
            craftableBlueprints.TryGetValue(selectedOption.asset, out selectedOption.craftable);
            int num = selectedOption.craftable.status?.EstimateOutputMaxAmount(selectedOption.craftable.structureOutputIndex) ?? 0;
            selectedItemCraftableAmountLabel.Text = localization.format("CraftableAmount", num);
        }
    }

    /// <summary>
    /// Currently saved craftableBlueprint for asset may have become uncraftable,
    /// in which case we try finding a craftable replacement.
    /// </summary>
    private void RefreshCraftableBlueprint(ItemStructureAsset forAsset)
    {
        if (craftableBlueprints.TryGetValue(forAsset, out var value))
        {
            blueprintStatusPool.Push(value.status);
            craftableBlueprints.Remove(forAsset);
        }
        PlayerCrafting crafting = base.player.crafting;
        foreach (RelevantBlueprint relevantBlueprint in relevantBlueprints)
        {
            Blueprint blueprint = relevantBlueprint.blueprint;
            if (blueprint.outputs[relevantBlueprint.structureOutputIndex].FindItemAsset<ItemStructureAsset>() != forAsset)
            {
                continue;
            }
            BlueprintStatus blueprintStatus = CreateBlueprintStatus();
            blueprintStatus.blueprint = blueprint;
            UpdateBlueprintStatusParameters updateBlueprintStatusParameters = default(UpdateBlueprintStatusParameters);
            updateBlueprintStatusParameters.status = blueprintStatus;
            updateBlueprintStatusParameters.shouldExitEarly = true;
            UpdateBlueprintStatusParameters p = updateBlueprintStatusParameters;
            crafting.UpdateBlueprintStaticStatus(in p, bypassWorkstationRequirements: true);
            if (!blueprintStatus.IsCraftable)
            {
                blueprintStatusPool.Push(blueprintStatus);
                continue;
            }
            crafting.UpdateBlueprintDynamicStatus(in p);
            if (!blueprintStatus.IsCraftable)
            {
                blueprintStatusPool.Push(blueprintStatus);
                continue;
            }
            craftableBlueprints.Add(forAsset, new CraftableBlueprint(blueprintStatus, relevantBlueprint.structureOutputIndex));
            break;
        }
    }

    /// <summary>
    /// Get a blank status from the pool or construct a new one.
    /// </summary>
    private BlueprintStatus CreateBlueprintStatus()
    {
        if (blueprintStatusPool.TryPop(out var result))
        {
            result.Reset();
            return result;
        }
        return new BlueprintStatus();
    }

    /// <summary>
    /// Search inventory for housing items, count the quantity of each, and remove
    /// duplicate entries from the list because it is used for the UI.
    /// </summary>
    private void RefreshAvailableItemsAndSelectedBlueprint()
    {
        itemSearchResults.Clear();
        itemAmounts.Clear();
        base.player.inventory.FindItemsByType(itemSearchResults, EItemType.STRUCTURE);
        for (int num = itemSearchResults.Count - 1; num >= 0; num--)
        {
            PlayerInventorySearchResultV2 playerInventorySearchResultV = itemSearchResults[num];
            if (itemAmounts.TryGetValue(playerInventorySearchResultV.Jar.item.id, out var value))
            {
                itemSearchResults.RemoveAtFast(num);
            }
            itemAmounts[playerInventorySearchResultV.Jar.item.id] = value + playerInventorySearchResultV.Jar.item.amount;
        }
        if (HasSelection)
        {
            itemAmounts.TryGetValue(selectedOption.asset.id, out var value2);
            RefreshCraftableBlueprint(selectedOption.asset);
            craftableBlueprints.TryGetValue(selectedOption.asset, out selectedOption.craftable);
            int num2 = selectedOption.craftable.status?.EstimateOutputMaxAmount(selectedOption.craftable.structureOutputIndex) ?? 0;
            if (value2 > 0 || num2 > 0)
            {
                selectedItemAvailableAmountLabel.Text = localization.format("AvailableAmount", value2);
                selectedItemCraftableAmountLabel.Text = localization.format("CraftableAmount", num2);
            }
            else
            {
                ClearSelectedOption();
            }
        }
    }

    private int CompareItemNames(ItemOption lhs, ItemOption rhs)
    {
        if (lhs.asset != null && rhs.asset != null)
        {
            return lhs.asset.itemName.CompareTo(rhs.asset.itemName);
        }
        return 0;
    }
}
