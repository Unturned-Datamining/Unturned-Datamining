using System;
using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Weather Event Hook")]
public class WeatherEventHook : MonoBehaviour
{
    public UnityEvent OnDay;

    public UnityEvent OnNight;

    public UnityEvent OnFullMoonBegin;

    public UnityEvent OnFullMoonEnd;

    public UnityEvent OnRainBegin;

    public UnityEvent OnRainEnd;

    public UnityEvent OnSnowBegin;

    public UnityEvent OnSnowEnd;

    protected void onDayNightUpdated(bool isDaytime)
    {
        if (isDaytime)
        {
            OnDay.TryInvoke(this);
        }
        else
        {
            OnNight.TryInvoke(this);
        }
    }

    protected void onMoonUpdated(bool isFullMoon)
    {
        if (isFullMoon)
        {
            OnFullMoonBegin.TryInvoke(this);
        }
        else
        {
            OnFullMoonEnd.TryInvoke(this);
        }
    }

    protected void onRainUpdated(ELightingRain rain)
    {
        if (rain == ELightingRain.DRIZZLE)
        {
            OnRainBegin.TryInvoke(this);
        }
        else
        {
            OnRainEnd.TryInvoke(this);
        }
    }

    protected void onSnowUpdated(ELightingSnow snow)
    {
        if (snow == ELightingSnow.BLIZZARD)
        {
            OnSnowBegin.TryInvoke(this);
        }
        else
        {
            OnSnowEnd.TryInvoke(this);
        }
    }

    protected void OnEnable()
    {
        LightingManager.onDayNightUpdated_ModHook = (DayNightUpdated)Delegate.Combine(LightingManager.onDayNightUpdated_ModHook, new DayNightUpdated(onDayNightUpdated));
        LightingManager.onMoonUpdated_ModHook = (MoonUpdated)Delegate.Combine(LightingManager.onMoonUpdated_ModHook, new MoonUpdated(onMoonUpdated));
        LightingManager.onRainUpdated_ModHook = (RainUpdated)Delegate.Combine(LightingManager.onRainUpdated_ModHook, new RainUpdated(onRainUpdated));
        LightingManager.onSnowUpdated_ModHook = (SnowUpdated)Delegate.Combine(LightingManager.onSnowUpdated_ModHook, new SnowUpdated(onSnowUpdated));
        onDayNightUpdated(LightingManager.isDaytime);
        onMoonUpdated(LightingManager.isFullMoon);
        onRainUpdated(LevelLighting.rainyness);
        onSnowUpdated(LevelLighting.snowyness);
    }

    protected void OnDisable()
    {
        LightingManager.onDayNightUpdated_ModHook = (DayNightUpdated)Delegate.Remove(LightingManager.onDayNightUpdated_ModHook, new DayNightUpdated(onDayNightUpdated));
        LightingManager.onMoonUpdated_ModHook = (MoonUpdated)Delegate.Remove(LightingManager.onMoonUpdated_ModHook, new MoonUpdated(onMoonUpdated));
        LightingManager.onRainUpdated_ModHook = (RainUpdated)Delegate.Remove(LightingManager.onRainUpdated_ModHook, new RainUpdated(onRainUpdated));
        LightingManager.onSnowUpdated_ModHook = (SnowUpdated)Delegate.Remove(LightingManager.onSnowUpdated_ModHook, new SnowUpdated(onSnowUpdated));
    }
}
