using UnityEngine;
using UnityEngine.Events;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Destroy Event Hook")]
public class DestroyEventHook : MonoBehaviour
{
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
