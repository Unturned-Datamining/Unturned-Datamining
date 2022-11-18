using Steamworks;

namespace SDG.Unturned;

public class CommandArmor : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        float result;
        float result2;
        if (componentsFromSerial.Length != 2)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
        }
        else if (!float.TryParse(componentsFromSerial[0], out result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
        }
        else if (!float.TryParse(componentsFromSerial[1], out result2))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
        }
        else
        {
            CommandWindow.Log(localization.format("ArmorText"));
        }
    }

    public CommandArmor(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("ArmorCommandText");
        _info = localization.format("ArmorInfoText");
        _help = localization.format("ArmorHelpText");
    }
}
