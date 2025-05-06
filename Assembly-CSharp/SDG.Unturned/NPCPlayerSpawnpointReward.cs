using System;

namespace SDG.Unturned;

public class NPCPlayerSpawnpointReward : INPCReward
{
    public string id { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.quests.npcSpawnId = id;
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        id = p.data.GetString("ID");
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        id = p.data.GetString(p.legacyPrefix + "_ID");
    }

    public NPCPlayerSpawnpointReward()
    {
    }

    [Obsolete]
    public NPCPlayerSpawnpointReward(string newID, string newText)
        : base(newText)
    {
        id = newID;
    }
}
