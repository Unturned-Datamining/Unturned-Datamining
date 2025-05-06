using System;
using UnityEngine;

namespace SDG.Unturned;

public class NPCRandomShortFlagReward : INPCReward
{
    public ushort id { get; protected set; }

    public short minValue { get; protected set; }

    public short maxValue { get; protected set; }

    public ENPCModificationType modificationType { get; protected set; }

    public override void GrantReward(Player player)
    {
        short num = (short)UnityEngine.Random.Range(minValue, maxValue + 1);
        if (modificationType == ENPCModificationType.ASSIGN)
        {
            player.quests.sendSetFlag(id, num);
            return;
        }
        player.quests.getFlag(id, out var value);
        if (modificationType == ENPCModificationType.INCREMENT)
        {
            value += num;
        }
        else if (modificationType == ENPCModificationType.DECREMENT)
        {
            value -= num;
        }
        player.quests.sendSetFlag(id, value);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseUInt16("ID", out var value))
        {
            id = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseInt16("Min_Value", out var value2))
        {
            minValue = value2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Min_Value");
        }
        if (p.data.TryParseInt16("Max_Value", out var value3))
        {
            maxValue = value3;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Max_Value");
        }
        if (p.data.TryParseEnum<ENPCModificationType>("Modification", out var value4))
        {
            modificationType = value4;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Modification");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseUInt16(p.legacyPrefix + "_ID", out var value))
        {
            id = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("ID");
        }
        if (p.data.TryParseInt16(p.legacyPrefix + "_Min_Value", out var value2))
        {
            minValue = value2;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Min_Value");
        }
        if (p.data.TryParseInt16(p.legacyPrefix + "_Max_Value", out var value3))
        {
            maxValue = value3;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Max_Value");
        }
        if (p.data.TryParseEnum<ENPCModificationType>(p.legacyPrefix + "_Modification", out var value4))
        {
            modificationType = value4;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Modification");
        }
    }

    public NPCRandomShortFlagReward()
    {
    }

    [Obsolete]
    public NPCRandomShortFlagReward(ushort newID, short newMinValue, short newMaxValue, ENPCModificationType newModificationType, string newText)
        : base(newText)
    {
        id = newID;
        minValue = newMinValue;
        maxValue = newMaxValue;
        modificationType = newModificationType;
    }
}
