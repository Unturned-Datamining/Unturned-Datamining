using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

[CreateAssetMenu(fileName = "OneShotAudioDef", menuName = "Unturned/One Shot Audio Def")]
public class OneShotAudioDefinition : ScriptableObject
{
    public List<AudioClip> clips;

    public float volumeMultiplier = 1f;

    public float minPitch = 0.95f;

    public float maxPitch = 1.05f;

    [NonSerialized]
    private int shuffledClipIndex = -1;

    public AudioClip GetRandomClip()
    {
        if (clips == null)
        {
            return null;
        }
        return clips.Count switch
        {
            0 => null, 
            1 => clips[0], 
            2 => clips[UnityEngine.Random.Range(0, 2)], 
            _ => GetRandomShuffledClip(), 
        };
    }

    private AudioClip GetRandomShuffledClip()
    {
        List<AudioClip> list = clips;
        if (shuffledClipIndex < 0)
        {
            ShuffleClips(list);
            shuffledClipIndex = 0;
        }
        else if (shuffledClipIndex >= list.Count)
        {
            ReshuffleClips(list);
            shuffledClipIndex = 0;
        }
        return list[shuffledClipIndex++];
    }

    private void Swap(List<AudioClip> list, int lhsIndex, int rhsIndex)
    {
        AudioClip value = list[rhsIndex];
        list[rhsIndex] = list[lhsIndex];
        list[lhsIndex] = value;
    }

    private void ShuffleClips(List<AudioClip> list)
    {
        for (int num = list.Count - 1; num > 0; num--)
        {
            int lhsIndex = UnityEngine.Random.Range(0, num + 1);
            Swap(list, lhsIndex, num);
        }
    }

    private void ReshuffleClips(List<AudioClip> list)
    {
        Swap(list, 0, UnityEngine.Random.Range(0, list.Count - 1));
        for (int num = list.Count - 1; num > 1; num--)
        {
            int lhsIndex = UnityEngine.Random.Range(1, num + 1);
            Swap(list, lhsIndex, num);
        }
    }
}
