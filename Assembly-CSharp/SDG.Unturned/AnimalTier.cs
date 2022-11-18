using System.Collections.Generic;

namespace SDG.Unturned;

public class AnimalTier
{
    private List<AnimalSpawn> _table;

    public string name;

    public float chance;

    public List<AnimalSpawn> table => _table;

    public void addAnimal(ushort id)
    {
        if (table.Count == 255)
        {
            return;
        }
        for (byte b = 0; b < table.Count; b = (byte)(b + 1))
        {
            if (table[b].animal == id)
            {
                return;
            }
        }
        table.Add(new AnimalSpawn(id));
    }

    public void removeAnimal(byte index)
    {
        table.RemoveAt(index);
    }

    public AnimalTier(List<AnimalSpawn> newTable, string newName, float newChance)
    {
        _table = newTable;
        name = newName;
        chance = newChance;
    }
}
