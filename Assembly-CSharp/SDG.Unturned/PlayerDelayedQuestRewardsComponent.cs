using System;
using System.Collections;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerDelayedQuestRewardsComponent : MonoBehaviour
{
    public Player player;

    internal void GrantReward(INPCReward reward)
    {
        StartCoroutine(GrantRewardCoroutine(reward));
    }

    private IEnumerator GrantRewardCoroutine(INPCReward reward)
    {
        yield return new WaitForSeconds(reward.grantDelaySeconds);
        try
        {
            reward.GrantReward(player);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Caught exception granting delayed NPC reward to {player?.channel?.owner?.playerID}:");
        }
    }
}
