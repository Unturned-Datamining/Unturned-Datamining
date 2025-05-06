using System;

namespace SDG.Unturned;

public class NPCPlayerLifeFoodReward : INPCReward
{
    public int value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.life.serverModifyFood(value);
    }

    public override string formatReward(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.read("Reward_Food");
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

    public NPCPlayerLifeFoodReward()
    {
    }

    [Obsolete]
    public NPCPlayerLifeFoodReward(int newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
