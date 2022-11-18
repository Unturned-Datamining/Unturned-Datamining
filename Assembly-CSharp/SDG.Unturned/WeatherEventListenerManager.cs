using System;
using System.Collections.Generic;
using Unturned.UnityEx;

namespace SDG.Unturned;

public static class WeatherEventListenerManager
{
    private class WeatherListenerGroup
    {
        public Guid assetGuid;

        public List<CustomWeatherEventHook> componentListeners;

        public List<WeatherBlendAlphaChangedListener> blendAlphaListeners;

        public List<WeatherStatusChangedListener> statusListeners;

        public WeatherListenerGroup(Guid assetGuid)
        {
            this.assetGuid = assetGuid;
            componentListeners = new List<CustomWeatherEventHook>();
            blendAlphaListeners = new List<WeatherBlendAlphaChangedListener>();
            statusListeners = new List<WeatherStatusChangedListener>();
        }
    }

    private static List<WeatherListenerGroup> customWeatherListeners = new List<WeatherListenerGroup>();

    public static void AddBlendAlphaListener(Guid assetGuid, WeatherBlendAlphaChangedListener listener)
    {
        FindOrAddGroupByAssetGuid(assetGuid)?.blendAlphaListeners.Add(listener);
    }

    public static void RemoveBlendAlphaListener(WeatherBlendAlphaChangedListener listener)
    {
        foreach (WeatherListenerGroup customWeatherListener in customWeatherListeners)
        {
            customWeatherListener.blendAlphaListeners.RemoveFast(listener);
        }
    }

    public static void AddStatusListener(Guid assetGuid, WeatherStatusChangedListener listener)
    {
        FindOrAddGroupByAssetGuid(assetGuid)?.statusListeners.Add(listener);
    }

    public static void RemoveStatusListener(WeatherStatusChangedListener listener)
    {
        foreach (WeatherListenerGroup customWeatherListener in customWeatherListeners)
        {
            customWeatherListener.statusListeners.RemoveFast(listener);
        }
    }

    internal static void AddComponentListener(Guid assetGuid, CustomWeatherEventHook listener)
    {
        FindOrAddComponentListenersByAssetGuid(assetGuid).Add(listener);
        if (LevelLighting.GetWeatherStateForListeners(assetGuid, out var isActive, out var isFullyTransitionedIn) && isActive)
        {
            if (isFullyTransitionedIn)
            {
                listener.OnWeatherEndTransitionIn.TryInvoke(listener);
            }
            else
            {
                listener.OnWeatherBeginTransitionIn.TryInvoke(listener);
            }
        }
    }

    internal static void RemoveComponentListener(Guid assetGuid, CustomWeatherEventHook listener)
    {
        FindComponentListenersByAssetGuid(assetGuid)?.RemoveFast(listener);
    }

    internal static void InvokeBeginTransitionIn(Guid assetGuid)
    {
        foreach (CustomWeatherEventHook item in EnumerateComponentListeners(assetGuid))
        {
            item.OnWeatherBeginTransitionIn.TryInvoke(item);
        }
    }

    internal static void InvokeEndTransitionIn(Guid assetGuid)
    {
        foreach (CustomWeatherEventHook item in EnumerateComponentListeners(assetGuid))
        {
            item.OnWeatherEndTransitionIn.TryInvoke(item);
        }
    }

    internal static void InvokeBeginTransitionOut(Guid assetGuid)
    {
        foreach (CustomWeatherEventHook item in EnumerateComponentListeners(assetGuid))
        {
            item.OnWeatherBeginTransitionOut.TryInvoke(item);
        }
    }

    internal static void InvokeEndTransitionOut(Guid assetGuid)
    {
        foreach (CustomWeatherEventHook item in EnumerateComponentListeners(assetGuid))
        {
            item.OnWeatherEndTransitionOut.TryInvoke(item);
        }
    }

    internal static void InvokeStatusChange(WeatherAssetBase asset, EWeatherStatusChange statusChange)
    {
        WeatherListenerGroup weatherListenerGroup = FindGroupByAssetGuid(asset.GUID);
        if (weatherListenerGroup == null)
        {
            return;
        }
        for (int num = weatherListenerGroup.statusListeners.Count - 1; num >= 0; num--)
        {
            WeatherStatusChangedListener weatherStatusChangedListener = weatherListenerGroup.statusListeners[num];
            if (weatherStatusChangedListener != null)
            {
                weatherStatusChangedListener(asset, statusChange);
            }
            else
            {
                weatherListenerGroup.blendAlphaListeners.RemoveAtFast(num);
            }
        }
    }

    internal static void InvokeBlendAlphaChanged(WeatherAssetBase asset, float blendAlpha)
    {
        WeatherListenerGroup weatherListenerGroup = FindGroupByAssetGuid(asset.GUID);
        if (weatherListenerGroup == null)
        {
            return;
        }
        for (int num = weatherListenerGroup.blendAlphaListeners.Count - 1; num >= 0; num--)
        {
            WeatherBlendAlphaChangedListener weatherBlendAlphaChangedListener = weatherListenerGroup.blendAlphaListeners[num];
            if (weatherBlendAlphaChangedListener != null)
            {
                weatherBlendAlphaChangedListener(asset, blendAlpha);
            }
            else
            {
                weatherListenerGroup.blendAlphaListeners.RemoveAtFast(num);
            }
        }
    }

    private static WeatherListenerGroup FindGroupByAssetGuid(Guid assetGuid)
    {
        foreach (WeatherListenerGroup customWeatherListener in customWeatherListeners)
        {
            if (customWeatherListener.assetGuid == assetGuid)
            {
                return customWeatherListener;
            }
        }
        return null;
    }

    private static WeatherListenerGroup FindOrAddGroupByAssetGuid(Guid assetGuid)
    {
        WeatherListenerGroup weatherListenerGroup = FindGroupByAssetGuid(assetGuid);
        if (weatherListenerGroup == null)
        {
            weatherListenerGroup = new WeatherListenerGroup(assetGuid);
            customWeatherListeners.Add(weatherListenerGroup);
        }
        return weatherListenerGroup;
    }

    private static List<CustomWeatherEventHook> FindComponentListenersByAssetGuid(Guid assetGuid)
    {
        return FindGroupByAssetGuid(assetGuid)?.componentListeners;
    }

    private static IEnumerable<CustomWeatherEventHook> EnumerateComponentListeners(Guid assetGuid)
    {
        WeatherListenerGroup group = FindGroupByAssetGuid(assetGuid);
        if (group == null)
        {
            yield break;
        }
        int index = group.componentListeners.Count - 1;
        while (index >= 0)
        {
            CustomWeatherEventHook customWeatherEventHook = group.componentListeners[index];
            if (customWeatherEventHook != null)
            {
                yield return customWeatherEventHook;
            }
            else
            {
                group.componentListeners.RemoveAtFast(index);
            }
            int num = index - 1;
            index = num;
        }
    }

    private static List<CustomWeatherEventHook> FindOrAddComponentListenersByAssetGuid(Guid assetGuid)
    {
        return FindOrAddGroupByAssetGuid(assetGuid).componentListeners;
    }
}
