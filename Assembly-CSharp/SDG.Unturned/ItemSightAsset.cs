using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemSightAsset : ItemCaliberAsset
{
    protected GameObject _sight;

    private ELightingVision _vision;

    public Color nightvisionColor;

    public float nightvisionFogIntensity;

    private bool _isHolographic;

    public bool shouldZoomUsingEyes;

    public GameObject sight => _sight;

    public ELightingVision vision => _vision;

    public float zoom { get; private set; }

    public bool isHolographic => _isHolographic;

    public ItemSightAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _sight = loadRequiredAsset<GameObject>(bundle, "Sight");
        if (data.has("Vision"))
        {
            _vision = (ELightingVision)Enum.Parse(typeof(ELightingVision), data.readString("Vision"), ignoreCase: true);
            if (vision == ELightingVision.CIVILIAN)
            {
                nightvisionColor = data.ReadColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_CIVILIAN);
                nightvisionFogIntensity = data.readSingle("Nightvision_Fog_Intensity", 0.5f);
            }
            else if (vision == ELightingVision.MILITARY)
            {
                nightvisionColor = data.ReadColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_MILITARY);
                nightvisionFogIntensity = data.readSingle("Nightvision_Fog_Intensity", 0.25f);
            }
        }
        else
        {
            _vision = ELightingVision.NONE;
        }
        zoom = Mathf.Max(1f, data.readSingle("Zoom"));
        shouldZoomUsingEyes = data.readBoolean("Zoom_Using_Eyes");
        _isHolographic = data.has("Holographic");
    }
}
