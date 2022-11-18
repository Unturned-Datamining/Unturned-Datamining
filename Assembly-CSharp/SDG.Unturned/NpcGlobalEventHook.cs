using System;
using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/NPC Global Event Hook")]
public class NpcGlobalEventHook : MonoBehaviour
{
    public string EventId;

    public bool AuthorityOnly;

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
