using UnityEngine;

namespace SDG.Unturned;

public class ItemArrestEndAsset : ItemAsset
{
    protected AudioClip _use;

    protected ushort _recover;

    public AudioClip use => _use;

    public ushort recover => _recover;

    public ItemArrestEndAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _use = bundle.load<AudioClip>("Use");
        _recover = data.readUInt16("Recover", 0);
    }
}
