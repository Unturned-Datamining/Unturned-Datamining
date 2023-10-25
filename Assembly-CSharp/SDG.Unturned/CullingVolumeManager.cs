using System.Collections.Generic;
using SDG.Framework.Utilities;
using UnityEngine;
using UnityEngine.Profiling;

namespace SDG.Unturned;

public class CullingVolumeManager : VolumeManager<CullingVolume, CullingVolumeManager>
{
    /// <summary>
    /// True for the next update after the player is teleported.
    /// </summary>
    private bool wasViewTeleported;

    private int cullingUpdateIndex;

    private List<CullingVolume> volumesWithObjects = new List<CullingVolume>();

    private HashSet<CullingVolume> volumesWithVisibilityUpdates = new HashSet<CullingVolume>();

    private List<CullingVolume> volumesToRemoveFromUpdatesList = new List<CullingVolume>();

    private CustomSampler relevanceSampler = CustomSampler.Create("CullingVolumeManager.UpdateRelevantCullingVolumes");

    private CustomSampler visibilitySampler = CustomSampler.Create("CullingVolumeManager.UpdateObjectsVisibility");

    private CustomSampler gizmoLabelSampler = CustomSampler.Create("CullingVolumeManager.LabelGizmos");

    /// <summary>
    /// Hide culling volume by default because new mappers might wonder what these purple boxes
    /// are and why their number goes away after moving objects.
    /// </summary>
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

    /// <summary>
    /// Called by navmesh baking to complete pending object changes that may affect which nav objects are enabled.
    /// </summary>
    internal void ImmediatelySyncAllVolumes()
    {
        wasViewTeleported = true;
        OnUpdateCullingVolumes();
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

    internal void OnPlayerTeleported()
    {
        wasViewTeleported = true;
    }

    /// <summary>
    /// Check a fixed number of volumes for visibility updates per frame.
    /// </summary>
    private void UpdateRelevantCullingVolumes()
    {
        bool forceCull = Level.isEditor && EditorVolumesUI.EditorWantsToPreviewCulling;
        Vector3 position = MainCamera.instance.transform.position;
        int num = (wasViewTeleported ? volumesWithObjects.Count : Mathf.Min(32, volumesWithObjects.Count));
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

    private void SyncAllVolumesVisibility()
    {
        foreach (CullingVolume volumesWithVisibilityUpdate in volumesWithVisibilityUpdates)
        {
            volumesWithVisibilityUpdate.SyncAllObjectsVisibility();
        }
        volumesWithVisibilityUpdates.Clear();
        volumesToRemoveFromUpdatesList.Clear();
    }

    /// <summary>
    /// Any volumes in the process of enabling/disabling get updated once per frame.
    /// </summary>
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
            if (wasViewTeleported)
            {
                SyncAllVolumesVisibility();
            }
            else
            {
                UpdateObjectsVisibility();
            }
            wasViewTeleported = false;
        }
    }
}
