using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Associates an ID with the instance of the sound being played. This ensures that if Stop() is called
/// on an old handle it will not stop playing the audio if the component has already been recycled.
/// </summary>
internal class PooledAudioSource
{
    public AudioSource component;

    public int sourceId;

    public int playId;

    /// <summary>
    /// True while inactive, false while playing.
    /// </summary>
    public bool isInPool;
}
