using System;
using System.Collections;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerDelayedQuestRewardsComponent : MonoBehaviour
{
    public Player player;

    internal void GrantReward(INPCReward reward, bool shouldSend)
    {
        StartCoroutine(GrantRewardCoroutine(reward, shouldSend));
    }

    private IEnumerator GrantRewardCoroutine(INPCReward reward, bool shouldSend)
    {
        yield return new WaitForSeconds(reward.grantDelaySeconds);
        try
        {
            reward.grantReward(player, shouldSend);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Caught exception granting delayed NPC reward to {player?.channel?.owner?.playerID}:");
        }
    }
}
