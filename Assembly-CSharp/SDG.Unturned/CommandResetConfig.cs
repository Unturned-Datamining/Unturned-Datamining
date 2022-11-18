using Steamworks;

namespace SDG.Unturned;

public class CommandResetConfig : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        Provider.resetConfig();
        CommandWindow.Log(localization.format("ResetConfigText"));
    }

    public CommandResetConfig(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("ResetConfigCommandText");
        _info = localization.format("ResetConfigInfoText");
        _help = localization.format("ResetConfigHelpText");
    }
}
