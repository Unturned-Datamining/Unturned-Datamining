using UnityEngine;

namespace SDG.Unturned;

public struct PlayerSpotLightConfig
{
    public float range;

    public float angle;

    public float intensity;

    public Color color;

    public void applyToLight(Light light)
    {
        if (!(light == null))
        {
            light.range = range;
            light.spotAngle = angle;
            light.intensity = intensity;
            light.color = color;
        }
    }

    public PlayerSpotLightConfig(Data data)
    {
        range = data.readSingle("SpotLight_Range", 64f);
        angle = data.readSingle("SpotLight_Angle", 90f);
        intensity = data.readSingle("SpotLight_Intensity", 1.3f);
        color = data.readColor("SpotLight_Color", new Color32(245, 223, 147, byte.MaxValue));
    }
}
