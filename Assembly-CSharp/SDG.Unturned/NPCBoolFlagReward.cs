using System;

namespace SDG.Unturned;

public class NPCBoolFlagReward : INPCReward
{
    public ushort id { get; protected set; }

    public bool value { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.quests.sendSetFlag(id, (short)(value ? 1 : 0));
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseUInt16("ID", out var num))
        {
            id = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
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
        if (p.data.TryParseUInt16(p.legacyPrefix + "_ID", out var num))
        {
            id = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseBool(p.legacyPrefix + "_Value", out var flag))
        {
            value = flag;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCBoolFlagReward()
    {
    }

    [Obsolete]
    public NPCBoolFlagReward(ushort newID, bool newValue, string newText)
        : base(newText)
    {
        id = newID;
        value = newValue;
    }
}
