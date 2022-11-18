using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Unadmined
{
    internal static void ReadMessage(NetPakReader reader)
    {
        reader.ReadUInt8(out var value);
        SteamPlayer steamPlayer = PlayerTool.findSteamPlayerByChannel(value);
        if (steamPlayer != null)
        {
            steamPlayer.isAdmin = false;
            return;
        }
        UnturnedLog.error("Unadmined unable to find channel {0}", value);
    }
}
