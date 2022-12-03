using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Verify
{
    public static readonly NetLength BUNDLE_NAMES_LENGTH = new NetLength(31u);

    private static List<string> requiredBundleNames = new List<string>();

    internal static void ReadMessage(NetPakReader reader)
    {
        Provider.isWaitingForConnectResponse = false;
        requiredBundleNames.Clear();
        reader.ReadList(requiredBundleNames, delegate(out string name)
        {
            return reader.ReadString(out name);
        }, BUNDLE_NAMES_LENGTH);
        byte[] ticket = Provider.openTicket();
        if (ticket == null)
        {
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_EMPTY;
            Provider.RequestDisconnect("opening Steam auth ticket failed");
            return;
        }
        byte[] mbHashes = MasterBundleValidation.clientHandleRequest(requiredBundleNames);
        UnturnedLog.info("Authenticating with server");
        NetMessages.SendMessageToServer(EServerMessage.Authenticate, ENetReliability.Reliable, delegate(NetPakWriter writer)
        {
            writer.WriteUInt16((ushort)ticket.Length);
            writer.WriteBytes(ticket);
            WriteEconomyDetails(writer);
            writer.WriteUInt16((ushort)mbHashes.Length);
            writer.WriteBytes(mbHashes);
        });
    }

    private static void WriteEconomyDetails(NetPakWriter writer)
    {
        if (Provider.provider.economyService.wearingResult == SteamInventoryResult_t.Invalid)
        {
            writer.WriteUInt16(0);
            return;
        }
        uint punOutItemsArraySize = 0u;
        SteamItemDetails_t[] array;
        if (SteamInventory.GetResultItems(Provider.provider.economyService.wearingResult, null, ref punOutItemsArraySize) && punOutItemsArraySize != 0)
        {
            array = new SteamItemDetails_t[punOutItemsArraySize];
            SteamInventory.GetResultItems(Provider.provider.economyService.wearingResult, array, ref punOutItemsArraySize);
        }
        else
        {
            array = new SteamItemDetails_t[punOutItemsArraySize];
        }
        writer.WriteUInt16((ushort)array.Length);
        for (uint num = 0u; num < array.Length; num++)
        {
            SteamItemDetails_t steamItemDetails_t = array[num];
            writer.WriteSteamItemDefID(steamItemDetails_t.m_iDefinition);
            writer.WriteSteamItemInstanceID(steamItemDetails_t.m_itemId);
            uint punValueBufferSizeOut = 1024u;
            if (!SteamInventory.GetResultItemProperty(Provider.provider.economyService.wearingResult, num, "tags", out var pchValueBuffer, ref punValueBufferSizeOut) || punValueBufferSizeOut == 0)
            {
                pchValueBuffer = string.Empty;
            }
            writer.WriteString(pchValueBuffer);
            uint punValueBufferSizeOut2 = 1024u;
            if (!SteamInventory.GetResultItemProperty(Provider.provider.economyService.wearingResult, num, "dynamic_props", out var pchValueBuffer2, ref punValueBufferSizeOut2) || punValueBufferSizeOut2 == 0)
            {
                pchValueBuffer2 = string.Empty;
            }
            writer.WriteString(pchValueBuffer2);
        }
        SteamInventory.DestroyResult(Provider.provider.economyService.wearingResult);
        Provider.provider.economyService.wearingResult = SteamInventoryResult_t.Invalid;
    }
}
