namespace SDG.Unturned;

public class ItemJar
{
    public byte x;

    public byte y;

    public byte rot;

    public byte size_x;

    public byte size_y;

    private Item _item;

    public InteractableItem interactableItem;

    public Item item => _item;

    public ItemAsset GetAsset()
    {
        if (_item == null)
        {
            return null;
        }
        return _item.GetAsset();
    }

    public T GetAsset<T>() where T : ItemAsset
    {
        if (_item == null)
        {
            return null;
        }
        return _item.GetAsset<T>();
    }

    public ItemJar(Item newItem)
    {
        _item = newItem;
        ItemAsset asset = item.GetAsset();
        if (asset != null)
        {
            size_x = asset.size_x;
            size_y = asset.size_y;
        }
    }

    public ItemJar(byte new_x, byte new_y, byte newRot, Item newItem)
    {
        x = new_x;
        y = new_y;
        rot = newRot;
        _item = newItem;
        ItemAsset asset = item.GetAsset();
        if (asset != null)
        {
            size_x = asset.size_x;
            size_y = asset.size_y;
        }
    }
}
