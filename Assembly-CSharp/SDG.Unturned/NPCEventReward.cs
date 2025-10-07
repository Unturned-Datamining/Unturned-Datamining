using System;

namespace SDG.Unturned;

public class NPCEventReward : INPCReward
{
    [Obsolete]
    public bool ShouldReplicate = true;

    public string id { get; protected set; }

    public ENPCEventReplicationMode ReplicationMode { get; set; }

    public override void GrantReward(Player player)
    {
        NPCEventManager.BroadcastEvent(player, id, ReplicationMode);
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
        if (p.data.ParseBool("InstigatorOnly"))
        {
            ReplicationMode = ENPCEventReplicationMode.InstigatorOnly;
            return;
        }
        ShouldReplicate = p.data.ParseBool("Replicate", defaultValue: true);
        ReplicationMode = (ShouldReplicate ? ENPCEventReplicationMode.AuthorityAndClients : ENPCEventReplicationMode.AuthorityOnly);
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
        if (p.data.ParseBool(p.legacyPrefix + "_InstigatorOnly"))
        {
            ReplicationMode = ENPCEventReplicationMode.InstigatorOnly;
            return;
        }
        ShouldReplicate = p.data.ParseBool(p.legacyPrefix + "_Replicate", defaultValue: true);
        ReplicationMode = (ShouldReplicate ? ENPCEventReplicationMode.AuthorityAndClients : ENPCEventReplicationMode.AuthorityOnly);
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
