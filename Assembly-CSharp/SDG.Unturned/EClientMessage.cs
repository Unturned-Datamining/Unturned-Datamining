namespace SDG.Unturned;

public enum EClientMessage
{
    UPDATE_RELIABLE_BUFFER,
    UPDATE_UNRELIABLE_BUFFER,
    PingRequest,
    PingResponse,
    Shutdown,
    PlayerConnected,
    PlayerDisconnected,
    DownloadWorkshopFiles,
    Verify,
    Accepted,
    Rejected,
    Banned,
    Kicked,
    Admined,
    Unadmined,
    BattlEye,
    QueuePositionChanged,
    InvokeMethod
}
