using System;
using System.Collections.Generic;

namespace SDG.Unturned;

internal class LegacyObjectRedirectorMap
{
    public bool hasAllAssets;

    private Dictionary<Guid, ObjectAsset> redirectedIds;

    public LegacyObjectRedirectorMap()
    {
        hasAllAssets = true;
        redirectedIds = new Dictionary<Guid, ObjectAsset>();
    }

    public ObjectAsset redirect(Guid originalGUID)
    {
        ObjectAsset value = null;
        if (!redirectedIds.TryGetValue(originalGUID, out value))
        {
            if (Assets.find(originalGUID) is ObjectAsset objectAsset)
            {
                AssetReference<ObjectAsset> holidayRedirect = objectAsset.getHolidayRedirect();
                if (holidayRedirect.isValid)
                {
                    value = holidayRedirect.Find();
                    if (value == null && (bool)Assets.shouldLoadAnyAssets)
                    {
                        UnturnedLog.error("Missing holiday redirect for object {0}", objectAsset);
                        hasAllAssets = false;
                    }
                }
                else
                {
                    value = objectAsset;
                }
            }
            redirectedIds.Add(originalGUID, value);
        }
        return value;
    }
}
