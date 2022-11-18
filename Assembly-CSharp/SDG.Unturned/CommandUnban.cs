using Steamworks;

namespace SDG.Unturned;

public class CommandUnban : Command
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
        else if (!Provider.requestUnbanPlayer(executorID, steamID))
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", steamID));
        }
        else
        {
            CommandWindow.Log(localization.format("UnbanText", steamID));
        }
    }

    public CommandUnban(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("UnbanCommandText");
        _info = localization.format("UnbanInfoText");
        _help = localization.format("UnbanHelpText");
    }
}
