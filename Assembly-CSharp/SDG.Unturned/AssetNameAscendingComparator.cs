using System.Collections.Generic;

namespace SDG.Unturned;

public class AssetNameAscendingComparator : IComparer<Asset>
{
    public int Compare(Asset a, Asset b)
    {
        return a.name.CompareTo(b.name);
    }
}
