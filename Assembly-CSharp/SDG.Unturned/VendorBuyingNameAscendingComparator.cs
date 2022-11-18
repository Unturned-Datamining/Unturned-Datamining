using System.Collections.Generic;

namespace SDG.Unturned;

public class VendorBuyingNameAscendingComparator : IComparer<VendorBuying>
{
    public int Compare(VendorBuying a, VendorBuying b)
    {
        ItemAsset itemAsset = Assets.find(EAssetType.ITEM, a.id) as ItemAsset;
        ItemAsset itemAsset2 = Assets.find(EAssetType.ITEM, b.id) as ItemAsset;
        if (itemAsset == null || itemAsset2 == null)
        {
            return 0;
        }
        return itemAsset.itemName.CompareTo(itemAsset2.itemName);
    }
}
