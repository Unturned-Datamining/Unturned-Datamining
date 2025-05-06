using System;

namespace SDG.Unturned;

public class NPCPlayerLifeStaminaCondition : NPCLogicCondition
{
    public int stamina { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(player.life.stamina, stamina);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        return Local.FormatText(text, player.life.stamina, stamina);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseInt32("Value", out var value))
        {
            stamina = value;
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
            stamina = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCPlayerLifeStaminaCondition()
    {
    }

    [Obsolete]
    public NPCPlayerLifeStaminaCondition(int newStamina, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        stamina = newStamina;
    }
}
