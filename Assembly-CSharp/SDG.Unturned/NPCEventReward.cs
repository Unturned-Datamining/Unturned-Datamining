using System;

namespace SDG.Unturned;

public class NPCEventReward : INPCReward
{
    public string id { get; protected set; }

    public override void GrantReward(Player player)
    {
        NPCEventManager.broadcastEvent(player, id, shouldReplicate: true);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryGetString("ID", out var value))
        {
            id = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryGetString(p.legacyPrefix + "_ID", out var value))
        {
            id = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
    }

    public NPCEventReward()
    {
    }

    [Obsolete]
    public NPCEventReward(string newID, string newText)
        : base(newText)
    {
        id = newID;
    }
}
