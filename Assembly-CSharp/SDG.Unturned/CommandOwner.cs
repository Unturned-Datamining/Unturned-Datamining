using Steamworks;

namespace SDG.Unturned;

public class CommandOwner : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer)
        {
            CommandWindow.LogError(localization.format("RunningErrorText"));
            return;
        }
        if (!PlayerTool.tryGetSteamID(parameter, out var steamID))
        {
            CommandWindow.LogError(localization.format("InvalidSteamIDErrorText", parameter));
            return;
        }
        SteamAdminlist.ownerID = steamID;
        CommandWindow.Log(localization.format("OwnerText", steamID));
    }

    public CommandOwner(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("OwnerCommandText");
        _info = localization.format("OwnerInfoText");
        _help = localization.format("OwnerHelpText");
    }
}
