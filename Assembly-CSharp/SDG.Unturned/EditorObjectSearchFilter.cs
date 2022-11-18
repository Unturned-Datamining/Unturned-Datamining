namespace SDG.Unturned;

public class EditorObjectSearchFilter
{
    private TokenSearchFilter? tokenFilter;

    private MasterBundleSearchFilter? mbFilter;

    private FavoriteSearchFilter<EditorObjectSearchFilter>? fvFilter;

    public static EditorObjectSearchFilter parse(string filter)
    {
        if (string.IsNullOrEmpty(filter))
        {
            return null;
        }
        return new EditorObjectSearchFilter(filter);
    }

    public bool matches(ObjectAsset objectAsset)
    {
        if (mbFilter.HasValue && mbFilter.Value.ignores(objectAsset))
        {
            return false;
        }
        if (tokenFilter.HasValue && tokenFilter.Value.ignores(objectAsset.objectName))
        {
            return false;
        }
        if (fvFilter.HasValue)
        {
            EditorObjectSearchFilter[] subFilters = fvFilter.Value.subFilters;
            for (int i = 0; i < subFilters.Length; i++)
            {
                if (subFilters[i].matches(objectAsset))
                {
                    return true;
                }
            }
            return false;
        }
        return true;
    }

    public bool ignores(ObjectAsset objectAsset)
    {
        return !matches(objectAsset);
    }

    public bool matches(ItemAsset itemAsset)
    {
        if (mbFilter.HasValue && mbFilter.Value.ignores(itemAsset))
        {
            return false;
        }
        if (tokenFilter.HasValue && tokenFilter.Value.ignores(itemAsset.itemName))
        {
            return false;
        }
        if (fvFilter.HasValue)
        {
            EditorObjectSearchFilter[] subFilters = fvFilter.Value.subFilters;
            for (int i = 0; i < subFilters.Length; i++)
            {
                if (subFilters[i].matches(itemAsset))
                {
                    return true;
                }
            }
            return false;
        }
        return true;
    }

    public bool ignores(ItemAsset itemAsset)
    {
        return !matches(itemAsset);
    }

    private EditorObjectSearchFilter(string filter)
    {
        tokenFilter = TokenSearchFilter.parse(filter);
        mbFilter = MasterBundleSearchFilter.parse(filter);
        fvFilter = FavoriteSearchFilter<EditorObjectSearchFilter>.parse(filter, parseFavoriteSubFilter);
    }

    private bool parseFavoriteSubFilter(string filter, out EditorObjectSearchFilter subFilter)
    {
        subFilter = parse(filter);
        return subFilter != null;
    }
}
