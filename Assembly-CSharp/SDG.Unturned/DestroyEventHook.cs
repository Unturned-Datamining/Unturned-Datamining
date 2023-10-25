using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Destroy Event Hook")]
public class DestroyEventHook : MonoBehaviour
{
    /// <summary>
    /// If true the event will only be invoked in offline mode and on the server.
    /// </summary>
    public bool AuthorityOnly;

    public UnityEvent OnDestroyed;

    private void OnDestroy()
    {
        if (!AuthorityOnly || Provider.isServer)
        {
            OnDestroyed.Invoke();
        }
    }
}
