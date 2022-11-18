using Steamworks;

namespace SDG.Unturned;

public class CommandAirdrop : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (LevelManager.hasAirdrop)
        {
            LevelManager.airdropFrequency = 0u;
            CommandWindow.Log(localization.format("AirdropText"));
        }
    }

    public CommandAirdrop(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("AirdropCommandText");
        _info = localization.format("AirdropInfoText");
        _help = localization.format("AirdropHelpText");
    }
}
