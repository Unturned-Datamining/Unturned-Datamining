using System;

namespace SDG.Unturned;

public class NPCPlayerLifeFoodCondition : NPCLogicCondition
{
    public int food { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.food, food);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return Local.FormatText(text, player.life.food, food);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseInt32("Value", out var value))
        {
            food = value;
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
            food = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCPlayerLifeFoodCondition()
    {
    }

    [Obsolete]
    public NPCPlayerLifeFoodCondition(int newFood, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        food = newFood;
    }
}
