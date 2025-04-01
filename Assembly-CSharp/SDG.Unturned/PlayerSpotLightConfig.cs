using UnityEngine;

namespace SDG.Unturned;

public struct PlayerSpotLightConfig
{
    /// <summary>
    /// If true, light contributes to player spotlight. Defaults to true.
    ///
    /// Can be set to false for modders with a custom light setup. For example, this was added
    /// for a modder who is using melee lights to toggle a lightsaber-style glow.
    /// </summary>
    public bool isEnabled;

    public float range;

    public float angle;

    public Color color;

    public void applyToLight(Light light)
    {
        if (!(light == null))
        {
            light.range = range;
            light.spotAngle = angle;
            light.intensity = 1f;
            light.color = color;
        }
    }

    public override string ToString()
    {
        return $"(Enabled: {isEnabled} Range: {range}m Angle: {angle}° Color: {color})";
    }

    public PlayerSpotLightConfig(DatDictionary data)
    {
        isEnabled = data.ParseBool("SpotLight_Enabled", defaultValue: true);
        range = data.ParseFloat("SpotLight_Range", 64f);
        angle = data.ParseFloat("SpotLight_Angle", 90f);
        float num = data.ParseFloat("SpotLight_Intensity", 1.3f);
        color = data.LegacyParseColor("SpotLight_Color", new Color32(245, 223, 147, byte.MaxValue)) * num;
    }
}
