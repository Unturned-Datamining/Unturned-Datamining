using System;

namespace SDG.Unturned;

[Obsolete]
public enum ESteamPacket
{
    [Obsolete]
    UPDATE_RELIABLE_BUFFER,
    [Obsolete]
    UPDATE_UNRELIABLE_BUFFER,
    [Obsolete]
    UPDATE_RELIABLE_CHUNK_BUFFER,
    [Obsolete]
    UPDATE_UNRELIABLE_CHUNK_BUFFER,
    [Obsolete]
    UPDATE_VOICE,
    [Obsolete]
    SHUTDOWN,
    [Obsolete]
    WORKSHOP,
    [Obsolete]
    CONNECT,
    [Obsolete]
    VERIFY,
    [Obsolete]
    AUTHENTICATE,
    [Obsolete]
    REJECTED,
    [Obsolete]
    ACCEPTED,
    [Obsolete]
    ADMINED,
    [Obsolete]
    UNADMINED,
    [Obsolete]
    BANNED,
    [Obsolete]
    KICKED,
    [Obsolete]
    CONNECTED,
    [Obsolete]
    DISCONNECTED,
    [Obsolete]
    PING_REQUEST,
    [Obsolete]
    PING_RESPONSE,
    [Obsolete("Unused and will kick sender.")]
    UPDATE_RELIABLE_INSTANT,
    [Obsolete("Unused and will kick sender.")]
    UPDATE_UNRELIABLE_INSTANT,
    [Obsolete("Unused and will kick sender.")]
    UPDATE_RELIABLE_CHUNK_INSTANT,
    [Obsolete("Unused and will kick sender.")]
    UPDATE_UNRELIABLE_CHUNK_INSTANT,
    [Obsolete]
    BATTLEYE,
    [Obsolete]
    GUIDTABLE,
    [Obsolete]
    CLIENT_PENDING,
    [Obsolete]
    MAX
}
