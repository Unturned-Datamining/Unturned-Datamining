using UnityEngine;

namespace SDG.Unturned;

internal class LevelObjectRefComponent : MonoBehaviour, ICraftingTagProvider
{
    internal LevelObject levelObjectOwner;

    private CraftingTagProviderComponent modHook;

    public Asset GetTagProviderAsset()
    {
        return levelObjectOwner?.asset;
    }

    public void GetAvailableTags(ref CraftingTagProviderGetAvailableTagsParameters p)
    {
        if (modHook != null)
        {
            p.ApplyModHooks(modHook);
        }
    }

    public bool HasAnyCraftingTagsConfigured()
    {
        return modHook != null;
    }

    public bool Equals(ICraftingTagProvider obj)
    {
        return this == obj;
    }

    private void Start()
    {
        modHook = GetComponent<CraftingTagProviderComponent>();
    }

    private void OnDestroy()
    {
        if (levelObjectOwner != null)
        {
            levelObjectOwner.OnDestroy();
        }
    }
}
