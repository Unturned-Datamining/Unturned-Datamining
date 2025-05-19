namespace SDG.Unturned;

/// <summary>
/// Implemented by "root" component of each entity type that can provide crafting tags to nearby players.
/// This allows overlap with a barricade attached to a vehicle to find the barricade from barricade collider and
/// vehicle from vehicle collider rather than using transform root. Any mod hook extensions to crafting tags will
/// be sibling components or descendants of this component.
/// </summary>
public interface ICraftingTagProvider
{
    /// <summary>
    /// Asset providing tags. For example, a barricade item.
    /// </summary>
    Asset GetTagProviderAsset();

    void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p);

    /// <summary>
    /// True if GetAvailableTags can ever add any tags.
    /// Used to skip unnecessary line-of-sight tests against (for example) ordinary structures and the like.
    /// </summary>
    bool HasAnyCraftingTagsConfigured();
}
