using Steamworks;

namespace SDG.Unturned;

public class CommandPort : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!ushort.TryParse(parameter, out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
            return;
        }
        if (Provider.isServer)
        {
            CommandWindow.LogError(localization.format("RunningErrorText"));
            return;
        }
        Provider.port = result;
        CommandWindow.Log(localization.format("PortText", result));
    }

    public CommandPort(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("PortCommandText");
        _info = localization.format("PortInfoText");
        _help = localization.format("PortHelpText");
    }
}
