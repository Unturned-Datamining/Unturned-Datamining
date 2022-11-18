namespace SDG.Unturned;

public class ItemBoxAsset : ItemAsset
{
    protected int _generate;

    protected int _destroy;

    protected int[] _drops;

    public int generate => _generate;

    public int destroy => _destroy;

    public int[] drops => _drops;

    public EBoxItemOrigin itemOrigin { get; protected set; }

    public EBoxProbabilityModel probabilityModel { get; protected set; }

    public bool containsBonusItems { get; protected set; }

    public ItemBoxAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _generate = data.readInt32("Generate");
        _destroy = data.readInt32("Destroy");
        _drops = new int[data.readInt32("Drops")];
        for (int i = 0; i < drops.Length; i++)
        {
            drops[i] = data.readInt32("Drop_" + i);
        }
        itemOrigin = data.readEnum("Item_Origin", EBoxItemOrigin.Unbox);
        probabilityModel = data.readEnum("Probability_Model", EBoxProbabilityModel.Original);
        containsBonusItems = data.readBoolean("Contains_Bonus_Items");
    }
}
