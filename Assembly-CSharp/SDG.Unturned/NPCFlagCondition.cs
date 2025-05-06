using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCFlagCondition : NPCLogicCondition
{
    public ushort id { get; protected set; }

    public bool allowUnset { get; protected set; }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        return flagID == id;
    }

    internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
        associatedFlags.Add(id);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseUInt16("ID", out var value))
        {
            id = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        allowUnset = p.data.ParseBool("Allow_Unset");
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseUInt16(p.legacyPrefix + "_ID", out var value))
        {
            id = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        allowUnset = p.data.ContainsKey(p.legacyPrefix + "_Allow_Unset");
    }

    public NPCFlagCondition()
    {
    }

    [Obsolete]
    public NPCFlagCondition(ushort newID, bool newAllowUnset, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        id = newID;
        allowUnset = newAllowUnset;
    }
}
