using Steamworks;

namespace SDG.Unturned;

public class CommandPermit : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length != 2)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        if (!PlayerTool.tryGetSteamID(componentsFromSerial[0], out var steamID))
        {
            CommandWindow.LogError(localization.format("InvalidSteamIDErrorText", componentsFromSerial[0]));
            return;
        }
        SteamWhitelist.whitelist(steamID, componentsFromSerial[1], executorID);
        CommandWindow.Log(localization.format("PermitText", steamID, componentsFromSerial[1]));
    }

    public CommandPermit(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("PermitCommandText");
        _info = localization.format("PermitInfoText");
        _help = localization.format("PermitHelpText");
    }
}
