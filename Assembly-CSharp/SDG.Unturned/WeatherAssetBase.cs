using System;
using UnityEngine;

namespace SDG.Unturned;

public class WeatherAssetBase : Asset
{
    public static readonly AssetReference<WeatherAssetBase> DEFAULT_SNOW = new AssetReference<WeatherAssetBase>("903577da2ecd4f5784b2f7aed8c300c1");

    public static readonly AssetReference<WeatherAssetBase> DEFAULT_RAIN = new AssetReference<WeatherAssetBase>("d73923f4416c43dfa5bc8b6234cf0257");

    public float minLightningInterval;

    public float maxLightningInterval;

    public float lightningTargetRadius;

    /// <summary>
    /// Seconds between weather event starting and reaching full intensity.
    /// </summary>
    public float fadeInDuration { get; protected set; }

    /// <summary>
    /// Seconds between weather event ending and reaching zero intensity.
    /// </summary>
    public float fadeOutDuration { get; protected set; }

    /// <summary>
    /// Sound clip to play. Volume matches the intensity.
    /// </summary>
    public MasterBundleReference<AudioClip> ambientAudio { get; protected set; }

    /// <summary>
    /// Component to spawn for additional weather logic.
    /// </summary>
    public Type componentType { get; protected set; }

    /// <summary>
    /// If per-volume mask AND is non zero the weather will blend in.
    /// </summary>
    public uint volumeMask { get; protected set; }

    public bool hasLightning { get; protected set; }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        fadeInDuration = data.ParseFloat("Fade_In_Duration");
        fadeOutDuration = data.ParseFloat("Fade_Out_Duration");
        ambientAudio = data.ParseStruct<MasterBundleReference<AudioClip>>("Ambient_Audio_Clip");
        componentType = data.ParseType("Component_Type");
        if (componentType == null)
        {
            componentType = typeof(WeatherComponentBase);
        }
        if (data.ContainsKey("Volume_Mask"))
        {
            volumeMask = data.ParseUInt32("Volume_Mask");
        }
        else
        {
            volumeMask = uint.MaxValue;
        }
        hasLightning = data.ParseBool("Has_Lightning");
        if (hasLightning)
        {
            minLightningInterval = Mathf.Max(5f, data.ParseFloat("Min_Lightning_Interval"));
            maxLightningInterval = Mathf.Max(5f, data.ParseFloat("Max_Lightning_Interval"));
            if (data.ContainsKey("Lightning_Target_Radius"))
            {
                lightningTargetRadius = Mathf.Max(0f, data.ParseFloat("Lightning_Target_Radius"));
            }
            else
            {
                lightningTargetRadius = 500f;
            }
        }
    }
}
