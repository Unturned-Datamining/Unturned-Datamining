using UnityEngine;

namespace SDG.Unturned;

public class ItemArrestStartAsset : ItemAsset
{
    protected AudioClip _use;

    protected ushort _strength;

    public AudioClip use => _use;

    public ushort strength => _strength;

    public override bool shouldFriendlySentryTargetUser => true;

    public ItemArrestStartAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = bundle.load<AudioClip>("Use");
        _strength = data.readUInt16("Strength", 0);
    }
}
