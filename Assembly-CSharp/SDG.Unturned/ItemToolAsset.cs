using UnityEngine;

namespace SDG.Unturned;

public class ItemToolAsset : ItemAsset
{
    protected AudioClip _use;

    public AudioClip use => _use;

    public override bool shouldFriendlySentryTargetUser => base.useableType != typeof(UseableWalkieTalkie);

    public override bool canBeUsedInSafezone(SafezoneNode safezone, bool byAdmin)
    {
        if (byAdmin)
        {
            return true;
        }
        if (base.useableType == typeof(UseableCarjack))
        {
            return true;
        }
        return base.canBeUsedInSafezone(safezone, byAdmin);
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _use = bundle.load<AudioClip>("Use");
    }
}
