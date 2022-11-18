using System.Diagnostics;
using SDG.Unturned;

namespace SDG.NetTransport.SteamNetworking;

public abstract class TransportBase_SteamNetworking
{
    [Conditional("LOG_NETTRANSPORT_STEAMNETWORKING")]
    internal static void Log(string format, params object[] args)
    {
        UnturnedLog.info(format, args);
    }
}
