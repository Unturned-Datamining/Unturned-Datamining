using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Consolidates parameters for older, separate inventory search methods.
///
/// The "player" part of the name refers to the PlayerInventory-specific parameters. It can still be used to search
/// the Items class, in which case those parameters do not apply.
/// </summary>
public struct PlayerInventorySearchParameters
{
    private CachingBcAssetRef? _assetRef;

    /// <summary>
    /// List to populate with matching items.
    /// </summary>
    public List<PlayerInventorySearchResultV2> Results { get; set; }

    /// <summary>
    /// If true, search player's primary and secondary weapon slots.
    /// Only applicable when used with PlayerInventory class. (I.e., not Items class.)
    /// </summary>
    public bool IncludeEquipmentSlots { get; set; }

    /// <summary>
    /// If true, search storage container player is currently interacting with (if any).
    /// Only applicable when used with PlayerInventory class. (I.e., not Items class.)
    /// </summary>
    public bool IncludeActiveStorageContainer { get; set; }

    /// <summary>
    /// If greater than zero, search exits early once Results count meets MaxResultCount.
    /// </summary>
    public int MaxResultsCount { get; set; }

    /// <summary>
    /// If set, item must be this type to match.
    /// </summary>
    public EItemType? ItemType { get; set; }

    /// <summary>
    /// If set, AssetRef must be a reference to item's asset to match.
    /// Replaces older "id" parameter which matched if item's legacy asset ID was the same.
    /// </summary>
    public CachingBcAssetRef? AssetRef
    {
        get
        {
            return _assetRef;
        }
        set
        {
            if (value.HasValue)
            {
                CachingBcAssetRef value2 = value.Value;
                value2.Get();
                _assetRef = value2;
            }
            else
            {
                _assetRef = value;
            }
        }
    }

    /// <summary>
    /// If true, items with amount of zero can match. Otherwise, they are ignored.
    /// Replaces older "findEmpty" parameter which matched if (findEmpty || amount &gt; 0).
    /// </summary>
    public bool IncludeEmpty { get; set; }

    /// <summary>
    /// If true, items with an "amount" &gt;= their MaxAmount are ignored. Otherwise, they can match (default).
    /// </summary>
    public bool ExcludeFullAmount { get; set; }

    public bool IncludeMaxQuality { get; set; }

    /// <summary>
    /// If set, item must be of type ItemCaliberAsset. Asset's caliber list must either:
    /// • Contain this caliber ID.
    /// • Or, if empty, IncludeUnspecifiedCaliber must be true.
    /// Otherwise, item is ignored.
    /// </summary>
    public ushort? CaliberId { get; set; }

    /// <summary>
    /// If set, item must be of type ItemCaliberAsset. Asset's caliber list must either:
    /// • Contain one of these caliber IDs.
    /// • Or, if empty, IncludeUnspecifiedCaliber must be true.
    /// Otherwise, item is ignored.
    /// </summary>
    public ushort[] AnyCaliberIds { get; set; }

    /// <summary>
    /// Only applicable if CaliberId or AnyCaliberIds is set.
    /// If true, assets with an empty calibers list can match. Otherwise, they are ignore.d
    /// </summary>
    public bool IncludeUnspecifiedCaliber { get; set; }

    /// <summary>
    /// If set, do not include this specific item instance in search results.
    /// Kind of hacked-in for ignoring "target item" as a potential input item.
    /// </summary>
    public ItemJar ItemToIgnore { get; set; }
}
