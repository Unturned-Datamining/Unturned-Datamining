using Steamworks;

namespace SDG.Unturned;

public class CommandDay : Command
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
        LightingManager.time = (uint)((float)LightingManager.cycle * LevelLighting.transition);
        CommandWindow.Log(localization.format("DayText"));
    }

    public CommandDay(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("DayCommandText");
        _info = localization.format("DayInfoText");
        _help = localization.format("DayHelpText");
    }
}
