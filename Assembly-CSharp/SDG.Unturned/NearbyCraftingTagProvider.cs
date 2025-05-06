using System;
using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// This prevents identical tag provider setups from listing in the UI.
/// For example, two workbenches providing the same tags shouldn't show two UI listings.
/// </summary>
internal struct NearbyCraftingTagProvider : IEquatable<NearbyCraftingTagProvider>
{
    public ICraftingTagProvider component;

    public Asset asset;

    public HashSet<TagAsset> tags;

    public override string ToString()
    {
        return string.Format("(Component: {0} Asset: {1} Tags: {2}", component, asset, string.Join(", ", tags));
    }

    public bool Equals(NearbyCraftingTagProvider other)
    {
        if (asset.Equals(other.asset))
        {
            return tags.SetEquals(other.tags);
        }
        return false;
    }
}
