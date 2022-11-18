using Steamworks;

namespace SDG.Unturned;

public class CommandVehicle : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        if (!Provider.hasCheats)
        {
            CommandWindow.LogError(localization.format("CheatsErrorText"));
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length < 1 || componentsFromSerial.Length > 3)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        bool flag = false;
        if (!PlayerTool.tryGetSteamPlayer(componentsFromSerial[0], out var player))
        {
            player = PlayerTool.getSteamPlayer(executorID);
            if (player == null)
            {
                CommandWindow.LogError(localization.format("NoPlayerErrorText", componentsFromSerial[0]));
                return;
            }
            flag = true;
        }
        if (!ushort.TryParse(componentsFromSerial[(!flag) ? 1 : 0], out var result))
        {
            CommandWindow.LogError(localization.format("InvalidVehicleIDErrorText", componentsFromSerial[(!flag) ? 1 : 0]));
        }
        else if (!VehicleTool.giveVehicle(player.player, result))
        {
            CommandWindow.LogError(localization.format("NoVehicleIDErrorText", result));
        }
        else
        {
            CommandWindow.Log(localization.format("VehicleText", player.playerID.playerName, result));
        }
    }

    public CommandVehicle(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("VehicleCommandText");
        _info = localization.format("VehicleInfoText");
        _help = localization.format("VehicleHelpText");
    }
}
