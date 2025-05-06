using System;

namespace SDG.Unturned;

public class NPCAchievementReward : INPCReward
{
    public string id { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.sendAchievementUnlocked(id);
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
        if (!Provider.statusData.Achievements.canBeGrantedByNPC(id))
        {
            p.ReportError("achievement \"" + id + "\" cannot be granted by NPCs");
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
        if (!Provider.statusData.Achievements.canBeGrantedByNPC(id))
        {
            p.ReportError("achievement \"" + id + "\" cannot be granted by NPCs");
        }
    }

    public NPCAchievementReward()
    {
    }

    [Obsolete]
    public NPCAchievementReward(string newID, string newText)
        : base(newText)
    {
        id = newID;
    }
}
