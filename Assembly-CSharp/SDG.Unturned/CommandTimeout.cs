using Steamworks;

namespace SDG.Unturned;

public class CommandTimeout : Command
{
    private static readonly ushort MIN_NUMBER = 50;

    private static readonly ushort MAX_NUMBER = 10000;

    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        if (!ushort.TryParse(parameter, out var result))
        {
            CommandWindow.LogError(localization.format("InvalidNumberErrorText", parameter));
            return;
        }
        if (result < MIN_NUMBER)
        {
            CommandWindow.LogError(localization.format("MinNumberErrorText", MIN_NUMBER));
            return;
        }
        if (result > MAX_NUMBER)
        {
            CommandWindow.LogError(localization.format("MaxNumberErrorText", MAX_NUMBER));
            return;
        }
        if (Provider.configData != null)
        {
            Provider.configData.Server.Max_Ping_Milliseconds = result;
        }
        CommandWindow.Log(localization.format("TimeoutText", result));
    }

    public CommandTimeout(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("TimeoutCommandText");
        _info = localization.format("TimeoutInfoText");
        _help = localization.format("TimeoutHelpText");
    }
}
