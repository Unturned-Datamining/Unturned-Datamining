namespace SDG.Unturned;

public struct VolumeAlphaPair<TVolume>
{
    public TVolume volume;

    public float alpha;

    public VolumeAlphaPair(TVolume volume, float alpha)
    {
        this.volume = volume;
        this.alpha = alpha;
    }
}
