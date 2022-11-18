using System;
using System.Collections.Generic;
using SDG.Framework.IO.FormattedFiles;
using SDG.Framework.Landscapes;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageTile : IFormattedFileReadable, IFormattedFileWritable
{
    protected FoliageCoord _coord;

    public Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instances;

    public bool hasUnsavedChanges;

    public bool isRelevantToViewer;

    private List<FoliageCut> cuts;

    public FoliageCoord coord
    {
        get
        {
            return _coord;
        }
        protected set
        {
            _coord = value;
            updateBounds();
        }
    }

    public Bounds worldBounds { get; protected set; }

    [Obsolete]
    public void addCut(IShapeVolume cut)
    {
    }

    internal void AddCut(FoliageCut cut)
    {
        cuts.Add(cut);
        foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in instances)
        {
            FoliageInstanceList value = instance.Value;
            for (int i = 0; i < value.matrices.Count; i++)
            {
                List<Matrix4x4> list = value.matrices[i];
                List<bool> list2 = value.clearWhenBaked[i];
                for (int num = list.Count - 1; num >= 0; num--)
                {
                    if (cut.ContainsPoint(list[num].GetPosition()))
                    {
                        list.RemoveAt(num);
                        list2.RemoveAt(num);
                    }
                }
            }
        }
    }

    internal void RemoveCut(FoliageCut cut)
    {
        cuts.RemoveFast(cut);
    }

    public bool isInstanceCut(Vector3 point)
    {
        foreach (FoliageCut cut in cuts)
        {
            if (cut.ContainsPoint(point))
            {
                return true;
            }
        }
        return false;
    }

    public bool isEmpty()
    {
        foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in instances)
        {
            if (!instance.Value.IsListEmpty())
            {
                return false;
            }
        }
        return true;
    }

    public FoliageInstanceList getOrAddList(AssetReference<FoliageInstancedMeshInfoAsset> assetReference)
    {
        if (!instances.TryGetValue(assetReference, out var value))
        {
            value = PoolablePool<FoliageInstanceList>.claim();
            value.assetReference = assetReference;
            instances.Add(assetReference, value);
        }
        return value;
    }

    public void addInstance(FoliageInstanceGroup instance)
    {
        getOrAddList(instance.assetReference).addInstanceRandom(instance);
        updateBounds();
        hasUnsavedChanges = true;
    }

    public void removeInstance(FoliageInstanceList list, int matricesIndex, int matrixIndex)
    {
        list.removeInstance(matricesIndex, matrixIndex);
        hasUnsavedChanges = true;
    }

    public void clearAndReleaseInstances()
    {
        if (instances.Count > 0)
        {
            foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in instances)
            {
                PoolablePool<FoliageInstanceList>.release(instance.Value);
            }
        }
        instances.Clear();
    }

    public void clearGeneratedInstances()
    {
        foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in instances)
        {
            instance.Value.clearGeneratedInstances();
        }
    }

    public void applyScale()
    {
        foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in instances)
        {
            instance.Value.applyScale();
        }
    }

    public virtual void read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        coord = reader.readValue<FoliageCoord>("Coord");
    }

    public virtual void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writer.writeValue("Coord", coord);
        writer.endObject();
    }

    public void updateBounds()
    {
        if (instances.Count > 0)
        {
            float num = Landscape.TILE_HEIGHT;
            float num2 = 0f - Landscape.TILE_HEIGHT;
            foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in instances)
            {
                foreach (List<Matrix4x4> matrix in instance.Value.matrices)
                {
                    foreach (Matrix4x4 item in matrix)
                    {
                        float m = item.m13;
                        if (m < num)
                        {
                            num = m;
                        }
                        if (m > num2)
                        {
                            num2 = m;
                        }
                    }
                }
            }
            float num3 = num2 - num;
            worldBounds = new Bounds(new Vector3((float)coord.x * FoliageSystem.TILE_SIZE + FoliageSystem.TILE_SIZE / 2f, num + num3 / 2f, (float)coord.y * FoliageSystem.TILE_SIZE + FoliageSystem.TILE_SIZE / 2f), new Vector3(FoliageSystem.TILE_SIZE, num3, FoliageSystem.TILE_SIZE));
        }
        else
        {
            worldBounds = new Bounds(new Vector3((float)coord.x * FoliageSystem.TILE_SIZE + FoliageSystem.TILE_SIZE / 2f, 0f, (float)coord.y * FoliageSystem.TILE_SIZE + FoliageSystem.TILE_SIZE / 2f), new Vector3(FoliageSystem.TILE_SIZE, Landscape.TILE_HEIGHT, FoliageSystem.TILE_SIZE));
        }
    }

    public FoliageTile(FoliageCoord newCoord)
    {
        instances = new Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList>();
        coord = newCoord;
        cuts = new List<FoliageCut>();
    }
}
