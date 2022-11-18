using Steamworks;

namespace SDG.Unturned;

public class CommandFlag : Command
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
        if (componentsFromSerial.Length < 2 || componentsFromSerial.Length > 3)
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
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[(!flag) ? 1 : 0]));
            return;
        }
        if (!short.TryParse(componentsFromSerial[flag ? 1 : 2], out var result2))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[flag ? 1 : 2]));
            return;
        }
        player.player.quests.sendSetFlag(result, result2);
        CommandWindow.Log(localization.format("FlagText", player.playerID.playerName, result, result2));
    }

    public CommandFlag(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("FlagCommandText");
        _info = localization.format("FlagInfoText");
        _help = localization.format("FlagHelpText");
    }
}
