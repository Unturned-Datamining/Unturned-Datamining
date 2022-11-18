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

    public ItemTacticalAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _tactical = loadRequiredAsset<GameObject>(bundle, "Tactical");
        _isLaser = data.has("Laser");
        _isLight = data.has("Light");
        if (isLight)
        {
            lightConfig = new PlayerSpotLightConfig(data);
        }
        _isRangefinder = data.has("Rangefinder");
        _isMelee = data.has("Melee");
    }
}
