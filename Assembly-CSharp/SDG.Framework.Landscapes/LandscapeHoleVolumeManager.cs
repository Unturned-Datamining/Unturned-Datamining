using System.Collections.Generic;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Landscapes;

public class LandscapeHoleVolumeManager : VolumeManager<LandscapeHoleVolume, LandscapeHoleVolumeManager>
{
    private struct HoleModification
    {
        public LandscapeTile tile;

        public SplatmapCoord splatmapCoord;

        public HoleModification(LandscapeTile tile, SplatmapCoord splatmapCoord)
        {
            this.tile = tile;
            this.splatmapCoord = splatmapCoord;
        }
    }

    private bool isListeningForUpdates;

    private bool ignoreHolesChanged;

    private HashSet<LandscapeTile> modifiedTiles = new HashSet<LandscapeTile>();

    private List<HoleModification> holeModifications = new List<HoleModification>();

    public void ApplyToTerrain()
    {
        modifiedTiles.Clear();
        holeModifications.Clear();
        if (allVolumes.Count > 0)
        {
            ConvertHoleVolumesToModifications();
            UnturnedLog.info($"Applied {allVolumes.Count} hole volume(s) to {modifiedTiles.Count} terrain tile(s)");
        }
        if (Level.isEditor && !isListeningForUpdates)
        {
            isListeningForUpdates = true;
            ignoreHolesChanged = true;
            TimeUtility.updated += OnUpdateHoles;
        }
    }

    public LandscapeHoleVolumeManager()
    {
        base.FriendlyName = "Landscape Hole (legacy do NOT use!)";
        SetDebugColor(new Color32(71, 44, 20, byte.MaxValue));
        allowInstantiation = false;
    }

    private void OnUpdateHoles()
    {
        if (!Level.isEditor)
        {
            if (isListeningForUpdates)
            {
                isListeningForUpdates = false;
                TimeUtility.updated -= OnUpdateHoles;
            }
        }
        else
        {
            if (allVolumes.Count < 1)
            {
                return;
            }
            bool flag = false;
            foreach (LandscapeHoleVolume allVolume in allVolumes)
            {
                flag |= allVolume.transform.hasChanged;
                allVolume.transform.hasChanged = false;
            }
            if (ignoreHolesChanged)
            {
                ignoreHolesChanged = false;
            }
            else if (flag)
            {
                modifiedTiles.Clear();
                UndoHoleModifications();
                ConvertHoleVolumesToModifications();
            }
        }
    }

    private void UndoHoleModifications()
    {
        foreach (HoleModification holeModification in holeModifications)
        {
            if (holeModification.tile != null)
            {
                modifiedTiles.Add(holeModification.tile);
                holeModification.tile.holes[holeModification.splatmapCoord.x, holeModification.splatmapCoord.y] = true;
            }
        }
        holeModifications.Clear();
    }

    private void ConvertHoleVolumesToModifications()
    {
        foreach (LandscapeHoleVolume allVolume in allVolumes)
        {
            Bounds worldBounds = allVolume.CalculateWorldBounds();
            LandscapeBounds landscapeBounds = new LandscapeBounds(worldBounds);
            for (int i = landscapeBounds.min.x; i <= landscapeBounds.max.x; i++)
            {
                for (int j = landscapeBounds.min.y; j <= landscapeBounds.max.y; j++)
                {
                    LandscapeCoord landscapeCoord = new LandscapeCoord(i, j);
                    LandscapeTile tile = Landscape.getTile(landscapeCoord);
                    if (tile == null)
                    {
                        continue;
                    }
                    modifiedTiles.Add(tile);
                    SplatmapBounds splatmapBounds = new SplatmapBounds(landscapeCoord, worldBounds);
                    for (int k = splatmapBounds.min.x; k <= splatmapBounds.max.x; k++)
                    {
                        for (int l = splatmapBounds.min.y; l <= splatmapBounds.max.y; l++)
                        {
                            SplatmapCoord splatmapCoord = new SplatmapCoord(k, l);
                            Vector3 worldPosition = Landscape.getWorldPosition(landscapeCoord, splatmapCoord);
                            Vector3 vector = allVolume.transform.InverseTransformPoint(worldPosition);
                            Vector3 vector2 = new Vector3(2.828427f, 2.828427f, 2.828427f);
                            vector2.x = Mathf.Abs(vector2.x / allVolume.transform.localScale.x);
                            vector2.y = Mathf.Abs(vector2.y / allVolume.transform.localScale.y);
                            vector2.z = Mathf.Abs(vector2.z / allVolume.transform.localScale.z);
                            if (Mathf.Abs(vector.x) < 0.5f + vector2.x && Mathf.Abs(vector.y) < 0.5f + vector2.y && Mathf.Abs(vector.z) < 0.5f + vector2.z)
                            {
                                tile.holes[k, l] = false;
                                tile.hasAnyHolesData = true;
                                holeModifications.Add(new HoleModification(tile, splatmapCoord));
                            }
                        }
                    }
                }
            }
        }
        foreach (LandscapeTile modifiedTile in modifiedTiles)
        {
            modifiedTile.data.SetHoles(0, 0, modifiedTile.holes);
        }
    }
}
