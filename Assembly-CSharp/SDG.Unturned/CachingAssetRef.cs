using System;

namespace SDG.Unturned;

/// <summary>
/// • Does not support legacy ID.
/// • Caches resolved asset and updates if asset has been reloaded.
/// • See CachingBcAssetRef if legacy ID support is necessary.
/// </summary>
public struct CachingAssetRef : IEquatable<CachingAssetRef>, IDatParseable
{
    public static readonly CachingAssetRef Empty;

    private Guid guid;

    /// <summary>
    /// Internal so that CachingBcAssetRef can copy cachedAsset.
    /// </summary>
    internal Asset cachedAsset;

    /// <summary>
    /// If true, doesn't reference anything.
    /// Could also be called "IsZero" or "IsNull".
    /// </summary>
    public bool IsEmpty => guid == Guid.Empty;

    /// <summary>
    /// Opposite of IsEmpty.
    /// </summary>
    public bool IsAssigned => guid != Guid.Empty;

    /// <summary>
    /// Assigned GUID, not the referenced asset's GUID.
    /// </summary>
    public Guid Guid => guid;

    public Asset Get()
    {
        if (cachedAsset == null || cachedAsset.hasBeenReplaced)
        {
            cachedAsset = Assets.find(guid);
        }
        return cachedAsset;
    }

    public T Get<T>() where T : class
    {
        return Get() as T;
    }

    /// <summary>
    /// Doesn't only check (Get() == asset) because a new asset may have loaded.
    /// Rather, checks whether GUID points at asset.
    /// If asset is null, returns true if GUID and legacy ID are zero.
    /// </summary>
    public bool IsReferenceTo(Asset asset)
    {
        if (asset != null)
        {
            if (!asset.hasBeenReplaced && cachedAsset != null)
            {
                return cachedAsset == asset;
            }
            return guid == asset.GUID;
        }
        return guid == Guid.Empty;
    }

    public void Clear()
    {
        guid = Guid.Empty;
        cachedAsset = null;
    }

    public static bool operator ==(CachingAssetRef lhs, CachingAssetRef rhs)
    {
        return lhs.guid == rhs.guid;
    }

    public static bool operator !=(CachingAssetRef lhs, CachingAssetRef rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        if (obj is CachingAssetRef cachingAssetRef)
        {
            return this == cachingAssetRef;
        }
        return false;
    }

    public bool Equals(CachingAssetRef other)
    {
        return this == other;
    }

    public override int GetHashCode()
    {
        return guid.GetHashCode();
    }

    public override string ToString()
    {
        string arg = Get()?.FriendlyName ?? "null";
        return $"(ID: {guid:N}, Asset: {arg})";
    }

    public bool TryParse(IDatNode node)
    {
        if (node is IDatValue datValue)
        {
            return TryParse(datValue.Value, out this);
        }
        if (node is IDatDictionary dictionary && dictionary.TryParseGuid("GUID", out guid))
        {
            return true;
        }
        return false;
    }

    public static bool TryParse(IDatNode node, out CachingAssetRef result)
    {
        if (node is IDatValue datValue)
        {
            return TryParse(datValue.Value, out result);
        }
        if (node is IDatDictionary dictionary && dictionary.TryParseGuid("GUID", out var value))
        {
            result = new CachingAssetRef(value);
            return true;
        }
        result = Empty;
        return false;
    }

    public static bool TryParse(string input, out CachingAssetRef result)
    {
        if (!string.IsNullOrEmpty(input) && !string.Equals(input, "0") && Guid.TryParse(input, out var result2))
        {
            result = new CachingAssetRef(result2);
            return true;
        }
        result = Empty;
        return false;
    }

    /// <summary>
    /// Returns Empty if TryParse returns false.
    /// </summary>
    public static CachingAssetRef Parse(string input)
    {
        TryParse(input, out var result);
        return result;
    }

    public CachingAssetRef(Asset asset)
    {
        guid = asset?.GUID ?? Guid.Empty;
        cachedAsset = asset;
    }

    public CachingAssetRef(Guid guid)
    {
        this.guid = guid;
        cachedAsset = null;
    }

    /// <summary>
    /// Enables assigning assetRef from an existing asset without manually calling constructor.
    /// </summary>
    public static implicit operator CachingAssetRef(Asset asset)
    {
        return new CachingAssetRef(asset);
    }

    /// <summary>
    /// Enables assigning assetRef from an asset GUID without manually calling constructor.
    /// </summary>
    public static implicit operator CachingAssetRef(Guid guid)
    {
        return new CachingAssetRef(guid);
    }
}
