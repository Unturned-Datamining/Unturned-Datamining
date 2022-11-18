using Steamworks;

namespace SDG.Unturned;

public class CommandNpcEvent : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer && Provider.hasCheats)
        {
            SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(executorID);
            if (steamPlayer != null && !(steamPlayer.player == null))
            {
                NPCEventManager.broadcastEvent(steamPlayer.player, parameter, shouldReplicate: true);
            }
        }
    }

    public CommandNpcEvent(Local newLocalization)
    {
        localization = newLocalization;
        _command = "NpcEvent";
        _info = string.Empty;
        _help = string.Empty;
    }
}
