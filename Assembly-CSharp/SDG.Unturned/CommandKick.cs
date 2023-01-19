using Steamworks;

namespace SDG.Unturned;

public class CommandKick : Command
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
        if (componentsFromSerial.Length != 1 && componentsFromSerial.Length != 2)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        if (!PlayerTool.tryGetSteamPlayer(componentsFromSerial[0], out var player))
        {
            CommandWindow.LogError(localization.format("NoPlayerErrorText", componentsFromSerial[0]));
            return;
        }
        if (componentsFromSerial.Length == 1)
        {
            Provider.kick(player.playerID.steamID, localization.format("KickTextReason"));
        }
        else if (componentsFromSerial.Length == 2)
        {
            Provider.kick(player.playerID.steamID, componentsFromSerial[1]);
        }
        CommandWindow.Log(localization.format("KickText", player.playerID.playerName));
    }

    public CommandKick(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("KickCommandText");
        _info = localization.format("KickInfoText");
        _help = localization.format("KickHelpText");
    }
}
