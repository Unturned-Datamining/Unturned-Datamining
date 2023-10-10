using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace SDG.Unturned;

public static class GrantPackagePromo
{
    private static float RequestRealtime;

    private static bool CheckRateLimit()
    {
        float realtimeSinceStartup = Time.realtimeSinceStartup;
        if (realtimeSinceStartup - RequestRealtime < 1f)
        {
            return false;
        }
        RequestRealtime = realtimeSinceStartup;
        return true;
    }

    public static bool IsEligible()
    {
        if (Provider.statusData == null || Provider.statusData.Game == null)
        {
            return false;
        }
        if (Provider.statusData.Game.GrantPackageIDs.Length < 1 || string.IsNullOrEmpty(Provider.statusData.Game.GrantPackageURL))
        {
            return false;
        }
        if (SteamApps.BIsSubscribedApp(new AppId_t(427840u)))
        {
            return false;
        }
        int[] grantPackageIDs = Provider.statusData.Game.GrantPackageIDs;
        foreach (int item in grantPackageIDs)
        {
            if (Provider.provider.economyService.getInventoryPackage(item) != 0)
            {
                return true;
            }
        }
        return false;
    }

    public static void SendRequest()
    {
        if (!CheckRateLimit() || !IsEligible())
        {
            return;
        }
        if (!Provider.allowWebRequests)
        {
            UnturnedLog.warn("Not granting package promo because web requests are disabled");
            return;
        }
        string grantPackageURL = Provider.statusData.Game.GrantPackageURL;
        grantPackageURL = string.Format(grantPackageURL, SteamUser.GetSteamID().m_SteamID);
        UnturnedLog.info("Grant package promo requested: '{0}'", grantPackageURL);
        using UnityWebRequest unityWebRequest = UnityWebRequest.Get(grantPackageURL);
        unityWebRequest.timeout = 15;
        UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = unityWebRequest.SendWebRequest();
        while (!unityWebRequestAsyncOperation.isDone)
        {
        }
        if (unityWebRequest.result != UnityWebRequest.Result.Success)
        {
            UnturnedLog.warn("Grand package promo error: {0}", unityWebRequest.error);
        }
        else
        {
            UnturnedLog.info("Response: '{0}'", unityWebRequest.downloadHandler.text);
        }
    }

    static GrantPackagePromo()
    {
        RequestRealtime = -999f;
    }
}
