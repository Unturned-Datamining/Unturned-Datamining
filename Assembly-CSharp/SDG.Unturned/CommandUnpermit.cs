using Steamworks;

namespace SDG.Unturned;

public class CommandUnpermit : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        CSteamID steamID;
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
        }
        else if (!PlayerTool.tryGetSteamID(parameter, out steamID))
        {
            CommandWindow.LogError(localization.format("InvalidSteamIDErrorText", parameter));
        }
        else if (!SteamWhitelist.unwhitelist(steamID))
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", steamID));
        }
        else
        {
            CommandWindow.Log(localization.format("UnpermitText", steamID));
        }
    }

    public CommandUnpermit(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("UnpermitCommandText");
        _info = localization.format("UnpermitInfoText");
        _help = localization.format("UnpermitHelpText");
    }
}
