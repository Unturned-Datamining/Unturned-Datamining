using System;
using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

/// <summary>
/// Can be added to any GameObject to listen for the Event NPC reward type.
/// </summary>
[AddComponentMenu("Unturned/NPC Global Event Hook")]
public class NpcGlobalEventHook : MonoBehaviour
{
    /// <summary>
    /// *_ID configured in NPC rewards list.
    /// </summary>
    public string EventId;

    /// <summary>
    /// If true the event will only be invoked in offline mode and on the server.
    /// </summary>
    public bool AuthorityOnly;

    /// <summary>
    /// Invoked when NPC global event matching EventId is processed.
    /// </summary>
    public UnityEvent OnTriggered;

    private bool isListening;

    private void OnEnable()
    {
        if (!AuthorityOnly || Provider.isServer)
        {
            if (string.IsNullOrWhiteSpace(EventId))
            {
                UnturnedLog.warn("{0} EventId is empty", base.transform.GetSceneHierarchyPath());
            }
            else
            {
                NPCEventManager.onEvent += OnEvent;
                isListening = true;
            }
        }
    }

    private void OnDisable()
    {
        if (isListening)
        {
            NPCEventManager.onEvent -= OnEvent;
            isListening = false;
        }
    }

    private void OnEvent(Player instigatingPlayer, string eventId)
    {
        if (string.Equals(EventId, eventId, StringComparison.OrdinalIgnoreCase))
        {
            OnTriggered.TryInvoke(this);
        }
    }
}
