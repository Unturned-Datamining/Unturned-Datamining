using System;

namespace SDG.Unturned;

public class NPCExperienceCondition : NPCLogicCondition
{
    public uint experience { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.skills.experience, experience);
    }

    public override void ApplyCondition(Player player)
    {
        if (shouldReset)
        {
            player.skills.askSpend(experience);
        }
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Condition_Experience");
        }
        return Local.FormatText(text, player.skills.experience, experience);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseUInt32("Value", out var value))
        {
            experience = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseUInt32(p.legacyPrefix + "_Value", out var value))
        {
            experience = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCExperienceCondition()
    {
    }

    [Obsolete]
    public NPCExperienceCondition(uint newExperience, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        experience = newExperience;
    }
}
