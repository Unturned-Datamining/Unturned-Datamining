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

    public NPCDateCounterCondition(long newValue, long newDivisor, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        value = newValue;
        divisor = newDivisor;
    }
}
