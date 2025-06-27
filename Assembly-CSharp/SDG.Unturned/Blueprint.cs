using System;
using System.Collections.Generic;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class Blueprint
{
    private byte index;

    internal EBlueprintOperation _operation;

    internal CachingAssetRef _categoryTagRef;

    private BlueprintSupply[] _supplies;

    private BlueprintOutput[] _outputs;

    private bool hasCheckedForVanillaHeatSourceTag;

    private bool requiresVanillaHeatSourceTag;

    private static CachingAssetRef[] onlyVanillaHeatSourceTag = new CachingAssetRef[1] { PowerTool.VanillaCraftingHeatTag };

    internal CachingBcAssetRef effectAssetRef;

    private byte _level;

    private bool _transferState;

    /// <summary>
    /// If true, and transferState is enabled, delete attached items.
    /// </summary>
    public bool withoutAttachments;

    protected NPCConditionsList questConditionsList;

    protected NPCRewardsList questRewardsList;

    /// <summary>
    /// 2023-05-27: requested by Renaxon because some Arid blueprints are debug-only and
    /// should not be visible when players search by name. (the 3.23.7.0 update made
    /// non-craftable blueprints searchable for Buak)
    /// </summary>
    public bool canBeVisibleWhenSearchedWithoutRequiredItems = true;

    /// <summary>
    /// Optional case-sensitive identifier in list of blueprints.
    /// Added as an alternative to referencing blueprints by index.
    /// Defaults to null.
    /// </summary>
    public string Name { get; internal set; }

    public IBlueprintOwner Owner { get; internal set; }

    /// <summary>
    /// Index into Owner's blueprints list.
    /// </summary>
    public byte Index => index;

    /// <summary>
    /// Operation replaces the special behavior for EBlueprintType.Ammo and EBlueprintType.Repair.
    /// </summary>
    public EBlueprintOperation Operation => _operation;

    /// <summary>
    /// Note: if resolving ref please use GetCategoryTag instead for caching.
    /// </summary>
    public CachingAssetRef CategoryTagRef => _categoryTagRef;

    public BlueprintSupply[] supplies => _supplies;

    /// <summary>
    /// Only applicable for operations with a target item.
    ///
    /// Nelson 2025-04-11: initially, this was implemented as the last item in supplies list. However, there are a
    /// lot of checks for special handling of target item, so I think it makes sense to separate.
    /// </summary>
    public BlueprintSupply TargetItem { get; set; }

    public BlueprintOutput[] outputs => _outputs;

    /// <summary>
    /// If not null, these tags must be provided by nearby objects to craft this blueprint.
    /// Note: this is the list as-configured. It has not been filtered according to gameplay config.
    /// </summary>
    public CachingAssetRef[] RequiresNearbyCraftingTags { get; internal set; }

    public Guid BuildEffectGuid => effectAssetRef.Guid;

    public byte level => _level;

    public int SkillSpecialityIndex { get; internal set; } = -1;


    public int SkillIndex { get; internal set; } = -1;


    public bool transferState => _transferState;

    public string map { get; private set; }

    /// <summary>
    /// Must match conditions to craft.
    /// </summary>
    public INPCCondition[] questConditions => questConditionsList.conditions;

    /// <summary>
    /// Extra rewards given after crafting. Not displayed.
    /// </summary>
    public INPCReward[] questRewards => questRewardsList.rewards;

    /// <summary>
    /// Defaults to false. If true, blueprint can become visible in the crafting list even when NPC conditions
    /// are not met. This should typically only be enabled if all conditions are configured to be visible in the
    /// details panel. Otherwise, the default "conditions unmet" label isn't very informative for players.
    /// </summary>
    public bool CanBeVisibleWithUnmetConditions { get; set; }

    internal bool IsOutputFreeformBuildable
    {
        get
        {
            if (_outputs == null || _outputs.Length < 1)
            {
                return false;
            }
            BlueprintOutput[] array = _outputs;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].FindItemAsset() is ItemBarricadeAsset { build: EBuild.FREEFORM })
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool RequiresSkill
    {
        get
        {
            if (SkillSpecialityIndex >= 0 && SkillIndex >= 0)
            {
                return level > 0;
            }
            return false;
        }
    }

    [Obsolete("Changed to OwnerAsset because blueprints can be contained in CraftingAsset now")]
    public ItemAsset sourceItem => GetOwnerAsset() as ItemAsset;

    [Obsolete("Replaced by input item ShouldConsume false")]
    public ushort tool
    {
        get
        {
            if (_supplies != null && _supplies.Length != 0)
            {
                BlueprintSupply blueprintSupply = _supplies[_supplies.Length - 1];
                if (!blueprintSupply.ShouldConsume)
                {
                    return blueprintSupply.id;
                }
            }
            return 0;
        }
    }

    [Obsolete("Replaced by input item ShouldConsume false")]
    public bool toolCritical
    {
        get
        {
            if (_supplies != null && _supplies.Length != 0)
            {
                BlueprintSupply blueprintSupply = _supplies[_supplies.Length - 1];
                if (!blueprintSupply.ShouldConsume)
                {
                    return blueprintSupply.ShouldConsume;
                }
            }
            return false;
        }
    }

    [Obsolete("Renamed to Index to distinguish from named blueprint")]
    public byte id => index;

    [Obsolete]
    public ushort build => effectAssetRef.LegacyId;

    [Obsolete("Please use CategoryTags and Operation properties instead.")]
    public EBlueprintType type
    {
        get
        {
            switch (_operation)
            {
            case EBlueprintOperation.FillTargetItem:
                return EBlueprintType.AMMO;
            case EBlueprintOperation.RepairTargetItem:
                return EBlueprintType.REPAIR;
            default:
            {
                for (int i = 0; i < EBlueprintTypeEx.legacyBlueprintTypeCategoryTagRefs.Length; i++)
                {
                    if (_categoryTagRef == EBlueprintTypeEx.legacyBlueprintTypeCategoryTagRefs[i])
                    {
                        return (EBlueprintType)i;
                    }
                }
                return EBlueprintType.TOOL;
            }
            }
        }
    }

    [Obsolete("Replaced in favor of supporting all skills, ideally more customizable in future.")]
    public EBlueprintSkill skill => GetLegacyBlueprintSkill();

    public Asset GetOwnerAsset()
    {
        return Owner.GetBlueprintOwnerAsset();
    }

    /// <summary>
    /// Category tag replaces the blueprint "Type" which acted as both category AND behaviour modifier.
    /// </summary>
    public TagAsset GetCategoryTag()
    {
        return _categoryTagRef.Get<TagAsset>();
    }

    public CachingAssetRef[] GetApplicableRequiredNearbyCraftingTags()
    {
        if (RequiresNearbyCraftingTags == null || RequiresNearbyCraftingTags.Length < 1)
        {
            return null;
        }
        if (Provider.modeConfigData?.Gameplay?.Enable_Workstation_Requirements ?? true)
        {
            return RequiresNearbyCraftingTags;
        }
        if (!hasCheckedForVanillaHeatSourceTag)
        {
            hasCheckedForVanillaHeatSourceTag = true;
            CachingAssetRef[] requiresNearbyCraftingTags = RequiresNearbyCraftingTags;
            for (int i = 0; i < requiresNearbyCraftingTags.Length; i++)
            {
                if (requiresNearbyCraftingTags[i] == PowerTool.VanillaCraftingHeatTag)
                {
                    requiresVanillaHeatSourceTag = true;
                    break;
                }
            }
        }
        if (!requiresVanillaHeatSourceTag)
        {
            return null;
        }
        return onlyVanillaHeatSourceTag;
    }

    public EffectAsset FindBuildEffectAsset()
    {
        return effectAssetRef.Get<EffectAsset>();
    }

    public string DebugGetSkillName()
    {
        if (RequiresSkill)
        {
            switch ((EPlayerSpeciality)SkillSpecialityIndex)
            {
            case EPlayerSpeciality.OFFENSE:
                return ((EPlayerOffense)SkillIndex).ToString();
            case EPlayerSpeciality.DEFENSE:
                return ((EPlayerDefense)SkillIndex).ToString();
            case EPlayerSpeciality.SUPPORT:
                return ((EPlayerSupport)SkillIndex).ToString();
            }
        }
        return null;
    }

    public bool areConditionsMet(Player player)
    {
        return questConditionsList.AreConditionsMet(player);
    }

    public void ApplyConditions(Player player)
    {
        questConditionsList.ApplyConditions(player);
    }

    public void GrantRewards(Player player)
    {
        questRewardsList.Grant(player);
    }

    public bool DoesRequireNearbyCraftingTag(TagAsset tag)
    {
        if (tag == null || RequiresNearbyCraftingTags == null)
        {
            return false;
        }
        for (int i = 0; i < RequiresNearbyCraftingTags.Length; i++)
        {
            if (RequiresNearbyCraftingTags[i].IsReferenceTo(tag))
            {
                return true;
            }
        }
        return false;
    }

    public int CountOverlappingRequiredNearbyCraftingTags(HashSet<TagAsset> tags)
    {
        if (tags == null || tags.Count < 1 || RequiresNearbyCraftingTags.IsNullOrEmpty())
        {
            return 0;
        }
        int num = 0;
        for (int i = 0; i < RequiresNearbyCraftingTags.Length; i++)
        {
            TagAsset tagAsset = RequiresNearbyCraftingTags[i].Get<TagAsset>();
            if (tagAsset != null && tags.Contains(tagAsset))
            {
                num++;
            }
        }
        return num;
    }

    public bool ContainsAnyOfItems(HashSet<ItemAsset> availableItems)
    {
        if (supplies == null)
        {
            return false;
        }
        BlueprintSupply[] array = supplies;
        for (int i = 0; i < array.Length; i++)
        {
            ItemAsset itemAsset = array[i].FindItemAsset();
            if (itemAsset != null && availableItems.Contains(itemAsset))
            {
                return true;
            }
        }
        ItemAsset itemAsset2 = TargetItem?.FindItemAsset();
        if (itemAsset2 != null && availableItems.Contains(itemAsset2))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Search output items (excluding target item) for specific item.
    /// </summary>
    public bool DoesOutputCreateItem(ItemAsset itemAsset)
    {
        if (itemAsset == null || _outputs == null || _outputs.Length < 1)
        {
            return false;
        }
        BlueprintOutput[] array = _outputs;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].FindItemAsset() == itemAsset)
            {
                return true;
            }
        }
        return false;
    }

    public EBlueprintSkill GetLegacyBlueprintSkill()
    {
        if (SkillSpecialityIndex == 2)
        {
            if (SkillIndex == 1)
            {
                return EBlueprintSkill.CRAFT;
            }
            if (SkillIndex == 3)
            {
                return EBlueprintSkill.COOK;
            }
            if (SkillIndex == 7)
            {
                return EBlueprintSkill.REPAIR;
            }
        }
        return EBlueprintSkill.NONE;
    }

    public int GetPlayerSkillLevel(Player player)
    {
        return player.skills.skills[SkillSpecialityIndex][SkillIndex].level;
    }

    [Obsolete]
    public Blueprint(ItemAsset newSourceItem, byte newID, EBlueprintType newType, BlueprintSupply[] newSupplies, BlueprintOutput[] newOutputs, ushort newTool, bool newToolCritical, ushort newBuild, byte newLevel, EBlueprintSkill newSkill, bool newTransferState, string newMap, NPCConditionsList newQuestConditionsList, NPCRewardsList newQuestRewardsList)
        : this(newID, newSupplies, newOutputs, newLevel, newSkill, newTransferState, newWithoutAttachments: false, newMap, newQuestConditionsList, newQuestRewardsList)
    {
    }

    public Blueprint(byte newIndex, BlueprintSupply[] newSupplies, BlueprintOutput[] newOutputs, byte newLevel, EBlueprintSkill newSkill, bool newTransferState, bool newWithoutAttachments, string newMap, NPCConditionsList newQuestConditionsList, NPCRewardsList newQuestRewardsList)
    {
        index = newIndex;
        _supplies = newSupplies;
        _outputs = newOutputs;
        _level = newLevel;
        _transferState = newTransferState;
        withoutAttachments = newWithoutAttachments;
        map = newMap;
        questConditionsList = newQuestConditionsList;
        questRewardsList = newQuestRewardsList;
    }

    public override string ToString()
    {
        string empty = string.Empty;
        empty += GetCategoryTag()?.FriendlyName;
        empty += ": ";
        for (int i = 0; i < supplies.Length; i++)
        {
            if (i > 0)
            {
                empty += " + ";
            }
            empty += supplies[i].FindItemAsset()?.FriendlyName ?? "null";
            empty += " x";
            empty += supplies[i].amount;
        }
        if (TargetItem != null)
        {
            empty += " -> ";
            empty += TargetItem.FindItemAsset()?.FriendlyName ?? "null";
            empty += " x";
            empty += TargetItem.amount;
        }
        if (outputs != null && outputs.Length != 0)
        {
            empty += " = ";
            for (int j = 0; j < outputs.Length; j++)
            {
                if (j > 0)
                {
                    empty += " + ";
                }
                empty += outputs[j].FindItemAsset()?.FriendlyName ?? "null";
                empty += " x";
                empty += outputs[j].amount;
            }
        }
        return empty;
    }

    [Obsolete("Removed shouldSend parameter")]
    public void applyConditions(Player player, bool shouldSend)
    {
        ApplyConditions(player);
    }

    [Obsolete("Removed shouldSend parameter")]
    public void grantRewards(Player player, bool shouldSend)
    {
        GrantRewards(player);
    }
}
