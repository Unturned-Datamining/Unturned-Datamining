using Steamworks;

namespace SDG.Unturned;

public class CommandMode : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        string text = parameter.ToLower();
        EGameMode mode;
        if (text == localization.format("ModeEasy").ToLower())
        {
            mode = EGameMode.EASY;
        }
        else if (text == localization.format("ModeNormal").ToLower())
        {
            mode = EGameMode.NORMAL;
        }
        else
        {
            if (!(text == localization.format("ModeHard").ToLower()))
            {
                CommandWindow.LogError(localization.format("NoModeErrorText", text));
                return;
            }
            mode = EGameMode.HARD;
        }
        if (Provider.isServer)
        {
            CommandWindow.LogError(localization.format("RunningErrorText"));
            return;
        }
        Provider.mode = mode;
        CommandWindow.Log(localization.format("ModeText", text));
    }

    public CommandMode(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("ModeCommandText");
        _info = localization.format("ModeInfoText");
        _help = localization.format("ModeHelpText");
    }
}
