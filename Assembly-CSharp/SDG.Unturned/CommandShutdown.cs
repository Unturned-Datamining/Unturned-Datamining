using Steamworks;

namespace SDG.Unturned;

public class CommandShutdown : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        string[] componentsFromSerial = Parser.getComponentsFromSerial(parameter, '/');
        if (componentsFromSerial.Length > 2)
        {
            CommandWindow.LogError(localization.format("InvalidParameterErrorText"));
            return;
        }
        if (componentsFromSerial.Length == 0)
        {
            Provider.shutdown();
            return;
        }
        if (!int.TryParse(componentsFromSerial[0], out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
            return;
        }
        string explanation = "";
        if (componentsFromSerial.Length > 1)
        {
            explanation = componentsFromSerial[1];
        }
        Provider.shutdown(result, explanation);
        CommandWindow.LogError(localization.format("ShutdownText", parameter));
    }

    public CommandShutdown(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("ShutdownCommandText");
        _info = localization.format("ShutdownInfoText");
        _help = localization.format("ShutdownHelpText");
    }
}
