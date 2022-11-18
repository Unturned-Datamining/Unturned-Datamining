using System.Collections.Generic;

namespace SDG.Unturned;

public class ZombieSlot
{
    private List<ZombieCloth> _table;

    public float chance;

    public List<ZombieCloth> table => _table;

    public void addCloth(ushort id)
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
        table.Add(new ZombieCloth(id));
    }

    public void removeCloth(byte index)
    {
        table.RemoveAt(index);
    }

    public ZombieSlot(float newChance, List<ZombieCloth> newTable)
    {
        _table = newTable;
        chance = newChance;
    }
}
