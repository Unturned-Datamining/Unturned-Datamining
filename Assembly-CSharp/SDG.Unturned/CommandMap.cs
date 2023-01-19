using Steamworks;

namespace SDG.Unturned;

public class CommandMap : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Dedicator.IsDedicatedServer)
        {
            if (Provider.isServer)
            {
                CommandWindow.LogError(localization.format("RunningErrorText"));
                return;
            }
            Provider.map = parameter;
            CommandWindow.Log(localization.format("MapText", parameter));
        }
    }

    public CommandMap(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("MapCommandText");
        _info = localization.format("MapInfoText");
        _help = localization.format("MapHelpText");
    }
}
