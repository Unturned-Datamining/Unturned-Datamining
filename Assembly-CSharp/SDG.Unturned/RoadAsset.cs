using UnityEngine;

namespace SDG.Unturned;

public class RoadAsset : Asset
{
    private string displayName;

    public Texture2D RoadTexture { get; set; }

    public Material RenderMaterial { get; set; }

    /// <summary>
    /// Horizontal distance before road begins tapering off into the terrain.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Size along the "up" axis.
    /// </summary>
    public float Depth { get; set; }

    /// <summary>
    /// Distance along the terrain surface normal to move each road vertex.
    /// </summary>
    public float OffsetAlongNormal { get; set; }

    /// <summary>
    /// Multiplier for how far along the road before texture repeats.
    /// </summary>
    public float RepeatDistanceScale { get; set; }

    /// <summary>
    /// Defaults to None, in which case the backwards-compatible chart classification is used.
    /// </summary>
    public EObjectChart ChartOverride { get; set; }

    /// <summary>
    /// Physics material to assign to road colliders.
    /// Replaces the "concrete" toggle in the older editor.
    /// </summary>
    public PhysicMaterial UnityPhysicsMaterial { get; set; }

    public override string FriendlyName
    {
        get
        {
            if (displayName == null)
            {
                return base.FriendlyName;
            }
            return displayName;
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (p.localization != null)
        {
            displayName = p.localization.format("Name");
        }
        if (!Dedicator.IsDedicatedServer)
        {
            RoadTexture = LoadRedirectableAsset<Texture2D>(p.bundle, "Texture", p.data, "TexturePath");
            if (RoadTexture == null)
            {
                ReportAssetError("missing Texture");
            }
            Material material = new Material(RoadMaterial.shader);
            material.mainTexture = RoadTexture;
            RenderMaterial = material;
        }
        Width = p.data.ParseFloat("Width");
        Depth = p.data.ParseFloat("Depth");
        OffsetAlongNormal = p.data.ParseFloat("OffsetAlongNormal");
        RepeatDistanceScale = p.data.ParseFloat("RepeatDistanceScale", 1f);
        ChartOverride = p.data.ParseEnum("Chart", EObjectChart.NONE);
        if (p.data.TryParseEnum<EPhysicsMaterial>("VanillaPhysicsMaterial", out var value))
        {
            UnityPhysicsMaterial = PhysicsTool.LoadResourceForLegacyMaterial(value);
        }
        else
        {
            UnityPhysicsMaterial = p.data.readMasterBundleReference<PhysicMaterial>("PhysicsMaterial", p.bundle).loadAsset();
        }
        if (UnityPhysicsMaterial == null)
        {
            ReportAssetError("missing PhysicsMaterial");
        }
    }
}
