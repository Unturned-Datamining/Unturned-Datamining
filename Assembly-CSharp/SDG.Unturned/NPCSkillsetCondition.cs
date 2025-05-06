using System;

namespace SDG.Unturned;

public class NPCSkillsetCondition : NPCLogicCondition
{
    public EPlayerSkillset skillset { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.channel.owner.skillset, skillset);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseEnum<EPlayerSkillset>("Value", out var value))
        {
            skillset = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseEnum<EPlayerSkillset>(p.legacyPrefix + "_Value", out var value))
        {
            skillset = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCSkillsetCondition()
    {
    }

    [Obsolete]
    public NPCSkillsetCondition(EPlayerSkillset newSkillset, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        skillset = newSkillset;
    }
}
