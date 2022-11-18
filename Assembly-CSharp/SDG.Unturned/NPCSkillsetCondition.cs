namespace SDG.Unturned;

public class NPCSkillsetCondition : NPCLogicCondition
{
    public EPlayerSkillset skillset { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.channel.owner.skillset, skillset);
    }

    public NPCSkillsetCondition(EPlayerSkillset newSkillset, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        skillset = newSkillset;
    }
}
