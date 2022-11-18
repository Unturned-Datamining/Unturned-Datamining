using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public abstract class VolumeManagerBase
{
    internal static List<VolumeManagerBase> allManagers = new List<VolumeManagerBase>();

    public string FriendlyName { get; protected set; }

    public virtual ELevelVolumeVisibility Visibility { get; set; }

    public abstract bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance);

    public abstract void InstantiateVolume(Vector3 position, Quaternion rotation, Vector3 scale);

    public abstract IEnumerable<VolumeBase> EnumerateAllVolumes();
}
