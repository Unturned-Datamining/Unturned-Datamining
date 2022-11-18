using Steamworks;

namespace SDG.Unturned;

public class CommandSpy : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length != 1)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        if (!PlayerTool.tryGetSteamPlayer(componentsFromSerial[0], out var player) || player.player == null)
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", componentsFromSerial[0]));
            return;
        }
        player.player.sendScreenshot(executorID);
        CommandWindow.Log(localization.format("SpyText", player.playerID.playerName));
    }

    public CommandSpy(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("SpyCommandText");
        _info = localization.format("SpyInfoText");
        _help = localization.format("SpyHelpText");
    }
}
