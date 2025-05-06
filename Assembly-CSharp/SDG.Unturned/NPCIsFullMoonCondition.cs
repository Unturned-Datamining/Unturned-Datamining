using System;

namespace SDG.Unturned;

public class NPCIsFullMoonCondition : INPCCondition
{
    public bool value { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return LightingManager.isFullMoon == value;
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseBool("Value", out var flag))
        {
            value = flag;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseBool(p.legacyPrefix + "_Value", out var flag))
        {
            value = flag;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCIsFullMoonCondition()
    {
    }

    [Obsolete]
    public NPCIsFullMoonCondition(bool newValue, string newText)
        : base(newText, newShouldReset: false)
    {
        value = newValue;
    }
}
