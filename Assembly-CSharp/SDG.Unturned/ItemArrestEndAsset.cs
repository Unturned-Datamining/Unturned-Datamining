using UnityEngine;

namespace SDG.Unturned;

public class ItemArrestEndAsset : ItemAsset
{
    protected AudioClip _use;

    protected ushort _recover;

    public AudioClip use => _use;

    public ushort recover => _recover;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _use = bundle.load<AudioClip>("Use");
        _recover = data.ParseUInt16("Recover", 0);
    }
}
