using UnityEngine;

namespace SDG.Unturned;

public struct OneShotAudioParameters
{
    public Vector3 position;

    public AudioClip clip;

    public Transform parent;

    public float volume;

    public float pitch;

    public float spatialBlend;

    public AudioRolloffMode rolloffMode;

    public float minDistance;

    public float maxDistance;

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
    }

    public OneShotAudioParameters(Vector3 position, AudioClip clip)
        : this(position, clip, null)
    {
    }

    public OneShotAudioParameters(Transform parent, AudioClip clip)
        : this(parent.position, clip, parent)
    {
    }

    public OneShotAudioParameters(AudioClip clip)
        : this(Vector3.zero, clip, null)
    {
        spatialBlend = 0f;
    }

    public void RandomizeVolume(float min, float max)
    {
        volume = Random.Range(min, max);
    }

    public void RandomizePitch(float min, float max)
    {
        pitch = Random.Range(min, max);
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

    public void Play()
    {
        AudioSourcePool.Get().Play(ref this);
    }
}
