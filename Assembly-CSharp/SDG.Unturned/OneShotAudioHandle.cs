namespace SDG.Unturned;

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
            if (playId == audioSource.playId)
            {
                AudioSourcePool.Get().StopAndReleaseAudioSource(audioSource);
            }
            audioSource = null;
            playId = 0;
        }
    }
}
