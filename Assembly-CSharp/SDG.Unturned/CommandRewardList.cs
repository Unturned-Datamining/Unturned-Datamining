using Steamworks;

namespace SDG.Unturned;

public class CommandRewardList : Command
{
    protected override void execute(CSteamID executorID, string parameter)
    {
        if (!Provider.isServer || !Provider.hasCheats)
        {
            return;
        }
        if (executorID == CSteamID.Nil && Provider.clients.Count > 0)
        {
            executorID = Provider.clients[0].playerID.steamID;
        }
        Player player = PlayerTool.getPlayer(executorID);
        if (player == null)
        {
            return;
        }
        if (!CachingAssetRef.TryParse(parameter, out var result))
        {
            CommandWindow.LogWarning("Unable to parse \"" + parameter + "\" as asset");
            return;
        }
        NPCRewardsAsset nPCRewardsAsset = result.Get<NPCRewardsAsset>();
        if (nPCRewardsAsset == null)
        {
            CommandWindow.LogWarning($"No reward list for \"{result}\"");
        }
        else if (nPCRewardsAsset.AreConditionsMet(player))
        {
            CommandWindow.Log("Running \"" + nPCRewardsAsset.FriendlyName + "\"");
            nPCRewardsAsset.ApplyConditions(player);
            nPCRewardsAsset.GrantRewards(player);
        }
        else
        {
            CommandWindow.Log("Cannot run \"" + nPCRewardsAsset.FriendlyName + "\" because conditions are unmet:");
            CommandWindow.Log(nPCRewardsAsset.conditionsList.DebugDumpToString(player));
        }
    }

    public CommandRewardList(Local newLocalization)
    {
        localization = newLocalization;
        _command = "RunRewardList";
        _info = string.Empty;
        _help = string.Empty;
    }
}
