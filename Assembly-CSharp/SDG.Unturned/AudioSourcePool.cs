using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class AudioSourcePool : MonoBehaviour
{
    private WaitForSeconds waitForOneSecond = new WaitForSeconds(1f);

    private static AudioSourcePool instance;

    private List<PooledAudioSource> availableComponents = new List<PooledAudioSource>();

    private int nextPlayId = 1;

    private int nextSourceId = 1;

    public static AudioSourcePool Get()
    {
        if (instance == null)
        {
            instance = new GameObject("AudioSourcePool").AddComponent<AudioSourcePool>();
        }
        return instance;
    }

    internal OneShotAudioHandle Play(ref OneShotAudioParameters parameters)
    {
        if (parameters.clip == null || Dedicator.IsDedicatedServer || MainCamera.instance == null)
        {
            return default(OneShotAudioHandle);
        }
        if (MathfEx.IsNearlyEqual(parameters.spatialBlend, 1f, 0.001f) && (MainCamera.instance.transform.position - parameters.position).sqrMagnitude > MathfEx.Square(parameters.maxDistance))
        {
            return default(OneShotAudioHandle);
        }
        int count = availableComponents.Count;
        PooledAudioSource pooledAudioSource;
        if (count > 0)
        {
            int index = count - 1;
            pooledAudioSource = availableComponents[index];
            availableComponents.RemoveAt(index);
            pooledAudioSource.component.enabled = true;
        }
        else
        {
            pooledAudioSource = new PooledAudioSource();
            pooledAudioSource.sourceId = nextSourceId;
            nextSourceId++;
            GameObject gameObject = new GameObject("PooledAudioSource");
            pooledAudioSource.component = gameObject.AddComponent<AudioSource>();
            pooledAudioSource.component.playOnAwake = false;
            pooledAudioSource.component.outputAudioMixerGroup = UnturnedAudioMixer.GetDefaultGroup();
        }
        Transform obj = pooledAudioSource.component.transform;
        obj.parent = parameters.parent;
        obj.localScale = Vector3.one;
        obj.position = parameters.position;
        pooledAudioSource.component.clip = parameters.clip;
        pooledAudioSource.component.volume = parameters.volume;
        pooledAudioSource.component.pitch = parameters.pitch;
        pooledAudioSource.component.spatialBlend = parameters.spatialBlend;
        pooledAudioSource.component.rolloffMode = parameters.rolloffMode;
        pooledAudioSource.component.minDistance = parameters.minDistance;
        pooledAudioSource.component.maxDistance = parameters.maxDistance;
        pooledAudioSource.component.Play();
        pooledAudioSource.isInPool = false;
        pooledAudioSource.playId = nextPlayId;
        nextPlayId++;
        StartCoroutine(PlayCoroutine(pooledAudioSource, pooledAudioSource.playId, parameters.clip.length / parameters.pitch + 0.1f));
        return new OneShotAudioHandle(pooledAudioSource);
    }

    internal void StopAndReleaseAudioSource(PooledAudioSource audioSource)
    {
        if (audioSource.component != null)
        {
            audioSource.component.enabled = false;
            if (audioSource.component.transform.parent != null)
            {
                audioSource.component.transform.parent = null;
            }
            availableComponents.Add(audioSource);
            audioSource.isInPool = true;
            audioSource.playId = 0;
        }
    }

    /// <summary>
    /// Timer needs playId as well in case source has been recycled by the time duration expires.
    /// </summary>
    private IEnumerator PlayCoroutine(PooledAudioSource audioSource, int playId, float duration)
    {
        if (duration < 1f)
        {
            yield return waitForOneSecond;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
        if (!audioSource.isInPool && audioSource.playId == playId)
        {
            StopAndReleaseAudioSource(audioSource);
        }
    }

    private void OnEnable()
    {
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Combine(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
    }

    private void OnDisable()
    {
        CommandLogMemoryUsage.OnExecuted = (Action<List<string>>)Delegate.Remove(CommandLogMemoryUsage.OnExecuted, new Action<List<string>>(OnLogMemoryUsage));
    }

    private void OnLogMemoryUsage(List<string> results)
    {
        results.Add($"Audio source pool size: {availableComponents.Count}");
    }
}
