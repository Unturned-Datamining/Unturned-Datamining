using System;

namespace SDG.Unturned;

public class BlueprintOutput
{
    private CachingBcAssetRef _itemRef;

    public int amount;

    public EItemOrigin origin;

    /// <summary>
    /// Note: if calling ItemRef.Get() please use FindItemAsset instead to avoid redundant asset lookups.
    /// </summary>
    public CachingBcAssetRef ItemRef
    {
        get
        {
            return _itemRef;
        }
        internal set
        {
            _itemRef = value;
        }
    }

    [Obsolete("Please use FindItemAsset instead for GUID support")]
    public ushort id => _itemRef.LegacyId;

    public ItemAsset FindItemAsset()
    {
        return _itemRef.Get<ItemAsset>();
    }

    /// <summary>
    /// Does this blueprint output create the specified item?
    /// </summary>
    public bool IsItem(ItemAsset asset)
    {
        if (asset == null)
        {
            return false;
        }
        return _itemRef.IsReferenceTo(asset);
    }

    public BlueprintOutput(ushort newID, int newAmount, EItemOrigin newOrigin)
    {
        amount = newAmount;
        origin = newOrigin;
    }
}
