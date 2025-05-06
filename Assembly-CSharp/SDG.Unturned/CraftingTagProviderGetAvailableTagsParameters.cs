using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public struct CraftingTagProviderGetAvailableTagsParameters
{
    /// <summary>
    /// All tags added by this crafting tag provider.
    /// </summary>
    public HashSet<TagAsset> ResultTags { get; set; }

    internal void ApplyModHooks(GameObject gameObject)
    {
        CraftingTagProviderComponent craftingTagProviderComponent = gameObject?.GetComponent<CraftingTagProviderComponent>();
        if (craftingTagProviderComponent == null || craftingTagProviderComponent.modifiers == null)
        {
            return;
        }
        CraftingTagModifierComponent[] modifiers = craftingTagProviderComponent.modifiers;
        foreach (CraftingTagModifierComponent craftingTagModifierComponent in modifiers)
        {
            if (craftingTagModifierComponent == null || craftingTagModifierComponent.activationRequirement switch
            {
                CraftingTagModifierComponent.EActivationRequirement.ActiveAndEnabled => craftingTagModifierComponent.isActiveAndEnabled ? 1 : 0, 
                CraftingTagModifierComponent.EActivationRequirement.Invert => (!craftingTagModifierComponent.isActiveAndEnabled) ? 1 : 0, 
                _ => 1, 
            } == 0)
            {
                continue;
            }
            CachingAssetRef[] tagRefs = craftingTagModifierComponent.GetTagRefs();
            if (tagRefs == null)
            {
                continue;
            }
            for (int j = 0; j < tagRefs.Length; j++)
            {
                TagAsset tagAsset = tagRefs[j].Get<TagAsset>();
                if (tagAsset != null)
                {
                    switch (craftingTagModifierComponent.mode)
                    {
                    case CraftingTagModifierComponent.EMode.Add:
                        ResultTags.Add(tagAsset);
                        break;
                    case CraftingTagModifierComponent.EMode.Remove:
                        ResultTags.Remove(tagAsset);
                        break;
                    }
                }
            }
        }
    }
}
