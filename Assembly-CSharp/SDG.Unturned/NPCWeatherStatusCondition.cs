namespace SDG.Unturned;

public class NPCWeatherStatusCondition : NPCLogicCondition
{
    public AssetReference<WeatherAssetBase> weather { get; private set; }

    public ENPCWeatherStatus value { get; private set; }

    public override bool isConditionMet(Player player)
    {
        return value switch
        {
            ENPCWeatherStatus.Active => doesLogicPass(LevelLighting.IsWeatherActive(weather.Find()), b: true), 
            ENPCWeatherStatus.Transitioning_In => doesLogicPass(LevelLighting.IsWeatherTransitioningIn(weather.Find()), b: true), 
            ENPCWeatherStatus.Fully_Transitioned_In => doesLogicPass(LevelLighting.IsWeatherFullyTransitionedIn(weather.Find()), b: true), 
            ENPCWeatherStatus.Transitioning_Out => doesLogicPass(LevelLighting.IsWeatherTransitioningOut(weather.Find()), b: true), 
            ENPCWeatherStatus.Fully_Transitioned_Out => doesLogicPass(LevelLighting.IsWeatherFullyTransitionedOut(weather.Find()), b: true), 
            ENPCWeatherStatus.Transitioning => doesLogicPass(LevelLighting.IsWeatherTransitioning(weather.Find()), b: true), 
            _ => false, 
        };
    }

    public NPCWeatherStatusCondition(AssetReference<WeatherAssetBase> newWeather, ENPCWeatherStatus newValue, ENPCLogicType newLogicType, string newText)
        : base(newLogicType, newText, newShouldReset: false)
    {
        weather = newWeather;
        value = newValue;
    }
}
