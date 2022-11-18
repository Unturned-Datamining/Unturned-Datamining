using UnityEngine;

namespace SDG.Unturned;

public class SteamCaller : MonoBehaviour
{
    protected SteamChannel _channel;

    public SteamChannel channel => _channel;

    private void Awake()
    {
        _channel = GetComponent<SteamChannel>();
    }
}
