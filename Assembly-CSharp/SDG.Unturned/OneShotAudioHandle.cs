namespace SDG.Unturned;

/// <summary>
/// Wraps audio source to prevent caller from meddling with it, and to allow the implementation
/// to change in the future if necessary.
/// </summary>
public struct OneShotAudioHandle
{
    private PooledAudioSource audioSource;

    private int playId;

    public bool IsValid
    {
        get
        {
            if (audioSource != null && !audioSource.isInPool)
            {
                return playId == audioSource.playId;
            }
            return false;
        }
    }

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
