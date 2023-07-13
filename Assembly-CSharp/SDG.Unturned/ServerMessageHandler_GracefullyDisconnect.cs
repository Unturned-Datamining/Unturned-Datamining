using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

internal static class ServerMessageHandler_GracefullyDisconnect
{
    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        SteamPlayer steamPlayer = Provider.findPlayer(transportConnection);
        if (steamPlayer == null)
        {
            if ((bool)NetMessages.shouldLogBadMessages)
            {
                UnturnedLog.info($"Ignoring GracefullyDisconnect message from {transportConnection} because there is no associated player");
            }
        }
        else
        {
            UnturnedLog.info($"Removing player {transportConnection} after graceful disconnect message");
            Provider.dismiss(steamPlayer.playerID.steamID);
        }
    }
}
