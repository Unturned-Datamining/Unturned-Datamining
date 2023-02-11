using System.Collections.Generic;
using SDG.Framework.Utilities;
using UnityEngine;

namespace SDG.Unturned;

public class CullingVolumeManager : VolumeManager<CullingVolume, CullingVolumeManager>
{
    private HashSet<CullingVolume> volumesWithObjects = new HashSet<CullingVolume>();

    protected override ELevelVolumeVisibility DefaultVisibility => ELevelVolumeVisibility.Hidden;

    protected override void OnUpdateGizmos(RuntimeGizmos runtimeGizmos)
    {
        base.OnUpdateGizmos(runtimeGizmos);
        foreach (CullingVolume volumesWithObject in volumesWithObjects)
        {
            Color color = (volumesWithObject.isCulled ? Color.red : Color.green);
            runtimeGizmos.Label(volumesWithObject.transform.position, volumesWithObject.objects.Count.ToString(), color);
        }
    }

    public CullingVolumeManager()
    {
        base.FriendlyName = "Manual Object Culling";
        SetDebugColor(new Color32(150, 30, 150, byte.MaxValue));
        TimeUtility.updated += OnUpdateCullingVolumes;
    }

    internal void ClearOverlappingObjects()
    {
        foreach (CullingVolume allVolume in allVolumes)
        {
            if (allVolume.objects != null && allVolume.objects.Count > 0)
            {
                allVolume.ClearObjects();
            }
        }
    }

    internal void RefreshOverlappingObjects()
    {
        foreach (CullingVolume allVolume in allVolumes)
        {
            allVolume.FindObjectsInsideVolume();
        }
    }

    internal void AddVolumeWithObjects(CullingVolume volume)
    {
        volumesWithObjects.Add(volume);
    }

    internal void RemoveVolumeWithObjects(CullingVolume volume)
    {
        volumesWithObjects.Remove(volume);
    }

    private void OnUpdateCullingVolumes()
    {
        if (MainCamera.instance == null)
        {
            return;
        }
        bool forceCull = Level.isEditor && EditorVolumesUI.EditorWantsToPreviewCulling;
        Vector3 position = MainCamera.instance.transform.position;
        foreach (CullingVolume volumesWithObject in volumesWithObjects)
        {
            volumesWithObject.UpdateCulling(position, forceCull);
        }
    }
}
