namespace SDG.Unturned;

public class ItemSpawn
{
    private ushort _item;

    public ushort item => _item;

    public ItemSpawn(ushort newItem)
    {
        _item = newItem;
    }
}
