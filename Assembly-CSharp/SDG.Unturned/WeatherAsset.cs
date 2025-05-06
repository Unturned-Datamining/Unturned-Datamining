using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows map makers to create custom weather events.
/// </summary>
public class WeatherAsset : WeatherAssetBase
{
    public struct WeatherColor
    {
        public Color customColor;

        /// <summary>
        /// If specified level editor color can be used rather than a per-asset color.
        /// </summary>
        public ELightingColor levelEnum;

        public WeatherColor(IDatDictionary data)
        {
            if (data == null)
            {
                customColor = Color.black;
                levelEnum = ELightingColor.CUSTOM_OVERRIDE;
                return;
            }
            byte r = (data.ContainsKey("R") ? data.ParseUInt8("R", 0) : byte.MaxValue);
            byte g = (data.ContainsKey("G") ? data.ParseUInt8("G", 0) : byte.MaxValue);
            byte b = (data.ContainsKey("B") ? data.ParseUInt8("B", 0) : byte.MaxValue);
            customColor = new Color32(r, g, b, byte.MaxValue);
            if (data.ContainsKey("Level_Enum"))
            {
                levelEnum = data.ParseEnum("Level_Enum", ELightingColor.SUN);
            }
            else
            {
                levelEnum = ELightingColor.CUSTOM_OVERRIDE;
            }
        }

        public Color Evaluate(LightingInfo levelValues)
        {
            if (levelEnum != ELightingColor.CUSTOM_OVERRIDE)
            {
                return levelValues.colors[(int)levelEnum] * customColor;
            }
            return customColor;
        }
    }

    public class TimeValues
    {
        public WeatherColor fogColor;

        public float fogDensity;

        public WeatherColor cloudColor;

        public WeatherColor cloudRimColor;

        public float brightnessMultiplier;

        public TimeValues(IDatDictionary data)
        {
            if (data == null)
            {
                brightnessMultiplier = 1f;
                return;
            }
            fogColor = new WeatherColor(data.GetDictionary("Fog_Color"));
            fogDensity = data.ParseFloat("Fog_Density");
            cloudColor = new WeatherColor(data.GetDictionary("Cloud_Color"));
            cloudRimColor = new WeatherColor(data.GetDictionary("Cloud_Rim_Color"));
            if (data.ContainsKey("Brightness_Multiplier"))
            {
                brightnessMultiplier = data.ParseFloat("Brightness_Multiplier");
            }
            else
            {
                brightnessMultiplier = 1f;
            }
        }
    }

    public struct Effect : IDatParseable
    {
        public MasterBundleReference<GameObject> prefab;

        public float emissionExponent;

        public float pitch;

        public bool translateWithView;

        public bool rotateYawWithWind;

        public bool TryParse(IDatNode node)
        {
            if (!(node is IDatDictionary dictionary))
            {
                return false;
            }
            prefab = dictionary.ParseStruct<MasterBundleReference<GameObject>>("Prefab");
            emissionExponent = dictionary.ParseFloat("Emission_Exponent");
            pitch = dictionary.ParseFloat("Pitch");
            translateWithView = dictionary.ParseBool("Translate_With_View");
            rotateYawWithWind = dictionary.ParseBool("Rotate_Yaw_With_Wind");
            return true;
        }
    }

    /// <summary>
    /// Directional light shadow strength multiplier.
    /// </summary>
    public float shadowStrengthMultiplier;

    /// <summary>
    /// Exponent applied to effect blend alpha.
    /// </summary>
    public float fogBlendExponent;

    /// <summary>
    /// Exponent applied to effect blend alpha.
    /// </summary>
    public float cloudBlendExponent;

    /// <summary>
    /// SpeedTree wind strength for blizzard. Should be removed?
    /// </summary>
    public float windMain;

    public float staminaPerSecond;

    public float healthPerSecond;

    public float foodPerSecond;

    public float waterPerSecond;

    public float virusPerSecond;

    public Effect[] effects;

    protected TimeValues[] timeValues;

    /// <summary>
    /// Does this weather affect fog color and density?
    /// </summary>
    public bool overrideFog { get; protected set; }

    /// <summary>
    /// Does this weather affect sky fog color?
    /// </summary>
    public bool overrideAtmosphericFog { get; protected set; }

    /// <summary>
    /// Does this weather affect cloud colors?
    /// </summary>
    public bool overrideCloudColors { get; protected set; }

    public void getTimeValues(int blendKey, int currentKey, out TimeValues blendFrom, out TimeValues blendTo)
    {
        blendTo = timeValues[currentKey];
        blendFrom = ((blendKey == -1) ? blendTo : timeValues[blendKey]);
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (base.componentType == typeof(WeatherComponentBase))
        {
            base.componentType = typeof(CustomWeatherComponent);
        }
        overrideFog = p.data.ParseBool("Override_Fog");
        overrideAtmosphericFog = p.data.ParseBool("Override_Atmospheric_Fog");
        overrideCloudColors = p.data.ParseBool("Override_Cloud_Colors");
        if (p.data.ContainsKey("Shadow_Strength_Multiplier"))
        {
            shadowStrengthMultiplier = p.data.ParseFloat("Shadow_Strength_Multiplier");
        }
        else
        {
            shadowStrengthMultiplier = 1f;
        }
        if (p.data.ContainsKey("Fog_Blend_Exponent"))
        {
            fogBlendExponent = p.data.ParseFloat("Fog_Blend_Exponent");
        }
        else
        {
            fogBlendExponent = 1f;
        }
        if (p.data.ContainsKey("Cloud_Blend_Exponent"))
        {
            cloudBlendExponent = p.data.ParseFloat("Cloud_Blend_Exponent");
        }
        else
        {
            cloudBlendExponent = 1f;
        }
        windMain = p.data.ParseFloat("Wind_Main");
        staminaPerSecond = p.data.ParseFloat("Stamina_Per_Second");
        healthPerSecond = p.data.ParseFloat("Health_Per_Second");
        foodPerSecond = p.data.ParseFloat("Food_Per_Second");
        waterPerSecond = p.data.ParseFloat("Water_Per_Second");
        virusPerSecond = p.data.ParseFloat("Virus_Per_Second");
        timeValues = new TimeValues[4];
        timeValues[0] = new TimeValues(p.data.GetDictionary("Dawn"));
        timeValues[1] = new TimeValues(p.data.GetDictionary("Midday"));
        timeValues[2] = new TimeValues(p.data.GetDictionary("Dusk"));
        timeValues[3] = new TimeValues(p.data.GetDictionary("Midnight"));
        if (p.data.TryGetList("Effects", out var node))
        {
            effects = new Effect[node.Count];
            for (int i = 0; i < node.Count; i++)
            {
                effects[i] = node[i].ParseStruct<Effect>();
            }
        }
    }
}
