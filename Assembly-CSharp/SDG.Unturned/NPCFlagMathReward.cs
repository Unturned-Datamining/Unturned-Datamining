using System;
using UnityEngine;

namespace SDG.Unturned;

public class NPCFlagMathReward : INPCReward
{
    private short defaultFlag_B_Value;

    public ushort flag_A_ID { get; protected set; }

    public ushort flag_B_ID { get; protected set; }

    public ENPCOperationType operationType { get; protected set; }

    public override void GrantReward(Player player)
    {
        player.quests.getFlag(flag_A_ID, out var value);
        if (flag_B_ID == 0 || !player.quests.getFlag(flag_B_ID, out var value2))
        {
            value2 = defaultFlag_B_Value;
        }
        switch (operationType)
        {
        default:
            return;
        case ENPCOperationType.ASSIGN:
            value = value2;
            break;
        case ENPCOperationType.ADDITION:
            value += value2;
            break;
        case ENPCOperationType.SUBTRACTION:
            value -= value2;
            break;
        case ENPCOperationType.MULTIPLICATION:
            value *= value2;
            break;
        case ENPCOperationType.DIVISION:
            value /= value2;
            break;
        case ENPCOperationType.MODULO:
            value %= value2;
            break;
        case ENPCOperationType.RANDOM_INCLUSIVE:
            if (value != value2)
            {
                int num = value;
                int num2 = value2;
                if (num > num2)
                {
                    int num3 = num2;
                    num2 = num;
                    num = num3;
                }
                value = MathfEx.ClampToShort(UnityEngine.Random.Range(num, num2 + 1));
            }
            break;
        case ENPCOperationType.RANDOM_EXCLUSIVE:
            if (value != value2)
            {
                value = MathfEx.ClampToShort(UnityEngine.Random.Range(value, value2));
            }
            break;
        }
        player.quests.sendSetFlag(flag_A_ID, value);
    }

    internal override void PopulateV2(in PopulateRewardParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseUInt16("A_ID", out var value))
        {
            flag_A_ID = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("A_ID");
        }
        ushort value2;
        bool num = p.data.TryParseUInt16("B_ID", out value2);
        flag_B_ID = value2;
        short value3;
        bool flag = p.data.TryParseInt16("B_Value", out value3);
        defaultFlag_B_Value = value3;
        if (!num && !flag)
        {
            p.ReportError("requires B_ID or B_Value");
        }
        if (p.data.TryParseEnum<ENPCOperationType>("Operation", out var value4))
        {
            operationType = value4;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Operation");
        }
    }

    internal override void PopulateLegacy(in PopulateRewardParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseUInt16(p.legacyPrefix + "_A_ID", out var value))
        {
            flag_A_ID = value;
        }
        else
        {
            p.ReportRequiredOptionInvalid("A_ID");
        }
        ushort value2;
        bool num = p.data.TryParseUInt16(p.legacyPrefix + "_B_ID", out value2);
        flag_B_ID = value2;
        short value3;
        bool flag = p.data.TryParseInt16(p.legacyPrefix + "_B_Value", out value3);
        defaultFlag_B_Value = value3;
        if (!num && !flag)
        {
            p.ReportError("requires B_ID or B_Value");
        }
        if (p.data.TryParseEnum<ENPCOperationType>(p.legacyPrefix + "_Operation", out var value4))
        {
            operationType = value4;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Operation");
        }
    }

    public NPCFlagMathReward()
    {
    }

    [Obsolete]
    public NPCFlagMathReward(ushort newFlag_A_ID, ushort newFlag_B_ID, short newFlag_B_Value, ENPCOperationType newOperationType, string newText)
        : base(newText)
    {
        flag_A_ID = newFlag_A_ID;
        flag_B_ID = newFlag_B_ID;
        defaultFlag_B_Value = newFlag_B_Value;
        operationType = newOperationType;
    }
}
