using System.Collections.Generic;

namespace SDG.Unturned;

internal class TreeRedirectorMap
{
    public bool hasAllAssets;

    private Dictionary<ushort, ushort> redirectedIds;

    public TreeRedirectorMap()
    {
        hasAllAssets = true;
        redirectedIds = new Dictionary<ushort, ushort>();
    }

    public ushort redirect(ushort originalId)
    {
        ushort value = 0;
        if (!redirectedIds.TryGetValue(originalId, out value))
        {
            if (Assets.find(EAssetType.RESOURCE, originalId) is ResourceAsset resourceAsset)
            {
                AssetReference<ResourceAsset> holidayRedirect = resourceAsset.getHolidayRedirect();
                if (holidayRedirect.isValid)
                {
                    ResourceAsset resourceAsset2 = holidayRedirect.Find();
                    if (resourceAsset2 != null)
                    {
                        value = resourceAsset2.id;
                    }
                    else if (!Assets.shouldLoadAnyAssets)
                    {
                        UnturnedLog.error("Missing holiday redirect for tree {0}", resourceAsset);
                        hasAllAssets = false;
                    }
                }
                else
                {
                    value = originalId;
                }
            }
            redirectedIds.Add(originalId, value);
        }
        return value;
    }
}
