using System;

namespace SDG.Unturned;

public class NPCCutsceneModeReward : INPCReward
{
    private bool value;

    public override void GrantReward(Player player)
    {
        player.quests.ServerSetCutsceneModeActive(value);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseBool("Value", out var flag))
        {
            value = flag;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseBool(p.legacyPrefix + "_Value", out var flag))
        {
            value = flag;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCCutsceneModeReward()
    {
    }

    [Obsolete]
    public NPCCutsceneModeReward(bool newValue, string newText)
        : base(newText)
    {
        value = newValue;
    }
}
