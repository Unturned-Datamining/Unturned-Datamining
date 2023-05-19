using UnityEngine;

namespace SDG.Unturned;

public struct PlayerSpotLightConfig
{
    public bool isEnabled;

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

    public PlayerSpotLightConfig(DatDictionary data)
    {
        isEnabled = data.ParseBool("SpotLight_Enabled", defaultValue: true);
        range = data.ParseFloat("SpotLight_Range", 64f);
        angle = data.ParseFloat("SpotLight_Angle", 90f);
        intensity = data.ParseFloat("SpotLight_Intensity", 1.3f);
        color = data.LegacyParseColor("SpotLight_Color", new Color32(245, 223, 147, byte.MaxValue));
    }
}
