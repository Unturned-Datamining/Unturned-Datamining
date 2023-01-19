using Steamworks;

namespace SDG.Unturned;

public class CommandPvE : Command
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
            Provider.isPvP = false;
            CommandWindow.Log(localization.format("PvEText"));
        }
    }

    public CommandPvE(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("PvECommandText");
        _info = localization.format("PvEInfoText");
        _help = localization.format("PvEHelpText");
    }
}
