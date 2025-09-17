using System;

namespace SDG.Unturned;

public class NPCEventReward : INPCReward
{
    /// <summary>
    /// If true, the server will replicate the event to clients.
    /// Defaults to true.
    /// </summary>
    public bool ShouldReplicate = true;

    public string id { get; protected set; }

    public override void GrantReward(Player player)
    {
        NPCEventManager.broadcastEvent(player, id, ShouldReplicate);
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
        ShouldReplicate = p.data.ParseBool("Replicate", defaultValue: true);
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
        ShouldReplicate = p.data.ParseBool(p.legacyPrefix + "_Replicate", defaultValue: true);
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
