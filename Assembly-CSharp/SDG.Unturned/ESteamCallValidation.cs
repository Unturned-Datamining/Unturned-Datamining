namespace SDG.Unturned;

public enum ESteamCallValidation
{
    NONE,
    /// <summary>
    /// Only RPCs from the server will be allowed to invoke this method.
    /// </summary>
    ONLY_FROM_SERVER,
    /// <summary>
    /// RPCs are only allowed to invoke this method if we're running as server.
    /// </summary>
    SERVERSIDE,
    /// <summary>
    /// Only RPCs from the owner of the object will be allowed to invoke this method.
    /// </summary>
    ONLY_FROM_OWNER
}
