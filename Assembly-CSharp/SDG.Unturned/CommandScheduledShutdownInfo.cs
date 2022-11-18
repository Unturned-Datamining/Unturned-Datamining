using System;
using Steamworks;

namespace SDG.Unturned;

public class CommandScheduledShutdownInfo : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer && !(executorID != CSteamID.Nil))
        {
            if (Provider.autoShutdownManager.isScheduledShutdownEnabled)
            {
                CommandWindow.Log($"Shutdown is scheduled for {Provider.autoShutdownManager.scheduledShutdownTime.ToLocalTime()} ({Provider.autoShutdownManager.scheduledShutdownTime - DateTime.UtcNow:g} from now)");
            }
            else
            {
                CommandWindow.Log("Scheduled shutdown is disabled");
            }
        }
    }

    public CommandScheduledShutdownInfo(Local newLocalization)
    {
        localization = newLocalization;
        _command = "ScheduledShutdownInfo";
        _info = string.Empty;
        _help = string.Empty;
    }
}
