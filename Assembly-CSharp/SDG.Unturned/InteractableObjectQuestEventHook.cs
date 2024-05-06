using SDG.NetTransport;
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
    /// Only invoked on clients if ShouldReplicate is true.
    /// </summary>
    public UnityEvent OnUsed;

    /// <summary>
    /// If true, the server will replicate the OnUsed event to clients as well.
    /// </summary>
    public bool ShouldReplicate;

    /// <summary>
    /// If ShouldReplicate is enabled, should the RPC be called in reliable mode?
    /// Unreliable might not be received by clients.
    /// </summary>
    public bool Reliable = true;

    /// <summary>
    /// Applied if greater than zero. Defaults to 128.
    /// </summary>
    public float OverrideRelevantDistance;

    private static readonly ClientStaticMethod<Transform> SendUsedNotification = ClientStaticMethod<Transform>.Get(ReceiveUsedNotification);

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

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveUsedNotification(Transform eventHookTransform)
    {
        if (eventHookTransform == null)
        {
            UnturnedLog.info("Received InteractableObjectQuestEventHook.OnUsed event from server, but matching transform doesn't exist on client! Server prefab is likely different from client prefab.");
            return;
        }
        InteractableObjectQuestEventHook component = eventHookTransform.GetComponent<InteractableObjectQuestEventHook>();
        if (component == null)
        {
            UnturnedLog.info("Received InteractableObjectQuestEventHook.OnUsed event from server, but matching transform doesn't have component on client! Server prefab is likely different from client prefab. (" + eventHookTransform.GetSceneHierarchyPath() + ")");
        }
        else
        {
            component.OnUsed.TryInvoke(component);
        }
    }

    private void OnUsedInternal()
    {
        OnUsed.TryInvoke(this);
        if (ShouldReplicate)
        {
            ENetReliability reliability = ((!Reliable) ? ENetReliability.Unreliable : ENetReliability.Reliable);
            float radius = ((OverrideRelevantDistance > 0.01f) ? OverrideRelevantDistance : 128f);
            PooledTransportConnectionList transportConnections = Provider.GatherClientConnectionsWithinSphere(base.transform.position, radius);
            SendUsedNotification.Invoke(reliability, transportConnections, base.transform);
        }
    }
}
