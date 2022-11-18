using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Admined
{
    internal static void ReadMessage(NetPakReader reader)
    {
        reader.ReadUInt8(out var value);
        SteamPlayer steamPlayer = PlayerTool.findSteamPlayerByChannel(value);
        if (steamPlayer != null)
        {
            steamPlayer.isAdmin = true;
            return;
        }
        UnturnedLog.error("Admined unable to find channel {0}", value);
    }
}
