using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandSay : Command
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
        if (componentsFromSerial.Length != 1 && componentsFromSerial.Length != 4)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
        }
        else if (componentsFromSerial.Length == 1)
        {
            ChatManager.say(componentsFromSerial[0], Palette.SERVER);
        }
        else if (componentsFromSerial.Length == 4)
        {
            byte result2;
            byte result3;
            if (!byte.TryParse(componentsFromSerial[1], out var result))
            {
                CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[0]));
            }
            else if (!byte.TryParse(componentsFromSerial[2], out result2))
            {
                CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[1]));
            }
            else if (!byte.TryParse(componentsFromSerial[3], out result3))
            {
                CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[2]));
            }
            else
            {
                ChatManager.say(componentsFromSerial[0], new Color((float)(int)result / 255f, (float)(int)result2 / 255f, (float)(int)result3 / 255f));
            }
        }
    }

    public CommandSay(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("SayCommandText");
        _info = localization.format("SayInfoText");
        _help = localization.format("SayHelpText");
    }
}
