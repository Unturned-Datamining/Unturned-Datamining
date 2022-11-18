using System;

namespace SDG.Unturned;

public class ItemTankAsset : ItemBarricadeAsset
{
    protected ETankSource _source;

    protected ushort _resource;

    private byte[] resourceState;

    public ETankSource source => _source;

    public ushort resource => _resource;

    public override byte[] getState(EItemOrigin origin)
    {
        byte[] array = new byte[2];
        if (origin == EItemOrigin.ADMIN)
        {
            array[0] = resourceState[0];
            array[1] = resourceState[1];
        }
        return array;
    }

    public ItemTankAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _source = (ETankSource)Enum.Parse(typeof(ETankSource), data.readString("Source"), ignoreCase: true);
        _resource = data.readUInt16("Resource", 0);
        resourceState = BitConverter.GetBytes(resource);
    }
}
