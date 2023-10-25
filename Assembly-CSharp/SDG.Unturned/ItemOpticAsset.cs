using UnityEngine;

namespace SDG.Unturned;

public class ItemOpticAsset : ItemAsset
{
    /// <summary>
    /// Factor e.g. 2 is a 2x multiplier.
    /// Prior to 2022-04-11 this was the target field of view. (90/fov)
    /// </summary>
    public float zoom { get; private set; }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent && zoom != 1f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ZoomFactor", zoom), 10000);
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        zoom = Mathf.Max(1f, data.ParseFloat("Zoom"));
    }
}
