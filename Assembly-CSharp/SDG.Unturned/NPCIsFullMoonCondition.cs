namespace SDG.Unturned;

public class NPCIsFullMoonCondition : INPCCondition
{
    public bool value { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return LightingManager.isFullMoon == value;
    }

    public NPCIsFullMoonCondition(bool newValue, string newText)
        : base(newText, newShouldReset: false)
    {
        value = newValue;
    }
}
