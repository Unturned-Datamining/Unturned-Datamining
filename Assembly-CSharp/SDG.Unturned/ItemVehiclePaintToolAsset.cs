using UnityEngine;

namespace SDG.Unturned;

public class ItemVehiclePaintToolAsset : ItemToolAsset
{
    public Color32 PaintColor { get; protected set; }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (p.data.TryParseColor32RGB("PaintColor", out var value))
        {
            PaintColor = value;
        }
        else
        {
            Assets.ReportError(this, "missing PaintColor");
        }
    }
}
