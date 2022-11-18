using SDG.NetPak;

namespace SDG.Unturned;

[NetInvokableGeneratedClass(typeof(NPCEventManager))]
public static class NPCEventManager_NetMethods
{
    [NetInvokableGeneratedMethod("ReceiveBroadcast", ENetInvokableGeneratedMethodPurpose.Read)]
    public static void ReceiveBroadcast_Read(in ClientInvocationContext context)
    {
        NetPakReader reader = context.reader;
        reader.ReadUInt8(out var value);
        reader.ReadString(out var value2);
        NPCEventManager.ReceiveBroadcast(value, value2);
    }

    [NetInvokableGeneratedMethod("ReceiveBroadcast", ENetInvokableGeneratedMethodPurpose.Write)]
    public static void ReceiveBroadcast_Write(NetPakWriter writer, byte channelId, string eventId)
    {
        writer.WriteUInt8(channelId);
        writer.WriteString(eventId);
    }
}
