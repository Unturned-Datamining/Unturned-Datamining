using System.Collections.Generic;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

/// <summary>
/// Represents whether a player can craft a provided blueprint. If yes, which items to use, if no, why not.
/// Previously, some of this data was (confusingly) stored in the blueprint definition.
/// For performance, caller should re-use a list of BlueprintStatus and *not* discard unused results.
/// </summary>
internal class BlueprintStatus
{
    public Blueprint blueprint;

    public List<BlueprintInputItemStatus> inputItems = new List<BlueprintInputItemStatus>();

    public BlueprintInputItemStatus targetStatus;

    public bool isMissingRequiredSkill;

    /// <summary>
    /// Total number of missing required nearby crafting tags.
    /// </summary>
    public int missingCraftingTagsCount;

    public bool isMissingAnyNpcConditions;

    public bool isMissingTargetItem;

    /// <summary>
    /// Total required input item count minus available input item count.
    /// </summary>
    public int totalMissingInputItemsCount;

    public bool isMissingAnyCriticalInputItem;

    public bool hasAnyInputItem;

    /// <summary>
    /// Used to sort blueprints from "most craftable" (1) to "least craftable" (0).
    /// </summary>
    public float normalizedCraftability;

    private static List<BlueprintInputItemStatus> inputItemsPool = new List<BlueprintInputItemStatus>();

    public bool IsCraftable
    {
        get
        {
            if (!isMissingRequiredSkill && missingCraftingTagsCount <= 0 && !isMissingAnyNpcConditions && !isMissingTargetItem && totalMissingInputItemsCount <= 0)
            {
                return !isMissingAnyCriticalInputItem;
            }
            return false;
        }
    }

    /// <summary>
    /// Currently only used by housing planner.
    /// Doesn't work with NPC conditions / rewards.
    /// </summary>
    public int EstimateMaxCraftingRepeatCount()
    {
        if (!IsCraftable)
        {
            return 0;
        }
        int num = -1;
        for (int i = 0; i < blueprint.supplies.Length; i++)
        {
            BlueprintSupply blueprintSupply = blueprint.supplies[i];
            BlueprintInputItemStatus blueprintInputItemStatus = inputItems[i];
            if (blueprintSupply.ShouldConsume)
            {
                int num2 = blueprintInputItemStatus.totalAmount / blueprintSupply.amount;
                num = ((num != -1) ? Mathf.Min(num, num2) : num2);
            }
        }
        return Mathf.Max(0, num);
    }

    /// <summary>
    /// Currently only used by housing planner.
    /// Doesn't work with NPC conditions / rewards.
    /// </summary>
    public int EstimateOutputMaxAmount(int outputIndex)
    {
        if (outputIndex < 0 || blueprint.outputs == null || outputIndex >= blueprint.outputs.Length)
        {
            return 0;
        }
        return blueprint.outputs[outputIndex].amount * EstimateMaxCraftingRepeatCount();
    }

    internal void UpdateCraftabilityScore()
    {
        normalizedCraftability = 1f;
        if (!blueprint.supplies.IsNullOrEmpty())
        {
            for (int i = 0; i < blueprint.supplies.Length; i++)
            {
                BlueprintSupply blueprintSupply = blueprint.supplies[i];
                BlueprintInputItemStatus blueprintInputItemStatus = inputItems[i];
                if (blueprintInputItemStatus.isMissingRequiredAmount)
                {
                    float t = Mathf.Clamp01((float)blueprintInputItemStatus.totalAmount / (float)blueprintSupply.amount);
                    normalizedCraftability *= Mathf.Lerp(0.1f, 1f, t);
                }
            }
        }
        if (blueprint.TargetItem != null && targetStatus.isMissingRequiredAmount)
        {
            float t2 = Mathf.Clamp01((float)targetStatus.totalAmount / (float)blueprint.TargetItem.amount);
            normalizedCraftability *= Mathf.Lerp(0.1f, 1f, t2);
        }
        if (isMissingRequiredSkill)
        {
            normalizedCraftability *= 0.1f;
        }
        if (missingCraftingTagsCount > 0)
        {
            CachingAssetRef[] applicableRequiredNearbyCraftingTags = blueprint.GetApplicableRequiredNearbyCraftingTags();
            float t3 = (float)missingCraftingTagsCount / (float)applicableRequiredNearbyCraftingTags.Length;
            normalizedCraftability *= Mathf.Lerp(1f, 0.1f, t3);
        }
        if (isMissingAnyNpcConditions)
        {
            normalizedCraftability *= 0.1f;
        }
    }

    public void Reset()
    {
        isMissingRequiredSkill = false;
        missingCraftingTagsCount = 0;
        ResetDynamicStatus();
    }

    /// <summary>
    /// Reset values set by PlayerCrafting.UpdateBlueprintDynamicStatus.
    /// </summary>
    public void ResetDynamicStatus()
    {
        isMissingAnyNpcConditions = false;
        isMissingTargetItem = false;
        totalMissingInputItemsCount = 0;
        isMissingAnyCriticalInputItem = false;
        hasAnyInputItem = false;
        inputItemsPool.AddRange(inputItems);
        inputItems.Clear();
        if (targetStatus != null)
        {
            inputItemsPool.Add(targetStatus);
            targetStatus = null;
        }
    }

    public BlueprintInputItemStatus AddInputItem()
    {
        BlueprintInputItemStatus blueprintInputItemStatus = CreateInputItemStatus();
        inputItems.Add(blueprintInputItemStatus);
        return blueprintInputItemStatus;
    }

    public BlueprintInputItemStatus AddTargetItem()
    {
        targetStatus = CreateInputItemStatus();
        return targetStatus;
    }

    internal void GetPreviewOutputTransferState(ItemAsset outputItemAsset, out byte quality, out byte[] state)
    {
        if (blueprint.withoutAttachments)
        {
            quality = 100;
            state = new byte[18];
            return;
        }
        Item item = null;
        if (inputItems.Count > 0 && inputItems[0].searchResults.Count > 0)
        {
            item = inputItems[0].FirstItemOrNull;
        }
        if (item != null)
        {
            quality = item.quality;
            state = new byte[item.state.Length];
            item.state.CopyTo(state, 0);
            return;
        }
        quality = 100;
        ItemAsset itemAsset = null;
        if (!blueprint.supplies.IsNullOrEmpty())
        {
            itemAsset = blueprint.supplies[0].FindItemAsset();
        }
        if (itemAsset != null)
        {
            state = itemAsset.getState();
        }
        else
        {
            state = outputItemAsset.getState();
        }
    }

    private BlueprintInputItemStatus CreateInputItemStatus()
    {
        BlueprintInputItemStatus blueprintInputItemStatus;
        if (inputItemsPool.Count > 0)
        {
            blueprintInputItemStatus = inputItemsPool.GetAndRemoveTail();
            blueprintInputItemStatus.searchResults.Clear();
        }
        else
        {
            blueprintInputItemStatus = new BlueprintInputItemStatus
            {
                searchResults = new List<PlayerInventorySearchResultV2>()
            };
        }
        blueprintInputItemStatus.totalAmount = 0;
        blueprintInputItemStatus.requiredAmountOverride = 0;
        blueprintInputItemStatus.isMissingRequiredAmount = false;
        return blueprintInputItemStatus;
    }
}
