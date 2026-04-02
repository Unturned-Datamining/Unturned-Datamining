namespace SDG.Unturned;

/// <summary>
/// Wraps audio source to prevent caller from meddling with it, and to allow the implementation
/// to change in the future if necessary.
/// </summary>
public struct OneShotAudioHandle
{
    private PooledAudioSource audioSource;

    private int playId;

    internal OneShotAudioHandle(PooledAudioSource audioSource)
    {
        this.audioSource = audioSource;
        playId = audioSource.playId;
    }

    public void Stop()
    {
        if (audioSource != null)
        {
            if (!audioSource.isInPool && playId == audioSource.playId)
            {
                AudioSourcePool.Get().StopAndReleaseAudioSource(audioSource);
            }
            audioSource = null;
            playId = 0;
        }
    }
}
