using UnityEngine;

namespace SDG.Unturned;

public class ItemTacticalAsset : ItemCaliberAsset
{
    protected GameObject _tactical;

    private bool _isLaser;

    private bool _isLight;

    private bool _isRangefinder;

    private bool _isMelee;

    public GameObject tactical => _tactical;

    public bool isLaser => _isLaser;

    public bool isLight => _isLight;

    public PlayerSpotLightConfig lightConfig { get; protected set; }

    public bool isRangefinder => _isRangefinder;

    public bool isMelee => _isMelee;

    public ItemTacticalAssetMeleeProperties MeleeProperties { get; set; }

    public Color laserColor { get; protected set; }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent && MeleeProperties != null)
        {
            MeleeProperties.BuildDescription(builder);
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _tactical = loadRequiredAsset<GameObject>(p.bundle, "Tactical");
        _isLaser = p.data.ContainsKey("Laser");
        _isLight = p.data.ContainsKey("Light");
        if (isLight)
        {
            lightConfig = new PlayerSpotLightConfig(p.data);
        }
        _isRangefinder = p.data.ContainsKey("Rangefinder");
        _isMelee = p.data.ContainsKey("Melee");
        if (_isMelee)
        {
            MeleeProperties = new ItemTacticalAssetMeleeProperties();
            MeleeProperties.PopulateAsset(in p);
        }
        Color value = p.data.LegacyParseColor("Laser_Color", Color.red);
        value = MathfEx.Clamp01(value);
        value.a = 1f;
        laserColor = value;
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Tactical");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Laser", isLaser);
        orAddDeclaration.Append("Light", isLight);
        orAddDeclaration.Append("Rangefinder", isRangefinder);
        orAddDeclaration.Append("Melee", isMelee);
        orAddDeclaration.Append("Laser_Color", laserColor);
    }
}
