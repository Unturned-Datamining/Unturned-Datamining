namespace SDG.Unturned;

public class ItemIconInfo
{
    public ushort id;

    public ushort skin;

    public byte quality;

    public byte[] state;

    public ItemAsset itemAsset;

    public SkinAsset skinAsset;

    public string tags;

    public string dynamic_props;

    public int x;

    public int y;

    public bool scale;

    public bool readableOnCPU;

    public ItemIconReady callback;
}
