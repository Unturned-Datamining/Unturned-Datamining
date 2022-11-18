namespace SDG.Unturned;

public class LegacyRainComponent : CustomWeatherComponent
{
    public override void OnBeginTransitionIn()
    {
        SetRain(ELightingRain.PRE_DRIZZLE);
    }

    public override void OnEndTransitionIn()
    {
        SetRain(ELightingRain.DRIZZLE);
    }

    public override void OnBeginTransitionOut()
    {
        SetRain(ELightingRain.POST_DRIZZLE);
    }

    public override void OnEndTransitionOut()
    {
        SetRain(ELightingRain.NONE);
    }

    private void SetRain(ELightingRain rain)
    {
        LevelLighting.rainyness = rain;
        LightingManager.broadcastRainUpdated(rain);
    }

    private void OnEnable()
    {
        puddleWaterLevel = 0.75f;
        puddleIntensity = 2f;
    }
}
