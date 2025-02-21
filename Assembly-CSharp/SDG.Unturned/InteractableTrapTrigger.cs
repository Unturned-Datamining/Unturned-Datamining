using UnityEngine;

namespace SDG.Unturned;

public class InteractableTrapTrigger : MonoBehaviour
{
    public InteractableTrap parentTrap;

    private void OnTriggerEnter(Collider other)
    {
        if (parentTrap != null)
        {
            parentTrap.NotifyTrapEntered(other);
        }
    }
}
