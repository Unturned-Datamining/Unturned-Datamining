using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

/// <summary>
/// Can be added to any GameObject with a Dropper, Note, or Quest interactable object in its parents.
/// </summary>
[AddComponentMenu("Unturned/Interactable Object Quest Event Hook")]
public class InteractableObjectQuestEventHook : MonoBehaviour
{
    /// <summary>
    /// Invoked on authority when interactable object is used successfully.
    /// </summary>
    public UnityEvent OnUsed;

    private InteractableObjectTriggerableBase interactable;

    protected void Start()
    {
        interactable = base.gameObject.GetComponentInParent<InteractableObjectTriggerableBase>();
        if (interactable == null)
        {
            UnturnedLog.warn("InteractableObjectQuestEventHook {0} unable to find interactable", this.GetSceneHierarchyPath());
        }
        else
        {
            interactable.OnUsedForModHooks += OnUsedInternal;
        }
    }

    protected void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.OnUsedForModHooks -= OnUsedInternal;
            interactable = null;
        }
    }

    private void OnUsedInternal()
    {
        OnUsed.TryInvoke(this);
    }
}
