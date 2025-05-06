using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to modify which crafting tags are provided by owning object.
/// Must be connected to a Crafting Tag Provider.
/// </summary>
[AddComponentMenu("Unturned/Crafting Tag Modifier")]
public class CraftingTagModifierComponent : MonoBehaviour
{
    public enum EMode
    {
        /// <summary>
        /// Add listed tags to provided tags.
        /// </summary>
        Add,
        /// <summary>
        /// Remove listed tags from provided tags.
        /// </summary>
        Remove
    }

    public enum EActivationRequirement
    {
        /// <summary>
        /// Apply only if this component is active and enabled.
        /// </summary>
        ActiveAndEnabled,
        /// <summary>
        /// Apply only if this component is inactive and/or disabled.
        /// </summary>
        Invert,
        /// <summary>
        /// Always apply.
        /// </summary>
        Bypass
    }

    /// <summary>
    /// GUIDs of Unturned tag assets to modify.
    /// </summary>
    public string[] tagGuids;

    public EMode mode;

    public EActivationRequirement activationRequirement;

    private CachingAssetRef[] tagRefs;

    private bool hasParsedTags;

    private static List<CachingAssetRef> pendingTagRefs = new List<CachingAssetRef>();

    /// <summary>
    /// Get wrapper method rather than Awake because component might be inactive but should still apply modifiers.
    /// </summary>
    internal CachingAssetRef[] GetTagRefs()
    {
        if (!hasParsedTags)
        {
            hasParsedTags = true;
            ParseTags();
        }
        return tagRefs;
    }

    private void ParseTags()
    {
        if (tagGuids == null || tagGuids.Length < 1)
        {
            return;
        }
        pendingTagRefs.Clear();
        string[] array = tagGuids;
        foreach (string text in array)
        {
            if (CachingAssetRef.TryParse(text, out var result) && result.IsAssigned)
            {
                pendingTagRefs.Add(result);
            }
            else
            {
                UnturnedLog.warn(base.transform.GetSceneHierarchyPath() + " unable to parse tag \"" + text + "\"");
            }
        }
        if (pendingTagRefs.Count > 0)
        {
            tagRefs = pendingTagRefs.ToArray();
        }
    }
}
