namespace SDG.Unturned;

public class NPCBoolFlagCondition : NPCFlagCondition
{
    public bool value { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        if (player.quests.getFlag(base.id, out var num))
        {
            return doesLogicPass(num == 1, value);
        }
        return base.allowUnset;
    }

    public override void ApplyCondition(Player player)
    {
        if (shouldReset)
        {
            player.quests.sendRemoveFlag(base.id);
        }
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return string.Format(text, isConditionMet(player) ? 1 : 0);
    }

    public NPCBoolFlagCondition(ushort newID, bool newValue, bool newAllowUnset, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newID, newAllowUnset, newLogicType, newText, newShouldReset)
    {
        value = newValue;
    }
}
