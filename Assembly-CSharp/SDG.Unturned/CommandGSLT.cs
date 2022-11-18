using Steamworks;

namespace SDG.Unturned;

public class CommandGSLT : Command
{
    public static string loginToken { get; private set; }

    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer)
        {
            CommandWindow.LogError(localization.format("RunningErrorText"));
            return;
        }
        loginToken = parameter;
        CommandWindow.Log(localization.format("SetText"));
    }

    public CommandGSLT(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("GSLTCommandText");
        _info = localization.format("GSLTInfoText");
        _help = localization.format("GSLTHelpText");
    }
}
