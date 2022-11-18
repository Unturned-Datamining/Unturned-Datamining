using Steamworks;

namespace SDG.Unturned;

public class CommandReputation : Command
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
        if (componentsFromSerial.Length < 1 || componentsFromSerial.Length > 2)
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
        if (!int.TryParse(componentsFromSerial[(!flag) ? 1 : 0], out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[(!flag) ? 1 : 0]));
            return;
        }
        player.player.skills.askRep(result);
        string text = result.ToString();
        if (result > 0)
        {
            text = "+" + text;
        }
        CommandWindow.Log(localization.format("ReputationText", player.playerID.playerName, text));
    }

    public CommandReputation(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("ReputationCommandText");
        _info = localization.format("ReputationInfoText");
        _help = localization.format("ReputationHelpText");
    }
}
