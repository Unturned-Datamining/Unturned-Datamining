using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Provider;
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
        if (!ReadEconomyDetails(steamPending, reader))
        {
            return;
        }
        reader.ReadUInt16(out var value2);
        byte[] array2 = new byte[value2];
        reader.ReadBytes(array2);
        if (steamPending.playerID.BypassIntegrityChecks || MasterBundleValidation.serverHandleResponse(steamPending, array2))
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
        SteamItemDetails_t[] array = new SteamItemDetails_t[value];
        Dictionary<ulong, DynamicEconDetails> dictionary = new Dictionary<ulong, DynamicEconDetails>();
        for (uint num = 0u; num < value; num++)
        {
            SteamItemDetails_t steamItemDetails_t = default(SteamItemDetails_t);
            bool num2 = reader.ReadSteamItemDefID(out steamItemDetails_t.m_iDefinition) & reader.ReadSteamItemInstanceID(out steamItemDetails_t.m_itemId);
            DynamicEconDetails value2 = default(DynamicEconDetails);
            if (!(num2 & reader.ReadString(out value2.tags) & reader.ReadString(out value2.dynamic_props) & !dictionary.ContainsKey(steamItemDetails_t.m_itemId.m_SteamItemInstanceID)))
            {
                Provider.reject(player.playerID.steamID, ESteamRejection.AUTH_ECON_DESERIALIZE);
                return false;
            }
            array[num] = steamItemDetails_t;
            dictionary.Add(steamItemDetails_t.m_itemId.m_SteamItemInstanceID, value2);
        }
        player.inventoryResult = SteamInventoryResult_t.Invalid;
        player.inventoryDetails = array;
        player.dynamicInventoryDetails = dictionary;
        player.inventoryDetailsReady();
        return true;
    }
}
