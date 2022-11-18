namespace SDG.Unturned;

public class LegacySnowComponent : CustomWeatherComponent
{
    public override void OnBeginTransitionIn()
    {
        SetSnow(ELightingSnow.PRE_BLIZZARD);
    }

    public override void OnEndTransitionIn()
    {
        SetSnow(ELightingSnow.BLIZZARD);
    }

    public override void OnBeginTransitionOut()
    {
        SetSnow(ELightingSnow.POST_BLIZZARD);
    }

    public override void OnEndTransitionOut()
    {
        SetSnow(ELightingSnow.NONE);
    }

    private void SetSnow(ELightingSnow snow)
    {
        LevelLighting.snowyness = snow;
        LightingManager.broadcastSnowUpdated(snow);
    }
}
