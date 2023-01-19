using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class WeatherComponentBase : MonoBehaviour
{
    public WeatherAssetBase asset;

    public float globalBlendAlpha;

    public float localVolumeBlendAlpha;

    public bool isWeatherActive;

    public bool hasTickedBlending;

    public bool isFullyTransitionedIn;

    public Color fogColor;

    public float fogDensity;

    public bool overrideFog;

    public bool overrideAtmosphericFog;

    public bool overrideCloudColors;

    public Color cloudColor;

    public Color cloudRimColor;

    public float puddleWaterLevel;

    public float puddleIntensity;

    public float brightnessMultiplier = 1f;

    public float shadowStrengthMultiplier = 1f;

    public float fogBlendExponent = 1f;

    public float cloudBlendExponent = 1f;

    public float windMain;

    public AudioSource ambientAudioSource;

    internal NetId netId;

    public float EffectBlendAlpha => Mathf.Min(globalBlendAlpha, localVolumeBlendAlpha);

    public NetId GetNetId()
    {
        return netId;
    }

    public virtual void InitializeWeather()
    {
        if (!Dedicator.IsDedicatedServer && asset.ambientAudio.isValid)
        {
            StartCoroutine(AsyncLoadAmbientAudio());
        }
    }

    public virtual void UpdateWeather()
    {
    }

    public virtual void UpdateLightingTime(int blendKey, int currentKey, float timeAlpha)
    {
    }

    public virtual void PreDestroyWeather()
    {
    }

    public virtual void OnBeginTransitionIn()
    {
    }

    public virtual void OnEndTransitionIn()
    {
    }

    public virtual void OnBeginTransitionOut()
    {
    }

    public virtual void OnEndTransitionOut()
    {
    }

    public IEnumerable<Player> EnumerateMaskedPlayers()
    {
        foreach (Player item in PlayerTool.EnumeratePlayers())
        {
            if ((item.movement.WeatherMask & asset.volumeMask) != 0)
            {
                yield return item;
            }
        }
    }

    private IEnumerator AsyncLoadAmbientAudio()
    {
        AssetBundleRequest request = asset.ambientAudio.LoadAssetAsync();
        if (request != null)
        {
            yield return request;
            AudioClip audioClip = request.asset as AudioClip;
            if (audioClip != null)
            {
                ambientAudioSource = base.gameObject.AddComponent<AudioSource>();
                ambientAudioSource.loop = true;
                ambientAudioSource.playOnAwake = false;
                ambientAudioSource.volume = 0f;
                ambientAudioSource.spatialBlend = 0f;
                ambientAudioSource.clip = audioClip;
                ambientAudioSource.Play();
            }
        }
    }
}
