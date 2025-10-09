using System;
using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public struct NPCConditionsList
{
    internal INPCCondition[] conditions;

    private static List<INPCCondition> tempConditions = new List<INPCCondition>();

    public bool IsEmpty
    {
        get
        {
            if (conditions != null)
            {
                return conditions.Length < 1;
            }
            return true;
        }
    }

    /// <summary>
    /// Exposed for plugins. Can be null. Please do not modify.
    /// </summary>
    public INPCCondition[] GetConditions()
    {
        return conditions;
    }

    public INPCCondition GetFirstUnmetCondition(Player player)
    {
        if (conditions != null)
        {
            INPCCondition[] array = conditions;
            foreach (INPCCondition iNPCCondition in array)
            {
                if (!iNPCCondition.isConditionMet(player))
                {
                    return iNPCCondition;
                }
            }
        }
        return null;
    }

    public bool AreConditionsMet(Player player)
    {
        if (conditions != null)
        {
            INPCCondition[] array = conditions;
            for (int i = 0; i < array.Length; i++)
            {
                if (!array[i].isConditionMet(player))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void ApplyConditions(Player player)
    {
        if (conditions != null)
        {
            INPCCondition[] array = conditions;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].ApplyCondition(player);
            }
        }
    }

    /// <summary>
    /// This overload supports legacy Condition_# format.
    /// </summary>
    public void Parse(IDatDictionary data, Local localization, Asset assetContext, string countKey, string legacyPrefixKey)
    {
        if (!data.TryGetNode(countKey, out var node))
        {
            return;
        }
        if (node is IDatValue valueNode)
        {
            int num = valueNode.ParseInt32();
            if (num > 0)
            {
                conditions = new INPCCondition[num];
                NPCTool.readConditions(data, localization, legacyPrefixKey, conditions, assetContext);
            }
        }
        else if (node is IDatList listNode)
        {
            Parse(localization, assetContext, listNode, countKey);
        }
    }

    /// <summary>
    /// This overload doesn't support legacy Condition_# format.
    /// </summary>
    public void Parse(IDatDictionary data, Local localization, Asset assetContext, string key)
    {
        if (data.TryGetList(key, out var node))
        {
            Parse(localization, assetContext, node, key);
        }
    }

    private void Parse(Local localization, Asset assetContext, IDatList listNode, string countKey)
    {
        tempConditions.Clear();
        int num = -1;
        foreach (IDatNode item in listNode)
        {
            num++;
            if (!(item is IDatDictionary datDictionary))
            {
                continue;
            }
            string text = $"{countKey}[{num}]";
            if (!datDictionary.TryParseEnum<ENPCConditionType>("Type", out var value))
            {
                if (datDictionary.ContainsKey("Type"))
                {
                    assetContext.ReportAssetError(text + " missing condition Type");
                }
                else
                {
                    assetContext.ReportAssetError(text + " unable to parse condition type \"" + datDictionary.GetString("Type") + "\"");
                }
                continue;
            }
            Type type = NPCTool.conditionTypes[(int)value];
            if (type == null)
            {
                assetContext.ReportAssetError($"{text} unable to create type {value}");
                continue;
            }
            INPCCondition iNPCCondition;
            try
            {
                iNPCCondition = Activator.CreateInstance(type) as INPCCondition;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, $"Caught exception instantiating {type}:");
                assetContext.ReportAssetError($"{text} error creating type {value}");
                continue;
            }
            PopulateConditionParameters p = new PopulateConditionParameters(value, datDictionary, localization, assetContext, text, null, num, listNode.Count);
            try
            {
                iNPCCondition.PopulateV2(in p);
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2, $"Caught exception populating {text} {type}:");
                continue;
            }
            tempConditions.Add(iNPCCondition);
        }
        if (tempConditions.Count > 0)
        {
            conditions = tempConditions.ToArray();
        }
    }

    public void DebugDumpToStringBuilder(StringBuilder output)
    {
        output.AppendLine($"{conditions?.Length} conditions(s)");
        if (conditions != null)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                output.AppendLine($"[{i}]: {conditions[i]}");
            }
        }
    }

    public void DebugDumpToStringBuilder(Player player, StringBuilder output)
    {
        if (conditions != null)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                output.Append("[");
                output.Append(i);
                output.Append("] ");
                conditions[i].DebugDumpToStringBuilder(player, output);
                output.AppendLine();
            }
        }
    }

    public string DebugDumpToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        DebugDumpToStringBuilder(stringBuilder);
        return stringBuilder.ToString();
    }

    public string DebugDumpToString(Player player)
    {
        StringBuilder stringBuilder = new StringBuilder();
        DebugDumpToStringBuilder(player, stringBuilder);
        return stringBuilder.ToString();
    }
}
