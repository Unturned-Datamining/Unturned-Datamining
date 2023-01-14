namespace SDG.Unturned;

public class NPCHolidayCondition : NPCLogicCondition
{
    public ENPCHoliday holiday { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(HolidayUtil.getActiveHoliday(), holiday);
    }

    public NPCHolidayCondition(ENPCHoliday newHoliday, ENPCLogicType newLogicType)
        : base(newLogicType, null, newShouldReset: false)
    {
        holiday = newHoliday;
    }
}
