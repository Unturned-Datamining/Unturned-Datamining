using System;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class WeatherAssetBase : Asset
{
    public static readonly AssetReference<WeatherAssetBase> DEFAULT_SNOW = new AssetReference<WeatherAssetBase>("903577da2ecd4f5784b2f7aed8c300c1");

    public static readonly AssetReference<WeatherAssetBase> DEFAULT_RAIN = new AssetReference<WeatherAssetBase>("d73923f4416c43dfa5bc8b6234cf0257");

    public float minLightningInterval;

    public float maxLightningInterval;

    public float lightningTargetRadius;

    public float fadeInDuration { get; protected set; }

    public float fadeOutDuration { get; protected set; }

    public MasterBundleReference<AudioClip> ambientAudio { get; protected set; }

    public Type componentType { get; protected set; }

    public uint volumeMask { get; protected set; }

    public bool hasLightning { get; protected set; }

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        fadeInDuration = reader.readValue<float>("Fade_In_Duration");
        fadeOutDuration = reader.readValue<float>("Fade_Out_Duration");
        ambientAudio = reader.readValue<MasterBundleReference<AudioClip>>("Ambient_Audio_Clip");
        componentType = reader.readValue<Type>("Component_Type");
        if (componentType == null)
        {
            componentType = typeof(WeatherComponentBase);
        }
        if (reader.containsKey("Volume_Mask"))
        {
            volumeMask = reader.readValue<uint>("Volume_Mask");
        }
        else
        {
            volumeMask = uint.MaxValue;
        }
        hasLightning = reader.readValue<bool>("Has_Lightning");
        if (hasLightning)
        {
            minLightningInterval = Mathf.Max(5f, reader.readValue<float>("Min_Lightning_Interval"));
            maxLightningInterval = Mathf.Max(5f, reader.readValue<float>("Max_Lightning_Interval"));
            if (reader.containsKey("Lightning_Target_Radius"))
            {
                lightningTargetRadius = Mathf.Max(0f, reader.readValue<float>("Lightning_Target_Radius"));
            }
            else
            {
                lightningTargetRadius = 500f;
            }
        }
    }

    public WeatherAssetBase(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
