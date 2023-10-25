namespace SDG.Unturned;

public enum ESteamConnectionFailureInfo
{
    NONE,
    SHUTDOWN,
    MAP,
    BANNED,
    KICKED,
    WHITELISTED,
    PASSWORD,
    FULL,
    HASH_LEVEL,
    HASH_ASSEMBLY,
    VERSION,
    PRO_SERVER,
    PRO_CHARACTER,
    PRO_DESYNC,
    PRO_APPEARANCE,
    AUTH_VERIFICATION,
    AUTH_NO_STEAM,
    AUTH_LICENSE_EXPIRED,
    AUTH_VAC_BAN,
    AUTH_ELSEWHERE,
    AUTH_TIMED_OUT,
    AUTH_USED,
    AUTH_NO_USER,
    AUTH_PUB_BAN,
    AUTH_ECON_SERIALIZE,
    AUTH_ECON_DESERIALIZE,
    AUTH_ECON_VERIFY,
    AUTH_EMPTY,
    ALREADY_CONNECTED,
    ALREADY_PENDING,
    LATE_PENDING,
    NOT_PENDING,
    NAME_PLAYER_SHORT,
    NAME_PLAYER_LONG,
    NAME_PLAYER_INVALID,
    NAME_PLAYER_NUMBER,
    NAME_CHARACTER_SHORT,
    NAME_CHARACTER_LONG,
    NAME_CHARACTER_INVALID,
    NAME_CHARACTER_NUMBER,
    TIMED_OUT,
    PING,
    PLUGIN,
    BARRICADE,
    STRUCTURE,
    VEHICLE,
    CLIENT_MODULE_DESYNC,
    SERVER_MODULE_DESYNC,
    BATTLEYE_BROKEN,
    BATTLEYE_UPDATE,
    BATTLEYE_UNKNOWN,
    LEVEL_VERSION,
    /// <summary>
    /// EconInfo.json hash does not match.
    /// </summary>
    ECON_HASH,
    /// <summary>
    /// Master bundle hashes do not match.
    /// </summary>
    HASH_MASTER_BUNDLE,
    REJECT_UNKNOWN,
    WORKSHOP_DOWNLOAD_RESTRICTION,
    /// <summary>
    /// Workshop usage advertised on server list does not match during connect.
    /// </summary>
    WORKSHOP_ADVERTISEMENT_MISMATCH,
    /// <summary>
    /// Used by client transport to show a custom localized message.
    /// </summary>
    CUSTOM,
    /// <summary>
    /// Server has not received an auth session response from Steam yet.
    /// </summary>
    LATE_PENDING_STEAM_AUTH,
    /// <summary>
    /// Server has not received an economy response from Steam yet.
    /// </summary>
    LATE_PENDING_STEAM_ECON,
    /// <summary>
    /// Server has not received a groups response from Steam yet.
    /// </summary>
    LATE_PENDING_STEAM_GROUPS,
    /// <summary>
    /// Player nickname exceeds limit.
    /// </summary>
    NAME_PRIVATE_LONG,
    /// <summary>
    /// Player nickname contains invalid characters.
    /// </summary>
    NAME_PRIVATE_INVALID,
    /// <summary>
    /// Player nickname should not be a number.
    /// </summary>
    NAME_PRIVATE_NUMBER,
    /// <summary>
    /// Server did not respond to EServerMessage.Authenticate
    /// </summary>
    TIMED_OUT_LOGIN,
    /// <summary>
    /// Player resources folders don't match.
    /// </summary>
    HASH_RESOURCES,
    /// <summary>
    /// The network identity in the ticket does not match the server authenticating the ticket.
    /// This can happen if server's Steam ID has changed from what the client thinks it is.
    /// For example, joining a stale entry in the server list. (public issue #4101)
    /// </summary>
    AUTH_NETWORK_IDENTITY_FAILURE
}
