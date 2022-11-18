using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class WeatherAsset : WeatherAssetBase
{
    public struct WeatherColor
    {
        public Color customColor;

        public ELightingColor levelEnum;

        public WeatherColor(IFormattedFileReader reader)
        {
            if (reader == null)
            {
                customColor = Color.black;
                levelEnum = ELightingColor.CUSTOM_OVERRIDE;
                return;
            }
            byte r = (reader.containsKey("R") ? reader.readValue<byte>("R") : byte.MaxValue);
            byte g = (reader.containsKey("G") ? reader.readValue<byte>("G") : byte.MaxValue);
            byte b = (reader.containsKey("B") ? reader.readValue<byte>("B") : byte.MaxValue);
            customColor = new Color32(r, g, b, byte.MaxValue);
            if (reader.containsKey("Level_Enum"))
            {
                levelEnum = reader.readValue<ELightingColor>("Level_Enum");
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

        public TimeValues(IFormattedFileReader reader)
        {
            if (reader == null)
            {
                brightnessMultiplier = 1f;
                return;
            }
            fogColor = new WeatherColor(reader.readObject("Fog_Color"));
            fogDensity = reader.readValue<float>("Fog_Density");
            cloudColor = new WeatherColor(reader.readObject("Cloud_Color"));
            cloudRimColor = new WeatherColor(reader.readObject("Cloud_Rim_Color"));
            if (reader.containsKey("Brightness_Multiplier"))
            {
                brightnessMultiplier = reader.readValue<float>("Brightness_Multiplier");
            }
            else
            {
                brightnessMultiplier = 1f;
            }
        }
    }

    public class Effect : IFormattedFileReadable
    {
        public MasterBundleReference<GameObject> prefab;

        public float emissionExponent;

        public float pitch;

        public bool translateWithView;

        public bool rotateYawWithWind;

        public void read(IFormattedFileReader reader)
        {
            reader = reader.readObject();
            prefab = reader.readValue<MasterBundleReference<GameObject>>("Prefab");
            emissionExponent = reader.readValue<float>("Emission_Exponent");
            pitch = reader.readValue<float>("Pitch");
            translateWithView = reader.readValue<bool>("Translate_With_View");
            rotateYawWithWind = reader.readValue<bool>("Rotate_Yaw_With_Wind");
        }
    }

    public float shadowStrengthMultiplier;

    public float fogBlendExponent;

    public float cloudBlendExponent;

    public float windMain;

    public float staminaPerSecond;

    public float healthPerSecond;

    public float foodPerSecond;

    public float waterPerSecond;

    public float virusPerSecond;

    public Effect[] effects;

    protected TimeValues[] timeValues;

    public bool overrideFog { get; protected set; }

    public bool overrideAtmosphericFog { get; protected set; }

    public bool overrideCloudColors { get; protected set; }

    public void getTimeValues(int blendKey, int currentKey, out TimeValues blendFrom, out TimeValues blendTo)
    {
        blendTo = timeValues[currentKey];
        blendFrom = ((blendKey == -1) ? blendTo : timeValues[blendKey]);
    }

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        if (base.componentType == typeof(WeatherComponentBase))
        {
            base.componentType = typeof(CustomWeatherComponent);
        }
        overrideFog = reader.readValue<bool>("Override_Fog");
        overrideAtmosphericFog = reader.readValue<bool>("Override_Atmospheric_Fog");
        overrideCloudColors = reader.readValue<bool>("Override_Cloud_Colors");
        if (reader.containsKey("Shadow_Strength_Multiplier"))
        {
            shadowStrengthMultiplier = reader.readValue<float>("Shadow_Strength_Multiplier");
        }
        else
        {
            shadowStrengthMultiplier = 1f;
        }
        if (reader.containsKey("Fog_Blend_Exponent"))
        {
            fogBlendExponent = reader.readValue<float>("Fog_Blend_Exponent");
        }
        else
        {
            fogBlendExponent = 1f;
        }
        if (reader.containsKey("Cloud_Blend_Exponent"))
        {
            cloudBlendExponent = reader.readValue<float>("Cloud_Blend_Exponent");
        }
        else
        {
            cloudBlendExponent = 1f;
        }
        windMain = reader.readValue<float>("Wind_Main");
        staminaPerSecond = reader.readValue<float>("Stamina_Per_Second");
        healthPerSecond = reader.readValue<float>("Health_Per_Second");
        foodPerSecond = reader.readValue<float>("Food_Per_Second");
        waterPerSecond = reader.readValue<float>("Water_Per_Second");
        virusPerSecond = reader.readValue<float>("Virus_Per_Second");
        timeValues = new TimeValues[4];
        timeValues[0] = new TimeValues(reader.readObject("Dawn"));
        timeValues[1] = new TimeValues(reader.readObject("Midday"));
        timeValues[2] = new TimeValues(reader.readObject("Dusk"));
        timeValues[3] = new TimeValues(reader.readObject("Midnight"));
        int num = reader.readArrayLength("Effects");
        if (num > 0)
        {
            effects = new Effect[num];
            for (int i = 0; i < num; i++)
            {
                effects[i] = reader.readValue<Effect>(i);
            }
        }
    }

    public WeatherAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
