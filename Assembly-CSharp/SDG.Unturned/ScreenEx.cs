using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Extensions to the built-in Screen class.
/// We have run into multiple problems with the Screen.resolutions property over the years, so this class aims to
/// protect against bad data.
/// </summary>
public static class ScreenEx
{
    private static Resolution[] cachedResolutions;

    private static CommandLineFlag clNoUnityResolutions = new CommandLineFlag(defaultValue: false, "-NoUnityResolutions");

    public static float GetCurrentAspectRatio()
    {
        Resolution currentResolution = Screen.currentResolution;
        if (currentResolution.height > 0)
        {
            return (float)currentResolution.width / (float)currentResolution.height;
        }
        return 1f;
    }

    public static Resolution[] GetRecommendedResolutions()
    {
        if (cachedResolutions == null)
        {
            CacheResolutions();
        }
        return cachedResolutions;
    }

    public static Resolution GetHighestRecommendedResolution()
    {
        Resolution[] recommendedResolutions = GetRecommendedResolutions();
        if (recommendedResolutions.Length != 0)
        {
            return recommendedResolutions[^1];
        }
        return Screen.currentResolution;
    }

    private static void CacheResolutions()
    {
        List<Resolution> list = new List<Resolution>();
        if ((bool)clNoUnityResolutions)
        {
            list.Add(Screen.currentResolution);
        }
        else
        {
            Resolution[] resolutions = Screen.resolutions;
            int i;
            if (resolutions.Length > 200)
            {
                i = resolutions.Length - 200;
                UnturnedLog.warn("Unity returned {0} recommended resolutions, clamping to {1}", resolutions.Length, 200);
            }
            else
            {
                i = 0;
            }
            for (; i < resolutions.Length; i++)
            {
                Resolution item = resolutions[i];
                if (item.width >= 640 && item.height >= 480)
                {
                    list.Add(item);
                }
            }
        }
        list.Sort(delegate(Resolution lhs, Resolution rhs)
        {
            int num = lhs.width.CompareTo(rhs.width);
            if (num == 0)
            {
                int num2 = lhs.height.CompareTo(rhs.height);
                if (num2 == 0)
                {
                    return lhs.refreshRate.CompareTo(rhs.refreshRate);
                }
                return num2;
            }
            return num;
        });
        cachedResolutions = list.ToArray();
    }
}
