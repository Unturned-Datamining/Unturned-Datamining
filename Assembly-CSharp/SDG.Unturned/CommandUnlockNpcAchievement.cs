using Steamworks;

namespace SDG.Unturned;

public class CommandUnlockNpcAchievement : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer && Provider.hasCheats)
        {
            SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(executorID);
            if (steamPlayer != null && !(steamPlayer.player == null))
            {
                steamPlayer.player.sendAchievementUnlocked(parameter);
            }
        }
    }

    public CommandUnlockNpcAchievement(Local newLocalization)
    {
        localization = newLocalization;
        _command = "UnlockNpcAchievement";
        _info = string.Empty;
        _help = string.Empty;
    }
}
