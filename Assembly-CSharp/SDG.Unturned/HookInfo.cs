using System;
using UnityEngine;

namespace SDG.Unturned;

public class HookInfo
{
    [Obsolete("This is vehicle's root transform. Will be removed in a future release.")]
    public Transform target;

    public InteractableVehicle vehicle;

    public Vector3 deltaPosition;

    public Quaternion deltaRotation;
}
