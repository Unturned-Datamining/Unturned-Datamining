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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _capacity = data.ParseUInt16("Capacity", 0);
        _wirerange = data.ParseFloat("Wirerange");
        if (wirerange > PowerTool.MAX_POWER_RANGE + 0.1f)
        {
            float mAX_POWER_RANGE = PowerTool.MAX_POWER_RANGE;
            Assets.reportError(this, "Wirerange is further than the max supported power range of " + mAX_POWER_RANGE);
        }
        _burn = data.ParseFloat("Burn");
    }
}
