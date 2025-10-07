using System;
using SDG.NetTransport;

namespace SDG.Unturned;

/// <summary>
/// Allows NPCs to trigger plugin or script events.
/// </summary>
public class NPCEventManager
{
    private static readonly ClientStaticMethod<byte, string> SendBroadcast = ClientStaticMethod<byte, string>.Get(ReceiveBroadcast);

    [Obsolete("onEvent provides the instigating player.")]
    public static event NPCEventTriggeredHandler eventTriggered;

    /// <summary>
    /// instigatingPlayer can be null. For example, if instigated by NpcGlobalEventMessenger.
    /// </summary>
    public static event NPCEventHandler onEvent;

    [Obsolete("broadcastEvent provides the instigating player.")]
    public static void triggerEventTriggered(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            NPCEventManager.eventTriggered?.Invoke(id);
        }
    }

    public static void broadcastEvent(Player instigatingPlayer, string eventId)
    {
        BroadcastEvent(instigatingPlayer, eventId, ENPCEventReplicationMode.AuthorityOnly);
    }

    public static void broadcastEvent(Player instigatingPlayer, string eventId, bool shouldReplicate = false)
    {
        ENPCEventReplicationMode replicationMode = (shouldReplicate ? ENPCEventReplicationMode.AuthorityAndClients : ENPCEventReplicationMode.AuthorityOnly);
        BroadcastEvent(instigatingPlayer, eventId, replicationMode);
    }

    public static void BroadcastEvent(Player instigatingPlayer, string eventId, ENPCEventReplicationMode replicationMode)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return;
        }
        bool flag;
        bool flag2;
        switch (replicationMode)
        {
        default:
            flag = true;
            flag2 = false;
            break;
        case ENPCEventReplicationMode.AuthorityAndClients:
            flag = true;
            flag2 = true;
            break;
        case ENPCEventReplicationMode.InstigatorOnly:
            if (instigatingPlayer == null)
            {
                return;
            }
            flag = instigatingPlayer.channel.IsLocalPlayer;
            flag2 = !flag;
            break;
        }
        if (flag)
        {
            try
            {
                NPCEventManager.onEvent?.Invoke(instigatingPlayer, eventId);
            }
            catch (Exception e)
            {
                UnturnedLog.exception(e, "Exception raised during server NPC event \"{0}\"", eventId);
            }
        }
        if (flag2)
        {
            byte arg = (byte)((instigatingPlayer != null && instigatingPlayer.channel != null && instigatingPlayer.channel.owner != null) ? ((byte)instigatingPlayer.channel.owner.channel) : 0);
            if (replicationMode == ENPCEventReplicationMode.InstigatorOnly)
            {
                SendBroadcast.Invoke(ENetReliability.Reliable, instigatingPlayer.channel?.GetOwnerTransportConnection(), arg, eventId);
            }
            else
            {
                SendBroadcast.Invoke(ENetReliability.Reliable, Provider.GatherRemoteClientConnections(), arg, eventId);
            }
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveBroadcast(byte channelId, string eventId)
    {
        Player instigatingPlayer = PlayerTool.findSteamPlayerByChannel(channelId)?.player;
        try
        {
            NPCEventManager.onEvent?.Invoke(instigatingPlayer, eventId);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception raised during client NPC event \"{0}\"", eventId);
        }
    }
}
