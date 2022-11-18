using System.Collections.Generic;

namespace SDG.Unturned;

public class VendorSellingNameAscendingComparator : IComparer<VendorSellingBase>
{
    public int Compare(VendorSellingBase a, VendorSellingBase b)
    {
        string displayName = a.displayName;
        string displayName2 = b.displayName;
        if (displayName == null || displayName2 == null)
        {
            return 0;
        }
        return displayName.CompareTo(displayName2);
    }
}
