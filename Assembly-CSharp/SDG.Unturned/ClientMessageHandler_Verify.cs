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
        uint punOutBufferSize;
        if (Provider.provider.economyService.wearingResult == SteamInventoryResult_t.Invalid)
        {
            writer.WriteUInt16(0);
        }
        else if (SteamInventory.SerializeResult(Provider.provider.economyService.wearingResult, null, out punOutBufferSize))
        {
            byte[] pOutBuffer = new byte[punOutBufferSize];
            if (!SteamInventory.SerializeResult(Provider.provider.economyService.wearingResult, pOutBuffer, out punOutBufferSize))
            {
                UnturnedLog.warn("SteamInventory.SerializeResult returned false the second time");
            }
            SteamInventory.DestroyResult(Provider.provider.economyService.wearingResult);
            Provider.provider.economyService.wearingResult = SteamInventoryResult_t.Invalid;
        }
        else
        {
            SteamInventory.DestroyResult(Provider.provider.economyService.wearingResult);
            Provider.provider.economyService.wearingResult = SteamInventoryResult_t.Invalid;
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_ECON_SERIALIZE;
            Provider.RequestDisconnect("SteamInventory.SerializeResult failed");
        }
    }
}
