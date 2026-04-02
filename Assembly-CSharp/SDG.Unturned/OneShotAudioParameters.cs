using UnityEngine;
using UnityEngine.Audio;

namespace SDG.Unturned;

public struct OneShotAudioParameters
{
    public Vector3 position;

    public AudioClip clip;

    /// <summary>
    /// Optional parent transform to attach the audio source to.
    /// </summary>
    public Transform parent;

    public float volume;

    public float pitch;

    /// <summary>
    /// 0 = 2D, 1 = 3D
    /// </summary>
    public float spatialBlend;

    public AudioRolloffMode rolloffMode;

    public float minDistance;

    public float maxDistance;

    /// <summary>
    /// If true, caller is responsible for returning audio source to the pool.
    /// Antithetical to the OneShot naming (originally referring to AudioSource.PlayOneShot),
    /// but this struct is moreso for requesting an audio source from the pool.
    /// </summary>
    public bool looping;

    public AudioMixerGroup outputAudioMixerGroup;

    public OneShotAudioParameters(Vector3 position, AudioClip clip, Transform parent)
    {
        this.position = position;
        this.clip = clip;
        this.parent = parent;
        volume = 1f;
        pitch = 1f;
        spatialBlend = 1f;
        rolloffMode = AudioRolloffMode.Linear;
        minDistance = 1f;
        maxDistance = 32f;
        looping = false;
        outputAudioMixerGroup = null;
    }

    public OneShotAudioParameters(Vector3 position, AudioClip clip)
        : this(position, clip, null)
    {
    }

    public OneShotAudioParameters(Transform parent, AudioClip clip)
        : this(parent.position, clip, parent)
    {
    }

    public OneShotAudioParameters(Transform parent, AudioReference audioRef)
        : this(parent.position, null, parent)
    {
        clip = audioRef.LoadAudioClip(out volume, out pitch);
    }

    public OneShotAudioParameters(Vector3 position, AudioReference audioRef)
        : this(position, null, null)
    {
        clip = audioRef.LoadAudioClip(out volume, out pitch);
    }

    /// <summary>
    /// 2D audio.
    /// </summary>
    public OneShotAudioParameters(AudioClip clip)
        : this(Vector3.zero, clip, null)
    {
        spatialBlend = 0f;
    }

    /// <summary>
    /// 2D audio.
    /// </summary>
    public OneShotAudioParameters(AudioReference audioRef)
        : this(Vector3.zero, null, null)
    {
        clip = audioRef.LoadAudioClip(out volume, out pitch);
        spatialBlend = 0f;
    }

    public void RandomizeVolume(float min, float max)
    {
        volume *= Random.Range(min, max);
    }

    public void RandomizePitch(float min, float max)
    {
        pitch *= Random.Range(min, max);
    }

    public void SetSpatialBlend2D()
    {
        spatialBlend = 0f;
    }

    public void SetSpatialBlend3D()
    {
        spatialBlend = 1f;
    }

    public void SetLinearRolloff(float min, float max)
    {
        rolloffMode = AudioRolloffMode.Linear;
        minDistance = min;
        maxDistance = max;
    }

    public OneShotAudioHandle Play()
    {
        return AudioSourcePool.Get().Play(ref this);
    }
}
