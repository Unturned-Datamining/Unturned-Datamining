using System;

namespace SDG.Unturned;

public class NPCPlayerLifeVirusCondition : NPCLogicCondition
{
    public int virus { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.virus, virus);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return Local.FormatText(text, player.life.virus, virus);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseInt32("Value", out var value))
        {
            virus = value;
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
            virus = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCPlayerLifeVirusCondition()
    {
    }

    [Obsolete]
    public NPCPlayerLifeVirusCondition(int newVirus, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        virus = newVirus;
    }
}
