namespace SDG.Unturned;

public class NPCReputationCondition : NPCLogicCondition
{
    public int reputation { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.skills.reputation, reputation);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(base.text))
        {
            base.text = PlayerNPCQuestUI.localization.read("Condition_Reputation");
        }
        string text = player.skills.reputation.ToString();
        if (player.skills.reputation > 0)
        {
            text = "+" + text;
        }
        string text2 = reputation.ToString();
        if (reputation > 0)
        {
            text2 = "+" + text2;
        }
        return string.Format(base.text, text, text2);
    }

    public NPCReputationCondition(int newReputation, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        reputation = newReputation;
    }
}
