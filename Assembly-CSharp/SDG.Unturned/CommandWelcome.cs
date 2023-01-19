using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CommandWelcome : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length != 1 && componentsFromSerial.Length != 4)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        ChatManager.welcomeText = componentsFromSerial[0];
        if (componentsFromSerial.Length == 1)
        {
            ChatManager.welcomeColor = Palette.SERVER;
        }
        else if (componentsFromSerial.Length == 4)
        {
            if (!byte.TryParse(componentsFromSerial[1], out var result))
            {
                CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[0]));
                return;
            }
            if (!byte.TryParse(componentsFromSerial[2], out var result2))
            {
                CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[1]));
                return;
            }
            if (!byte.TryParse(componentsFromSerial[3], out var result3))
            {
                CommandWindow.LogError(localization.format("InvalidNumberErrorText", componentsFromSerial[2]));
                return;
            }
            ChatManager.welcomeColor = new Color((float)(int)result / 255f, (float)(int)result2 / 255f, (float)(int)result3 / 255f);
        }
        CommandWindow.Log(localization.format("WelcomeText", componentsFromSerial[0]));
    }

    public CommandWelcome(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("WelcomeCommandText");
        _info = localization.format("WelcomeInfoText");
        _help = localization.format("WelcomeHelpText");
    }
}
