namespace SDG.Unturned;

/// <summary>
/// Nearly identical to InventorySearch aside from:
/// • Struct instead of class to improve garbage collection performance in pooled lists.
/// • More understandable name.
/// • Provides reference to Items holding "Jar." Longer-term this should be preferred over the "Page" property.
/// </summary>
public struct PlayerInventorySearchResultV2
{
    private Items _jarOwner;

    private ItemJar _jar;

    public byte Page => _jarOwner?.page ?? 0;

    public Items JarOwner => _jarOwner;

    public ItemJar Jar => _jar;

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

    public void DequipIfEquipped(Player player)
    {
        if (player.equipment.checkSelection(_jarOwner.page, _jar.x, _jar.y))
        {
            player.equipment.dequip();
        }
    }

    public override string ToString()
    {
        return $"(Page: {Page} X: {_jar?.x} Y: {_jar?.y} Item: {GetAsset()})";
    }

    public void Delete(Player player)
    {
        DequipIfEquipped(player);
        player.crafting.removeItem(_jarOwner.page, _jar);
        if (_jarOwner.page < PlayerInventory.SLOTS)
        {
            player.equipment.sendSlot(_jarOwner.page);
        }
    }

    /// <summary>
    /// Serverside delete an amount of this item.
    /// </summary>
    /// <param name="alwaysDeleteAtZeroAmount">False for crafting where original item can be kept, true when selling to vendors.</param>
    /// <returns>Total amount deleted.</returns>
    public uint DeleteAmount(Player player, uint desiredAmount, bool alwaysDeleteAtZeroAmount = true)
    {
        DequipIfEquipped(player);
        uint amount = _jar.item.amount;
        if (amount > desiredAmount)
        {
            player.inventory.sendUpdateAmount(_jarOwner.page, _jar.x, _jar.y, (byte)(_jar.item.amount - desiredAmount));
            return desiredAmount;
        }
        player.inventory.sendUpdateAmount(_jarOwner.page, _jar.x, _jar.y, 0);
        if (alwaysDeleteAtZeroAmount || (GetAsset()?.ShouldDeleteAtZeroAmount ?? true))
        {
            player.crafting.removeItem(_jarOwner.page, _jar);
            if (_jarOwner.page < PlayerInventory.SLOTS)
            {
                player.equipment.sendSlot(_jarOwner.page);
            }
        }
        return amount;
    }

    public PlayerInventorySearchResultV2(Items newJarOwner, ItemJar newJar)
    {
        _jarOwner = newJarOwner;
        _jar = newJar;
    }
}
