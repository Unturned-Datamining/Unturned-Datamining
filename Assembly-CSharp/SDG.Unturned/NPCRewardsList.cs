using System;
using System.Collections.Generic;
using System.Text;

namespace SDG.Unturned;

public struct NPCRewardsList
{
    internal INPCReward[] rewards;

    private static List<INPCReward> tempRewards = new List<INPCReward>();

    public void Grant(Player player)
    {
        if (rewards == null || rewards.Length == 0)
        {
            return;
        }
        try
        {
            INPCReward[] array = rewards;
            foreach (INPCReward iNPCReward in array)
            {
                if (iNPCReward.grantDelaySeconds > 0f)
                {
                    player.quests.GetOrCreateDelayedQuestRewards().GrantReward(iNPCReward);
                }
                else
                {
                    iNPCReward.GrantReward(player);
                }
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Caught exception granting NPC reward to {player?.channel?.owner?.playerID}:");
        }
    }

    /// <summary>
    /// This overload supports legacy Reward_# format.
    /// </summary>
    public void Parse(IDatDictionary data, Local localization, Asset assetContext, string countKey, string prefixKey)
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
                rewards = new INPCReward[num];
                NPCTool.readRewards(data, localization, prefixKey, rewards, assetContext);
            }
        }
        else if (node is IDatList listNode)
        {
            Parse(localization, assetContext, listNode, countKey);
        }
    }

    /// <summary>
    /// This overload doesn't support legacy Reward_# format.
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
        tempRewards.Clear();
        int num = -1;
        foreach (IDatNode item in listNode)
        {
            num++;
            if (!(item is IDatDictionary datDictionary))
            {
                continue;
            }
            string text = $"{countKey}[{num}]";
            if (!datDictionary.TryParseEnum<ENPCRewardType>("Type", out var value))
            {
                if (datDictionary.ContainsKey("Type"))
                {
                    assetContext.ReportAssetError(text + " missing reward Type");
                }
                else
                {
                    assetContext.ReportAssetError(text + " unable to parse reward type \"" + datDictionary.GetString("Type") + "\"");
                }
                continue;
            }
            Type type = NPCTool.rewardTypes[(int)value];
            if (type == null)
            {
                assetContext.ReportAssetError($"{text} unable to create type {value}");
                continue;
            }
            INPCReward iNPCReward;
            try
            {
                iNPCReward = Activator.CreateInstance(type) as INPCReward;
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, $"Caught exception instantiating {type}:");
                assetContext.ReportAssetError($"{text} error creating type {value}");
                continue;
            }
            PopulateRewardParameters p = new PopulateRewardParameters(value, datDictionary, localization, assetContext, text, null);
            try
            {
                iNPCReward.PopulateV2(in p);
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2, $"Caught exception populating {text} {type}:");
                continue;
            }
            tempRewards.Add(iNPCReward);
        }
        if (tempRewards.Count > 0)
        {
            rewards = tempRewards.ToArray();
        }
    }

    public void DebugDumpToStringBuilder(StringBuilder output)
    {
        output.AppendLine($"{rewards?.Length} reward(s)");
        if (rewards != null)
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                output.AppendLine($"[{i}]: {rewards[i]}");
            }
        }
    }

    public string DebugDumpToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        DebugDumpToStringBuilder(stringBuilder);
        return stringBuilder.ToString();
    }

    [Obsolete("Removed shouldSend parameter")]
    public void Grant(Player player, bool shouldSend = true)
    {
        Grant(player);
    }
}
