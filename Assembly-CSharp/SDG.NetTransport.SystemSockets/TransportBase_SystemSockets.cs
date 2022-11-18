using System.Diagnostics;
using SDG.Unturned;

namespace SDG.NetTransport.SystemSockets;

public abstract class TransportBase_SystemSockets
{
    [Conditional("LOG_NETTRANSPORT_SYSTEMSOCKETS")]
    internal static void Log(string format, params object[] args)
    {
        UnturnedLog.info(format, args);
    }
}
