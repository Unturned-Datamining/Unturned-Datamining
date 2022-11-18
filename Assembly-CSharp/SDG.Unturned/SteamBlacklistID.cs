using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

public class SteamBlacklistID
{
    private CSteamID _playerID;

    private uint _ip;

    internal byte[][] hwids;

    public CSteamID judgeID;

    public string reason;

    public uint duration;

    public uint banned;

    public CSteamID playerID => _playerID;

    public uint ip => _ip;

    public bool isExpired => Provider.time > banned + duration;

    public uint getTime()
    {
        return duration - (Provider.time - banned);
    }

    public bool DoesAnyHwidMatch(IEnumerable<byte[]> clientHwids)
    {
        if (hwids == null || clientHwids == null)
        {
            return false;
        }
        byte[][] array = hwids;
        foreach (byte[] hash_ in array)
        {
            foreach (byte[] clientHwid in clientHwids)
            {
                if (Hash.verifyHash(hash_, clientHwid))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public SteamBlacklistID(CSteamID newPlayerID, uint newIP, CSteamID newJudgeID, string newReason, uint newDuration, uint newBanned, byte[][] newHwids)
    {
        _playerID = newPlayerID;
        _ip = newIP;
        judgeID = newJudgeID;
        reason = newReason;
        duration = newDuration;
        banned = newBanned;
        hwids = newHwids;
    }
}
