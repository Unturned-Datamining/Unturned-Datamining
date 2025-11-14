using System;

namespace SDG.Unturned;

public class NPCPlayerLifeHealthReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.life.serverModifyHealth(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.FormatOrEmpty("Reward_Health");
        }
        return Local.FormatText(text, value);
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

    public NPCPlayerLifeHealthReward()
    {
    }

    [Obsolete]
    public NPCPlayerLifeHealthReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
