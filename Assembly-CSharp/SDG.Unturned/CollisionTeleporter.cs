using UnityEngine;

namespace SDG.Unturned;

[AddComponentMenu("Unturned/Collision Teleporter")]
public class CollisionTeleporter : MonoBehaviour
{
    public Transform DestinationTransform;

    public Color GizmoColor = Color.blue;

    private void OnTriggerEnter(Collider other)
    {
        if (DestinationTransform != null)
        {
            PlayerMovement component = other.gameObject.GetComponent<PlayerMovement>();
            if (component != null && component.CanEnterTeleporter)
            {
                component.EnterCollisionTeleporter(this);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!(DestinationTransform == null))
        {
            BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
            if (!(component == null))
            {
                Gizmos.color = GizmoColor;
                Gizmos.DrawLine(base.transform.position, DestinationTransform.position);
                Gizmos.matrix = base.transform.localToWorldMatrix;
                Gizmos.DrawLine(Vector3.zero, new Vector3(0f, 0f, 1f));
                Gizmos.DrawLine(new Vector3(0f, 0f, 1f), new Vector3(-0.25f, 0f, 0.75f));
                Gizmos.DrawLine(new Vector3(0f, 0f, 1f), new Vector3(0.25f, 0f, 0.75f));
                Gizmos.matrix = DestinationTransform.localToWorldMatrix;
                Gizmos.DrawWireCube(component.center, component.size);
                Gizmos.DrawLine(Vector3.zero, new Vector3(0f, 0f, 1f));
                Gizmos.DrawLine(new Vector3(0f, 0f, 1f), new Vector3(-0.25f, 0f, 0.75f));
                Gizmos.DrawLine(new Vector3(0f, 0f, 1f), new Vector3(0.25f, 0f, 0.75f));
            }
        }
    }
}
