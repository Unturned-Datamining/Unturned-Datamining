using Steamworks;

namespace SDG.Unturned;

public class CommandDecay : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        uint result;
        uint result2;
        if (componentsFromSerial.Length != 2)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
        }
        else if (!uint.TryParse(componentsFromSerial[0], out result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
        }
        else if (!uint.TryParse(componentsFromSerial[1], out result2))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
        }
        else
        {
            CommandWindow.Log(localization.format("DecayText"));
        }
    }

    public CommandDecay(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("DecayCommandText");
        _info = localization.format("DecayInfoText");
        _help = localization.format("DecayHelpText");
    }
}
