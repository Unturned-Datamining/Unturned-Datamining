using UnityEngine;

namespace SDG.Unturned;

public class ItemArrestStartAsset : ItemAsset
{
    protected AudioClip _use;

    protected ushort _strength;

    public AudioClip use => _use;

    public ushort strength => _strength;

    public override bool shouldFriendlySentryTargetUser => true;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _use = bundle.load<AudioClip>("Use");
        _strength = data.ParseUInt16("Strength", 0);
    }
}
