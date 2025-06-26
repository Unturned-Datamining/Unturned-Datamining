using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allow Unity events to forcefully remove any barricades inside a sphere.
/// </summary>
[AddComponentMenu("Unturned/Barricade Destroyer")]
public class BarricadeDestroyerComponent : MonoBehaviour
{
    [Tooltip("Barricades whose pivot points are within this radius will be removed.")]
    public float Radius;

    [Tooltip("If true, barricade's Explosion effect is played.")]
    public bool PlayEffect = true;

    [Tooltip("If true, barricade's Item_Dropped_On_Destroy is spawned. (Different from stored items.)")]
    public bool SpawnItems = true;

    public void Apply()
    {
        if (Provider.isServer)
        {
            BarricadeManager.DestroyBarricadesInSphere(base.transform.position, Radius, PlayEffect, SpawnItems);
        }
    }
}
