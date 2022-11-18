namespace SDG.Unturned;

public class NPCPlayerLifeFoodCondition : NPCLogicCondition
{
    public int food { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.food, food);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return string.Format(text, player.life.food, food);
    }

    public NPCPlayerLifeFoodCondition(int newFood, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        food = newFood;
    }
}
