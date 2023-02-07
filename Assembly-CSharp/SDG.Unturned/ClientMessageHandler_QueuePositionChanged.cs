using SDG.NetPak;

namespace SDG.Unturned;

internal static class ClientMessageHandler_QueuePositionChanged
{
    internal static void ReadMessage(NetPakReader reader)
    {
        if (Provider.isWaitingForConnectResponse)
        {
            Provider.isWaitingForConnectResponse = false;
            UnturnedLog.info("Connection pending verification");
        }
        byte queuePosition = Provider.queuePosition;
        reader.ReadUInt8(out var value);
        Provider._queuePosition = value;
        if (queuePosition != value)
        {
            UnturnedLog.info("Queue position: {0}", Provider.queuePosition);
        }
        Provider.onQueuePositionUpdated?.Invoke();
    }
}
