using Steamworks;

namespace SDG.Unturned;

public class CommandToggleNpcCutsceneMode : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer && Provider.hasCheats)
        {
            SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(executorID);
            if (steamPlayer != null && !(steamPlayer.player == null))
            {
                steamPlayer.player.quests.ServerSetCutsceneModeActive(!steamPlayer.player.quests.IsCutsceneModeActive());
            }
        }
    }

    public CommandToggleNpcCutsceneMode(Local newLocalization)
    {
        localization = newLocalization;
        _command = "ToggleNpcCutsceneMode";
        _info = string.Empty;
        _help = string.Empty;
    }
}
