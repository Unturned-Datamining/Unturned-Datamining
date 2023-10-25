using System;
using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Caches uint16 ID to ID redirects.
/// </summary>
internal class LegacyObjectRedirectorMap
{
    private Dictionary<Guid, ObjectAsset> redirectedIds;

    public LegacyObjectRedirectorMap()
    {
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
