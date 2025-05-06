using System;

namespace SDG.Unturned;

public class NPCHolidayCondition : NPCLogicCondition
{
    public ENPCHoliday holiday { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(HolidayUtil.getActiveHoliday(), holiday);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseEnum<ENPCHoliday>("Value", out var value))
        {
            holiday = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseEnum<ENPCHoliday>(p.legacyPrefix + "_Value", out var value))
        {
            holiday = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCHolidayCondition()
    {
    }

    [Obsolete]
    public NPCHolidayCondition(ENPCHoliday newHoliday, ENPCLogicType newLogicType)
        : base(newLogicType, null, newShouldReset: false)
    {
        holiday = newHoliday;
    }
}
