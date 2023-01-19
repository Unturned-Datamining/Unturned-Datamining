using Steamworks;

namespace SDG.Unturned;

public class CommandBans : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Dedicator.IsDedicatedServer)
        {
            return;
        }
        if (SteamBlacklist.list.Count == 0)
        {
            CommandWindow.LogError(localization.format("NoBansErrorText"));
            return;
        }
        CommandWindow.Log(localization.format("BansText"));
        for (int i = 0; i < SteamBlacklist.list.Count; i++)
        {
            SteamBlacklistID steamBlacklistID = SteamBlacklist.list[i];
            CommandWindow.Log(localization.format("BanNameText", steamBlacklistID.playerID));
            CommandWindow.Log(localization.format("BanJudgeText", steamBlacklistID.judgeID));
            CommandWindow.Log(localization.format("BanStatusText", steamBlacklistID.reason, steamBlacklistID.duration, steamBlacklistID.getTime()));
        }
    }

    public CommandBans(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("BansCommandText");
        _info = localization.format("BansInfoText");
        _help = localization.format("BansHelpText");
    }
}
