using System;
using System.Text;

namespace SDG.Unturned;

public struct NPCRewardsList
{
    internal INPCReward[] rewards;

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

    public void Parse(DatDictionary data, Local localization, Asset assetContext, string countKey, string prefixKey)
    {
        int num = data.ParseInt32(countKey);
        if (num > 0)
        {
            rewards = new INPCReward[num];
            NPCTool.readRewards(data, localization, prefixKey, rewards, assetContext);
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
