using Steamworks;

namespace SDG.Unturned;

public class CommandAnimal : Command
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
            CommandWindow.LogError(localization.format("InvalidAnimalIDErrorText", componentsFromSerial[(!flag) ? 1 : 0]));
        }
        else if (!AnimalManager.giveAnimal(player.player, result))
        {
            CommandWindow.LogError(localization.format("NoAnimalIDErrorText", result));
        }
        else
        {
            CommandWindow.Log(localization.format("AnimalText", player.playerID.playerName, result));
        }
    }

    public CommandAnimal(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("AnimalCommandText");
        _info = localization.format("AnimalInfoText");
        _help = localization.format("AnimalHelpText");
    }
}
