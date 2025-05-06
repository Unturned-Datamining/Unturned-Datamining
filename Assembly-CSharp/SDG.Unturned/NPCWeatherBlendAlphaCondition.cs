using System;

namespace SDG.Unturned;

/// <summary>
/// Compares weather intensity to value.
/// </summary>
public class NPCWeatherBlendAlphaCondition : NPCLogicCondition
{
    public AssetReference<WeatherAssetBase> weather { get; private set; }

    public float value { get; private set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(LevelLighting.GetWeatherGlobalBlendAlpha(weather.Find()), value);
    }

    internal override void PopulateV2(in PopulateConditionParameters p)
    {
        base.PopulateV2(in p);
        if (p.data.TryParseGuid("GUID", out var gUID))
        {
            weather = new AssetReference<WeatherAssetBase>(gUID);
        }
        else
        {
            p.ReportRequiredOptionInvalid("GUID");
        }
        if (p.data.TryParseFloat("Value", out var num))
        {
            value = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    internal override void PopulateLegacy(in PopulateConditionParameters p)
    {
        base.PopulateLegacy(in p);
        if (p.data.TryParseGuid(p.legacyPrefix + "_GUID", out var gUID))
        {
            weather = new AssetReference<WeatherAssetBase>(gUID);
        }
        else
        {
            p.ReportRequiredOptionInvalid("GUID");
        }
        if (p.data.TryParseFloat(p.legacyPrefix + "_Value", out var num))
        {
            value = num;
        }
        else
        {
            p.ReportRequiredOptionInvalid("Value");
        }
    }

    public NPCWeatherBlendAlphaCondition()
    {
    }

    [Obsolete]
    public NPCWeatherBlendAlphaCondition(AssetReference<WeatherAssetBase> newWeather, float newValue, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        weather = newWeather;
        value = newValue;
    }
}
