using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCTreeKillsCondition : INPCCondition
{
    public ushort id { get; protected set; }

    public short value { get; protected set; }

    public Guid treeGuid { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        if (player.quests.getFlag(id, out var num))
        {
            return num >= value;
        }
        return false;
    }

    public override void ApplyCondition(Player player)
    {
        if (shouldReset)
        {
            player.quests.sendRemoveFlag(id);
        }
    }

    public override string formatCondition(Player player)
    {
        if (!player.quests.getFlag(id, out var num))
        {
            num = 0;
        }
        string arg = ((!(Assets.find(treeGuid) is ResourceAsset resourceAsset)) ? "?" : resourceAsset.resourceName);
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.format("Condition_TreeKills");
        }
        return Local.FormatText(text, num, value, arg);
    }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        return flagID == id;
    }

    internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
        associatedFlags.Add(id);
    }

    public NPCTreeKillsCondition(ushort newID, short newValue, Guid newTreeGuid, string newText, bool newShouldReset)
        : base(newText, newShouldReset)
    {
        id = newID;
        value = newValue;
        treeGuid = newTreeGuid;
    }
}
