using System;
using System.Collections.Generic;

namespace SDG.Unturned;

internal class TreeRedirectorMap
{
    private Dictionary<Guid, ResourceAsset> redirectedIds;

    public TreeRedirectorMap()
    {
        redirectedIds = new Dictionary<Guid, ResourceAsset>();
    }

    public ResourceAsset redirect(Guid originalId)
    {
        if (!redirectedIds.TryGetValue(originalId, out var value))
        {
            if (Assets.find(originalId) is ResourceAsset resourceAsset)
            {
                AssetReference<ResourceAsset> holidayRedirect = resourceAsset.getHolidayRedirect();
                if (holidayRedirect.isValid)
                {
                    value = holidayRedirect.Find();
                    if (value == null && (bool)Assets.shouldLoadAnyAssets)
                    {
                        UnturnedLog.error("Missing holiday redirect for tree {0}", resourceAsset);
                    }
                }
                else
                {
                    value = resourceAsset;
                }
            }
            redirectedIds.Add(originalId, value);
        }
        return value;
    }
}
