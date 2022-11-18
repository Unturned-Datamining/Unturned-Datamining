using Steamworks;

namespace SDG.Unturned;

public class CommandCheats : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer)
        {
            CommandWindow.LogError(localization.format("RunningErrorText"));
            return;
        }
        Provider.hasCheats = true;
        CommandWindow.Log(localization.format("CheatsText"));
    }

    public CommandCheats(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("CheatsCommandText");
        _info = localization.format("CheatsInfoText");
        _help = localization.format("CheatsHelpText");
    }
}
