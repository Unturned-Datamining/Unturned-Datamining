using Steamworks;

namespace SDG.Unturned;

public class CommandNight : Command
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
        LightingManager.time = (uint)((float)LightingManager.cycle * (LevelLighting.bias + LevelLighting.transition));
        CommandWindow.Log(localization.format("NightText"));
    }

    public CommandNight(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("NightCommandText");
        _info = localization.format("NightInfoText");
        _help = localization.format("NightHelpText");
    }
}
