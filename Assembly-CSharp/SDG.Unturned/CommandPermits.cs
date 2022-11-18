using Steamworks;

namespace SDG.Unturned;

public class CommandPermits : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (SteamWhitelist.list.Count == 0)
        {
            CommandWindow.LogError(localization.format("NoPermitsErrorText"));
            return;
        }
        CommandWindow.Log(localization.format("PermitsText"));
        for (int i = 0; i < SteamWhitelist.list.Count; i++)
        {
            SteamWhitelistID steamWhitelistID = SteamWhitelist.list[i];
            CommandWindow.Log(localization.format("PermitNameText", steamWhitelistID.steamID, steamWhitelistID.tag));
            CommandWindow.Log(localization.format("PermitJudgeText", steamWhitelistID.judgeID));
        }
    }

    public CommandPermits(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("PermitsCommandText");
        _info = localization.format("PermitsInfoText");
        _help = localization.format("PermitsHelpText");
    }
}
