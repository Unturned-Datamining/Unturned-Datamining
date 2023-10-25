namespace SDG.Unturned;

public enum EWorkshopDownloadRestrictionResult
{
    /// <summary>
    /// Workshop item does not have any IP restrictions in place.
    /// </summary>
    NoRestrictions,
    /// <summary>
    /// Workshop item has an IP whitelist, and server IP is not on it.
    /// </summary>
    NotWhitelisted,
    /// <summary>
    /// Workshop item has an IP blacklist, and server IP is on it.
    /// </summary>
    Blacklisted,
    /// <summary>
    /// Workshop item does have IP restrictions, and server IP is allowed.
    /// </summary>
    Allowed,
    /// <summary>
    /// Workshop item has been banned by an admin.
    /// </summary>
    Banned,
    /// <summary>
    /// Workshop item is hidden from everyone.
    /// </summary>
    PrivateVisibility
}
