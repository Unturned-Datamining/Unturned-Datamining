using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerDelayedQuestRewardsComponent : MonoBehaviour
{
    public Player player;

    private List<INPCReward> rewardsToApplyWhenInterrupted = new List<INPCReward>();

    internal void GrantReward(INPCReward reward)
    {
        StartCoroutine(GrantRewardCoroutine(reward));
    }

    internal void Interrupt(EDelayedQuestRewardsInterruption interruption)
    {
        StopAllCoroutines();
        foreach (INPCReward item in rewardsToApplyWhenInterrupted)
        {
            GrantRewardSafe(item, interruption);
        }
        rewardsToApplyWhenInterrupted.Clear();
    }

    private IEnumerator GrantRewardCoroutine(INPCReward reward)
    {
        bool shouldApplyWhenInterrupted = reward.grantDelayApplyWhenInterrupted;
        if (shouldApplyWhenInterrupted)
        {
            rewardsToApplyWhenInterrupted.Add(reward);
        }
        yield return new WaitForSeconds(reward.grantDelaySeconds);
        GrantRewardSafe(reward, EDelayedQuestRewardsInterruption.NotInterrupted);
        if (shouldApplyWhenInterrupted)
        {
            rewardsToApplyWhenInterrupted.Remove(reward);
        }
    }

    private void GrantRewardSafe(INPCReward reward, EDelayedQuestRewardsInterruption interruption)
    {
        try
        {
            reward.GrantReward(player);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, $"Caught exception granting delayed NPC reward to {player?.channel?.owner?.playerID} ({interruption}):");
        }
    }
}
