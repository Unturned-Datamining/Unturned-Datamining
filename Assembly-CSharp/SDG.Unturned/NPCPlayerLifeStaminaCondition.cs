namespace SDG.Unturned;

public class NPCPlayerLifeStaminaCondition : NPCLogicCondition
{
    public int stamina { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.stamina, stamina);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return Local.FormatText(text, player.life.stamina, stamina);
    }

    public NPCPlayerLifeStaminaCondition(int newStamina, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        stamina = newStamina;
    }
}
