using Steamworks;

namespace SDG.Unturned;

public class CommandTime : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer)
        {
            CommandWindow.LogError(localization.format("NotRunningErrorText"));
            return;
        }
        if (Provider.isServer && Level.info.type == ELevelType.HORDE)
        {
            CommandWindow.LogError(localization.format("HordeErrorText"));
            return;
        }
        if (Provider.isServer && Level.info.type == ELevelType.ARENA)
        {
            CommandWindow.LogError(localization.format("ArenaErrorText"));
            return;
        }
        if (!uint.TryParse(parameter, out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
            return;
        }
        LightingManager.time = result;
        CommandWindow.Log(localization.format("TimeText", result));
    }

    public CommandTime(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("TimeCommandText");
        _info = localization.format("TimeInfoText");
        _help = localization.format("TimeHelpText");
    }
}
