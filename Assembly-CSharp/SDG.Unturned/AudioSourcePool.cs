using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class AudioSourcePool : MonoBehaviour
{
    private WaitForSeconds waitForOneSecond = new WaitForSeconds(1f);

    private static AudioSourcePool instance;

    private List<AudioSource> availableComponents = new List<AudioSource>();

    public static AudioSourcePool Get()
    {
        if (instance == null)
        {
            instance = new GameObject("AudioSourcePool").AddComponent<AudioSourcePool>();
        }
        return instance;
    }

    internal void Play(ref OneShotAudioParameters parameters)
    {
        if (!(parameters.clip == null) && !Dedicator.IsDedicatedServer && !(MainCamera.instance == null) && (!MathfEx.IsNearlyEqual(parameters.spatialBlend, 1f, 0.001f) || !((MainCamera.instance.transform.position - parameters.position).sqrMagnitude > MathfEx.Square(parameters.maxDistance))))
        {
            int count = availableComponents.Count;
            AudioSource audioSource;
            if (count > 0)
            {
                int index = count - 1;
                audioSource = availableComponents[index];
                availableComponents.RemoveAt(index);
            }
            else
            {
                audioSource = new GameObject("PooledAudioSource").AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
            Transform obj = audioSource.transform;
            obj.parent = parameters.parent;
            obj.localScale = Vector3.one;
            obj.position = parameters.position;
            audioSource.clip = parameters.clip;
            audioSource.volume = parameters.volume;
            audioSource.pitch = parameters.pitch;
            audioSource.spatialBlend = parameters.spatialBlend;
            audioSource.rolloffMode = parameters.rolloffMode;
            audioSource.minDistance = parameters.minDistance;
            audioSource.maxDistance = parameters.maxDistance;
            audioSource.Play();
            StartCoroutine(PlayCoroutine(audioSource, parameters.clip.length / parameters.pitch + 0.1f));
        }
    }

    private IEnumerator PlayCoroutine(AudioSource component, float duration)
    {
        if (duration < 1f)
        {
            yield return waitForOneSecond;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
        if (component != null)
        {
            component.transform.parent = null;
            availableComponents.Add(component);
        }
    }
}
