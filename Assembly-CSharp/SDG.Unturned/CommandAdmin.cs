using Steamworks;

namespace SDG.Unturned;

public class CommandAdmin : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        if (!PlayerTool.tryGetSteamID(parameter, out var steamID))
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", parameter));
            return;
        }
        SteamAdminlist.admin(steamID, executorID);
        CommandWindow.Log(localization.format("AdminText", steamID));
    }

    public CommandAdmin(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("AdminCommandText");
        _info = localization.format("AdminInfoText");
        _help = localization.format("AdminHelpText");
    }
}
