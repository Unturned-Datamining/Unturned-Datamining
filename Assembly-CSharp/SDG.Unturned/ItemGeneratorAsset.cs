namespace SDG.Unturned;

public class ItemGeneratorAsset : ItemBarricadeAsset
{
    protected ushort _capacity;

    protected float _wirerange;

    protected float _burn;

    public ushort capacity => _capacity;

    public float wirerange => _wirerange;

    public float burn => _burn;

    public override byte[] getState(EItemOrigin origin)
    {
        return new byte[3];
    }

    public ItemGeneratorAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _capacity = data.readUInt16("Capacity", 0);
        _wirerange = data.readSingle("Wirerange");
        if (wirerange > PowerTool.MAX_POWER_RANGE + 0.1f)
        {
            Assets.reportError(this, "Wirerange is further than the max supported power range of " + PowerTool.MAX_POWER_RANGE);
        }
        _burn = data.readSingle("Burn");
    }
}
