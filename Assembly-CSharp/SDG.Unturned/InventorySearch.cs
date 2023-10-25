namespace SDG.Unturned;

public class InventorySearch
{
    private byte _page;

    private ItemJar _jar;

    public byte page => _page;

    public ItemJar jar => _jar;

    public ItemAsset GetAsset()
    {
        if (_jar == null)
        {
            return null;
        }
        return _jar.GetAsset();
    }

    public T GetAsset<T>() where T : ItemAsset
    {
        if (_jar == null)
        {
            return null;
        }
        return _jar.GetAsset<T>();
    }

    private void dequipIfEquipped(Player player)
    {
        if (player.equipment.checkSelection(page, jar.x, jar.y))
        {
            player.equipment.dequip();
        }
    }

    /// <summary>
    /// Serverside delete an amount of this item.
    /// </summary>
    /// <returns>Total amount deleted.</returns>
    public uint deleteAmount(Player player, uint desiredAmount)
    {
        dequipIfEquipped(player);
        uint amount = jar.item.amount;
        if (amount > desiredAmount)
        {
            player.inventory.sendUpdateAmount(page, jar.x, jar.y, (byte)(jar.item.amount - desiredAmount));
            return desiredAmount;
        }
        player.inventory.sendUpdateAmount(page, jar.x, jar.y, 0);
        player.crafting.removeItem(page, jar);
        if (page < PlayerInventory.SLOTS)
        {
            player.equipment.sendSlot(page);
        }
        return amount;
    }

    public InventorySearch(byte newPage, ItemJar newJar)
    {
        _page = newPage;
        _jar = newJar;
    }
}
