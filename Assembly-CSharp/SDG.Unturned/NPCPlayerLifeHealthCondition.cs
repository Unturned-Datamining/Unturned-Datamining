using System;

namespace SDG.Unturned;

public class NPCPlayerLifeHealthCondition : NPCLogicCondition
{
    public int health { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.health, health);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return Local.FormatText(text, player.life.health, health);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseInt32("Value", out var value))
        {
            health = value;
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
            health = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCPlayerLifeHealthCondition()
    {
    }

    [Obsolete]
    public NPCPlayerLifeHealthCondition(int newHealth, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        health = newHealth;
    }
}
