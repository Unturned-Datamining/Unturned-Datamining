using System.Collections.Generic;

namespace SDG.Unturned;

public class VehicleTier
{
    private List<VehicleSpawn> _table;

    public string name;

    public float chance;

    public List<VehicleSpawn> table => _table;

    public void addVehicle(ushort id)
    {
        if (table.Count == 255)
        {
            return;
        }
        for (byte b = 0; b < table.Count; b++)
        {
            if (table[b].vehicle == id)
            {
                return;
            }
        }
        table.Add(new VehicleSpawn(id));
    }

    public void removeVehicle(byte index)
    {
        table.RemoveAt(index);
    }

    public VehicleTier(List<VehicleSpawn> newTable, string newName, float newChance)
    {
        _table = newTable;
        name = newName;
        chance = newChance;
    }
}
