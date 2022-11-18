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

    public bool hasSupplies;

    public bool hasTool;

    public bool hasItem;

    public bool hasSkills;

    public ushort tools;

    public ushort products;

    public ushort items;

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

    public INPCCondition[] questConditions { get; protected set; }

    public INPCReward[] questRewards { get; protected set; }

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

    public void applyConditions(Player player, bool shouldSend)
    {
        if (questConditions != null)
        {
            for (int i = 0; i < questConditions.Length; i++)
            {
                questConditions[i].applyCondition(player, shouldSend);
            }
        }
    }

    public void grantRewards(Player player, bool shouldSend)
    {
        if (questRewards != null)
        {
            for (int i = 0; i < questRewards.Length; i++)
            {
                questRewards[i].grantReward(player, shouldSend);
            }
        }
    }

    public Blueprint(ItemAsset newSourceItem, byte newID, EBlueprintType newType, BlueprintSupply[] newSupplies, BlueprintOutput[] newOutputs, ushort newTool, bool newToolCritical, ushort newBuild, byte newLevel, EBlueprintSkill newSkill, bool newTransferState, string newMap, INPCCondition[] newQuestConditions, INPCReward[] newQuestRewards)
        : this(newSourceItem, newID, newType, newSupplies, newOutputs, newTool, newToolCritical, newBuild, default(Guid), newLevel, newSkill, newTransferState, newMap, newQuestConditions, newQuestRewards)
    {
    }

    public Blueprint(ItemAsset newSourceItem, byte newID, EBlueprintType newType, BlueprintSupply[] newSupplies, BlueprintOutput[] newOutputs, ushort newTool, bool newToolCritical, ushort newBuild, Guid newBuildEffectGuid, byte newLevel, EBlueprintSkill newSkill, bool newTransferState, string newMap, INPCCondition[] newQuestConditions, INPCReward[] newQuestRewards)
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
        map = newMap;
        questConditions = newQuestConditions;
        questRewards = newQuestRewards;
        hasSupplies = false;
        hasTool = false;
        tools = 0;
    }

    public override string ToString()
    {
        string empty = string.Empty;
        empty += type;
        empty += ": ";
        for (byte b = 0; b < supplies.Length; b = (byte)(b + 1))
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
        for (byte b2 = 0; b2 < outputs.Length; b2 = (byte)(b2 + 1))
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
}
