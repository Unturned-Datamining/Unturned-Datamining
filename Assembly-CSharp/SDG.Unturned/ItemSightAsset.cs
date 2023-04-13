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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _sight = loadRequiredAsset<GameObject>(bundle, "Sight");
        if (data.ContainsKey("Vision"))
        {
            _vision = (ELightingVision)Enum.Parse(typeof(ELightingVision), data.GetString("Vision"), ignoreCase: true);
            if (vision == ELightingVision.CIVILIAN)
            {
                nightvisionColor = data.LegacyParseColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_CIVILIAN);
                nightvisionFogIntensity = data.ParseFloat("Nightvision_Fog_Intensity", 0.5f);
            }
            else if (vision == ELightingVision.MILITARY)
            {
                nightvisionColor = data.LegacyParseColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_MILITARY);
                nightvisionFogIntensity = data.ParseFloat("Nightvision_Fog_Intensity", 0.25f);
            }
        }
        else
        {
            _vision = ELightingVision.NONE;
        }
        zoom = Mathf.Max(1f, data.ParseFloat("Zoom"));
        shouldZoomUsingEyes = data.ParseBool("Zoom_Using_Eyes");
        _isHolographic = data.ContainsKey("Holographic");
    }
}
