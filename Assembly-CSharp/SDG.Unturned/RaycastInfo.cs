using System;
using UnityEngine;

namespace SDG.Unturned;

public class RaycastInfo
{
    public Transform transform;

    public Collider collider;

    public float distance;

    public Vector3 point;

    public Vector3 direction;

    public Vector3 normal;

    public Player player;

    public Zombie zombie;

    public Animal animal;

    public ELimb limb;

    public string materialName;

    [Obsolete]
    public EPhysicsMaterial material;

    public InteractableVehicle vehicle;

    public byte section;

    public RaycastInfo(RaycastHit hit)
    {
        transform = ((hit.collider != null) ? hit.collider.transform : null);
        collider = hit.collider;
        distance = hit.distance;
        point = hit.point;
        normal = hit.normal;
        section = byte.MaxValue;
    }

    public RaycastInfo(Transform hit)
    {
        transform = hit;
        point = hit.position;
        section = byte.MaxValue;
    }
}
