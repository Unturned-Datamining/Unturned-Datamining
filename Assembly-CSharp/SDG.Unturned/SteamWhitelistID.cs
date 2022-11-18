using Steamworks;

namespace SDG.Unturned;

public class SteamWhitelistID
{
    private CSteamID _steamID;

    public string tag;

    public CSteamID judgeID;

    public CSteamID steamID => _steamID;

    public SteamWhitelistID(CSteamID newSteamID, string newTag, CSteamID newJudgeID)
    {
        _steamID = newSteamID;
        tag = newTag;
        judgeID = newJudgeID;
    }
}
