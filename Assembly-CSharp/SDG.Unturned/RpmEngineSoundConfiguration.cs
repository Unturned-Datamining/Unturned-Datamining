namespace SDG.Unturned;

internal class RpmEngineSoundConfiguration : IDatParseable
{
    public float idlePitch;

    public float idleVolume;

    public float maxPitch;

    public float maxVolume;

    public bool TryParse(IDatNode node)
    {
        if (node is IDatDictionary dictionary)
        {
            idlePitch = dictionary.ParseFloat("IdlePitch");
            idleVolume = dictionary.ParseFloat("IdleVolume");
            maxPitch = dictionary.ParseFloat("MaxPitch");
            maxVolume = dictionary.ParseFloat("MaxVolume");
            return true;
        }
        return false;
    }
}
