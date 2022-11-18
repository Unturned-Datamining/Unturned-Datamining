namespace SDG.Unturned;

public class NPCExperienceCondition : NPCLogicCondition
{
    public uint experience { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.skills.experience, experience);
    }

    public override void applyCondition(Player player, bool shouldSend)
    {
        if (shouldReset)
        {
            if (shouldSend)
            {
                player.skills.askSpend(experience);
            }
            else
            {
                player.skills.modXp2(experience);
            }
        }
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Condition_Experience");
        }
        return string.Format(text, player.skills.experience, experience);
    }

    public NPCExperienceCondition(uint newExperience, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        experience = newExperience;
    }
}
