using UnityEngine;

namespace SDG.Unturned;

public class TriggerGrenadeBase : MonoBehaviour
{
    public Transform ignoreTransform;

    private bool isStuck;

    protected virtual void GrenadeTriggered()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isStuck && !other.isTrigger && (!(ignoreTransform != null) || (!(other.transform == ignoreTransform) && !other.transform.IsChildOf(ignoreTransform))))
        {
            isStuck = true;
            GrenadeTriggered();
        }
    }

    private void Awake()
    {
        Collider component = GetComponent<Collider>();
        if (component != null)
        {
            component.isTrigger = true;
            if (component is BoxCollider boxCollider)
            {
                boxCollider.size *= 2f;
            }
        }
    }
}
