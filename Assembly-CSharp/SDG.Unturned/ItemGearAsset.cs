using UnityEngine;

namespace SDG.Unturned;

public class ItemGearAsset : ItemClothingAsset
{
    /// <summary>
    /// If set, find a child meshrenderer with this name and change its material to the character hair material.
    /// </summary>
    public string hairOverride { get; protected set; }

    /// <summary>
    /// For items using hairOverride, the hair material color will default to this for players without the
    /// Gold Upgrade. (Since the Gold Upgrade is required for full RGB control, the default hair colors may
    /// look boring for items that cover the hair but aren't hair in of themselves.) Also used as the color
    /// in the cosmetic preview.
    /// </summary>
    public Color32? hairOverrideNonGoldColor { get; set; }

    /// <summary>
    /// If set, find a child meshrenderer with this name and change its material to the character beard material.
    /// </summary>
    public string BeardOverride { get; set; }

    /// <summary>
    /// For items using BeardOverride, the beard material color will default to this for players without the
    /// Gold Upgrade. (Since the Gold Upgrade is required for full RGB control, the default beard colors may
    /// look boring for items that cover the beard but aren't beards in of themselves.)
    /// Also used as the color in the cosmetic preview.
    /// </summary>
    public Color32? beardOverrideNonGoldColor { get; set; }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        base.hairVisible = p.data.ContainsKey("Hair");
        base.beardVisible = p.data.ContainsKey("Beard");
        hairOverride = p.data.GetString("Hair_Override");
        if (!string.IsNullOrEmpty(hairOverride) && p.data.TryParseColor32RGB("Hair_Override_NonGoldColor", out var value))
        {
            hairOverrideNonGoldColor = value;
        }
        BeardOverride = p.data.GetString("Beard_Override");
        if (!string.IsNullOrEmpty(BeardOverride) && p.data.TryParseColor32RGB("Beard_Override_NonGoldColor", out var value2))
        {
            beardOverrideNonGoldColor = value2;
        }
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
