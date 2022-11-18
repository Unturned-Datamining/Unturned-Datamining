namespace SDG.Unturned;

public class NPCPlayerLifeHealthCondition : NPCLogicCondition
{
    public int health { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.health, health);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return string.Format(text, player.life.health, health);
    }

    public NPCPlayerLifeHealthCondition(int newHealth, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        health = newHealth;
    }
}
