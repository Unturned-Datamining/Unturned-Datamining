namespace SDG.Unturned;

public class NPCHolidayCondition : INPCCondition
{
    public ENPCHoliday holiday { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return HolidayUtil.isHolidayActive(holiday);
    }

    public NPCHolidayCondition(ENPCHoliday newHoliday)
        : base(null, newShouldReset: false)
    {
        holiday = newHoliday;
    }
}
