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

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _generate = data.ParseInt32("Generate");
        _destroy = data.ParseInt32("Destroy");
        _drops = new int[data.ParseInt32("Drops")];
        for (int i = 0; i < drops.Length; i++)
        {
            drops[i] = data.ParseInt32("Drop_" + i);
        }
        itemOrigin = data.ParseEnum("Item_Origin", EBoxItemOrigin.Unbox);
        probabilityModel = data.ParseEnum("Probability_Model", EBoxProbabilityModel.Original);
        containsBonusItems = data.ParseBool("Contains_Bonus_Items");
    }
}
