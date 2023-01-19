using Steamworks;

namespace SDG.Unturned;

public class CommandFilter : Command
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
            Provider.filterName = true;
            CommandWindow.Log(localization.format("FilterText"));
        }
    }

    public CommandFilter(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("FilterCommandText");
        _info = localization.format("FilterInfoText");
        _help = localization.format("FilterHelpText");
    }
}
