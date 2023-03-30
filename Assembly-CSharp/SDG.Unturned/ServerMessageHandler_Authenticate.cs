using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned;

internal static class ServerMessageHandler_Authenticate
{
    internal static void ReadMessage(ITransportConnection transportConnection, NetPakReader reader)
    {
        SteamPending steamPending = Provider.findPendingPlayer(transportConnection);
        if (steamPending == null || !steamPending.hasSentVerifyPacket)
        {
            Provider.reject(transportConnection, ESteamRejection.NOT_PENDING);
            return;
        }
        if (Provider.clients.Count + 1 > Provider.maxPlayers)
        {
            Provider.reject(transportConnection, ESteamRejection.SERVER_FULL);
            return;
        }
        reader.ReadUInt16(out var value);
        byte[] array = new byte[value];
        reader.ReadBytes(array);
        if ((bool)Dedicator.offlineOnly)
        {
            steamPending.assignedPro = steamPending.isPro;
            steamPending.assignedAdmin = SteamAdminlist.checkAdmin(steamPending.playerID.steamID);
            steamPending.hasAuthentication = true;
        }
        else if (!Provider.verifyTicket(steamPending.playerID.steamID, array))
        {
            Provider.reject(transportConnection, ESteamRejection.AUTH_VERIFICATION);
            return;
        }
        if (ReadEconomyDetails(steamPending, reader))
        {
            if (steamPending.playerID.group == CSteamID.Nil || (bool)Dedicator.offlineOnly)
            {
                steamPending.hasGroup = true;
            }
            else if (!SteamGameServer.RequestUserGroupStatus(steamPending.playerID.steamID, steamPending.playerID.group))
            {
                steamPending.playerID.group = CSteamID.Nil;
                steamPending.hasGroup = true;
            }
            if (steamPending.canAcceptYet)
            {
                Provider.accept(steamPending);
            }
        }
    }

    private static bool ReadEconomyDetails(SteamPending player, NetPakReader reader)
    {
        reader.ReadUInt16(out var value);
        if (value > 0)
        {
            byte[] array = new byte[value];
            reader.ReadBytes(array);
            if (!SteamGameServerInventory.DeserializeResult(out player.inventoryResult, array, value))
            {
                Provider.reject(player.transportConnection, ESteamRejection.AUTH_ECON_DESERIALIZE);
                return false;
            }
        }
        else
        {
            player.shirtItem = 0;
            player.pantsItem = 0;
            player.hatItem = 0;
            player.backpackItem = 0;
            player.vestItem = 0;
            player.maskItem = 0;
            player.glassesItem = 0;
            player.skinItems = new int[0];
            player.skinTags = new string[0];
            player.skinDynamicProps = new string[0];
            player.packageShirt = 0uL;
            player.packagePants = 0uL;
            player.packageHat = 0uL;
            player.packageBackpack = 0uL;
            player.packageVest = 0uL;
            player.packageMask = 0uL;
            player.packageGlasses = 0uL;
            player.packageSkins = new ulong[0];
            player.inventoryResult = SteamInventoryResult_t.Invalid;
            player.inventoryDetails = new SteamItemDetails_t[0];
            player.hasProof = true;
        }
        return true;
    }
}
