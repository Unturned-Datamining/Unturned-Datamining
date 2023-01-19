using Steamworks;

namespace SDG.Unturned;

public class CommandGameMode : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Dedicator.IsDedicatedServer)
        {
            if (Provider.isServer)
            {
                CommandWindow.LogError(localization.format("RunningErrorText"));
            }
            else
            {
                CommandWindow.Log(localization.format("GameModeText", parameter));
            }
        }
    }

    public CommandGameMode(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("GameModeCommandText");
        _info = localization.format("GameModeInfoText");
        _help = localization.format("GameModeHelpText");
    }
}
