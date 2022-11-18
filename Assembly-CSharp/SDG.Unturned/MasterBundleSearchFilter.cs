namespace SDG.Unturned;

public struct MasterBundleSearchFilter
{
    private MasterBundleConfig masterBundle;

    public static MasterBundleSearchFilter? parse(string filter)
    {
        if (!SearchFilterUtil.parseKeyValue(filter, "mb:", out var value))
        {
            return null;
        }
        MasterBundleConfig masterBundleConfig = Assets.findMasterBundleByName(value, matchExtension: false);
        if (masterBundleConfig == null)
        {
            return null;
        }
        return new MasterBundleSearchFilter(masterBundleConfig);
    }

    public bool ignores(Asset asset)
    {
        if (asset != null)
        {
            return asset.originMasterBundle != masterBundle;
        }
        return true;
    }

    public bool matches(Asset asset)
    {
        return !ignores(asset);
    }

    public MasterBundleSearchFilter(MasterBundleConfig masterBundle)
    {
        this.masterBundle = masterBundle;
    }
}
