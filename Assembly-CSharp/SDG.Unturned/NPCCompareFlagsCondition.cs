using System;
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

    public override void ApplyCondition(Player player)
    {
        if (shouldReset)
        {
            player.quests.sendRemoveFlag(flag_A_ID);
            player.quests.sendRemoveFlag(flag_B_ID);
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

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseUInt16("A_ID", out var value))
        {
            flag_A_ID = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("A_ID");
        }
        if (p.data.TryParseUInt16("B_ID", out var value2))
        {
            flag_B_ID = value2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("B_ID");
        }
        allowFlag_A_Unset = p.data.ParseBool("Allow_A_Unset");
        allowFlag_B_Unset = p.data.ParseBool("Allow_B_Unset");
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseUInt16(p.legacyPrefix + "_A_ID", out var value))
        {
            flag_A_ID = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("A_ID");
        }
        if (p.data.TryParseUInt16(p.legacyPrefix + "_B_ID", out var value2))
        {
            flag_B_ID = value2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("B_ID");
        }
        allowFlag_A_Unset = p.data.ContainsKey(p.legacyPrefix + "_Allow_A_Unset");
        allowFlag_B_Unset = p.data.ContainsKey(p.legacyPrefix + "_Allow_B_Unset");
    }

    public NPCCompareFlagsCondition()
    {
    }

    [Obsolete]
    public NPCCompareFlagsCondition(ushort newFlag_A_ID, ushort newFlag_B_ID, bool newAllowFlag_A_Unset, bool newAllowFlag_B_Unset, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        flag_A_ID = newFlag_A_ID;
        allowFlag_A_Unset = newAllowFlag_A_Unset;
        flag_B_ID = newFlag_B_ID;
        allowFlag_B_Unset = newAllowFlag_B_Unset;
    }
}
