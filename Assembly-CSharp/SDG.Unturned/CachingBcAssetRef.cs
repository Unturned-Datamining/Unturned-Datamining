using System;

namespace SDG.Unturned;

/// <summary>
/// Backwards-Compatible Asset Reference with Caching
/// • Supports both GUID and legacy ID.
/// • Caches resolved asset and updates if asset has been reloaded.
/// • Parsing legacy ID without context requires "LegacyType:LegacyID" format. E.g., "Item:4" for the Eaglefire.
/// • See CachingAssetRef if legacy ID support is unnecessary.
/// </summary>
public struct CachingBcAssetRef : IEquatable<CachingBcAssetRef>, IDatParseable
{
    public static readonly CachingBcAssetRef Empty;

    private Guid guid;

    private ushort legacyId;

    private EAssetType legacyType;

    private Asset cachedAsset;

    /// <summary>
    /// If true, doesn't reference anything.
    /// Could also be called "IsZero" or "IsNull".
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            if (guid == Guid.Empty)
            {
                return legacyId == 0;
            }
            return false;
        }
    }

    /// <summary>
    /// Opposite of IsEmpty.
    /// </summary>
    public bool IsAssigned
    {
        get
        {
            if (!(guid != Guid.Empty))
            {
                return legacyId != 0;
            }
            return true;
        }
    }

    /// <summary>
    /// Assigned GUID, not the referenced asset's GUID.
    /// </summary>
    public Guid Guid => guid;

    /// <summary>
    /// Assigned legacy ID, not the referenced asset's legacy ID.
    /// </summary>
    public ushort LegacyId => legacyId;

    /// <summary>
    /// Assigned legacy type, not the referenced asset's legacy type.
    /// </summary>
    public EAssetType LegacyType => legacyType;

    public Asset Get()
    {
        if (cachedAsset == null || cachedAsset.hasBeenReplaced)
        {
            if (legacyId == 0)
            {
                cachedAsset = Assets.find(guid);
            }
            else
            {
                cachedAsset = Assets.find(legacyType, legacyId);
            }
        }
        return cachedAsset;
    }

    public T Get<T>() where T : class
    {
        return Get() as T;
    }

    /// <summary>
    /// Doesn't only check (Get() == asset) because a new asset may have loaded.
    /// Rather, checks whether GUID or legacy ID (whichever is set) points at asset.
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
            if (guid != Guid.Empty)
            {
                return guid == asset.GUID;
            }
            if (legacyId != 0)
            {
                if (legacyType == asset.assetCategory)
                {
                    return legacyId == asset.id;
                }
                return false;
            }
            return false;
        }
        if (guid == Guid.Empty)
        {
            return legacyId == 0;
        }
        return false;
    }

    public void Clear()
    {
        guid = Guid.Empty;
        legacyId = 0;
        legacyType = EAssetType.NONE;
        cachedAsset = null;
    }

    public static bool operator ==(CachingBcAssetRef lhs, CachingBcAssetRef rhs)
    {
        if (lhs.guid == rhs.guid && lhs.legacyId == rhs.legacyId)
        {
            return lhs.legacyType == rhs.legacyType;
        }
        return false;
    }

    public static bool operator !=(CachingBcAssetRef lhs, CachingBcAssetRef rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        if (obj is CachingBcAssetRef cachingBcAssetRef)
        {
            return this == cachingBcAssetRef;
        }
        return false;
    }

    public bool Equals(CachingBcAssetRef other)
    {
        return this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(guid, legacyId, legacyType);
    }

    public override string ToString()
    {
        string text = Get()?.FriendlyName ?? "null";
        if (legacyId == 0)
        {
            return $"(GUID: {guid:N}, Asset: {text})";
        }
        return $"(Legacy Type: {legacyType}, Legacy ID: {legacyId}, Asset: {text})";
    }

    public bool TryParse(IDatNode node)
    {
        if (node is IDatValue datValue)
        {
            return TryParse(datValue.Value, out this);
        }
        if (node is IDatDictionary dictionary)
        {
            if (dictionary.TryParseGuid("GUID", out guid))
            {
                return true;
            }
            if (dictionary.TryParseEnum<EAssetType>("Type", out legacyType) && dictionary.TryParseUInt16("ID", out legacyId))
            {
                return true;
            }
        }
        return false;
    }

    public static bool TryParse(IDatNode node, EAssetType defaultLegacyType, out CachingBcAssetRef result)
    {
        if (node is IDatValue datValue)
        {
            return TryParse(datValue.Value, defaultLegacyType, out result);
        }
        if (node is IDatDictionary dictionary)
        {
            if (dictionary.TryParseGuid("GUID", out var value))
            {
                result = new CachingBcAssetRef(value);
                return true;
            }
            if (dictionary.TryParseEnum<EAssetType>("Type", out var value2) && dictionary.TryParseUInt16("ID", out var value3))
            {
                result = new CachingBcAssetRef(value2, value3);
                return true;
            }
        }
        result = Empty;
        return false;
    }

    public static bool TryParse(IDatNode node, out CachingBcAssetRef result)
    {
        if (node is IDatValue datValue)
        {
            return TryParse(datValue.Value, out result);
        }
        if (node is IDatDictionary dictionary)
        {
            if (dictionary.TryParseGuid("GUID", out var value))
            {
                result = new CachingBcAssetRef(value);
                return true;
            }
            if (dictionary.TryParseEnum<EAssetType>("Type", out var value2) && dictionary.TryParseUInt16("ID", out var value3))
            {
                result = new CachingBcAssetRef(value2, value3);
                return true;
            }
        }
        result = Empty;
        return false;
    }

    /// <summary>
    /// Supports both GUID and legacy ID formats.
    /// - If input string contains ':' the first part is EAssetType and the second part is legacy ID.
    /// - If defaultLegacyType is not None the input string can be parsed as a legacy ID.
    /// - Otherwise, parsed as GUID.
    /// </summary>
    public static bool TryParse(string input, EAssetType defaultLegacyType, out CachingBcAssetRef result)
    {
        if (!string.IsNullOrEmpty(input) && !string.Equals(input, "0"))
        {
            int num = input.IndexOf(':');
            if (num > 0)
            {
                if (!Enum.TryParse<EAssetType>(input.Substring(0, num), ignoreCase: true, out var result2))
                {
                    result = Empty;
                    return false;
                }
                if (result2 == EAssetType.NONE)
                {
                    result = Empty;
                    return true;
                }
                if (ushort.TryParse(input.Substring(num + 1), out var result3))
                {
                    result = new CachingBcAssetRef(result2, result3);
                    return true;
                }
            }
            else
            {
                if (defaultLegacyType != 0 && ushort.TryParse(input, out var result4))
                {
                    result = new CachingBcAssetRef(defaultLegacyType, result4);
                    return true;
                }
                if (Guid.TryParse(input, out var result5))
                {
                    result = new CachingBcAssetRef(result5);
                    return true;
                }
            }
        }
        result = Empty;
        return false;
    }

    /// <summary>
    /// Supports both GUID and legacy ID formats.
    /// - If input string contains ':' the first part is EAssetType and the second part is legacy ID.
    /// - Otherwise, parsed as GUID.
    /// </summary>
    public static bool TryParse(string input, out CachingBcAssetRef result)
    {
        return TryParse(input, EAssetType.NONE, out result);
    }

    /// <summary>
    /// Returns Empty if TryParse returns false.
    /// </summary>
    public static CachingBcAssetRef Parse(string input, EAssetType defaultLegacyAssetType)
    {
        TryParse(input, defaultLegacyAssetType, out var result);
        return result;
    }

    /// <summary>
    /// Returns Empty if TryParse returns false.
    /// </summary>
    public static CachingBcAssetRef Parse(string input)
    {
        TryParse(input, out var result);
        return result;
    }

    public CachingBcAssetRef(Asset asset)
    {
        guid = asset?.GUID ?? Guid.Empty;
        legacyType = EAssetType.NONE;
        legacyId = 0;
        cachedAsset = asset;
    }

    public CachingBcAssetRef(Guid guid)
    {
        this.guid = guid;
        legacyId = 0;
        legacyType = EAssetType.NONE;
        cachedAsset = null;
    }

    public CachingBcAssetRef(EAssetType legacyType, ushort legacyId)
    {
        guid = Guid.Empty;
        this.legacyType = ((legacyId > 0) ? legacyType : EAssetType.NONE);
        this.legacyId = legacyId;
        cachedAsset = null;
    }

    public CachingBcAssetRef(Guid guid, EAssetType legacyType, ushort legacyId)
    {
        this.guid = ((legacyId > 0) ? Guid.Empty : guid);
        this.legacyType = ((legacyId > 0) ? legacyType : EAssetType.NONE);
        this.legacyId = legacyId;
        cachedAsset = null;
    }

    public CachingBcAssetRef(CachingAssetRef assetRef)
    {
        guid = assetRef.Guid;
        legacyType = EAssetType.NONE;
        legacyId = 0;
        cachedAsset = assetRef.cachedAsset;
    }

    /// <summary>
    /// Enables assigning assetRef from an existing asset without manually calling constructor.
    /// </summary>
    public static implicit operator CachingBcAssetRef(Asset asset)
    {
        return new CachingBcAssetRef(asset);
    }

    /// <summary>
    /// Enables assigning assetRef from an asset GUID without manually calling constructor.
    /// </summary>
    public static implicit operator CachingBcAssetRef(Guid guid)
    {
        return new CachingBcAssetRef(guid);
    }

    /// <summary>
    /// Enables assigning assetRef from a non-backwards-compatible asset ref without manually calling constructor.
    /// </summary>
    public static implicit operator CachingBcAssetRef(CachingAssetRef assetRef)
    {
        return new CachingBcAssetRef(assetRef);
    }
}
