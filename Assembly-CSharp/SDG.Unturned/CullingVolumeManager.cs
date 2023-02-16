using System.Collections.Generic;
using SDG.Framework.Utilities;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned;

public class CullingVolumeManager : VolumeManager<CullingVolume, CullingVolumeManager>
{
    private int cullingUpdateIndex;

    private List<CullingVolume> volumesWithObjects = new List<CullingVolume>();

    private HashSet<CullingVolume> volumesWithVisibilityUpdates = new HashSet<CullingVolume>();

    private List<CullingVolume> volumesToRemoveFromUpdatesList = new List<CullingVolume>();

    private CustomSampler relevanceSampler = CustomSampler.Create("CullingVolumeManager.UpdateRelevantCullingVolumes");

    private CustomSampler visibilitySampler = CustomSampler.Create("CullingVolumeManager.UpdateObjectsVisibility");

    private CustomSampler gizmoLabelSampler = CustomSampler.Create("CullingVolumeManager.LabelGizmos");

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
        volumesWithObjects.RemoveFast(volume);
        volumesWithVisibilityUpdates.Remove(volume);
    }

    private void UpdateRelevantCullingVolumes()
    {
        bool forceCull = Level.isEditor && EditorVolumesUI.EditorWantsToPreviewCulling;
        Vector3 position = MainCamera.instance.transform.position;
        int num = Mathf.Min(32, volumesWithObjects.Count);
        for (int i = 0; i < num; i++)
        {
            cullingUpdateIndex++;
            if (cullingUpdateIndex >= volumesWithObjects.Count)
            {
                cullingUpdateIndex = 0;
            }
            CullingVolume cullingVolume = volumesWithObjects[cullingUpdateIndex];
            if (cullingVolume.UpdateCulling(position, forceCull))
            {
                volumesWithVisibilityUpdates.Add(cullingVolume);
            }
        }
    }

    private void UpdateObjectsVisibility()
    {
        foreach (CullingVolume volumesWithVisibilityUpdate in volumesWithVisibilityUpdates)
        {
            volumesWithVisibilityUpdate.UpdateObjectsVisibility();
            if (!volumesWithVisibilityUpdate.HasPendingVisibilityUpdates)
            {
                volumesToRemoveFromUpdatesList.Add(volumesWithVisibilityUpdate);
            }
        }
        volumesWithVisibilityUpdates.ExceptWith(volumesToRemoveFromUpdatesList);
        volumesToRemoveFromUpdatesList.Clear();
    }

    private void OnUpdateCullingVolumes()
    {
        if (!(MainCamera.instance == null))
        {
            UpdateRelevantCullingVolumes();
            UpdateObjectsVisibility();
        }
    }
}
