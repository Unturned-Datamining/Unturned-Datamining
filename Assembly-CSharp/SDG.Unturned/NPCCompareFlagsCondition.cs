using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCCompareFlagsCondition : NPCLogicCondition
{
    public ushort flag_B_ID;

    public ushort flag_A_ID { get; protected set; }

    public bool allowFlag_A_Unset { get; protected set; }

    public bool allowFlag_B_Unset { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        if (!player.quests.getFlag(flag_A_ID, out var value) && !allowFlag_A_Unset)
        {
            return false;
        }
        if (!player.quests.getFlag(flag_B_ID, out var value2) && !allowFlag_B_Unset)
        {
            return false;
        }
        return doesLogicPass(value, value2);
    }

    public override void applyCondition(Player player, bool shouldSend)
    {
        if (shouldReset)
        {
            if (shouldSend)
            {
                player.quests.sendRemoveFlag(flag_A_ID);
                player.quests.sendRemoveFlag(flag_B_ID);
            }
            else
            {
                player.quests.removeFlag(flag_A_ID);
                player.quests.removeFlag(flag_B_ID);
            }
        }
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return text;
    }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        if (flagID != flag_A_ID)
        {
            return flagID == flag_B_ID;
        }
        return true;
    }

    internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
        associatedFlags.Add(flag_A_ID);
        associatedFlags.Add(flag_B_ID);
    }

    public NPCCompareFlagsCondition(ushort newFlag_A_ID, ushort newFlag_B_ID, bool newAllowFlag_A_Unset, bool newAllowFlag_B_Unset, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        flag_A_ID = newFlag_A_ID;
        allowFlag_A_Unset = newAllowFlag_A_Unset;
        flag_B_ID = newFlag_B_ID;
        allowFlag_B_Unset = newAllowFlag_B_Unset;
    }
}
