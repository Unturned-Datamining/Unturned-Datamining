using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ItemAsset : Asset, ISkinableAsset
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

    public bool shouldVerifyHash => _shouldVerifyHash;

    internal override bool ShouldVerifyHash => _shouldVerifyHash;

    public override string FriendlyName => _itemName;

    public string itemName => _itemName;

    public string itemDescription => _itemDescription;

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
                return amount;
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
    /// If true, equipable prefab is a child of the left hand rather than the right.
    /// Defaults to false.
    /// </summary>
    public bool ShouldAttachEquippedModelToLeftHand
    {
        get
        {
            return isBackward;
        }
        protected set
        {
            isBackward = value;
        }
    }

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
        if (amount > 1)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_AmountWithCapacity", itemInstance.amount, amount), 400);
        }
        if (!builder.shouldRestrictToLegacyContent && equipableMovementSpeedMultiplier != 1f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_EquipableMovementSpeedModifier", PlayerDashboardInventoryUI.FormatStatModifier(equipableMovementSpeedMultiplier, higherIsPositive: true, higherIsBeneficial: true)), 10000);
        }
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
    private void updateUseableType()
    {
        if (string.IsNullOrEmpty(useable))
        {
            useableType = null;
            return;
        }
        useableType = Assets.useableTypes.getType(useable);
        if (useableType == null)
        {
            Assets.reportError(this, "cannot find useable type \"{0}\"", useable);
        }
        else if (!typeof(Useable).IsAssignableFrom(useableType))
        {
            Assets.reportError(this, "type \"{0}\" is not useable", useableType);
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
            Assets.reportError(this, "missing Animation component on 'Animations' GameObject");
            return;
        }
        _animations = new AnimationClip[component.GetClipCount()];
        if (animations.Length < 1)
        {
            Assets.reportError(this, "animation clips list is empty");
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
            Assets.reportError(this, "has invalid animation clip references");
        }
        if (!flag2)
        {
            Assets.reportError(this, "missing 'Equip' animation clip");
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        isPro = data.ContainsKey("Pro");
        if (id < 2000 && !base.OriginAllowsVanillaLegacyId && !data.ContainsKey("Bypass_ID_Limit"))
        {
            throw new NotSupportedException("ID < 2000");
        }
        _itemName = localization.format("Name");
        _itemDescription = localization.format("Description");
        _itemDescription = ItemTool.filterRarityRichText(itemDescription);
        RichTextUtil.replaceNewlineMarkup(ref _itemDescription);
        instantiatedItemName = data.GetString("Instantiated_Item_Name_Override", id.ToString());
        type = (EItemType)Enum.Parse(typeof(EItemType), data.GetString("Type"), ignoreCase: true);
        if (data.ContainsKey("Rarity"))
        {
            rarity = (EItemRarity)Enum.Parse(typeof(EItemRarity), data.GetString("Rarity"), ignoreCase: true);
        }
        else
        {
            rarity = EItemRarity.COMMON;
        }
        if (isPro)
        {
            econIconUseId = data.ParseBool("Econ_Icon_Use_Id");
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
        size_x = data.ParseUInt8("Size_X", 0);
        if (size_x < 1)
        {
            size_x = 1;
        }
        size_y = data.ParseUInt8("Size_Y", 0);
        if (size_y < 1)
        {
            size_y = 1;
        }
        iconCameraOrthographicSize = data.ParseFloat("Size_Z", -1f);
        isEligibleForAutomaticIconMeasurements = data.ParseBool("Use_Auto_Icon_Measurements", defaultValue: true);
        econIconCameraOrthographicSize = data.ParseFloat("Size2_Z", -1f);
        sharedSkinLookupID = data.ParseUInt16("Shared_Skin_Lookup_ID", id);
        amount = data.ParseUInt8("Amount", 0);
        if (amount < 1)
        {
            amount = 1;
        }
        countMin = data.ParseUInt8("Count_Min", 0);
        if (countMin < 1)
        {
            countMin = 1;
        }
        countMax = data.ParseUInt8("Count_Max", 0);
        if (countMax < 1)
        {
            countMax = 1;
        }
        if (data.ContainsKey("Quality_Min"))
        {
            qualityMin = data.ParseUInt8("Quality_Min", 0);
        }
        else
        {
            qualityMin = 10;
        }
        if (data.ContainsKey("Quality_Max"))
        {
            qualityMax = data.ParseUInt8("Quality_Max", 0);
        }
        else
        {
            qualityMax = 90;
        }
        ShouldAttachEquippedModelToLeftHand = data.ContainsKey("Backward");
        shouldLeftHandedCharactersMirrorEquippedItem = data.ParseBool("Left_Handed_Characters_Mirror_Equipable", defaultValue: true);
        isEligibleForAutoStatDescriptions = data.ParseBool("Use_Auto_Stat_Descriptions", defaultValue: true);
        shouldProcedurallyAnimateInertia = data.ParseBool("Procedurally_Animate_Inertia", defaultValue: true);
        useable = data.GetString("Useable");
        updateUseableType();
        bool defaultValue = useableType != null;
        canPlayerEquip = data.ParseBool("Can_Player_Equip", defaultValue);
        equipableMovementSpeedMultiplier = data.ParseFloat("Equipable_Movement_Speed_Multiplier", 1f);
        if (canPlayerEquip)
        {
            _equip = LoadRedirectableAsset<AudioClip>(bundle, "Equip", data, "EquipAudioClip");
            inspectAudio = data.ReadAudioReference("InspectAudioDef", bundle);
            MasterBundleReference<GameObject> masterBundleReference = data.readMasterBundleReference<GameObject>("EquipablePrefab", bundle);
            if (masterBundleReference.isValid)
            {
                equipablePrefab = masterBundleReference.loadAsset();
            }
            if (!isPro)
            {
                GameObject gameObject = bundle.load<GameObject>("Animations");
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
        if (data.ContainsKey("InventoryAudio"))
        {
            inventoryAudio = data.ReadAudioReference("InventoryAudio", bundle);
        }
        else
        {
            inventoryAudio = GetDefaultInventoryAudio();
        }
        slot = data.ParseEnum("Slot", ESlotType.NONE);
        bool defaultValue2 = slot != ESlotType.PRIMARY;
        canUseUnderwater = data.ParseBool("Can_Use_Underwater", defaultValue2);
        if (!Dedicator.IsDedicatedServer || type == EItemType.GUN || type == EItemType.MELEE || (bool)shouldAlwaysLoadItemPrefab)
        {
            _item = bundle.load<GameObject>("Item");
            if (item == null)
            {
                throw new NotSupportedException("missing 'Item' GameObject");
            }
            if (item.transform.Find("Icon") != null && item.transform.Find("Icon").GetComponent<Camera>() != null)
            {
                throw new NotSupportedException("'Icon' has a camera attached!");
            }
            if (id < 2000 && (type == EItemType.GUN || type == EItemType.MELEE) && item.transform.Find("Stat_Tracker") == null)
            {
                Assets.reportError(this, "missing 'Stat_Tracker' Transform");
            }
            AssetValidation.searchGameObjectForErrors(this, item);
        }
        byte b = data.ParseUInt8("Blueprints", 0);
        byte b2 = data.ParseUInt8("Actions", 0);
        _blueprints = new List<Blueprint>(b);
        _actions = new List<Action>(b2);
        for (byte b3 = 0; b3 < b; b3++)
        {
            if (!data.ContainsKey("Blueprint_" + b3 + "_Type"))
            {
                throw new NotSupportedException("Missing blueprint type");
            }
            EBlueprintType newType = (EBlueprintType)Enum.Parse(typeof(EBlueprintType), data.GetString("Blueprint_" + b3 + "_Type"), ignoreCase: true);
            byte b4 = data.ParseUInt8("Blueprint_" + b3 + "_Supplies", 0);
            if (b4 < 1)
            {
                b4 = 1;
            }
            BlueprintSupply[] array = new BlueprintSupply[b4];
            for (byte b5 = 0; b5 < array.Length; b5++)
            {
                ushort newID = data.ParseUInt16("Blueprint_" + b3 + "_Supply_" + b5 + "_ID", 0);
                bool newCritical = data.ContainsKey("Blueprint_" + b3 + "_Supply_" + b5 + "_Critical");
                byte b6 = data.ParseUInt8("Blueprint_" + b3 + "_Supply_" + b5 + "_Amount", 0);
                if (b6 < 1)
                {
                    b6 = 1;
                }
                array[b5] = new BlueprintSupply(newID, newCritical, b6);
            }
            byte b7 = data.ParseUInt8("Blueprint_" + b3 + "_Outputs", 0);
            BlueprintOutput[] array2;
            if (b7 > 0)
            {
                array2 = new BlueprintOutput[b7];
                for (byte b8 = 0; b8 < array2.Length; b8++)
                {
                    ushort newID2 = data.ParseUInt16("Blueprint_" + b3 + "_Output_" + b8 + "_ID", 0);
                    byte b9 = data.ParseUInt8("Blueprint_" + b3 + "_Output_" + b8 + "_Amount", 0);
                    if (b9 < 1)
                    {
                        b9 = 1;
                    }
                    EItemOrigin newOrigin = data.ParseEnum("Blueprint_" + b3 + "_Output_" + b8 + "_Origin", EItemOrigin.CRAFT);
                    array2[b8] = new BlueprintOutput(newID2, b9, newOrigin);
                }
            }
            else
            {
                array2 = new BlueprintOutput[1];
                ushort num = data.ParseUInt16("Blueprint_" + b3 + "_Product", 0);
                if (num == 0)
                {
                    num = id;
                }
                byte b10 = data.ParseUInt8("Blueprint_" + b3 + "_Products", 0);
                if (b10 < 1)
                {
                    b10 = 1;
                }
                EItemOrigin newOrigin2 = data.ParseEnum("Blueprint_" + b3 + "_Origin", EItemOrigin.CRAFT);
                array2[0] = new BlueprintOutput(num, b10, newOrigin2);
            }
            ushort newTool = data.ParseUInt16("Blueprint_" + b3 + "_Tool", 0);
            bool newToolCritical = data.ContainsKey("Blueprint_" + b3 + "_Tool_Critical");
            Guid guid;
            ushort newBuild = data.ParseGuidOrLegacyId("Blueprint_" + b3 + "_Build", out guid);
            byte b11 = data.ParseUInt8("Blueprint_" + b3 + "_Level", 0);
            EBlueprintSkill newSkill = EBlueprintSkill.NONE;
            if (b11 > 0)
            {
                newSkill = (EBlueprintSkill)Enum.Parse(typeof(EBlueprintSkill), data.GetString("Blueprint_" + b3 + "_Skill"), ignoreCase: true);
            }
            bool newTransferState = data.ContainsKey("Blueprint_" + b3 + "_State_Transfer");
            string @string = data.GetString("Blueprint_" + b3 + "_Map");
            INPCCondition[] array3 = new INPCCondition[data.ParseUInt8("Blueprint_" + b3 + "_Conditions", 0)];
            NPCTool.readConditions(data, localization, "Blueprint_" + b3 + "_Condition_", array3, this);
            NPCRewardsList newQuestRewardsList = default(NPCRewardsList);
            newQuestRewardsList.Parse(data, localization, this, "Blueprint_" + b3 + "_Rewards", "Blueprint_" + b3 + "_Reward_");
            Blueprint blueprint = new Blueprint(this, b3, newType, array, array2, newTool, newToolCritical, newBuild, guid, b11, newSkill, newTransferState, @string, array3, newQuestRewardsList);
            blueprint.canBeVisibleWhenSearchedWithoutRequiredItems = data.ParseBool($"Blueprint_{b3}_Searchable", defaultValue: true);
            blueprints.Add(blueprint);
        }
        for (byte b12 = 0; b12 < b2; b12++)
        {
            if (!data.ContainsKey("Action_" + b12 + "_Type"))
            {
                throw new NotSupportedException("Missing action type");
            }
            EActionType newType2 = (EActionType)Enum.Parse(typeof(EActionType), data.GetString("Action_" + b12 + "_Type"), ignoreCase: true);
            byte b13 = data.ParseUInt8("Action_" + b12 + "_Blueprints", 0);
            if (b13 < 1)
            {
                b13 = 1;
            }
            ActionBlueprint[] array4 = new ActionBlueprint[b13];
            for (byte b14 = 0; b14 < array4.Length; b14++)
            {
                byte newID3 = data.ParseUInt8("Action_" + b12 + "_Blueprint_" + b14 + "_Index", 0);
                bool newLink = data.ContainsKey("Action_" + b12 + "_Blueprint_" + b14 + "_Link");
                array4[b14] = new ActionBlueprint(newID3, newLink);
            }
            string string2 = data.GetString("Action_" + b12 + "_Key");
            string newText;
            string newTooltip;
            if (string.IsNullOrEmpty(string2))
            {
                string key = "Action_" + b12 + "_Text";
                newText = ((!localization.has(key)) ? data.GetString(key) : localization.format(key));
                string key2 = "Action_" + b12 + "_Tooltip";
                newTooltip = ((!localization.has(key2)) ? data.GetString(key2) : localization.format(key2));
            }
            else
            {
                newText = string.Empty;
                newTooltip = string.Empty;
            }
            ushort num2 = data.ParseUInt16("Action_" + b12 + "_Source", 0);
            if (num2 == 0)
            {
                num2 = id;
            }
            actions.Add(new Action(num2, newType2, array4, newText, newTooltip, string2));
        }
        if (b2 == 0)
        {
            bool flag = false;
            for (byte b15 = 0; b15 < blueprints.Count; b15++)
            {
                Blueprint blueprint2 = blueprints[b15];
                if (blueprint2.type == EBlueprintType.REPAIR)
                {
                    Action action = new Action(id, EActionType.BLUEPRINT, new ActionBlueprint[1]
                    {
                        new ActionBlueprint(b15, newLink: true)
                    }, null, null, "Repair");
                    actions.Insert(0, action);
                }
                else if (blueprint2.type == EBlueprintType.AMMO)
                {
                    flag = true;
                }
                else if (blueprint2.supplies.Length == 1 && blueprint2.supplies[0].id == id)
                {
                    Action action2 = new Action(id, EActionType.BLUEPRINT, new ActionBlueprint[1]
                    {
                        new ActionBlueprint(b15, type == EItemType.GUN || type == EItemType.MELEE)
                    }, null, null, "Salvage");
                    actions.Add(action2);
                }
            }
            if (flag)
            {
                List<ActionBlueprint> list = new List<ActionBlueprint>();
                for (byte b16 = 0; b16 < blueprints.Count; b16++)
                {
                    if (blueprints[b16].type == EBlueprintType.AMMO)
                    {
                        ActionBlueprint actionBlueprint = new ActionBlueprint(b16, newLink: true);
                        list.Add(actionBlueprint);
                    }
                }
                Action action3 = new Action(id, EActionType.BLUEPRINT, list.ToArray(), null, null, "Refill");
                actions.Add(action3);
            }
        }
        _shouldVerifyHash = !data.ContainsKey("Bypass_Hash_Verification");
        overrideShowQuality = data.ContainsKey("Override_Show_Quality");
        shouldDropOnDeath = data.ParseBool("Should_Drop_On_Death", defaultValue: true);
        allowManualDrop = data.ParseBool("Allow_Manual_Drop", defaultValue: true);
        shouldDeleteAtZeroQuality = data.ParseBool("Should_Delete_At_Zero_Quality");
        shouldDestroyItemColliders = data.ParseBool("Destroy_Item_Colliders", defaultValue: true);
        if (!Dedicator.IsDedicatedServer && id < 2000 && doesItemTypeHaveSkins)
        {
            _albedoBase = bundle.load<Texture2D>("Albedo_Base");
            _metallicBase = bundle.load<Texture2D>("Metallic_Base");
            _emissionBase = bundle.load<Texture2D>("Emission_Base");
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
}
