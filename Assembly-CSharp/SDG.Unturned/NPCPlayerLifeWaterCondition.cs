using System;

namespace SDG.Unturned;

public class NPCPlayerLifeWaterCondition : NPCLogicCondition
{
    public int water { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.water, water);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return Local.FormatText(text, player.life.water, water);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseInt32("Value", out var value))
        {
            water = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseInt32(p.legacyPrefix + "_Value", out var value))
        {
            water = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCPlayerLifeWaterCondition()
    {
    }

    [Obsolete]
    public NPCPlayerLifeWaterCondition(int newWater, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        water = newWater;
    }
}
