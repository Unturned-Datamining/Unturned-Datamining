namespace SDG.Unturned;

/// <summary>
/// When adding or removing entries remember to update NetMessages size and regenerate NetCode!
/// </summary>
public enum EClientMessage
{
    UPDATE_RELIABLE_BUFFER,
    UPDATE_UNRELIABLE_BUFFER,
    /// <summary>
    /// Server sent a ping.
    /// </summary>
    PingRequest,
    /// <summary>
    /// Server replying to our ping.
    /// </summary>
    PingResponse,
    /// <summary>
    /// Server is shutting down shortly.
    /// </summary>
    Shutdown,
    /// <summary>
    /// Create game object for player.
    /// </summary>
    PlayerConnected,
    /// <summary>
    /// Destroy game object for player.
    /// </summary>
    PlayerDisconnected,
    /// <summary>
    /// Download these files before loading the level.
    /// </summary>
    DownloadWorkshopFiles,
    /// <summary>
    /// Server wants additional info before accepting us.
    /// </summary>
    Verify,
    /// <summary>
    /// Server has accepted us and will create a player game object.
    /// </summary>
    Accepted,
    /// <summary>
    /// Server rejected us, we will go back to the menu.
    /// </summary>
    Rejected,
    /// <summary>
    /// Banned either during connect or gameplay.
    /// </summary>
    Banned,
    /// <summary>
    /// Kicked during gameplay.
    /// </summary>
    Kicked,
    /// <summary>
    /// Should be converted to an RPC. Leftover from prior to net messaging code.
    /// </summary>
    Admined,
    /// <summary>
    /// Should be converted to an RPC. Leftover from prior to net messaging code.
    /// </summary>
    Unadmined,
    /// <summary>
    /// Server sending BattlEye payload to client.
    /// </summary>
    BattlEye,
    /// <summary>
    /// Infrequent notification of queue position.
    /// </summary>
    QueuePositionChanged,
    /// <summary>
    /// Server calling an RPC.
    /// </summary>
    InvokeMethod
}
