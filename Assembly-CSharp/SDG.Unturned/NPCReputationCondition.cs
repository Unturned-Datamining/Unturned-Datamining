using System;

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
            base.text = PlayerNPCQuestUI.localization.FormatOrEmpty("Condition_Reputation");
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
        return Local.FormatText(base.text, text, text2);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseInt32("Value", out var value))
        {
            reputation = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseInt32(p.legacyPrefix + "_Value", out var value))
        {
            reputation = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCReputationCondition()
    {
    }

    [Obsolete]
    public NPCReputationCondition(int newReputation, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        reputation = newReputation;
    }
}
