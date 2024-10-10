using System;

namespace SDG.Unturned;

public class Blueprint
{
    private byte _id;

    private EBlueprintType _type;

    private BlueprintSupply[] _supplies;

    private BlueprintOutput[] _outputs;

    private ushort _tool;

    private bool _toolCritical;

    private Guid _buildEffectGuid;

    private ushort _build;

    private byte _level;

    private EBlueprintSkill _skill;

    private bool _transferState;

    /// <summary>
    /// If true, and transferState is enabled, delete attached items.
    /// </summary>
    public bool withoutAttachments;

    public bool hasSupplies;

    public bool hasTool;

    public bool hasItem;

    public bool hasSkills;

    public ushort tools;

    public ushort products;

    public ushort items;

    protected NPCRewardsList questRewardsList;

    /// <summary>
    /// 2023-05-27: requested by Renaxon because some Arid blueprints are debug-only and
    /// should not be visible when players search by name. (the 3.23.7.0 update made
    /// non-craftable blueprints searchable for Buak)
    /// </summary>
    public bool canBeVisibleWhenSearchedWithoutRequiredItems = true;

    public ItemAsset sourceItem { get; protected set; }

    public byte id => _id;

    public EBlueprintType type => _type;

    public BlueprintSupply[] supplies => _supplies;

    public BlueprintOutput[] outputs => _outputs;

    public ushort tool => _tool;

    public bool toolCritical => _toolCritical;

    public Guid BuildEffectGuid => _buildEffectGuid;

    public ushort build
    {
        [Obsolete]
        get
        {
            return _build;
        }
    }

    public byte level => _level;

    public EBlueprintSkill skill => _skill;

    public bool transferState => _transferState;

    public string map { get; private set; }

    /// <summary>
    /// Must match conditions to craft.
    /// </summary>
    public INPCCondition[] questConditions { get; protected set; }

    /// <summary>
    /// Extra rewards given after crafting. Not displayed.
    /// </summary>
    public INPCReward[] questRewards => questRewardsList.rewards;

    internal bool IsOutputFreeformBuildable
    {
        get
        {
            if (_outputs == null || _outputs.Length < 1)
            {
                return false;
            }
            BlueprintOutput[] array = _outputs;
            foreach (BlueprintOutput blueprintOutput in array)
            {
                if (Assets.find(EAssetType.ITEM, blueprintOutput.id) is ItemBarricadeAsset { build: EBuild.FREEFORM })
                {
                    return true;
                }
            }
            return false;
        }
    }

    public EffectAsset FindBuildEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(_buildEffectGuid, _build);
    }

    public bool areConditionsMet(Player player)
    {
        if (questConditions != null)
        {
            for (int i = 0; i < questConditions.Length; i++)
            {
                if (!questConditions[i].isConditionMet(player))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void ApplyConditions(Player player)
    {
        if (questConditions != null)
        {
            for (int i = 0; i < questConditions.Length; i++)
            {
                questConditions[i].ApplyCondition(player);
            }
        }
    }

    public void GrantRewards(Player player)
    {
        questRewardsList.Grant(player);
    }

    public Blueprint(ItemAsset newSourceItem, byte newID, EBlueprintType newType, BlueprintSupply[] newSupplies, BlueprintOutput[] newOutputs, ushort newTool, bool newToolCritical, ushort newBuild, byte newLevel, EBlueprintSkill newSkill, bool newTransferState, string newMap, INPCCondition[] newQuestConditions, NPCRewardsList newQuestRewardsList)
        : this(newSourceItem, newID, newType, newSupplies, newOutputs, newTool, newToolCritical, newBuild, default(Guid), newLevel, newSkill, newTransferState, newWithoutAttachments: false, newMap, newQuestConditions, newQuestRewardsList)
    {
    }

    public Blueprint(ItemAsset newSourceItem, byte newID, EBlueprintType newType, BlueprintSupply[] newSupplies, BlueprintOutput[] newOutputs, ushort newTool, bool newToolCritical, ushort newBuild, Guid newBuildEffectGuid, byte newLevel, EBlueprintSkill newSkill, bool newTransferState, bool newWithoutAttachments, string newMap, INPCCondition[] newQuestConditions, NPCRewardsList newQuestRewardsList)
    {
        sourceItem = newSourceItem;
        _id = newID;
        _type = newType;
        _supplies = newSupplies;
        _outputs = newOutputs;
        _tool = newTool;
        _toolCritical = newToolCritical;
        _buildEffectGuid = newBuildEffectGuid;
        _build = newBuild;
        _level = newLevel;
        _skill = newSkill;
        _transferState = newTransferState;
        withoutAttachments = newWithoutAttachments;
        map = newMap;
        questConditions = newQuestConditions;
        questRewardsList = newQuestRewardsList;
        hasSupplies = false;
        hasTool = false;
        tools = 0;
    }

    public override string ToString()
    {
        string empty = string.Empty;
        empty += type;
        empty += ": ";
        for (byte b = 0; b < supplies.Length; b++)
        {
            if (b > 0)
            {
                empty += " + ";
            }
            empty += supplies[b].id;
            empty += "x";
            empty += supplies[b].amount;
        }
        empty += " = ";
        for (byte b2 = 0; b2 < outputs.Length; b2++)
        {
            if (b2 > 0)
            {
                empty += " + ";
            }
            empty += outputs[b2].id;
            empty += "x";
            empty += outputs[b2].amount;
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
