namespace SDG.Unturned;

public class ItemGearAsset : ItemClothingAsset
{
    /// <summary>
    /// If set, find a child meshrenderer with this name and change its material to the character hair material.
    /// </summary>
    public string hairOverride { get; protected set; }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        base.hairVisible = p.data.ContainsKey("Hair");
        base.beardVisible = p.data.ContainsKey("Beard");
        hairOverride = p.data.GetString("Hair_Override");
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Gear");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Hair", base.hairVisible);
        orAddDeclaration.Append("Beard", base.beardVisible);
    }
}
