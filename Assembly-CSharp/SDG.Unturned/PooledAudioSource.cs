using UnityEngine;

namespace SDG.Unturned;

internal class PooledAudioSource
{
    public AudioSource component;

    public int sourceId;

    public int playId;

    public bool isInPool;
}
