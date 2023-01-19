using Steamworks;

namespace SDG.Unturned;

public class CommandSync : Command
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
            PlayerSavedata.hasSync = true;
            CommandWindow.Log(localization.format("SyncText"));
        }
    }

    public CommandSync(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("SyncCommandText");
        _info = localization.format("SyncInfoText");
        _help = localization.format("SyncHelpText");
    }
}
