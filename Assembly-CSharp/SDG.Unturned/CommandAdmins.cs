using Steamworks;

namespace SDG.Unturned;

public class CommandAdmins : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (SteamAdminlist.list.Count == 0)
        {
            CommandWindow.LogError(localization.format("NoAdminsErrorText"));
            return;
        }
        CommandWindow.Log(localization.format("AdminsText"));
        for (int i = 0; i < SteamAdminlist.list.Count; i++)
        {
            SteamAdminID steamAdminID = SteamAdminlist.list[i];
            CommandWindow.Log(localization.format("AdminNameText", steamAdminID.playerID));
            CommandWindow.Log(localization.format("AdminJudgeText", steamAdminID.judgeID));
        }
    }

    public CommandAdmins(Local newLocalization)
    {
        localization = newLocalization;
        _command = localization.format("AdminsCommandText");
        _info = localization.format("AdminsInfoText");
        _help = localization.format("AdminsHelpText");
    }
}
