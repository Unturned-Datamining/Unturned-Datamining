using Steamworks;

namespace SDG.Unturned;

public class SteamAdminID
{
    private CSteamID _playerID;

    public CSteamID judgeID;

    public CSteamID playerID => _playerID;

    public SteamAdminID(CSteamID newPlayerID, CSteamID newJudgeID)
    {
        _playerID = newPlayerID;
        judgeID = newJudgeID;
    }
}
