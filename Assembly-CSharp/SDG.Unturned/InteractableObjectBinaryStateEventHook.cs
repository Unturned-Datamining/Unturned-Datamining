using UnityEngine;
using UnityEngine.Events;
using Unturned.UnityEx;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/IOBS Event Hook")]
public class InteractableObjectBinaryStateEventHook : MonoBehaviour
{
    public UnityEvent OnStateEnabled;

    public UnityEvent OnStateDisabled;

    public bool InvokeWhenInitialized = true;

    private InteractableObjectBinaryState interactable;

    public void GotoEnabledState()
    {
        if (interactable != null)
        {
            interactable.setUsedFromClientOrServer(newUsed: true);
        }
    }

    public void GotoDisabledState()
    {
        if (interactable != null)
        {
            interactable.setUsedFromClientOrServer(newUsed: false);
        }
    }

    public void ToggleState()
    {
        if (interactable != null)
        {
            interactable.setUsedFromClientOrServer(!interactable.isUsed);
        }
    }

    protected void Start()
    {
        interactable = base.gameObject.GetComponentInParent<InteractableObjectBinaryState>();
        if (interactable == null)
        {
            UnturnedLog.warn("IOBS {0} unable to find interactable", this.GetSceneHierarchyPath());
            return;
        }
        interactable.modHookCounter++;
        interactable.onStateChanged += onStateChanged;
        if (InvokeWhenInitialized)
        {
            interactable.onStateInitialized += onStateChanged;
            onStateChanged(interactable);
        }
    }

    protected void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.onStateInitialized -= onStateChanged;
            interactable.onStateChanged -= onStateChanged;
            interactable.modHookCounter--;
            interactable = null;
        }
    }

    protected void onStateChanged(InteractableObjectBinaryState sender)
    {
        if (sender.isUsed)
        {
            OnStateEnabled.TryInvoke(this);
        }
        else
        {
            OnStateDisabled.TryInvoke(this);
        }
    }
}
