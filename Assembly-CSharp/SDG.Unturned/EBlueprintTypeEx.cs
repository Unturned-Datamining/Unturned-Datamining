namespace SDG.Unturned;

public static class EBlueprintTypeEx
{
    public static readonly CachingAssetRef[] legacyBlueprintTypeCategoryTagRefs = new CachingAssetRef[10]
    {
        CachingAssetRef.Parse("ad1804b6945145f3b308738b0b8ea447"),
        CachingAssetRef.Parse("ebe755533bdd42d1871c3ac66b89530f"),
        CachingAssetRef.Parse("d089feb7e43f40c5a7dfcefc36998cfb"),
        CachingAssetRef.Parse("cdb2df24b76d4c6e9d8411c940d8337f"),
        CachingAssetRef.Parse("d739926736374e5ba34b4ac6ffbb5c8f"),
        CachingAssetRef.Parse("31a59b5fec3f4ec5b2887b1ce4acb029"),
        CachingAssetRef.Parse("71d9e182c18b4aad8e87778e4f621995"),
        CachingAssetRef.Parse("bfac6026305f4737a95fd275ebff65a6"),
        CachingAssetRef.Parse("b0c6cc0a8b4346be89aef697ecdb8e46"),
        CachingAssetRef.Parse("732ee6ffeb18418985cf4f9fde33dd11")
    };

    public static readonly CachingAssetRef salvageCategoryTagRef = CachingAssetRef.Parse("7ed29f9101ae4523a3b2e389414b7bd9");

    public static CachingAssetRef GetCategoryTagRef(this EBlueprintType type)
    {
        return legacyBlueprintTypeCategoryTagRefs[(int)type];
    }

    public static TagAsset GetCategoryTag(this EBlueprintType type)
    {
        return legacyBlueprintTypeCategoryTagRefs[(int)type].Get<TagAsset>();
    }
}
