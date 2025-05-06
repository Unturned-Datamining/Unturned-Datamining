using System;

namespace SDG.Unturned;

/// <summary>
/// Special asset type that isn't (shouldn't be) returned by asset Find methods. Instead, if found when resolving
/// an asset legacy ID or GUID, Find is called again with the target specified by this asset.
/// </summary>
public class RedirectorAsset : Asset
{
    protected EAssetType assetCategoryOverride;

    private Guid _targetGuid;

    public override EAssetType assetCategory => assetCategoryOverride;

    public Guid TargetGuid => _targetGuid;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        assetCategoryOverride = p.data.ParseEnum("AssetCategory", EAssetType.NONE);
        if (id > 0 && assetCategoryOverride == EAssetType.NONE)
        {
            Assets.ReportError(this, "legacy ID was assigned but AssetCategory was not");
        }
        if (!p.data.TryParseGuid("TargetAsset", out _targetGuid))
        {
            Assets.ReportError(this, "unable to parse TargetAsset");
        }
    }
}
