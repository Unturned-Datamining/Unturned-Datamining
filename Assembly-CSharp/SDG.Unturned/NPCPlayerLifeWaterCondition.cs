namespace SDG.Unturned;

public class NPCPlayerLifeWaterCondition : NPCLogicCondition
{
    public int water { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.water, water);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return string.Format(text, player.life.water, water);
    }

    public NPCPlayerLifeWaterCondition(int newWater, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        water = newWater;
    }
}
