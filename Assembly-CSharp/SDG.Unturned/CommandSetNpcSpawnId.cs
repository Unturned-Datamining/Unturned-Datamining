using Steamworks;

namespace SDG.Unturned;

public class CommandSetNpcSpawnId : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer && Provider.hasCheats)
        {
            SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(executorID);
            if (steamPlayer != null && !(steamPlayer.player == null))
            {
                steamPlayer.player.quests.npcSpawnId = parameter;
            }
        }
    }

    public CommandSetNpcSpawnId(Local newLocalization)
    {
        localization = newLocalization;
        _command = "SetNpcSpawn";
        _info = string.Empty;
        _help = string.Empty;
    }
}
