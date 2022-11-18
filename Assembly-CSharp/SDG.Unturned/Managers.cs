using UnityEngine;

namespace SDG.Unturned;

public class Managers : MonoBehaviour
{
    private static bool _isInitialized;

    public static bool isInitialized => _isInitialized;

    private void Awake()
    {
        if (isInitialized)
        {
            Object.Destroy(base.gameObject);
            return;
        }
        _isInitialized = true;
        Object.DontDestroyOnLoad(base.gameObject);
        GetComponent<SteamChannel>().setup();
    }
}
