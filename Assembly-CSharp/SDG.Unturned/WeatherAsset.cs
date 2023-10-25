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

        public WeatherColor(DatDictionary data)
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

        public TimeValues(DatDictionary data)
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
            if (!(node is DatDictionary datDictionary))
            {
                return false;
            }
            prefab = datDictionary.ParseStruct<MasterBundleReference<GameObject>>("Prefab");
            emissionExponent = datDictionary.ParseFloat("Emission_Exponent");
            pitch = datDictionary.ParseFloat("Pitch");
            translateWithView = datDictionary.ParseBool("Translate_With_View");
            rotateYawWithWind = datDictionary.ParseBool("Rotate_Yaw_With_Wind");
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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (base.componentType == typeof(WeatherComponentBase))
        {
            base.componentType = typeof(CustomWeatherComponent);
        }
        overrideFog = data.ParseBool("Override_Fog");
        overrideAtmosphericFog = data.ParseBool("Override_Atmospheric_Fog");
        overrideCloudColors = data.ParseBool("Override_Cloud_Colors");
        if (data.ContainsKey("Shadow_Strength_Multiplier"))
        {
            shadowStrengthMultiplier = data.ParseFloat("Shadow_Strength_Multiplier");
        }
        else
        {
            shadowStrengthMultiplier = 1f;
        }
        if (data.ContainsKey("Fog_Blend_Exponent"))
        {
            fogBlendExponent = data.ParseFloat("Fog_Blend_Exponent");
        }
        else
        {
            fogBlendExponent = 1f;
        }
        if (data.ContainsKey("Cloud_Blend_Exponent"))
        {
            cloudBlendExponent = data.ParseFloat("Cloud_Blend_Exponent");
        }
        else
        {
            cloudBlendExponent = 1f;
        }
        windMain = data.ParseFloat("Wind_Main");
        staminaPerSecond = data.ParseFloat("Stamina_Per_Second");
        healthPerSecond = data.ParseFloat("Health_Per_Second");
        foodPerSecond = data.ParseFloat("Food_Per_Second");
        waterPerSecond = data.ParseFloat("Water_Per_Second");
        virusPerSecond = data.ParseFloat("Virus_Per_Second");
        timeValues = new TimeValues[4];
        timeValues[0] = new TimeValues(data.GetDictionary("Dawn"));
        timeValues[1] = new TimeValues(data.GetDictionary("Midday"));
        timeValues[2] = new TimeValues(data.GetDictionary("Dusk"));
        timeValues[3] = new TimeValues(data.GetDictionary("Midnight"));
        if (data.TryGetList("Effects", out var node))
        {
            effects = new Effect[node.Count];
            for (int i = 0; i < node.Count; i++)
            {
                effects[i] = node[i].ParseStruct<Effect>();
            }
        }
    }
}
