using SDG.NetPak;
using SDG.NetTransport;

namespace SDG.Unturned;

internal static class ServerMessageHandler_GracefullyDisconnect
{
    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        SteamPlayer steamPlayer = Provider.findPlayer(transportConnection);
        if (steamPlayer != null)
        {
            UnturnedLog.info($"Removing player {transportConnection} after graceful disconnect message");
            Provider.dismiss(steamPlayer.playerID.steamID);
        }
    }
}
