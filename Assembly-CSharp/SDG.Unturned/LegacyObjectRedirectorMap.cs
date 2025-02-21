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
            ObjectAsset objectAsset = Assets.find(originalGUID) as ObjectAsset;
            if (!Dedicator.IsDedicatedServer)
            {
                ClientAssetIntegrity.QueueRequest(originalGUID, objectAsset, "Object Holiday Redirect (Original)");
            }
            if (objectAsset != null)
            {
                AssetReference<ObjectAsset> holidayRedirect = objectAsset.getHolidayRedirect();
                if (holidayRedirect.isValid)
                {
                    value = holidayRedirect.Find();
                    if (!Dedicator.IsDedicatedServer)
                    {
                        ClientAssetIntegrity.QueueRequest(holidayRedirect.GUID, value, "Object Holiday Redirect");
                    }
                    if (value == null)
                    {
                        if ((bool)Assets.shouldLoadAnyAssets)
                        {
                            UnturnedLog.error("Missing holiday redirect for object {0}", objectAsset);
                        }
                        ClientAssetIntegrity.ServerAddKnownMissingAsset(holidayRedirect.GUID, "Object Holiday Redirect");
                    }
                }
                else
                {
                    value = objectAsset;
                }
            }
            else
            {
                ClientAssetIntegrity.ServerAddKnownMissingAsset(originalGUID, "Object Holiday Redirect (Original)");
            }
            redirectedIds.Add(originalGUID, value);
        }
        return value;
    }
}
