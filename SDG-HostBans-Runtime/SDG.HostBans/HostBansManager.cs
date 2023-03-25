using System;
using System.Collections;
using SDG.NetPak;
using UnityEngine;
using UnityEngine.Networking;

namespace SDG.HostBans;

public class HostBansManager : MonoBehaviour
{
    private HostBanFilters filters;

    private bool isRefreshing;

    private static HostBansManager instance;

    public static HostBansManager Get()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("HostBans");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;
            instance = obj.AddComponent<HostBansManager>();
        }
        return instance;
    }

    public EHostBanFlags MatchBasicDetails(uint ip, ushort port, string name, ulong steamId)
    {
        if (filters != null)
        {
            EHostBanFlags eHostBanFlags = filters.IsSteamIdMatch(steamId);
            if (eHostBanFlags == EHostBanFlags.None)
            {
                eHostBanFlags = filters.IsAddressMatch(ip, port);
                if (eHostBanFlags == EHostBanFlags.None)
                {
                    eHostBanFlags = filters.IsNameMatch(name);
                }
            }
            return eHostBanFlags;
        }
        return EHostBanFlags.None;
    }

    public EHostBanFlags MatchExtendedDetails(string description, string thumbnail)
    {
        if (filters != null)
        {
            EHostBanFlags eHostBanFlags = filters.IsDescriptionMatch(description);
            if (eHostBanFlags == EHostBanFlags.None)
            {
                eHostBanFlags = filters.IsThumbnailMatch(thumbnail);
            }
            return eHostBanFlags;
        }
        return EHostBanFlags.None;
    }

    public void Refresh()
    {
        if (!isRefreshing)
        {
            isRefreshing = true;
            StartCoroutine(RequestFilters());
        }
    }

    private IEnumerator RequestFiltersFromHost(string host)
    {
        string uri = host + "/UnturnedHostBans/filters.bin";
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.timeout = 10;
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error getting Steam matchmaking moderation filters from " + host + ": " + request.error);
            filters = null;
            yield break;
        }
        byte[] data = request.downloadHandler.data;
        try
        {
            NetPakReader netPakReader = new NetPakReader();
            netPakReader.SetBuffer(data);
            filters = new HostBanFilters();
            filters.ReadConfiguration(netPakReader);
        }
        catch (Exception exception)
        {
            filters = null;
            Debug.LogError("Caught exception requesting Steam matchmaking moderation filters from " + host + ":");
            Debug.LogException(exception);
        }
    }

    private IEnumerator RequestFilters()
    {
        yield return RequestFiltersFromHost("https://smartlydressedgames.com");
        if (filters == null)
        {
            yield return RequestFiltersFromHost("http://chaotic-island-paradise.s3-website-us-west-2.amazonaws.com");
        }
        isRefreshing = false;
    }
}
