using Steamworks;

namespace SDG.Unturned;

internal class OwnershipTool
{
    public static bool checkToggle(ulong player, ulong group)
    {
        if (Dedicator.IsDedicatedServer)
        {
            return false;
        }
        return checkToggle(Provider.client, player, Player.player.quests.groupID, group);
    }

    public static bool checkToggle(CSteamID player_0, ulong player_1, CSteamID group_0, ulong group_1)
    {
        if (Provider.isServer && !Dedicator.IsDedicatedServer)
        {
            return true;
        }
        if (player_0.m_SteamID != player_1)
        {
            if (group_0 != CSteamID.Nil)
            {
                return group_0.m_SteamID == group_1;
            }
            return false;
        }
        return true;
    }
}
