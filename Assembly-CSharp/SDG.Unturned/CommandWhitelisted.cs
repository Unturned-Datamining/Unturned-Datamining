using Steamworks;

namespace SDG.Unturned;

public class CommandWhitelisted : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer)
        {
            CommandWindow.LogError(localization.format("RunningErrorText"));
            return;
        }
        Provider.isWhitelisted = true;
        CommandWindow.Log(localization.format("WhitelistedText"));
    }

    public CommandWhitelisted(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("WhitelistedCommandText");
        _info = localization.format("WhitelistedInfoText");
        _help = localization.format("WhitelistedHelpText");
    }
}
