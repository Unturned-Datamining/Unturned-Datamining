using UnityEngine;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/NPC Global Event Messenger")]
public class NpcGlobalEventMessenger : MonoBehaviour
{
    public string DefaultEventId;

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
