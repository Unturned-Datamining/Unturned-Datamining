using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows Unity events to broadcast Event NPC rewards.
/// </summary>
[AddComponentMenu("Unturned/NPC Global Event Messenger")]
public class NpcGlobalEventMessenger : MonoBehaviour
{
    /// <summary>
    /// Event ID to use when SendDefaultEventId is invoked.
    /// </summary>
    public string DefaultEventId;

    /// <summary>
    /// The event messenger can only be triggered on the authority (server).
    /// If true, the server will replicate the event to clients.
    /// </summary>
    public bool ShouldReplicate;

    public void SendEventId(string eventId)
    {
        if (Provider.isServer && !string.IsNullOrEmpty(eventId))
        {
            NPCEventManager.broadcastEvent(null, eventId, ShouldReplicate);
        }
    }

    public void SendDefaultEventId()
    {
        SendEventId(DefaultEventId);
    }
}
