using System.Collections.Generic;

namespace SDG.Unturned;

public class ItemTier
{
    private List<ItemSpawn> _table;

    public string name;

    public float chance;

    public List<ItemSpawn> table => _table;

    public void addItem(ushort id)
    {
        if (table.Count == 255)
        {
            return;
        }
        for (byte b = 0; b < table.Count; b = (byte)(b + 1))
        {
            if (table[b].item == id)
            {
                return;
            }
        }
        table.Add(new ItemSpawn(id));
    }

    public void removeItem(byte index)
    {
        table.RemoveAt(index);
    }

    public ItemTier(List<ItemSpawn> newTable, string newName, float newChance)
    {
        _table = newTable;
        name = newName;
        chance = newChance;
    }
}
