using System;
using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class ItemAsset : Asset, ISkinableAsset, IBlueprintOwner
{
    /// <summary>
    /// Helper for plugins that want item prefabs server-side.
    /// e.g. Allows item icons to be captured on dedicated server.
    /// </summary>
    public static CommandLineFlag shouldAlwaysLoadItemPrefab = new CommandLineFlag(defaultValue: false, "-AlwaysLoadItemPrefab");

    protected bool _shouldVerifyHash;

    protected string _itemName;

    protected string _itemDescription;

    public EItemType type;

    public EItemRarity rarity;

    protected string _proPath;

    /// <summary>
    /// Hack for Kuwait aura icons.
    /// </summary>
    public bool econIconUseId;

    public bool isPro;

    public string useable;

    public ESlotType slot;

    public byte size_x;

    public byte size_y;

    /// <summary>
    /// Vertical half size of icon camera.
    /// Values less than zero are disabled.
    /// </summary>
    public float iconCameraOrthographicSize;

    /// <summary>
    /// Vertical half size of economy icon camera.
    /// </summary>
    public float econIconCameraOrthographicSize;

    /// <summary>
    /// Should the newer automatic placement and orthographic size for axis-aligned icon cameras be used?
    /// Enabled by default, but optionally disabled for manual adjustment.
    /// </summary>
    public bool isEligibleForAutomaticIconMeasurements;

    public byte amount;

    public byte countMin;

    public byte countMax;

    public byte qualityMin;

    public byte qualityMax;

    [Obsolete("Renamed to ShouldAttachEquippedModelToLeftHand")]
    public bool isBackward;

    /// <summary>
    /// Whether viewmodel should procedurally animate inertia of equipped item.
    /// Useful for low-quality older animations, but modders may wish to disable for high-quality newer animations.
    /// </summary>
    public bool shouldProcedurallyAnimateInertia;

    /// <summary>
    /// Defaults to true. If false, the equipped item model is flipped to counteract the flipped character.
    /// </summary>
    public bool shouldLeftHandedCharactersMirrorEquippedItem;

    /// <summary>
    /// If true, stats like damage, accuracy, health, etc. are automatically appended to the description.
    /// Defaults to true.
    /// </summary>
    public bool isEligibleForAutoStatDescriptions;

    protected GameObject _item;

    /// <summary>
    /// Optional alternative item prefab specifically for the PlayerEquipment prefab spawned.
    /// </summary>
    public GameObject equipablePrefab;

    /// <summary>
    /// Movement speed multiplier while the item is equipped in the hands.
    /// </summary>
    public float equipableMovementSpeedMultiplier = 1f;

    protected AudioClip _equip;

    protected AnimationClip[] _animations;

    /// <summary>
    /// Sound to play when inspecting the equipped item.
    /// </summary>
    public AudioReference inspectAudio;

    /// <summary>
    /// Sound to play when moving or rotating the item in the inventory.
    /// </summary>
    public AudioReference inventoryAudio;

    protected List<Blueprint> _blueprints;

    protected List<Action> _actions;

    protected Texture2D _albedoBase;

    protected Texture2D _metallicBase;

    protected Texture2D _emissionBase;

    /// sortOrder values for description lines.
    /// Difference in value greater than 100 creates an empty line.
    protected const int DescSort_RarityAndType = 0;

    protected const int DescSort_LoreText = 200;

    protected const int DescSort_QualityAndAmount = 400;

    protected const int DescSort_Important = 2000;

    protected const int DescSort_ItemStat = 10000;

    protected const int DescSort_ClothingStat = 10000;

    protected const int DescSort_ConsumeableStat = 10000;

    protected const int DescSort_GunStat = 10000;

    protected const int DescSort_GunAttachmentStat = 10000;

    protected const int DescSort_MeleeStat = 10000;

    protected const int DescSort_RefillStat = 10000;

    /// <summary>
    /// Properties common to Gun and Melee.
    /// </summary>
    protected const int DescSort_Weapon_NonExplosive_Common = 10000;

    protected const int DescSort_TrapKeyword = 10001;

    protected const int DescSort_TrapStat = 10002;

    protected const int DescSort_FarmableText = 15000;

    /// <summary>
    /// Properties common to Barricade and Structure.
    /// </summary>
    protected const int DescSort_BuildableCommon = 20000;

    protected const int DescSort_CraftingTags = 25000;

    protected const int DescSort_ExplosiveBulletDamage = 30000;

    protected const int DescSort_ExplosiveChargeDamage = 30000;

    protected const int DescSort_ExplosiveTrapDamage = 30000;

    /// <summary>
    /// Properties common to Gun, Consumable, and Throwable.
    /// </summary>
    protected const int DescSort_Weapon_Explosive_RangeAndDamage = 30000;

    /// <summary>
    /// Properties common to Gun and Melee.
    /// </summary>
    protected const int DescSort_Weapon_NonExplosive_PlayerDamage = 30000;

    /// <summary>
    /// Properties common to Gun and Melee.
    /// </summary>
    protected const int DescSort_Weapon_NonExplosive_ZombieDamage = 31000;

    /// <summary>
    /// Properties common to Gun and Melee.
    /// </summary>
    protected const int DescSort_Weapon_NonExplosive_AnimalDamage = 32000;

    /// <summary>
    /// Properties common to Gun and Melee.
    /// </summary>
    protected const int DescSort_Weapon_NonExplosive_OtherDamage = 33000;

    protected const int DescSort_Beneficial = -1;

    protected const int DescSort_Detrimental = 1;

    private static List<BlueprintSupply> tempBlueprintSupplies = new List<BlueprintSupply>();

    public bool shouldVerifyHash => _shouldVerifyHash;

    internal override bool ShouldVerifyHash => _shouldVerifyHash;

    public override string FriendlyName
    {
        get
        {
            if (!string.IsNullOrEmpty(_itemName))
            {
                return _itemName;
            }
            return name;
        }
    }

    public string itemName => _itemName;

    public string itemDescription => _itemDescription;

    /// <summary>
    /// Item name wrapped in color rich text tags according to rarity.
    /// </summary>
    public string RarityRichTextName => "<color=" + Palette.hex(ItemTool.getRarityColorUI(rarity)) + ">" + FriendlyName + "</color>";

    public string proPath => _proPath;

    /// <summary>
    /// Useable subclass.
    /// </summary>
    public Type useableType { get; protected set; }

    /// <summary>
    /// Can this useable be equipped by players?
    /// True for most items, but allows modders to create sentry-only weapons.
    /// </summary>
    public bool canPlayerEquip { get; protected set; }

    [Obsolete("Renamed to canPlayerEquip")]
    public bool isUseable
    {
        get
        {
            return canPlayerEquip;
        }
        set
        {
            canPlayerEquip = value;
        }
    }

    /// <summary>
    /// Can this useable be equipped while underwater?
    /// </summary>
    public bool canUseUnderwater { get; protected set; }

    /// <summary>
    /// Nelson 2025-04-10: adding this for semantics because amount isn't an obvious name.
    /// </summary>
    public int MaxAmount => amount;

    public byte MaxAmountAsByte => amount;

    /// <summary>
    /// If true, item should be removed when "amount" reaches zero.
    /// Defaults to true except for magazines.
    /// </summary>
    public bool ShouldDeleteAtZeroAmount { get; protected set; }

    public byte count
    {
        get
        {
            float num;
            float num2;
            if (Provider.modeConfigData != null)
            {
                if (type == EItemType.MAGAZINE)
                {
                    num = Provider.modeConfigData.Items.Magazine_Bullets_Full_Chance;
                    num2 = Provider.modeConfigData.Items.Magazine_Bullets_Multiplier;
                }
                else
                {
                    num = Provider.modeConfigData.Items.Crate_Bullets_Full_Chance;
                    num2 = Provider.modeConfigData.Items.Crate_Bullets_Multiplier;
                }
            }
            else
            {
                num = 0.9f;
                num2 = 1f;
            }
            if (UnityEngine.Random.value < num)
            {
                return MaxAmountAsByte;
            }
            return (byte)Mathf.CeilToInt((float)UnityEngine.Random.Range(countMin, countMax + 1) * num2);
        }
    }

    public byte quality
    {
        get
        {
            if (UnityEngine.Random.value < ((Provider.modeConfigData != null) ? Provider.modeConfigData.Items.Quality_Full_Chance : 0.9f))
            {
                return 100;
            }
            return (byte)Mathf.CeilToInt((float)UnityEngine.Random.Range(qualityMin, qualityMax + 1) * ((Provider.modeConfigData != null) ? Provider.modeConfigData.Items.Quality_Multiplier : 1f));
        }
    }

    /// <summary>
    /// Which parent to use when attaching an equipped/useable item to the player.
    /// </summary>
    public EEquipableModelParent EquipableModelParent { get; protected set; }

    /// <summary>
    /// If true, equipable prefab is a child of the left hand rather than the right.
    /// Defaults to false.
    /// </summary>
    [Obsolete("Replaced by EquipableModelParent property's LeftHook option.")]
    public bool ShouldAttachEquippedModelToLeftHand => EquipableModelParent == EEquipableModelParent.LeftHook;

    /// <summary>
    /// Nelson 2024-12-11: This can now be null for cosmetic items (<see cref="F:SDG.Unturned.ItemAsset.isPro" />). For those items it wasn't
    /// used outside of the main menu 3D item preview, in which case the clothing prefab is typically a better
    /// visualization.
    /// </summary>
    public GameObject item => _item;

    /// <summary>
    /// Name to use when instantiating item prefab.
    /// By default the asset legacy id is used, but it can be overridden because some
    /// modders rely on the name for Unity's legacy animation component. For example
    /// in Toothy Deerryte's case there were a lot of duplicate animations to work
    /// around the id naming, simplified by overriding name.
    /// </summary>
    public string instantiatedItemName { get; protected set; }

    public AudioClip equip => _equip;

    public AnimationClip[] animations => _animations;

    public List<Blueprint> blueprints => _blueprints;

    public List<Action> actions => _actions;

    public bool overrideShowQuality { get; protected set; }

    public virtual bool showQuality => overrideShowQuality;

    /// <summary>
    /// When a player dies with this item, should an item drop be spawned?
    /// </summary>
    public bool shouldDropOnDeath { get; protected set; }

    /// <summary>
    /// Can player click the drop button on this item?
    /// </summary>
    public bool allowManualDrop { get; protected set; }

    public Texture albedoBase => _albedoBase;

    public Texture metallicBase => _metallicBase;

    public Texture emissionBase => _emissionBase;

    /// <summary>
    /// If this item is compatible with skins for another item, lookup that item's ID instead.
    /// </summary>
    public ushort sharedSkinLookupID { get; protected set; }

    /// <summary>
    /// Defaults to true. If false, skin material and mesh are not applied when <see cref="P:SDG.Unturned.ItemAsset.sharedSkinLookupID" /> is
    /// set. For example, a custom axe can transfer the kill counter and ragdoll effect from a vanilla item's skin
    /// without also transferring the material and mesh.
    /// </summary>
    public bool SharedSkinShouldApplyVisuals { get; protected set; }

    /// <summary>
    /// Should friendly-mode sentry guns target a player who has this item equipped?
    /// </summary>
    public virtual bool shouldFriendlySentryTargetUser => false;

    /// <summary>
    /// Kept in case any plugins refer to it.
    /// Renamed to shouldFriendlySentryTargetUser.
    /// </summary>
    [Obsolete]
    public virtual bool isDangerous => shouldFriendlySentryTargetUser;

    /// <summary>
    /// Should this item be deleted when using and quality hits zero?
    /// e.g. final melee hit shatters the weapon.
    /// </summary>
    public bool shouldDeleteAtZeroQuality { get; protected set; }

    /// <summary>
    /// Should the game destroy all child colliders on the item when requested?
    /// Physics items in the world and on character preview don't request destroy,
    /// but items attached to the character do. Mods might be using colliders
    /// in unexpected ways (e.g., riot shield) so they can disable this default.
    /// </summary>
    public bool shouldDestroyItemColliders { get; protected set; }

    public override EAssetType assetCategory => EAssetType.ITEM;

    /// <summary>
    /// Are there any official skins for this item type?
    /// Skips checking for base textures if item cannot have skins.
    /// </summary>
    protected virtual bool doesItemTypeHaveSkins => false;

    public byte[] getState()
    {
        return getState(isFull: false);
    }

    public byte[] getState(bool isFull)
    {
        return getState(isFull ? EItemOrigin.ADMIN : EItemOrigin.WORLD);
    }

    public virtual byte[] getState(EItemOrigin origin)
    {
        return new byte[0];
    }

    public virtual void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        Local localization = PlayerDashboardInventoryUI.localization;
        int num = (int)rarity;
        string arg = localization.format("Rarity_" + num);
        Local localization2 = PlayerDashboardInventoryUI.localization;
        num = (int)type;
        string arg2 = localization2.format("Type_" + num);
        builder.Append("<color=" + Palette.hex(ItemTool.getRarityColorUI(rarity)) + ">" + PlayerDashboardInventoryUI.localization.format("Rarity_Type_Label", arg, arg2) + "</color>", 0);
        builder.Append(_itemDescription, 200);
        if (showQuality)
        {
            Color32 color = ItemTool.getQualityColor((float)(int)itemInstance.quality / 100f);
            builder.Append("<color=" + Palette.hex(color) + ">" + PlayerDashboardInventoryUI.localization.format("Quality", itemInstance.quality) + "</color>", 400);
        }
        if (MaxAmount > 1)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AmountWithCapacity", itemInstance.amount, MaxAmount), 400);
        }
        if (!builder.shouldRestrictToLegacyContent && equipableMovementSpeedMultiplier != 1f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_EquipableMovementSpeedModifier", PlayerDashboardInventoryUI.FormatStatModifier(equipableMovementSpeedMultiplier, higherIsPositive: true, higherIsBeneficial: true)), 10000 + DescSort_HigherIsBeneficial(equipableMovementSpeedMultiplier));
        }
    }

    public override string GetTypeFriendlyName()
    {
        string text = base.GetTypeFriendlyName();
        if (text.StartsWith("Item "))
        {
            text = text.Substring(5) + " Item";
        }
        return text;
    }

    public void applySkinBaseTextures(Material material)
    {
        if (sharedSkinLookupID > 0 && sharedSkinLookupID != id && Assets.find(EAssetType.ITEM, sharedSkinLookupID) is ItemAsset itemAsset)
        {
            itemAsset.applySkinBaseTextures(material);
            return;
        }
        material.SetTexture("_AlbedoBase", albedoBase);
        material.SetTexture("_MetallicBase", metallicBase);
        material.SetTexture("_EmissionBase", emissionBase);
    }

    [Obsolete("canBeUsedInSafezone now has special cases for admins")]
    public virtual bool canBeUsedInSafezone(SafezoneNode safezone)
    {
        return canBeUsedInSafezone(safezone, byAdmin: false);
    }

    /// <summary>
    /// Should players be allowed to start primary/secondary use of this item while inside given safezone?
    /// If returns false the primary/secondary inputs are set to false.
    /// </summary>
    public virtual bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
    {
        if (safezone.noWeapons)
        {
            return !shouldFriendlySentryTargetUser;
        }
        return true;
    }

    /// <summary>
    /// Find useableType by useable name.
    /// </summary>
    private void updateUseableType(IDatDictionary data)
    {
        useable = data.GetString("Useable");
        if (string.IsNullOrEmpty(useable))
        {
            useableType = null;
            return;
        }
        useableType = Assets.useableTypes.getType(useable);
        if (useableType == null)
        {
            useableType = data.ParseType("Useable");
            if (useableType == null)
            {
                Assets.ReportError(this, "cannot find useable type \"{0}\"", useable);
            }
        }
        if (useableType != null && !typeof(Useable).IsAssignableFrom(useableType))
        {
            Assets.ReportError(this, "type \"{0}\" is not useable", useableType);
            useableType = null;
        }
    }

    public ItemAsset()
    {
        _animations = new AnimationClip[0];
        _blueprints = new List<Blueprint>();
        _actions = new List<Action>();
    }

    private void initAnimations(GameObject anim)
    {
        Animation component = anim.GetComponent<Animation>();
        if (component == null)
        {
            Assets.ReportError(this, "missing Animation component on 'Animations' GameObject");
            return;
        }
        _animations = new AnimationClip[component.GetClipCount()];
        if (animations.Length < 1)
        {
            Assets.ReportError(this, "animation clips list is empty");
            return;
        }
        int num = 0;
        bool flag = false;
        bool flag2 = false;
        foreach (AnimationState item in component)
        {
            animations[num] = item.clip;
            num++;
            flag |= item.clip == null;
            flag2 = flag2 || (item.clip != null && item.clip.name == "Equip");
        }
        if (flag)
        {
            Assets.ReportError(this, "has invalid animation clip references");
        }
        if (!flag2)
        {
            Assets.ReportError(this, "missing 'Equip' animation clip");
        }
    }

    public Asset GetBlueprintOwnerAsset()
    {
        return this;
    }

    public List<Blueprint> GetBlueprints()
    {
        return _blueprints;
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        isPro = p.data.ContainsKey("Pro");
        if (id < 2000 && !base.OriginAllowsVanillaLegacyId && !p.data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        _itemName = p.localization.read("Name");
        if (string.IsNullOrEmpty(_itemName))
        {
            _itemName = string.Empty;
        }
        if ((bool)Assets.shouldValidateAssets && _itemName.Trim().Length != _itemName.Length)
        {
            Assets.ReportError(this, "Display name has leading or trailing whitespace");
        }
        _itemDescription = p.localization.format("Description");
        _itemDescription = ItemTool.filterRarityRichText(itemDescription);
        RichTextUtil.replaceNewlineMarkup(ref _itemDescription);
        instantiatedItemName = p.data.GetString("Instantiated_Item_Name_Override", id.ToString());
        type = (EItemType)Enum.Parse(typeof(EItemType), p.data.GetString("Type"), ignoreCase: true);
        if (p.data.ContainsKey("Rarity"))
        {
            rarity = (EItemRarity)Enum.Parse(typeof(EItemRarity), p.data.GetString("Rarity"), ignoreCase: true);
        }
        else
        {
            rarity = EItemRarity.COMMON;
        }
        if (isPro)
        {
            econIconUseId = p.data.ParseBool("Econ_Icon_Use_Id");
            if (type == EItemType.SHIRT)
            {
                _proPath = "/Shirts";
            }
            else if (type == EItemType.PANTS)
            {
                _proPath = "/Pants";
            }
            else if (type == EItemType.HAT)
            {
                _proPath = "/Hats";
            }
            else if (type == EItemType.BACKPACK)
            {
                _proPath = "/Backpacks";
            }
            else if (type == EItemType.VEST)
            {
                _proPath = "/Vests";
            }
            else if (type == EItemType.MASK)
            {
                _proPath = "/Masks";
            }
            else if (type == EItemType.GLASSES)
            {
                _proPath = "/Glasses";
            }
            else if (type == EItemType.KEY)
            {
                _proPath = "/Keys";
            }
            else if (type == EItemType.BOX)
            {
                _proPath = "/Boxes";
            }
            _proPath = _proPath + "/" + name;
        }
        size_x = p.data.ParseUInt8("Size_X", 0);
        if (size_x < 1)
        {
            size_x = 1;
        }
        size_y = p.data.ParseUInt8("Size_Y", 0);
        if (size_y < 1)
        {
            size_y = 1;
        }
        iconCameraOrthographicSize = p.data.ParseFloat("Size_Z", -1f);
        isEligibleForAutomaticIconMeasurements = p.data.ParseBool("Use_Auto_Icon_Measurements", defaultValue: true);
        econIconCameraOrthographicSize = p.data.ParseFloat("Size2_Z", -1f);
        sharedSkinLookupID = p.data.ParseUInt16("Shared_Skin_Lookup_ID", id);
        SharedSkinShouldApplyVisuals = p.data.ParseBool("Shared_Skin_Apply_Visuals", defaultValue: true);
        amount = p.data.ParseUInt8("Amount", 0);
        if (amount < 1)
        {
            amount = 1;
        }
        countMin = p.data.ParseUInt8("Count_Min", 0);
        if (countMin < 1)
        {
            countMin = 1;
        }
        countMax = p.data.ParseUInt8("Count_Max", 0);
        if (countMax < 1)
        {
            countMax = 1;
        }
        if (p.data.ContainsKey("Quality_Min"))
        {
            qualityMin = p.data.ParseUInt8("Quality_Min", 0);
        }
        else
        {
            qualityMin = 10;
        }
        if (p.data.ContainsKey("Quality_Max"))
        {
            qualityMax = p.data.ParseUInt8("Quality_Max", 0);
        }
        else
        {
            qualityMax = 90;
        }
        if (p.data.TryParseEnum<EEquipableModelParent>("EquipableModelParent", out var value))
        {
            EquipableModelParent = value;
        }
        else if (p.data.ContainsKey("Backward"))
        {
            EquipableModelParent = EEquipableModelParent.LeftHook;
        }
        else
        {
            EquipableModelParent = EEquipableModelParent.RightHook;
        }
        shouldLeftHandedCharactersMirrorEquippedItem = p.data.ParseBool("Left_Handed_Characters_Mirror_Equipable", defaultValue: true);
        isEligibleForAutoStatDescriptions = p.data.ParseBool("Use_Auto_Stat_Descriptions", defaultValue: true);
        shouldProcedurallyAnimateInertia = p.data.ParseBool("Procedurally_Animate_Inertia", defaultValue: true);
        updateUseableType(p.data);
        bool defaultValue = useableType != null;
        canPlayerEquip = p.data.ParseBool("Can_Player_Equip", defaultValue);
        equipableMovementSpeedMultiplier = p.data.ParseFloat("Equipable_Movement_Speed_Multiplier", 1f);
        if (canPlayerEquip)
        {
            _equip = LoadRedirectableAsset<AudioClip>(p.bundle, "Equip", p.data, "EquipAudioClip");
            inspectAudio = p.data.ReadAudioReference("InspectAudioDef", p.bundle);
            MasterBundleReference<GameObject> masterBundleReference = p.data.readMasterBundleReference<GameObject>("EquipablePrefab", p.bundle);
            if (masterBundleReference.isValid)
            {
                equipablePrefab = masterBundleReference.loadAsset();
            }
            if (!isPro)
            {
                GameObject gameObject = p.bundle.load<GameObject>("Animations");
                if (gameObject != null)
                {
                    initAnimations(gameObject);
                }
                else
                {
                    _animations = new AnimationClip[0];
                }
            }
        }
        if (p.data.ContainsKey("InventoryAudio"))
        {
            inventoryAudio = p.data.ReadAudioReference("InventoryAudio", p.bundle);
        }
        else
        {
            inventoryAudio = GetDefaultInventoryAudio();
        }
        slot = p.data.ParseEnum("Slot", ESlotType.NONE);
        bool defaultValue2 = slot != ESlotType.PRIMARY;
        canUseUnderwater = p.data.ParseBool("Can_Use_Underwater", defaultValue2);
        if (!Dedicator.IsDedicatedServer || type == EItemType.GUN || type == EItemType.MELEE || (bool)shouldAlwaysLoadItemPrefab)
        {
            _item = p.bundle.load<GameObject>("Item");
            if (item == null)
            {
                if (!isPro || type == EItemType.SHIRT || type == EItemType.PANTS)
                {
                    throw new NotSupportedException("missing \"Item\" GameObject (expected at " + p.bundle.WhereLoadLookedToString("Item") + ")");
                }
            }
            else
            {
                if (item.transform.Find("Icon") != null && item.transform.Find("Icon").GetComponent<Camera>() != null)
                {
                    throw new NotSupportedException("'Icon' has a camera attached!");
                }
                if (id < 2000 && (type == EItemType.GUN || type == EItemType.MELEE) && item.transform.Find("Stat_Tracker") == null)
                {
                    Assets.ReportError(this, "missing 'Stat_Tracker' Transform");
                }
                AssetValidation.searchGameObjectForErrors(this, item);
            }
        }
        bool flag = p.data.ParseBool("Add_Default_Actions", _actions.Count == 0);
        bool flag2 = false;
        if (p.data.TryGetNode("Blueprints", out var node))
        {
            if (node is IDatValue valueNode)
            {
                if (valueNode.TryParseUInt8(out var value2))
                {
                    flag2 = true;
                    PopulateBlueprintsLegacy(p.data, value2, p.localization, flag);
                }
                else
                {
                    ReportAssetError("unable to parse Blueprints count");
                }
            }
            else if (node is IDatList blueprintsList)
            {
                _blueprints = PopulateBlueprintsV2(blueprintsList, p.localization, this);
            }
        }
        if (_blueprints == null)
        {
            _blueprints = new List<Blueprint>();
        }
        else if (_blueprints.Count > 255)
        {
            ReportAssetError($"has more than {byte.MaxValue} Blueprints which breaks some assumptions");
        }
        if (p.data.TryGetNode("Actions", out var node2))
        {
            if (node2 is IDatValue valueNode2)
            {
                if (valueNode2.TryParseUInt8(out var value3))
                {
                    PopulateActionsLegacy(p.data, value3, p.localization);
                }
                else
                {
                    ReportAssetError("unable to parse Actions count");
                }
            }
            else if (node2 is IDatList actionsList)
            {
                PopulateActionsV2(actionsList, p.localization);
            }
        }
        if (_actions == null)
        {
            _actions = new List<Action>();
        }
        if (flag)
        {
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            for (byte b = 0; b < blueprints.Count; b++)
            {
                Blueprint blueprint = blueprints[b];
                if (blueprint.Operation == EBlueprintOperation.RepairTargetItem)
                {
                    if (!flag4)
                    {
                        if (flag2)
                        {
                            blueprint.Name = "Repair";
                        }
                        flag4 = true;
                        Action action = new Action(0, EActionType.BLUEPRINT, new ActionBlueprint[1]
                        {
                            new ActionBlueprint(b, newLink: true)
                        }, null, null, "Repair");
                        action.blueprintOwnerRef = this;
                        actions.Insert(0, action);
                    }
                }
                else if (blueprint.Operation == EBlueprintOperation.FillTargetItem)
                {
                    flag3 = true;
                }
                else if (blueprint.supplies.Length == 1 && blueprint.supplies[0].IsItem(this) && !flag5)
                {
                    if (flag2)
                    {
                        blueprint.Name = "Salvage";
                    }
                    flag5 = true;
                    Action action2 = new Action(0, EActionType.BLUEPRINT, new ActionBlueprint[1]
                    {
                        new ActionBlueprint(b, type == EItemType.GUN || type == EItemType.MELEE)
                    }, null, null, "Salvage");
                    action2.blueprintOwnerRef = this;
                    actions.Add(action2);
                }
            }
            if (flag3)
            {
                List<ActionBlueprint> list = new List<ActionBlueprint>();
                for (byte b2 = 0; b2 < blueprints.Count; b2++)
                {
                    if (blueprints[b2].Operation == EBlueprintOperation.FillTargetItem)
                    {
                        ActionBlueprint actionBlueprint = new ActionBlueprint(b2, newLink: true);
                        list.Add(actionBlueprint);
                    }
                }
                Action action3 = new Action(0, EActionType.BLUEPRINT, list.ToArray(), null, null, "Refill");
                action3.blueprintOwnerRef = this;
                actions.Add(action3);
            }
        }
        _shouldVerifyHash = !p.data.ContainsKey("Bypass_Hash_Verification");
        overrideShowQuality = p.data.ContainsKey("Override_Show_Quality");
        shouldDropOnDeath = p.data.ParseBool("Should_Drop_On_Death", defaultValue: true);
        allowManualDrop = p.data.ParseBool("Allow_Manual_Drop", defaultValue: true);
        shouldDeleteAtZeroQuality = p.data.ParseBool("Should_Delete_At_Zero_Quality");
        shouldDestroyItemColliders = p.data.ParseBool("Destroy_Item_Colliders", defaultValue: true);
        ShouldDeleteAtZeroAmount = p.data.ParseBool("Should_Delete_At_Zero_Amount", type != EItemType.MAGAZINE);
        if (type == EItemType.MAGAZINE && p.data.ContainsKey("Delete_Empty"))
        {
            ShouldDeleteAtZeroAmount = true;
        }
        if (!Dedicator.IsDedicatedServer && id < 2000 && doesItemTypeHaveSkins)
        {
            _albedoBase = p.bundle.load<Texture2D>("Albedo_Base");
            _metallicBase = p.bundle.load<Texture2D>("Metallic_Base");
            _emissionBase = p.bundle.load<Texture2D>("Emission_Base");
        }
    }

    /// <summary>
    /// V2 is for newer dat list features.
    /// </summary>
    internal static List<Blueprint> PopulateBlueprintsV2(IDatList blueprintsList, Local localization, Asset assetContext)
    {
        List<Blueprint> list = new List<Blueprint>(blueprintsList.Count);
        int num = -1;
        foreach (IDatNode blueprints in blueprintsList)
        {
            num++;
            if (!(blueprints is IDatDictionary datDictionary))
            {
                continue;
            }
            EBlueprintType value;
            bool flag = datDictionary.TryParseEnum<EBlueprintType>("Type", out value);
            if (!datDictionary.TryParseEnum<EBlueprintOperation>("Operation", out var value2) && flag)
            {
                switch (value)
                {
                case EBlueprintType.AMMO:
                    value2 = EBlueprintOperation.FillTargetItem;
                    break;
                case EBlueprintType.REPAIR:
                    value2 = EBlueprintOperation.RepairTargetItem;
                    break;
                }
            }
            BlueprintSupply[] newSupplies;
            if (datDictionary.TryGetNode("InputItems", out var node))
            {
                if (node is IDatList datList)
                {
                    List<BlueprintSupply> list2 = new List<BlueprintSupply>(datList.Count);
                    foreach (IDatNode item in datList)
                    {
                        BlueprintSupply blueprintSupply = ParseBlueprintInputItem(value2, item, assetContext);
                        string path;
                        if (blueprintSupply != null)
                        {
                            list2.Add(blueprintSupply);
                        }
                        else if (item.TryGetNodePath(out path))
                        {
                            item.TryGetParsedLineNumber(out var lineNumber);
                            assetContext.ReportAssetError($"unable to parse blueprint input item {path} on line {lineNumber}");
                        }
                        else
                        {
                            assetContext.ReportAssetError($"unable to parse blueprint {num} input item");
                        }
                    }
                    newSupplies = list2.ToArray();
                }
                else
                {
                    BlueprintSupply blueprintSupply2 = ParseBlueprintInputItem(value2, node, assetContext);
                    if (blueprintSupply2 != null)
                    {
                        newSupplies = new BlueprintSupply[1] { blueprintSupply2 };
                    }
                    else
                    {
                        assetContext.ReportAssetError("unable to parse InputItems");
                        newSupplies = new BlueprintSupply[0];
                    }
                }
            }
            else
            {
                newSupplies = new BlueprintSupply[0];
            }
            BlueprintOutput[] newOutputs;
            if (datDictionary.TryGetNode("OutputItems", out var node2))
            {
                if (node2 is IDatList datList2)
                {
                    List<BlueprintOutput> list3 = new List<BlueprintOutput>(datList2.Count);
                    foreach (IDatNode item2 in datList2)
                    {
                        BlueprintOutput blueprintOutput = ParseBlueprintOutputItem(item2, assetContext);
                        string path2;
                        if (blueprintOutput != null)
                        {
                            list3.Add(blueprintOutput);
                        }
                        else if (item2.TryGetNodePath(out path2))
                        {
                            item2.TryGetParsedLineNumber(out var lineNumber2);
                            assetContext.ReportAssetError($"unable to parse blueprint output item {path2} on line {lineNumber2}");
                        }
                        else
                        {
                            assetContext.ReportAssetError($"unable to parse blueprint {num} output item");
                        }
                    }
                    newOutputs = list3.ToArray();
                }
                else
                {
                    BlueprintOutput blueprintOutput2 = ParseBlueprintOutputItem(node2, assetContext);
                    if (blueprintOutput2 != null)
                    {
                        newOutputs = new BlueprintOutput[1] { blueprintOutput2 };
                    }
                    else
                    {
                        assetContext.ReportAssetError("unable to parse OutputItems");
                        newOutputs = new BlueprintOutput[0];
                    }
                }
            }
            else
            {
                newOutputs = new BlueprintOutput[0];
            }
            CachingBcAssetRef effectAssetRef = datDictionary.ParseGuidOrLegacyIdV2("Effect", EAssetType.EFFECT);
            byte b = datDictionary.ParseUInt8("Skill_Level", 0);
            EBlueprintSkill eBlueprintSkill = datDictionary.ParseEnum("Skill", EBlueprintSkill.NONE);
            if (eBlueprintSkill != 0 && b == 0)
            {
                assetContext.ReportAssetError($"blueprint has Skill {eBlueprintSkill} without Skill_Level");
            }
            else if (b > 0 && eBlueprintSkill == EBlueprintSkill.NONE)
            {
                assetContext.ReportAssetError($"blueprint has Skill_Level {b} without Skill");
            }
            bool newTransferState = datDictionary.ParseBool("StateTransfer");
            bool newWithoutAttachments = datDictionary.ParseBool("StateTransfer_DeleteAttachments");
            string @string = datDictionary.GetString("Map");
            NPCConditionsList newQuestConditionsList = default(NPCConditionsList);
            newQuestConditionsList.Parse(datDictionary, localization, assetContext, "Conditions");
            NPCRewardsList newQuestRewardsList = default(NPCRewardsList);
            newQuestRewardsList.Parse(datDictionary, localization, assetContext, "Rewards");
            Blueprint blueprint = new Blueprint((byte)list.Count, newSupplies, newOutputs, b, eBlueprintSkill, newTransferState, newWithoutAttachments, @string, newQuestConditionsList, newQuestRewardsList);
            blueprint.Owner = assetContext as IBlueprintOwner;
            blueprint.effectAssetRef = effectAssetRef;
            blueprint.canBeVisibleWhenSearchedWithoutRequiredItems = datDictionary.ParseBool("Searchable", defaultValue: true);
            blueprint.CanBeVisibleWithUnmetConditions = datDictionary.ParseBool("VisibleWithUnmetConditions");
            blueprint.Name = datDictionary.GetString("Name");
            blueprint.RequiresNearbyCraftingTags = datDictionary.ParseArrayOfStructs<CachingAssetRef>("RequiresNearbyCraftingTags");
            blueprint._operation = value2;
            if (blueprint.Operation != 0)
            {
                CachingBcAssetRef assetRef;
                if (datDictionary.TryGetString("TargetItem", out var value3))
                {
                    if (!ParseItemString(value3, out assetRef, out var _, assetContext))
                    {
                        assetRef = assetContext;
                    }
                }
                else
                {
                    assetRef = assetContext;
                }
                switch (value2)
                {
                case EBlueprintOperation.RepairTargetItem:
                {
                    BlueprintSupply blueprintSupply4 = new BlueprintSupply(0, newCritical: true, 1, newTreatEmptyAsOne: false, ECraftingInputPrioritization.LowestQuality);
                    blueprintSupply4.ItemRef = assetRef;
                    blueprintSupply4.ShouldIncludeMaxQuality = false;
                    blueprintSupply4.ShouldConsume = false;
                    blueprint.TargetItem = blueprintSupply4;
                    break;
                }
                case EBlueprintOperation.FillTargetItem:
                {
                    BlueprintSupply blueprintSupply3 = new BlueprintSupply(0, newCritical: true, 1, newTreatEmptyAsOne: true, ECraftingInputPrioritization.HighestAmount);
                    blueprintSupply3.ItemRef = assetRef;
                    blueprintSupply3.ShouldConsume = false;
                    blueprintSupply3.ShouldExcludeFullAmount = true;
                    blueprint.TargetItem = blueprintSupply3;
                    if (blueprint.supplies.Length != 0)
                    {
                        blueprint.supplies[0]._isCritical = true;
                    }
                    break;
                }
                }
            }
            if (!datDictionary.TryParseAssetRef("CategoryTag", out blueprint._categoryTagRef) && flag)
            {
                blueprint._categoryTagRef = value.GetCategoryTagRef();
            }
            if (blueprint.RequiresNearbyCraftingTags == null && datDictionary.ParseBool("RequiresHeat", eBlueprintSkill == EBlueprintSkill.COOK))
            {
                blueprint.RequiresNearbyCraftingTags = new CachingAssetRef[1] { PowerTool.VanillaCraftingHeatTag };
            }
            list.Add(blueprint);
        }
        return list;
    }

    private static BlueprintSupply ParseBlueprintInputItem(EBlueprintOperation operation, IDatNode inputItemNode, Asset assetContext)
    {
        ECraftingInputPrioritization eCraftingInputPrioritization = ((operation != EBlueprintOperation.FillTargetItem) ? ECraftingInputPrioritization.LowestQuality : ECraftingInputPrioritization.LowestAmount);
        ECraftingInputCountingMethod eCraftingInputCountingMethod = ((operation == EBlueprintOperation.FillTargetItem) ? ECraftingInputCountingMethod.TotalAmount : ECraftingInputCountingMethod.TotalItems);
        if (inputItemNode is IDatDictionary dictionary)
        {
            if (!dictionary.TryParseBcAssetRef("ID", EAssetType.ITEM, out var assetRef))
            {
                if (!string.Equals(dictionary.GetString("ID"), "this", StringComparison.InvariantCultureIgnoreCase))
                {
                    assetContext.ReportAssetError("Unable to parse blueprint input item ID: \"" + dictionary.GetString("ID") + "\"");
                    return null;
                }
                assetRef = assetContext;
            }
            bool newCritical = dictionary.ParseBool("Critical");
            bool flag = dictionary.ParseBool("CountEmptyAsOne");
            bool shouldIncludeEmptyAmount = dictionary.ParseBool("AllowEmpty", flag);
            bool flag2 = dictionary.ParseBool("Delete", defaultValue: true);
            if (!flag2)
            {
                eCraftingInputCountingMethod = ECraftingInputCountingMethod.TotalItems;
            }
            ECraftingInputPrioritization newPrioritization = dictionary.ParseEnum("Prioritization", eCraftingInputPrioritization);
            ECraftingInputCountingMethod countingMethod = dictionary.ParseEnum("CountingMethod", eCraftingInputCountingMethod);
            byte b = dictionary.ParseUInt8("Amount", 1);
            if (b < 1)
            {
                b = 1;
            }
            return new BlueprintSupply(0, newCritical, b, flag, newPrioritization)
            {
                ShouldConsume = flag2,
                ShouldIncludeEmptyAmount = shouldIncludeEmptyAmount,
                ShouldExcludeFullAmount = !dictionary.ParseBool("AllowFull", defaultValue: true),
                ShouldIncludeMaxQuality = dictionary.ParseBool("AllowMaxQuality", defaultValue: true),
                CountingMethod = countingMethod,
                ItemRef = assetRef
            };
        }
        if (inputItemNode is IDatValue datValue)
        {
            if (!ParseItemString(datValue.Value, out var assetRef2, out var newAmount, assetContext))
            {
                return null;
            }
            return new BlueprintSupply(0, newCritical: false, newAmount, newTreatEmptyAsOne: false, eCraftingInputPrioritization)
            {
                ShouldConsume = true,
                ItemRef = assetRef2,
                CountingMethod = eCraftingInputCountingMethod
            };
        }
        return null;
    }

    private static BlueprintOutput ParseBlueprintOutputItem(IDatNode outputItemNode, Asset assetContext)
    {
        if (outputItemNode is IDatDictionary dictionary)
        {
            if (!dictionary.TryParseBcAssetRef("ID", EAssetType.ITEM, out var assetRef))
            {
                if (string.Equals(dictionary.GetString("ID"), "this", StringComparison.InvariantCultureIgnoreCase))
                {
                    assetRef = assetContext;
                }
                else
                {
                    assetContext.ReportAssetError("Unable to parse blueprint output item ID: \"" + dictionary.GetString("ID") + "\"");
                }
            }
            byte b = dictionary.ParseUInt8("Amount", 1);
            if (b < 1)
            {
                b = 1;
            }
            EItemOrigin newOrigin = dictionary.ParseEnum("Origin", EItemOrigin.CRAFT);
            return new BlueprintOutput(0, b, newOrigin)
            {
                ItemRef = assetRef
            };
        }
        if (outputItemNode is IDatValue datValue)
        {
            if (!ParseItemString(datValue.Value, out var assetRef2, out var newAmount, assetContext))
            {
                return null;
            }
            return new BlueprintOutput(0, newAmount, EItemOrigin.CRAFT)
            {
                ItemRef = assetRef2
            };
        }
        return null;
    }

    private static bool ParseItemString(string input, out CachingBcAssetRef assetRef, out byte amount, Asset assetContext)
    {
        assetRef = CachingBcAssetRef.Empty;
        amount = 1;
        int num = input.IndexOf('x');
        string text;
        if (num < 0)
        {
            text = input;
        }
        else
        {
            text = input.Substring(0, num);
            string text2 = input.Substring(num + 1);
            if (!byte.TryParse(text2, out amount))
            {
                assetContext.ReportAssetError("Unable to parse blueprint input item amount: \"" + text2 + "\"");
                return false;
            }
            if (amount < 1)
            {
                amount = 1;
            }
        }
        if (!CachingBcAssetRef.TryParse(text, EAssetType.ITEM, out assetRef))
        {
            if (!string.Equals(text.Trim(), "this", StringComparison.InvariantCultureIgnoreCase))
            {
                assetContext.ReportAssetError("Unable to parse blueprint input item ID: \"" + text + "\"");
                return false;
            }
            assetRef = assetContext;
        }
        return true;
    }

    /// <summary>
    /// Legacy is for backwards compatibility with Blueprint_# format.
    /// </summary>
    private void PopulateBlueprintsLegacy(IDatDictionary data, byte blueprintCount, Local localization, bool shouldAddDefaultActions)
    {
        _blueprints = new List<Blueprint>(blueprintCount);
        for (byte b = 0; b < blueprintCount; b++)
        {
            if (!data.ContainsKey("Blueprint_" + b + "_Type"))
            {
                throw new NotSupportedException("Missing blueprint type");
            }
            EBlueprintOperation eBlueprintOperation = EBlueprintOperation.None;
            EBlueprintType eBlueprintType = (EBlueprintType)Enum.Parse(typeof(EBlueprintType), data.GetString("Blueprint_" + b + "_Type"), ignoreCase: true);
            switch (eBlueprintType)
            {
            case EBlueprintType.AMMO:
                eBlueprintOperation = EBlueprintOperation.FillTargetItem;
                break;
            case EBlueprintType.REPAIR:
                eBlueprintOperation = EBlueprintOperation.RepairTargetItem;
                break;
            }
            ECraftingInputPrioritization defaultValue = ((eBlueprintOperation != EBlueprintOperation.FillTargetItem) ? ECraftingInputPrioritization.LowestQuality : ECraftingInputPrioritization.LowestAmount);
            ECraftingInputCountingMethod countingMethod = ((eBlueprintOperation == EBlueprintOperation.FillTargetItem) ? ECraftingInputCountingMethod.TotalAmount : ECraftingInputCountingMethod.TotalItems);
            byte b2 = data.ParseUInt8("Blueprint_" + b + "_Supplies", 0);
            if (b2 < 1)
            {
                b2 = 1;
            }
            tempBlueprintSupplies.Clear();
            for (byte b3 = 0; b3 < b2; b3++)
            {
                string text = "Blueprint_" + b + "_Supply_" + b3 + "_ID";
                if (!data.TryParseUInt16(text, out var value))
                {
                    if (string.Equals(data.GetString(text), "this", StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = id;
                    }
                    else
                    {
                        Assets.ReportError(this, "Unable to parse " + text + ": \"" + data.GetString(text) + "\"");
                    }
                }
                bool flag = data.ContainsKey("Blueprint_" + b + "_Supply_" + b3 + "_Critical");
                flag = flag || (eBlueprintOperation == EBlueprintOperation.FillTargetItem && b3 == 0);
                bool newTreatEmptyAsOne = data.ParseBool("Blueprint_" + b + "_Supply_" + b3 + "_AllowEmpty");
                ECraftingInputPrioritization newPrioritization = data.ParseEnum("Blueprint_" + b + "_Supply_" + b3 + "_Prioritization", defaultValue);
                byte b4 = data.ParseUInt8("Blueprint_" + b + "_Supply_" + b3 + "_Amount", 0);
                if (b4 < 1)
                {
                    b4 = 1;
                }
                BlueprintSupply blueprintSupply = new BlueprintSupply(0, flag, b4, newTreatEmptyAsOne, newPrioritization);
                blueprintSupply.ItemRef = new CachingBcAssetRef(EAssetType.ITEM, value);
                blueprintSupply.CountingMethod = countingMethod;
                tempBlueprintSupplies.Add(blueprintSupply);
            }
            ushort num = data.ParseUInt16("Blueprint_" + b + "_Tool", 0);
            bool newCritical = data.ContainsKey("Blueprint_" + b + "_Tool_Critical");
            if (num != 0)
            {
                BlueprintSupply blueprintSupply2 = new BlueprintSupply(0, newCritical, 1, newTreatEmptyAsOne: false, ECraftingInputPrioritization.LowestQuality);
                blueprintSupply2.ItemRef = new CachingBcAssetRef(EAssetType.ITEM, num);
                blueprintSupply2.ShouldConsume = false;
                tempBlueprintSupplies.Add(blueprintSupply2);
            }
            BlueprintOutput[] array;
            if (eBlueprintOperation != 0)
            {
                array = new BlueprintOutput[0];
            }
            else
            {
                byte b5 = data.ParseUInt8("Blueprint_" + b + "_Outputs", 0);
                if (b5 > 0)
                {
                    array = new BlueprintOutput[b5];
                    for (byte b6 = 0; b6 < array.Length; b6++)
                    {
                        ushort legacyId = data.ParseUInt16("Blueprint_" + b + "_Output_" + b6 + "_ID", 0);
                        byte b7 = data.ParseUInt8("Blueprint_" + b + "_Output_" + b6 + "_Amount", 0);
                        if (b7 < 1)
                        {
                            b7 = 1;
                        }
                        EItemOrigin newOrigin = data.ParseEnum("Blueprint_" + b + "_Output_" + b6 + "_Origin", EItemOrigin.CRAFT);
                        array[b6] = new BlueprintOutput(0, b7, newOrigin);
                        array[b6].ItemRef = new CachingBcAssetRef(EAssetType.ITEM, legacyId);
                    }
                }
                else
                {
                    array = new BlueprintOutput[1];
                    ushort num2 = data.ParseUInt16("Blueprint_" + b + "_Product", 0);
                    if (num2 == 0)
                    {
                        num2 = id;
                    }
                    byte b8 = data.ParseUInt8("Blueprint_" + b + "_Products", 0);
                    if (b8 < 1)
                    {
                        b8 = 1;
                    }
                    EItemOrigin newOrigin2 = data.ParseEnum("Blueprint_" + b + "_Origin", EItemOrigin.CRAFT);
                    array[0] = new BlueprintOutput(0, b8, newOrigin2);
                    array[0].ItemRef = new CachingBcAssetRef(EAssetType.ITEM, num2);
                }
            }
            CachingBcAssetRef effectAssetRef = data.ParseGuidOrLegacyIdV2("Blueprint_" + b + "_Build", EAssetType.EFFECT);
            byte b9 = data.ParseUInt8("Blueprint_" + b + "_Level", 0);
            EBlueprintSkill eBlueprintSkill = EBlueprintSkill.NONE;
            if (b9 > 0)
            {
                eBlueprintSkill = (EBlueprintSkill)Enum.Parse(typeof(EBlueprintSkill), data.GetString("Blueprint_" + b + "_Skill"), ignoreCase: true);
            }
            bool newTransferState = data.ContainsKey("Blueprint_" + b + "_State_Transfer");
            bool newWithoutAttachments = data.ParseBool("Blueprint_" + b + "_State_Transfer_Delete_Attachments");
            string @string = data.GetString("Blueprint_" + b + "_Map");
            NPCConditionsList newQuestConditionsList = default(NPCConditionsList);
            newQuestConditionsList.Parse(data, localization, this, "Blueprint_" + b + "_Conditions", "Blueprint_" + b + "_Condition_");
            NPCRewardsList newQuestRewardsList = default(NPCRewardsList);
            newQuestRewardsList.Parse(data, localization, this, "Blueprint_" + b + "_Rewards", "Blueprint_" + b + "_Reward_");
            Blueprint blueprint = new Blueprint(b, tempBlueprintSupplies.ToArray(), array, b9, eBlueprintSkill, newTransferState, newWithoutAttachments, @string, newQuestConditionsList, newQuestRewardsList);
            blueprint.Owner = this;
            blueprint.effectAssetRef = effectAssetRef;
            blueprint.canBeVisibleWhenSearchedWithoutRequiredItems = data.ParseBool($"Blueprint_{b}_Searchable", defaultValue: true);
            blueprint._operation = eBlueprintOperation;
            CachingAssetRef categoryTagRef = ((!shouldAddDefaultActions || eBlueprintOperation != 0 || tempBlueprintSupplies.Count != 1 || !tempBlueprintSupplies[0].IsItem(this)) ? eBlueprintType.GetCategoryTagRef() : EBlueprintTypeEx.salvageCategoryTagRef);
            blueprint._categoryTagRef = categoryTagRef;
            switch (eBlueprintOperation)
            {
            case EBlueprintOperation.RepairTargetItem:
            {
                BlueprintSupply blueprintSupply4 = new BlueprintSupply(0, newCritical: true, 1, newTreatEmptyAsOne: false, ECraftingInputPrioritization.LowestQuality);
                blueprintSupply4.ItemRef = this;
                blueprintSupply4.ShouldIncludeMaxQuality = false;
                blueprintSupply4.ShouldConsume = false;
                blueprint.TargetItem = blueprintSupply4;
                break;
            }
            case EBlueprintOperation.FillTargetItem:
            {
                BlueprintSupply blueprintSupply3 = new BlueprintSupply(0, newCritical: true, 1, newTreatEmptyAsOne: true, ECraftingInputPrioritization.HighestAmount);
                blueprintSupply3.ItemRef = this;
                blueprintSupply3.ShouldConsume = false;
                blueprintSupply3.ShouldExcludeFullAmount = true;
                blueprint.TargetItem = blueprintSupply3;
                break;
            }
            }
            if (eBlueprintSkill == EBlueprintSkill.COOK)
            {
                blueprint.RequiresNearbyCraftingTags = new CachingAssetRef[1] { PowerTool.VanillaCraftingHeatTag };
            }
            blueprints.Add(blueprint);
        }
    }

    /// <summary>
    /// V2 is for newer dat list features.
    /// </summary>
    private void PopulateActionsV2(IDatList actionsList, Local localization)
    {
        _actions = new List<Action>(actionsList.Count);
        int num = -1;
        foreach (IDatNode actions in actionsList)
        {
            num++;
            if (!(actions is IDatDictionary dictionary))
            {
                continue;
            }
            if (!dictionary.TryParseEnum<EActionType>("Type", out var value))
            {
                ReportAssetError("unable to parse action Type \"" + dictionary.GetString("Type") + "\"");
                continue;
            }
            if (!dictionary.TryGetString("BlueprintName", out var value2))
            {
                ReportAssetError("action requires BlueprintName property");
                continue;
            }
            if (!dictionary.TryParseAssetRef("BlueprintOwner", out var assetRef))
            {
                assetRef = this;
            }
            bool newLink = dictionary.ParseBool("BlueprintLink");
            string newText = null;
            string newTooltip = null;
            string @string = dictionary.GetString("CommonTextId");
            if (string.IsNullOrEmpty(@string))
            {
                if (dictionary.TryGetString("TextId", out var value3))
                {
                    newText = localization.format(value3);
                }
                if (dictionary.TryGetString("TooltipId", out var value4))
                {
                    newTooltip = localization.format(value4);
                }
            }
            ActionBlueprint actionBlueprint = new ActionBlueprint(-1, newLink);
            actionBlueprint.blueprintName = value2;
            ActionBlueprint[] newBlueprints = new ActionBlueprint[1] { actionBlueprint };
            Action action = new Action(0, value, newBlueprints, newText, newTooltip, @string);
            action.blueprintOwnerRef = assetRef;
            this.actions.Add(action);
        }
    }

    /// <summary>
    /// Legacy is for backwards compatibility with Action_# format.
    /// </summary>
    private void PopulateActionsLegacy(IDatDictionary data, byte actionCount, Local localization)
    {
        _actions = new List<Action>(actionCount);
        for (byte b = 0; b < actionCount; b++)
        {
            if (!data.ContainsKey("Action_" + b + "_Type"))
            {
                throw new NotSupportedException("Missing action type");
            }
            EActionType newType = (EActionType)Enum.Parse(typeof(EActionType), data.GetString("Action_" + b + "_Type"), ignoreCase: true);
            string @string = data.GetString("Action_" + b + "_Key");
            string newText;
            string newTooltip;
            if (string.IsNullOrEmpty(@string))
            {
                string key = "Action_" + b + "_Text";
                newText = ((!localization.has(key)) ? data.GetString(key) : localization.format(key));
                string key2 = "Action_" + b + "_Tooltip";
                newTooltip = ((!localization.has(key2)) ? data.GetString(key2) : localization.format(key2));
            }
            else
            {
                newText = string.Empty;
                newTooltip = string.Empty;
            }
            string key3 = "Action_" + b + "_Source";
            if (!data.TryParseBcAssetRef(key3, EAssetType.ITEM, out var assetRef))
            {
                assetRef = this;
            }
            byte b2 = data.ParseUInt8("Action_" + b + "_Blueprints", 0);
            if (b2 < 1)
            {
                b2 = 1;
            }
            ActionBlueprint[] array = new ActionBlueprint[b2];
            Action action = new Action(0, newType, array, newText, newTooltip, @string);
            action.blueprintOwnerRef = assetRef;
            for (byte b3 = 0; b3 < array.Length; b3++)
            {
                int newIndex = -1;
                string key4 = "Action_" + b + "_Blueprint_" + b3 + "_Name";
                if (!data.TryGetString(key4, out var value))
                {
                    newIndex = data.ParseUInt8("Action_" + b + "_Blueprint_" + b3 + "_Index", 0);
                }
                bool newLink = data.ContainsKey("Action_" + b + "_Blueprint_" + b3 + "_Link");
                ActionBlueprint actionBlueprint = new ActionBlueprint(newIndex, newLink);
                actionBlueprint.blueprintName = value;
                array[b3] = actionBlueprint;
            }
            actions.Add(action);
        }
    }

    internal override void PreResaveAsset(IDatDictionary data)
    {
        base.PreResaveAsset(data);
        if (data.ParseUInt8("Blueprints", 0) <= 0)
        {
            return;
        }
        bool flag = true;
        foreach (Blueprint blueprint in blueprints)
        {
            if (blueprint.questConditions != null || blueprint.questRewards != null)
            {
                flag = false;
                break;
            }
        }
        if (flag)
        {
            ConvertLegacyBlueprintsFormat(data);
        }
        else
        {
            UnturnedLog.info("Cannot automatically convert " + base.FriendlyNameWithFriendlyType + " Blueprints (yet) because they use conditions/rewards");
        }
    }

    private void ConvertLegacyBlueprintsFormat(IDatDictionary data)
    {
        IEditableDatDictionary editableDatDictionary = data.Edit();
        UnturnedLog.info("Converting " + base.FriendlyNameWithFriendlyType + " Blueprints format");
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, IDatNode> datum in data)
        {
            if (datum.Key.StartsWith("Blueprint_", StringComparison.InvariantCultureIgnoreCase))
            {
                list.Add(datum.Key);
            }
        }
        UnturnedLog.info("Removing keys: " + string.Join(", ", list));
        foreach (string item in list)
        {
            editableDatDictionary.Remove(item);
        }
        IEditableDatList editableDatList = editableDatDictionary.ReplaceWithList("Blueprints");
        editableDatList.SetMargins(1);
        foreach (Blueprint blueprint in blueprints)
        {
            IEditableDatDictionary editableDatDictionary2 = editableDatList.AddDictionary();
            if (!string.IsNullOrEmpty(blueprint.Name))
            {
                editableDatDictionary2.AddValue("Name").SetString(blueprint.Name);
            }
            if (blueprint.CategoryTagRef.IsAssigned)
            {
                editableDatDictionary2.AddValue("CategoryTag").SetAssetRefWithInlineComment(blueprint.CategoryTagRef);
            }
            if (blueprint.Operation != 0)
            {
                editableDatDictionary2.AddValue("Operation").SetEnumString(blueprint.Operation);
            }
            ECraftingInputPrioritization eCraftingInputPrioritization = ((blueprint.Operation != EBlueprintOperation.FillTargetItem) ? ECraftingInputPrioritization.LowestQuality : ECraftingInputPrioritization.LowestAmount);
            _ = blueprint.Operation;
            if (!blueprint.supplies.IsNullOrEmpty())
            {
                bool flag = false;
                for (int i = 0; i < blueprint.supplies.Length; i++)
                {
                    BlueprintSupply blueprintSupply = blueprint.supplies[i];
                    bool flag2 = i == 0 && blueprint.Operation == EBlueprintOperation.FillTargetItem;
                    if (blueprintSupply.isCritical != flag2)
                    {
                        flag = true;
                        break;
                    }
                    if (!blueprintSupply.ShouldConsume)
                    {
                        flag = true;
                        break;
                    }
                    if (blueprintSupply.Prioritization != eCraftingInputPrioritization)
                    {
                        flag = true;
                        break;
                    }
                    if (blueprintSupply.ShouldCountEmptyAsOne)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    IEditableDatList editableDatList2 = editableDatDictionary2.AddList("InputItems");
                    BlueprintSupply[] supplies = blueprint.supplies;
                    foreach (BlueprintSupply blueprintSupply2 in supplies)
                    {
                        IEditableDatDictionary editableDatDictionary3 = editableDatList2.AddDictionary();
                        if (blueprintSupply2.ItemRef.IsReferenceTo(this))
                        {
                            editableDatDictionary3.AddValue("ID").SetString("this");
                        }
                        else
                        {
                            editableDatDictionary3.AddValue("ID").SetAssetRefWithInlineComment(blueprintSupply2.ItemRef);
                        }
                        if (blueprintSupply2.amount != 1)
                        {
                            editableDatDictionary3.AddValue("Amount").SetInt32(blueprintSupply2.amount);
                        }
                        if (blueprintSupply2.isCritical)
                        {
                            editableDatDictionary3.AddValue("Critical").SetBool(value: true);
                        }
                        if (!blueprintSupply2.ShouldConsume)
                        {
                            editableDatDictionary3.AddValue("Delete").SetBool(value: false);
                        }
                        if (blueprintSupply2.Prioritization != eCraftingInputPrioritization)
                        {
                            editableDatDictionary3.AddValue("Prioritization").SetEnumString(blueprintSupply2.Prioritization);
                        }
                        if (blueprintSupply2.ShouldCountEmptyAsOne)
                        {
                            editableDatDictionary3.AddValue("CountEmptyAsOne").SetBool(value: true);
                        }
                    }
                }
                else if (blueprint.supplies.Length > 1)
                {
                    IEditableDatList editableDatList3 = editableDatDictionary2.AddList("InputItems");
                    BlueprintSupply[] supplies = blueprint.supplies;
                    foreach (BlueprintSupply blueprintSupply3 in supplies)
                    {
                        IEditableDatValue valueNode = editableDatList3.AddValue();
                        ConvertSimpleItemRefAndAmount(blueprintSupply3.ItemRef, blueprintSupply3.amount, valueNode);
                    }
                }
                else
                {
                    BlueprintSupply blueprintSupply4 = blueprint.supplies[0];
                    ConvertSimpleItemRefAndAmount(blueprintSupply4.ItemRef, blueprintSupply4.amount, editableDatDictionary2.AddValue("InputItems"));
                }
            }
            if (!blueprint.outputs.IsNullOrEmpty())
            {
                bool flag3 = false;
                BlueprintOutput[] outputs = blueprint.outputs;
                foreach (BlueprintOutput blueprintOutput in outputs)
                {
                    flag3 |= blueprintOutput.origin != EItemOrigin.CRAFT;
                }
                if (flag3)
                {
                    IEditableDatList editableDatList4 = editableDatDictionary2.AddList("OutputItems");
                    outputs = blueprint.outputs;
                    foreach (BlueprintOutput blueprintOutput2 in outputs)
                    {
                        IEditableDatDictionary editableDatDictionary4 = editableDatList4.AddDictionary();
                        if (blueprintOutput2.ItemRef.IsReferenceTo(this))
                        {
                            editableDatDictionary4.AddValue("ID").SetString("this");
                        }
                        else
                        {
                            editableDatDictionary4.AddValue("ID").SetAssetRefWithInlineComment(blueprintOutput2.ItemRef);
                        }
                        if (blueprintOutput2.amount != 1)
                        {
                            editableDatDictionary4.AddValue("Amount").SetInt32(blueprintOutput2.amount);
                        }
                        if (blueprintOutput2.origin != EItemOrigin.CRAFT)
                        {
                            editableDatDictionary4.AddValue("Origin").SetString(blueprintOutput2.origin.ToStringPascalCase());
                        }
                    }
                }
                else if (blueprint.outputs.Length > 1)
                {
                    IEditableDatList editableDatList5 = editableDatDictionary2.AddList("OutputItems");
                    outputs = blueprint.outputs;
                    foreach (BlueprintOutput blueprintOutput3 in outputs)
                    {
                        IEditableDatValue valueNode2 = editableDatList5.AddValue();
                        ConvertSimpleItemRefAndAmount(blueprintOutput3.ItemRef, blueprintOutput3.amount, valueNode2);
                    }
                }
                else
                {
                    BlueprintOutput blueprintOutput4 = blueprint.outputs[0];
                    ConvertSimpleItemRefAndAmount(blueprintOutput4.ItemRef, blueprintOutput4.amount, editableDatDictionary2.AddValue("OutputItems"));
                }
            }
            if (blueprint.skill != 0)
            {
                editableDatDictionary2.AddValue("Skill").SetString(blueprint.skill.ToStringPascalCase());
                editableDatDictionary2.AddValue("Skill_Level").SetInt32(blueprint.level);
            }
            if (blueprint.transferState)
            {
                editableDatDictionary2.AddValue("StateTransfer").SetBool(value: true);
                if (blueprint.withoutAttachments)
                {
                    editableDatDictionary2.AddValue("StateTransfer_DeleteAttachments").SetBool(value: true);
                }
            }
            if (!string.IsNullOrEmpty(blueprint.map))
            {
                editableDatDictionary2.AddValue("Map").SetString(blueprint.map);
            }
            if (!blueprint.canBeVisibleWhenSearchedWithoutRequiredItems)
            {
                editableDatDictionary2.AddValue("Searchable").SetBool(value: false);
            }
            if (blueprint.RequiresNearbyCraftingTags != null && blueprint.RequiresNearbyCraftingTags.Length != 0)
            {
                IEditableDatList editableDatList6 = editableDatDictionary2.AddList("RequiresNearbyCraftingTags");
                CachingAssetRef[] requiresNearbyCraftingTags = blueprint.RequiresNearbyCraftingTags;
                foreach (CachingAssetRef assetRef in requiresNearbyCraftingTags)
                {
                    editableDatList6.AddValue().SetAssetRefWithInlineComment(assetRef);
                }
            }
            if (blueprint.effectAssetRef.IsAssigned)
            {
                editableDatDictionary2.AddValue("Effect").SetAssetRefWithInlineComment(blueprint.effectAssetRef);
            }
        }
    }

    private void ConvertSimpleItemRefAndAmount(CachingBcAssetRef itemRef, int amount, IEditableDatValue valueNode)
    {
        if (itemRef.IsReferenceTo(this))
        {
            if (amount > 1)
            {
                valueNode.SetString($"this x {amount}");
            }
            else
            {
                valueNode.SetString("this");
            }
            return;
        }
        valueNode.SetAssetRefWithInlineComment(itemRef);
        if (amount > 1)
        {
            valueNode.Value += $" x {amount}";
        }
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Locale_Item");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Name", FriendlyName);
        orAddDeclaration.Append("Description", itemDescription);
        CargoDeclaration orAddDeclaration2 = builder.GetOrAddDeclaration("Item");
        orAddDeclaration2.Append("GUID", GUID);
        orAddDeclaration2.Append("Actions", actions.Count);
        orAddDeclaration2.Append("Allow_Manual_Drop", allowManualDrop);
        orAddDeclaration2.Append("Amount", MaxAmount);
        orAddDeclaration2.Append("Blueprints", (object)blueprints.Count);
        orAddDeclaration2.Append("Can_Player_Equip", canPlayerEquip);
        orAddDeclaration2.Append("Can_Use_Underwater", canUseUnderwater);
        orAddDeclaration2.Append("Count_Max", countMax);
        orAddDeclaration2.Append("Count_Min", countMin);
        orAddDeclaration2.Append("Destroy_Item_Colliders", shouldDestroyItemColliders);
        orAddDeclaration2.Append("Equipable_Movement_Speed_Multiplier", equipableMovementSpeedMultiplier);
        orAddDeclaration2.Append("EquipableModelParent", EquipableModelParent);
        orAddDeclaration2.Append("EquipAudioClip", (object)equip);
        orAddDeclaration2.Append("Instantiated_Item_Name_Override", instantiatedItemName);
        orAddDeclaration2.Append("Left_Handed_Characters_Mirror_Equipable", shouldLeftHandedCharactersMirrorEquippedItem);
        orAddDeclaration2.Append("Pro", isPro);
        orAddDeclaration2.Append("Quality_Max", qualityMax);
        orAddDeclaration2.Append("Quality_Min", qualityMin);
        orAddDeclaration2.Append("Shared_Skin_Lookup_ID", sharedSkinLookupID);
        orAddDeclaration2.Append("Shared_Skin_Apply_Visuals", SharedSkinShouldApplyVisuals);
        orAddDeclaration2.Append("Should_Delete_At_Zero_Quality", shouldDeleteAtZeroQuality);
        orAddDeclaration2.Append("Should_Drop_On_Death", shouldDropOnDeath);
        orAddDeclaration2.Append("Size_Z", iconCameraOrthographicSize);
        orAddDeclaration2.Append("Size2_Z", econIconCameraOrthographicSize);
        orAddDeclaration2.Append("Rarity", rarity);
        orAddDeclaration2.Append("Size_X", size_x);
        orAddDeclaration2.Append("Size_Y", size_y);
        orAddDeclaration2.Append("Slot", slot);
        orAddDeclaration2.Append("Useable", (object)useable);
        orAddDeclaration2.Append("Use_Auto_Icon_Measurements", isEligibleForAutomaticIconMeasurements);
        orAddDeclaration2.Append("Use_Auto_Stat_Descriptions", isEligibleForAutoStatDescriptions);
        for (byte b = 0; b < blueprints.Count; b++)
        {
            CargoDeclaration cargoDeclaration = builder.AddDeclaration("Item_Blueprint");
            cargoDeclaration.Append("GUID", GUID);
            cargoDeclaration.Append("blueprintIndex", b);
            cargoDeclaration.Append("Build", blueprints[b].build);
            cargoDeclaration.Append("Level", blueprints[b].level);
            cargoDeclaration.Append("Map", blueprints[b].map);
            cargoDeclaration.Append("Outputs", blueprints[b].outputs.Length);
            cargoDeclaration.Append("Searchable", blueprints[b].canBeVisibleWhenSearchedWithoutRequiredItems);
            cargoDeclaration.Append("Skill", blueprints[b].skill);
            cargoDeclaration.Append("State_Transfer", blueprints[b].transferState);
            cargoDeclaration.Append("State_Transfer_Delete_Attachments", blueprints[b].withoutAttachments);
            cargoDeclaration.Append("Supplies", blueprints[b].supplies.Length);
            cargoDeclaration.Append("Type", blueprints[b].type);
            for (byte b2 = 0; b2 < blueprints[b].supplies.Length; b2++)
            {
                CargoDeclaration cargoDeclaration2 = builder.AddDeclaration("Item_Blueprint_Supply");
                cargoDeclaration2.Append("GUID", GUID);
                cargoDeclaration2.Append("blueprintIndex", b);
                cargoDeclaration2.Append("supplyIndex", b2);
                cargoDeclaration2.Append("ID", blueprints[b].supplies[b2].ItemRef.LegacyId);
                cargoDeclaration2.Append("ItemGUID", blueprints[b].supplies[b2].ItemRef.Guid);
                cargoDeclaration2.Append("Critical", blueprints[b].supplies[b2].isCritical);
                cargoDeclaration2.Append("Delete", blueprints[b].supplies[b2].ShouldConsume);
                cargoDeclaration2.Append("Amount", blueprints[b].supplies[b2].amount);
            }
            for (byte b3 = 0; b3 < blueprints[b].outputs.Length; b3++)
            {
                CargoDeclaration cargoDeclaration3 = builder.AddDeclaration("Item_Blueprint_Output");
                cargoDeclaration3.Append("GUID", GUID);
                cargoDeclaration3.Append("blueprintIndex", b);
                cargoDeclaration3.Append("outputIndex", b3);
                cargoDeclaration3.Append("ID", blueprints[b].outputs[b3].ItemRef.LegacyId);
                cargoDeclaration3.Append("ItemGUID", blueprints[b].outputs[b3].ItemRef.Guid);
                cargoDeclaration3.Append("Amount", blueprints[b].outputs[b3].amount);
                cargoDeclaration3.Append("Origin", blueprints[b].outputs[b3].origin);
            }
        }
    }

    protected virtual AudioReference GetDefaultInventoryAudio()
    {
        if (size_x < 2 && size_y < 2)
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/LightGrab.asset");
        }
        return new AudioReference("core.masterbundle", "Sounds/Inventory/RoughGrab.asset");
    }

    protected int DescSort_HigherIsBeneficial(float value)
    {
        if (!(value > 1f))
        {
            return 1;
        }
        return -1;
    }

    protected int DescSort_LowerIsBeneficial(float value)
    {
        if (!(value < 1f))
        {
            return 1;
        }
        return -1;
    }
}
