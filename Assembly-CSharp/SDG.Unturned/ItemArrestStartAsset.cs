using UnityEngine;

namespace SDG.Unturned;

public class ItemArrestStartAsset : ItemAsset
{
    protected AudioClip _use;

    protected ushort _strength;

    public AudioClip use => _use;

    public ushort strength => _strength;

    public override bool shouldFriendlySentryTargetUser => true;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _use = p.bundle.load<AudioClip>("Use");
        _strength = p.data.ParseUInt16("Strength", 0);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("ArrestStart");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Strength", strength);
    }
}
