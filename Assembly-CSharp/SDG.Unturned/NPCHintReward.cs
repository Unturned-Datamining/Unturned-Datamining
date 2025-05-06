using System;

namespace SDG.Unturned;

public class NPCHintReward : INPCReward
{
    /// <summary>
    /// How many seconds message should popup.
    /// </summary>
    private float duration;

    public override void GrantReward(Player player)
    {
        player.ServerShowHint(text, duration);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (string.IsNullOrEmpty(text))
        {
            text = p.data.GetString("Text");
        }
        duration = p.data.ParseFloat("Duration", 2f);
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (string.IsNullOrEmpty(text))
        {
            text = p.data.GetString(p.legacyPrefix + "_Text");
        }
        duration = p.data.ParseFloat(p.legacyPrefix + "_Duration", 2f);
    }

    public NPCHintReward()
    {
    }

    [Obsolete]
    public NPCHintReward(float newDuration, string newText)
        : base(newText)
    {
        duration = newDuration;
    }
}
