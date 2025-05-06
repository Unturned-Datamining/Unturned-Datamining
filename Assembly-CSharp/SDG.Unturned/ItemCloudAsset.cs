namespace SDG.Unturned;

public class ItemCloudAsset : ItemAsset
{
    private float _gravity;

    public float gravity => _gravity;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _gravity = p.data.ParseFloat("Gravity");
    }
}
