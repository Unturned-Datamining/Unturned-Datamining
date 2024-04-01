namespace SDG.Unturned;

public class NPCPlayerLifeVirusCondition : NPCLogicCondition
{
    public int virus { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.virus, virus);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return Local.FormatText(text, player.life.virus, virus);
    }

    public NPCPlayerLifeVirusCondition(int newVirus, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        virus = newVirus;
    }
}
