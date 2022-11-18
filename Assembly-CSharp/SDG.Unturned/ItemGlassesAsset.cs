using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemGlassesAsset : ItemGearAsset
{
    protected GameObject _glasses;

    private ELightingVision _vision;

    public Color nightvisionColor;

    public float nightvisionFogIntensity;

    public GameObject glasses => _glasses;

    public ELightingVision vision => _vision;

    public PlayerSpotLightConfig lightConfig { get; protected set; }

    public bool isBlindfold { get; protected set; }

    public override byte[] getState(EItemOrigin origin)
    {
        if (vision != 0)
        {
            return new byte[1] { 1 };
        }
        return new byte[0];
    }

    public ItemGlassesAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        if (data.has("Vision"))
        {
            _vision = (ELightingVision)Enum.Parse(typeof(ELightingVision), data.readString("Vision"), ignoreCase: true);
            if (vision == ELightingVision.HEADLAMP)
            {
                lightConfig = new PlayerSpotLightConfig(data);
            }
            else if (vision == ELightingVision.CIVILIAN)
            {
                nightvisionColor = data.ReadColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_CIVILIAN);
                nightvisionFogIntensity = data.readSingle("Nightvision_Fog_Intensity", 0.5f);
                nightvisionColor.g = nightvisionColor.r;
                nightvisionColor.b = nightvisionColor.r;
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
        isBlindfold = data.has("Blindfold");
    }
}
