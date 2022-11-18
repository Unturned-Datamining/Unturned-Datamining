namespace SDG.Unturned;

public class PlayerCaller : SteamCaller
{
    protected Player _player;

    internal NetId _netId;

    public Player player => _player;

    public NetId GetNetId()
    {
        return _netId;
    }

    internal void AssignNetId(NetId netId)
    {
        _netId = netId;
        NetIdRegistry.Assign(netId, this);
    }

    internal void ReleaseNetId()
    {
        NetIdRegistry.Release(_netId);
        _netId.Clear();
    }

    private void Awake()
    {
        _channel = GetComponent<SteamChannel>();
        _player = GetComponent<Player>();
    }
}
