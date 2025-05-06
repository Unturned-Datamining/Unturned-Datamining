using System;

namespace SDG.Unturned;

public class NPCDateCounterCondition : NPCLogicCondition
{
    protected long value;

    protected long divisor;

    public override bool isConditionMet(Player player)
    {
        long a = LightingManager.DateCounter % divisor;
        return NPCTool.doesLogicPass(base.logicType, a, value);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseInt64("Value", out var num))
        {
            value = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
        if (p.data.TryParseInt64("Divisor", out var num2))
        {
            divisor = num2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Divisor");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseInt64(p.legacyPrefix + "_Value", out var num))
        {
            value = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
        if (p.data.TryParseInt64(p.legacyPrefix + "_Divisor", out var num2))
        {
            divisor = num2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Divisor");
        }
    }

    public NPCDateCounterCondition()
    {
    }

    [Obsolete]
    public NPCDateCounterCondition(long newValue, long newDivisor, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        value = newValue;
        divisor = newDivisor;
    }
}
