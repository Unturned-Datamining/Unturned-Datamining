using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_Rejected
{
    internal static void ReadMessage(NetPakReader reader)
    {
        Provider.isWaitingForConnectResponse = false;
        reader.ReadEnum(out var value);
        reader.ReadString(out var value2);
        Provider._connectionFailureReason = string.Empty;
        switch (value)
        {
        case ESteamRejection.WHITELISTED:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.WHITELISTED;
            break;
        case ESteamRejection.WRONG_PASSWORD:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PASSWORD;
            break;
        case ESteamRejection.SERVER_FULL:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.FULL;
            break;
        case ESteamRejection.WRONG_HASH_LEVEL:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.HASH_LEVEL;
            break;
        case ESteamRejection.WRONG_HASH_ASSEMBLY:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.HASH_ASSEMBLY;
            break;
        case ESteamRejection.WRONG_VERSION:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.VERSION;
            Provider._connectionFailureReason = value2;
            break;
        case ESteamRejection.PRO_SERVER:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_SERVER;
            break;
        case ESteamRejection.PRO_CHARACTER:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_CHARACTER;
            break;
        case ESteamRejection.PRO_DESYNC:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_DESYNC;
            break;
        case ESteamRejection.PRO_APPEARANCE:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PRO_APPEARANCE;
            break;
        case ESteamRejection.AUTH_VERIFICATION:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_VERIFICATION;
            break;
        case ESteamRejection.AUTH_NO_STEAM:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_NO_STEAM;
            break;
        case ESteamRejection.AUTH_LICENSE_EXPIRED:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_LICENSE_EXPIRED;
            break;
        case ESteamRejection.AUTH_VAC_BAN:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_VAC_BAN;
            break;
        case ESteamRejection.AUTH_ELSEWHERE:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_ELSEWHERE;
            break;
        case ESteamRejection.AUTH_TIMED_OUT:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_TIMED_OUT;
            break;
        case ESteamRejection.AUTH_USED:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_USED;
            break;
        case ESteamRejection.AUTH_NO_USER:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_NO_USER;
            break;
        case ESteamRejection.AUTH_PUB_BAN:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_PUB_BAN;
            break;
        case ESteamRejection.AUTH_NETWORK_IDENTITY_FAILURE:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_NETWORK_IDENTITY_FAILURE;
            break;
        case ESteamRejection.AUTH_ECON_DESERIALIZE:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_ECON_DESERIALIZE;
            break;
        case ESteamRejection.AUTH_ECON_VERIFY:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.AUTH_ECON_VERIFY;
            break;
        case ESteamRejection.ALREADY_CONNECTED:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.ALREADY_CONNECTED;
            break;
        case ESteamRejection.ALREADY_PENDING:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.ALREADY_PENDING;
            break;
        case ESteamRejection.LATE_PENDING:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LATE_PENDING;
            break;
        case ESteamRejection.NOT_PENDING:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NOT_PENDING;
            break;
        case ESteamRejection.NAME_PLAYER_SHORT:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PLAYER_SHORT;
            break;
        case ESteamRejection.NAME_PLAYER_LONG:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PLAYER_LONG;
            break;
        case ESteamRejection.NAME_PLAYER_INVALID:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PLAYER_INVALID;
            break;
        case ESteamRejection.NAME_PLAYER_NUMBER:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PLAYER_NUMBER;
            break;
        case ESteamRejection.NAME_CHARACTER_SHORT:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_CHARACTER_SHORT;
            break;
        case ESteamRejection.NAME_CHARACTER_LONG:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_CHARACTER_LONG;
            break;
        case ESteamRejection.NAME_CHARACTER_INVALID:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_CHARACTER_INVALID;
            break;
        case ESteamRejection.NAME_CHARACTER_NUMBER:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_CHARACTER_NUMBER;
            break;
        case ESteamRejection.PING:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PING;
            if (MenuDashboardUI.localization.has("PingV2"))
            {
                int num = ((Provider.CurrentServerAdvertisement != null) ? Provider.CurrentServerAdvertisement.ping : (-1));
                Provider._connectionFailureReason = MenuDashboardUI.localization.format("PingV2", num, value2);
            }
            else
            {
                Provider._connectionFailureReason = MenuDashboardUI.localization.format("Ping");
            }
            break;
        case ESteamRejection.PLUGIN:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.PLUGIN;
            Provider._connectionFailureReason = value2;
            break;
        case ESteamRejection.CLIENT_MODULE_DESYNC:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.CLIENT_MODULE_DESYNC;
            break;
        case ESteamRejection.SERVER_MODULE_DESYNC:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.SERVER_MODULE_DESYNC;
            break;
        case ESteamRejection.WRONG_LEVEL_VERSION:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LEVEL_VERSION;
            Provider._connectionFailureReason = MenuDashboardUI.localization.format("Level_Version", value2, Level.info.getLocalizedName(), Level.version);
            break;
        case ESteamRejection.WRONG_HASH_ECON:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.ECON_HASH;
            break;
        case ESteamRejection.WRONG_HASH_MASTER_BUNDLE:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.HASH_MASTER_BUNDLE;
            Provider._connectionFailureReason = value2;
            break;
        case ESteamRejection.LATE_PENDING_STEAM_AUTH:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LATE_PENDING_STEAM_AUTH;
            break;
        case ESteamRejection.LATE_PENDING_STEAM_ECON:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LATE_PENDING_STEAM_ECON;
            break;
        case ESteamRejection.LATE_PENDING_STEAM_GROUPS:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.LATE_PENDING_STEAM_GROUPS;
            break;
        case ESteamRejection.NAME_PRIVATE_LONG:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PRIVATE_LONG;
            break;
        case ESteamRejection.NAME_PRIVATE_INVALID:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PRIVATE_INVALID;
            break;
        case ESteamRejection.NAME_PRIVATE_NUMBER:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.NAME_PRIVATE_NUMBER;
            break;
        case ESteamRejection.WRONG_HASH_RESOURCES:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.HASH_RESOURCES;
            break;
        case ESteamRejection.SKIN_COLOR_WITHIN_THRESHOLD_OF_TERRAIN_COLOR:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.SKIN_COLOR_WITHIN_THRESHOLD_OF_TERRAIN_COLOR;
            break;
        default:
            Provider._connectionFailureInfo = ESteamConnectionFailureInfo.REJECT_UNKNOWN;
            Provider._connectionFailureReason = value.ToString();
            break;
        }
        Provider.RequestDisconnect($"Rejected by server ({value}) --- Reason: \"{Provider.connectionFailureReason}\" Explanation: \"{value2}\"");
    }
}
