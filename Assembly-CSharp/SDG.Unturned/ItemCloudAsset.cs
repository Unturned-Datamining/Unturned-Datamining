namespace SDG.Unturned;

public class ItemCloudAsset : ItemAsset
{
    private float _gravity;

    public float gravity => _gravity;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _gravity = data.ParseFloat("Gravity");
    }
}
