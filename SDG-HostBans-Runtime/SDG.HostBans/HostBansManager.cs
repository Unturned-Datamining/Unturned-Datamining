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

    private bool _hasReceivedAnyResponse;

    private int retryIndex;

    private float[] retryIntervals = new float[4] { 30f, 300f, 600f, 1200f };

    private static HostBansManager instance;

    public bool HasReceivedAnyResponse => _hasReceivedAnyResponse;

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
            retryIndex = 0;
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
            _hasReceivedAnyResponse = true;
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
        yield return RequestFiltersWithRetries();
        isRefreshing = false;
    }

    private IEnumerator RequestFiltersFromAllHosts()
    {
        yield return RequestFiltersFromHost("https://smartlydressedgames.com");
        if (filters == null)
        {
            yield return RequestFiltersFromHost("http://egg-calculate-remarkable.s3-website-us-west-2.amazonaws.com");
        }
    }

    private IEnumerator RequestFiltersWithRetries()
    {
        while (retryIndex < retryIntervals.Length)
        {
            yield return RequestFiltersFromAllHosts();
            if (filters != null)
            {
                break;
            }
            float num = retryIntervals[retryIndex];
            retryIndex++;
            Debug.Log($"Will retry getting Steam matchmaking moderation filters in {num} seconds");
            yield return new WaitForSecondsRealtime(num);
            Debug.Log("Retrying getting Steam matchmaking moderation filters");
        }
        if (filters == null)
        {
            Debug.LogError("Failed to get Steam matchmaking moderations filters, no longer retrying");
        }
    }
}
