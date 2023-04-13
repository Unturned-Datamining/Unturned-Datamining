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

    public Color laserColor { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _tactical = loadRequiredAsset<GameObject>(bundle, "Tactical");
        _isLaser = data.ContainsKey("Laser");
        _isLight = data.ContainsKey("Light");
        if (isLight)
        {
            lightConfig = new PlayerSpotLightConfig(data);
        }
        _isRangefinder = data.ContainsKey("Rangefinder");
        _isMelee = data.ContainsKey("Melee");
        Color value = data.LegacyParseColor("Laser_Color", Color.red);
        value = MathfEx.Clamp01(value);
        value.a = 1f;
        laserColor = value;
    }
}
