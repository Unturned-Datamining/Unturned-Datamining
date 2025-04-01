using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Tracks activation and deactivation of Regions as camera moves around the level.
/// </summary>
public class RegionIncrementalVisibilityTracker
{
    private Vector2Int _cameraCoord;

    private bool hasOldCoords;

    private Dictionary<Vector2Int, RegionVisibilityData> regions = new Dictionary<Vector2Int, RegionVisibilityData>();

    private RegionRadiusMask mask = new RegionRadiusMask();

    private HashSet<Vector2Int> oldCoords = new HashSet<Vector2Int>();

    private HashSet<Vector2Int> newCoords = new HashSet<Vector2Int>();

    private HashSet<Vector2Int> updatingCoords = new HashSet<Vector2Int>();

    public Vector2Int CameraCoord
    {
        get
        {
            return _cameraCoord;
        }
        set
        {
            if (!(_cameraCoord == value))
            {
                PopulateCoordsInsideMask(oldCoords);
                _cameraCoord = value;
                PopulateCoordsInsideMask(newCoords);
                ApplyCoordChanges();
                hasOldCoords = true;
            }
        }
    }

    public float MaxDistance
    {
        get
        {
            return mask.Radius;
        }
        set
        {
            if (!Mathf.Approximately(mask.Radius, value))
            {
                PopulateCoordsInsideMask(oldCoords);
                mask.Radius = value;
                PopulateCoordsInsideMask(newCoords);
                ApplyCoordChanges();
                hasOldCoords = true;
            }
        }
    }

    /// <summary>
    /// Mark all cells as finished loading and remove cells outside the camera view.
    /// Used after teleporting or loading.
    /// </summary>
    public void FlushProgress()
    {
        regions.Clear();
        updatingCoords.Clear();
        PopulateCoordsInsideMask(newCoords);
        hasOldCoords = true;
        foreach (Vector2Int newCoord in newCoords)
        {
            regions[newCoord] = new RegionVisibilityData
            {
                progressIndex = -1,
                isInsideMask = true
            };
        }
    }

    /// <summary>
    /// Caller passes an empty dictionary to be filled with update info.
    /// Increments progressIndex for each returned region.
    /// If region is finished updating, call NotifyRegionFinishedUpdating.
    /// </summary>
    public void UpdateRegions(Dictionary<Vector2Int, RegionVisibilityData> result)
    {
        result.Clear();
        foreach (Vector2Int updatingCoord in updatingCoords)
        {
            RegionVisibilityData value = regions[updatingCoord];
            result.Add(updatingCoord, value);
            value.progressIndex++;
            regions[updatingCoord] = value;
        }
    }

    /// <summary>
    /// Called when progressIndex has reached end of given region.
    /// </summary>
    public void NotifyRegionFinishedUpdating(Vector2Int coord)
    {
        if (regions.TryGetValue(coord, out var value))
        {
            if (value.isInsideMask)
            {
                value.progressIndex = -1;
                regions[coord] = value;
            }
            else
            {
                regions.Remove(coord);
            }
        }
        updatingCoords.Remove(coord);
    }

    public bool IsRegionUpdating(Vector2Int coord)
    {
        return updatingCoords.Contains(coord);
    }

    /// <summary>
    /// Fill output set with mask offsets applied to current camera coordinate.
    /// </summary>
    private void PopulateCoordsInsideMask(HashSet<Vector2Int> coords)
    {
        coords.Clear();
        foreach (Vector2Int offset in mask.Offsets)
        {
            coords.Add(_cameraCoord + offset);
        }
    }

    /// <summary>
    /// Find changes between old and current coordinate sets to mark regions in/out of mask.
    /// </summary>
    private void ApplyCoordChanges()
    {
        if (hasOldCoords)
        {
            foreach (Vector2Int oldCoord in oldCoords)
            {
                if (!newCoords.Contains(oldCoord))
                {
                    SetCoordIsInsideMask(oldCoord, isInsideMask: false);
                }
            }
            {
                foreach (Vector2Int newCoord in newCoords)
                {
                    if (!oldCoords.Contains(newCoord))
                    {
                        SetCoordIsInsideMask(newCoord, isInsideMask: true);
                    }
                }
                return;
            }
        }
        foreach (Vector2Int newCoord2 in newCoords)
        {
            SetCoordIsInsideMask(newCoord2, isInsideMask: true);
        }
    }

    /// <summary>
    /// Reset region's progress counter and change inside/outside status.
    /// </summary>
    private void SetCoordIsInsideMask(Vector2Int coord, bool isInsideMask)
    {
        regions[coord] = new RegionVisibilityData
        {
            progressIndex = 0,
            isInsideMask = isInsideMask
        };
        updatingCoords.Add(coord);
    }
}
