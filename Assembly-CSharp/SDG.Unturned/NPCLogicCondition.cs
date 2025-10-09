using System;
using System.Text;

namespace SDG.Unturned;

public class NPCLogicCondition : INPCCondition
{
    public ENPCLogicType logicType { get; protected set; }

    protected bool doesLogicPass<T>(T a, T b) where T : IComparable
    {
        return NPCTool.doesLogicPass(logicType, a, b);
    }

    public override void DebugDumpToStringBuilder(Player player, StringBuilder sb)
    {
        base.DebugDumpToStringBuilder(player, sb);
        sb.Append(", Op: ");
        sb.Append(logicType.ToCharAbbr());
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        ENPCLogicType defaultLogicMode = GetDefaultLogicMode(p.conditionType);
        if (p.data.TryGetValue("Logic", out var node))
        {
            if (TryParseLogic(node, out var parsedValue))
            {
                logicType = parsedValue;
                return;
            }
            p.ReportRequiredOptionInvalid("Logic");
            logicType = defaultLogicMode;
        }
        else
        {
            logicType = defaultLogicMode;
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        ENPCLogicType defaultLogicMode = GetDefaultLogicMode(p.conditionType);
        if (p.data.TryGetValue(p.legacyPrefix + "_Logic", out var node))
        {
            if (TryParseLogic(node, out var parsedValue))
            {
                logicType = parsedValue;
                return;
            }
            p.ReportRequiredOptionInvalid("Logic");
            logicType = defaultLogicMode;
        }
        else
        {
            logicType = defaultLogicMode;
        }
    }

    private bool TryParseLogic(IDatValue node, out ENPCLogicType parsedValue)
    {
        if (node.TryParseEnum<ENPCLogicType>(out parsedValue))
        {
            return true;
        }
        if (!string.IsNullOrEmpty(node.Value))
        {
            if (node.Value == "<")
            {
                parsedValue = ENPCLogicType.LESS_THAN;
            }
            else if (node.Value == "<=" || node.Value == "≤")
            {
                parsedValue = ENPCLogicType.LESS_THAN_OR_EQUAL_TO;
            }
            else if (node.Value == "==" || node.Value == "=")
            {
                parsedValue = ENPCLogicType.EQUAL;
            }
            else if (node.Value == "!=" || node.Value == "≠")
            {
                parsedValue = ENPCLogicType.NOT_EQUAL;
            }
            else if (node.Value == ">=" || node.Value == "≥")
            {
                parsedValue = ENPCLogicType.GREATER_THAN_OR_EQUAL_TO;
            }
            else
            {
                if (!(node.Value == ">"))
                {
                    return false;
                }
                parsedValue = ENPCLogicType.GREATER_THAN;
            }
            return true;
        }
        return false;
    }

    private ENPCLogicType GetDefaultLogicMode(ENPCConditionType conditionType)
    {
        return conditionType switch
        {
            ENPCConditionType.ITEM => ENPCLogicType.GREATER_THAN_OR_EQUAL_TO, 
            ENPCConditionType.HOLIDAY => ENPCLogicType.EQUAL, 
            _ => ENPCLogicType.NONE, 
        };
    }

    public NPCLogicCondition()
    {
    }

    [Obsolete]
    public NPCLogicCondition(ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newText, newShouldReset)
    {
        logicType = newLogicType;
    }
}
