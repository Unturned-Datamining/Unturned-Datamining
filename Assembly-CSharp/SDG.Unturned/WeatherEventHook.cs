using System;
using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

/// <summary>
/// Can be added to any GameObject to receive weather events:
/// - Day/Night
/// - Full Moon
/// - Rain
/// - Snow
/// </summary>
[AddComponentMenu("Unturned/Weather Event Hook")]
public class WeatherEventHook : MonoBehaviour
{
    /// <summary>
    /// Invoked when night changes to day.
    /// </summary>
    public UnityEvent OnDay;

    /// <summary>
    /// Invoked when day changes to night.
    /// </summary>
    public UnityEvent OnNight;

    /// <summary>
    /// Invoked when a zombie full-moon event starts.
    /// </summary>
    public UnityEvent OnFullMoonBegin;

    /// <summary>
    /// Invoked when a zombie full-moon event finishes.
    /// </summary>
    public UnityEvent OnFullMoonEnd;

    /// <summary>
    /// Invoked when rain starts to fall.
    /// </summary>
    public UnityEvent OnRainBegin;

    /// <summary>
    /// Invoked when rain finishes falling.
    /// </summary>
    public UnityEvent OnRainEnd;

    /// <summary>
    /// Invoked when snow starts to fall.
    /// </summary>
    public UnityEvent OnSnowBegin;

    /// <summary>
    /// Invoked when snow finishes falling.
    /// </summary>
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
