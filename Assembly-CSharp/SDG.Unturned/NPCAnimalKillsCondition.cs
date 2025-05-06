using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class NPCAnimalKillsCondition : INPCCondition
{
    public ushort id { get; protected set; }

    public short value { get; protected set; }

    public ushort animal { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        if (player.quests.getFlag(id, out var num))
        {
            return num >= value;
        }
        return false;
    }

    public override void ApplyCondition(Player player)
    {
        if (shouldReset)
        {
            player.quests.sendRemoveFlag(id);
        }
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = PlayerNPCQuestUI.localization.format("Condition_AnimalKills");
        }
        if (!player.quests.getFlag(id, out var num))
        {
            num = 0;
        }
        return Local.FormatText(text, num, value);
    }

    public override bool isAssociatedWithFlag(ushort flagID)
    {
        return flagID == id;
    }

    internal override void GatherAssociatedFlags(HashSet<ushort> associatedFlags)
    {
        associatedFlags.Add(id);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
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
        if (p.data.TryParseInt16("Value", out var num2))
        {
            value = num2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
        if (p.data.TryParseUInt16("Animal", out var num3))
        {
            animal = num3;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Animal");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
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
        if (p.data.TryParseInt16(p.legacyPrefix + "_Value", out var num2))
        {
            value = num2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
        if (p.data.TryParseUInt16(p.legacyPrefix + "_Animal", out var num3))
        {
            animal = num3;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Animal");
        }
    }

    public NPCAnimalKillsCondition()
    {
    }

    [Obsolete]
    public NPCAnimalKillsCondition(ushort newID, short newValue, ushort newAnimal, string newText, bool newShouldReset)
        : base(newText, newShouldReset)
    {
        id = newID;
        value = newValue;
        animal = newAnimal;
    }
}
