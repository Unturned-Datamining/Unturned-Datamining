namespace SDG.Unturned;

internal class RpmEngineSoundConfiguration : IDatParseable
{
    public float idlePitch;

    public float idleVolume;

    public float maxPitch;

    public float maxVolume;

    public bool TryParse(IDatNode node)
    {
        if (node is DatDictionary datDictionary)
        {
            idlePitch = datDictionary.ParseFloat("IdlePitch");
            idleVolume = datDictionary.ParseFloat("IdleVolume");
            maxPitch = datDictionary.ParseFloat("MaxPitch");
            maxVolume = datDictionary.ParseFloat("MaxVolume");
            return true;
        }
        return false;
    }
}
