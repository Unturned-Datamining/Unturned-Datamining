namespace SDG.Unturned;

public class NPCWeatherBlendAlphaCondition : NPCLogicCondition
{
    public AssetReference<WeatherAssetBase> weather { get; private set; }

    public float value { get; private set; }

    public override bool isConditionMet(Player player)
    {
        return doesLogicPass(LevelLighting.GetWeatherGlobalBlendAlpha(weather.Find()), value);
    }

    public NPCWeatherBlendAlphaCondition(AssetReference<WeatherAssetBase> newWeather, float newValue, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        weather = newWeather;
        value = newValue;
    }
}
