using System.Collections.Generic;

namespace SDG.Unturned;

public class SpawnTableWeightAscendingComparator : IComparer<SpawnTable>
{
    public int Compare(SpawnTable a, SpawnTable b)
    {
        return b.weight - a.weight;
    }
}
