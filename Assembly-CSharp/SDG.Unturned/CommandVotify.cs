using Steamworks;

namespace SDG.Unturned;

public class CommandVotify : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length != 6)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        bool voteAllowed;
        if (componentsFromSerial[0].ToLower() == "y")
        {
            voteAllowed = true;
        }
        else
        {
            if (!(componentsFromSerial[0].ToLower() == "n"))
            {
                CommandWindow.LogError(localization.format("InvalidBooleanErrorText", componentsFromSerial[0]));
                return;
            }
            voteAllowed = false;
        }
        if (!float.TryParse(componentsFromSerial[1], out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[1]));
            return;
        }
        if (!float.TryParse(componentsFromSerial[2], out var result2))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[2]));
            return;
        }
        if (!float.TryParse(componentsFromSerial[3], out var result3))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[3]));
            return;
        }
        if (!float.TryParse(componentsFromSerial[4], out var result4))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[4]));
            return;
        }
        if (!byte.TryParse(componentsFromSerial[5], out var result5))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[5]));
            return;
        }
        ChatManager.voteAllowed = voteAllowed;
        ChatManager.votePassCooldown = result;
        ChatManager.voteFailCooldown = result2;
        ChatManager.voteDuration = result3;
        ChatManager.votePercentage = result4;
        ChatManager.votePlayers = result5;
        CommandWindow.Log(localization.format("VotifyText"));
    }

    public CommandVotify(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("VotifyCommandText");
        _info = localization.format("VotifyInfoText");
        _help = localization.format("VotifyHelpText");
    }
}
