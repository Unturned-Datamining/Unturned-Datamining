using System.Collections.Generic;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class VolumesEditor : SelectionTool
{
    private VolumeManagerBase _activeVolumeManager;

    public VolumeManagerBase activeVolumeManager
    {
        get
        {
            return _activeVolumeManager;
        }
        set
        {
            DevkitSelectionManager.clear();
            _activeVolumeManager = value;
        }
    }

    protected override bool RaycastSelectableObjects(Ray ray, out RaycastHit hitInfo)
    {
        if (activeVolumeManager != null)
        {
            return activeVolumeManager.Raycast(ray, out hitInfo, 8192f);
        }
        hitInfo = default(RaycastHit);
        return false;
    }

    protected override void RequestInstantiation(Vector3 position)
    {
        if (activeVolumeManager != null)
        {
            activeVolumeManager.InstantiateVolume(position, Quaternion.identity, Vector3.one);
        }
    }

    protected override bool HasBoxSelectableObjects()
    {
        return activeVolumeManager != null;
    }

    protected override IEnumerable<GameObject> EnumerateBoxSelectableObjects()
    {
        if (activeVolumeManager == null)
        {
            yield break;
        }
        foreach (VolumeBase item in activeVolumeManager.EnumerateAllVolumes())
        {
            yield return item.areaSelectGameObject;
        }
    }
}
