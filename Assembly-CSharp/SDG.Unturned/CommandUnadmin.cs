using Steamworks;

namespace SDG.Unturned;

public class CommandUnadmin : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Dedicator.IsDedicatedServer)
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
            SteamAdminlist.unadmin(steamID);
            CommandWindow.Log(localization.format("UnadminText", steamID));
        }
    }

    public CommandUnadmin(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("UnadminCommandText");
        _info = localization.format("UnadminInfoText");
        _help = localization.format("UnadminHelpText");
    }
}
