using System;

namespace SDG.Unturned;

public class NPCReputationReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.skills.askRep(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(base.text))
        {
            base.text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Reputation");
        }
        string text = value.ToString();
        if (value > 0)
        {
            text = "+" + text;
        }
        return Local.FormatText(base.text, text);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseInt32("Value", out var num))
        {
            value = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseInt32(p.legacyPrefix + "_Value", out var num))
        {
            value = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCReputationReward()
    {
    }

    [Obsolete]
    public NPCReputationReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
