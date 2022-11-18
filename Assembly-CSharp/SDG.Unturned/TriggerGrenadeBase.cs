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
        BoxCollider component = GetComponent<BoxCollider>();
        component.isTrigger = true;
        component.size *= 2f;
    }
}
