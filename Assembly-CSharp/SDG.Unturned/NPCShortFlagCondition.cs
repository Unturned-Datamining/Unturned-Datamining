namespace SDG.Unturned;

public class NPCShortFlagCondition : NPCFlagCondition
{
    public short value { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        if (player.quests.getFlag(base.id, out var a))
        {
            return doesLogicPass(a, value);
        }
        return base.allowUnset;
    }

    public override void applyCondition(Player player, bool shouldSend)
    {
        if (shouldReset)
        {
            if (shouldSend)
            {
                player.quests.sendRemoveFlag(base.id);
            }
            else
            {
                player.quests.removeFlag(base.id);
            }
        }
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        if (!player.quests.getFlag(base.id, out var num))
        {
            num = (short)(base.allowUnset ? value : 0);
        }
        return string.Format(text, num, value);
    }

    public NPCShortFlagCondition(ushort newID, short newValue, bool newAllowUnset, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newID, newAllowUnset, newLogicType, newText, newShouldReset)
    {
        value = newValue;
    }
}
