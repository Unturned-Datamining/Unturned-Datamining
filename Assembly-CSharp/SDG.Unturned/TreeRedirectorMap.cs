using System;
using System.Collections.Generic;

namespace SDG.Unturned;

/// <summary>
/// Caches uint16 ID to ID redirects.
/// </summary>
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
            ResourceAsset resourceAsset = Assets.find(originalId) as ResourceAsset;
            if (!Dedicator.IsDedicatedServer)
            {
                ClientAssetIntegrity.QueueRequest(originalId, resourceAsset, "Tree Holiday Redirect (Original)");
            }
            if (resourceAsset != null)
            {
                AssetReference<ResourceAsset> holidayRedirect = resourceAsset.getHolidayRedirect();
                if (holidayRedirect.isValid)
                {
                    value = holidayRedirect.Find();
                    if (!Dedicator.IsDedicatedServer)
                    {
                        ClientAssetIntegrity.QueueRequest(holidayRedirect.GUID, value, "Tree Holiday Redirect");
                    }
                    if (value == null)
                    {
                        if ((bool)Assets.shouldLoadAnyAssets)
                        {
                            UnturnedLog.error("Missing holiday redirect for tree {0}", resourceAsset);
                        }
                        ClientAssetIntegrity.ServerAddKnownMissingAsset(holidayRedirect.GUID, "Tree Holiday Redirect");
                    }
                }
                else
                {
                    value = resourceAsset;
                }
            }
            else
            {
                ClientAssetIntegrity.ServerAddKnownMissingAsset(originalId, "Tree Holiday Redirect (Original)");
            }
            redirectedIds.Add(originalId, value);
        }
        return value;
    }
}
