using UnityEngine;

namespace SDG.Unturned;

internal class LevelObjectRefComponent : MonoBehaviour, ICraftingTagProvider
{
    internal LevelObject levelObjectOwner;

    public Asset GetTagProviderAsset()
    {
        return levelObjectOwner?.asset;
    }

    public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p)
    {
        p.ApplyModHooks(base.gameObject);
    }

    public bool Equals(ICraftingTagProvider obj)
    {
        return this == obj;
    }
}
