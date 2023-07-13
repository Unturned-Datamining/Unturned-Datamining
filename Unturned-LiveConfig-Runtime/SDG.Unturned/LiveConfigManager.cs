using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SDG.Unturned;

public class LiveConfigManager : MonoBehaviour
{
    public LiveConfigData config = new LiveConfigData();

    public bool wasPopulated;

    private bool isRefreshing;

    private static LiveConfigManager instance;

    public event Action OnConfigRefreshed;

    public static LiveConfigManager Get()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("LiveConfig");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave;
            instance = obj.AddComponent<LiveConfigManager>();
        }
        return instance;
    }

    public void Refresh()
    {
        if (!isRefreshing)
        {
            isRefreshing = true;
            StartCoroutine(RequestConfig());
        }
    }

    private IEnumerator RequestConfig()
    {
        string uri = "https://smartlydressedgames.com/UnturnedLiveConfig.dat";
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.timeout = 10;
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error getting live config: " + request.error);
        }
        else
        {
            try
            {
                DatParser datParser = new DatParser();
                DatDictionary data = datParser.Parse(request.downloadHandler.data);
                if (datParser.HasError)
                {
                    Debug.LogError("Error parsing live config: \"" + datParser.ErrorMessage + "\"");
                }
                config = new LiveConfigData();
                config.Parse(data);
                wasPopulated = true;
            }
            catch (Exception exception)
            {
                Debug.LogError("Caught exception requesting live config:");
                Debug.LogException(exception);
            }
        }
        isRefreshing = false;
        this.OnConfigRefreshed?.Invoke();
    }
}
