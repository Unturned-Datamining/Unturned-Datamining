using Steamworks;

namespace SDG.Unturned;

public class CommandCycle : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!uint.TryParse(parameter, out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
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
        LightingManager.cycle = result;
        CommandWindow.Log(localization.format("CycleText", result));
    }

    public CommandCycle(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("CycleCommandText");
        _info = localization.format("CycleInfoText");
        _help = localization.format("CycleHelpText");
    }
}
