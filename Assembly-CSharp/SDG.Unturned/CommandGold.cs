using Steamworks;

namespace SDG.Unturned;

public class CommandGold : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (Provider.isServer)
        {
            CommandWindow.LogError(localization.format("RunningErrorText"));
            return;
        }
        Provider.isGold = true;
        CommandWindow.Log(localization.format("GoldText"));
    }

    public CommandGold(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("GoldCommandText");
        _info = localization.format("GoldInfoText");
        _help = localization.format("GoldHelpText");
    }
}
