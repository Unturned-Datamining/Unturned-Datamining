using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageStorageV1 : IFoliageStorage
{
    protected bool hasAllTilesInMemory;

    protected LinkedList<FoliageTile> pendingLoad = new LinkedList<FoliageTile>();

    private readonly int FOLIAGE_FILE_VERSION = 3;

    private byte[] GUID_BUFFER = new byte[16];

    public void Initialize()
    {
    }

    public void Shutdown()
    {
    }

    public void TileBecameRelevantToViewer(FoliageTile tile)
    {
        if (!hasAllTilesInMemory)
        {
            pendingLoad.AddLast(tile);
        }
    }

    public void TileNoLongerRelevantToViewer(FoliageTile tile)
    {
        if (!hasAllTilesInMemory)
        {
            pendingLoad.Remove(tile);
            tile.clearAndReleaseInstances();
        }
    }

    public void Update()
    {
        if (pendingLoad.Count > 0)
        {
            FoliageTile value = pendingLoad.First.Value;
            pendingLoad.RemoveFirst();
            readInstances(value);
        }
    }

    public void EditorLoadAllTiles(IEnumerable<FoliageTile> tiles)
    {
        hasAllTilesInMemory = true;
        foreach (FoliageTile tile in tiles)
        {
            readInstances(tile);
        }
    }

    public void EditorSaveAllTiles(IEnumerable<FoliageTile> tiles)
    {
        foreach (FoliageTile tile in tiles)
        {
            if (tile.hasUnsavedChanges)
            {
                tile.hasUnsavedChanges = false;
                writeInstances(tile);
            }
        }
    }

    protected string formatTilePath(FoliageTile tile)
    {
        string text = tile.coord.x.ToString(CultureInfo.InvariantCulture);
        string text2 = tile.coord.y.ToString(CultureInfo.InvariantCulture);
        return Level.info.path + "/Foliage/Tile_" + text + "_" + text2 + ".foliage";
    }

    protected void readInstances(FoliageTile tile)
    {
        string path = formatTilePath(tile);
        if (File.Exists(path))
        {
            using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            int num = binaryReader.ReadInt32();
            int num2 = binaryReader.ReadInt32();
            for (int i = 0; i < num2; i++)
            {
                GuidBuffer guidBuffer = default(GuidBuffer);
                fileStream.Read(GUID_BUFFER, 0, 16);
                guidBuffer.Read(GUID_BUFFER, 0);
                AssetReference<FoliageInstancedMeshInfoAsset> assetReference = new AssetReference<FoliageInstancedMeshInfoAsset>(guidBuffer.GUID);
                FoliageInstanceList orAddList = tile.getOrAddList(assetReference);
                int num3 = binaryReader.ReadInt32();
                for (int j = 0; j < num3; j++)
                {
                    Matrix4x4 matrix4x = default(Matrix4x4);
                    for (int k = 0; k < 16; k++)
                    {
                        matrix4x[k] = binaryReader.ReadSingle();
                    }
                    bool newClearWhenBaked = num <= 2 || binaryReader.ReadBoolean();
                    if (!tile.isInstanceCut(matrix4x.GetPosition()))
                    {
                        orAddList.addInstanceAppend(new FoliageInstanceGroup(assetReference, matrix4x, newClearWhenBaked));
                    }
                }
            }
        }
        tile.updateBounds();
    }

    public void writeInstances(FoliageTile tile)
    {
        string path = formatTilePath(tile);
        string directoryName = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
        using FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        BinaryWriter binaryWriter = new BinaryWriter(fileStream);
        binaryWriter.Write(FOLIAGE_FILE_VERSION);
        binaryWriter.Write(tile.instances.Count);
        foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in tile.instances)
        {
            GuidBuffer guidBuffer = new GuidBuffer(instance.Key.GUID);
            guidBuffer.Write(GUID_BUFFER, 0);
            fileStream.Write(GUID_BUFFER, 0, 16);
            int num = 0;
            foreach (List<Matrix4x4> matrix in instance.Value.matrices)
            {
                num += matrix.Count;
            }
            binaryWriter.Write(num);
            for (int i = 0; i < instance.Value.matrices.Count; i++)
            {
                List<Matrix4x4> list = instance.Value.matrices[i];
                List<bool> list2 = instance.Value.clearWhenBaked[i];
                for (int j = 0; j < list.Count; j++)
                {
                    Matrix4x4 matrix4x = list[j];
                    for (int k = 0; k < 16; k++)
                    {
                        binaryWriter.Write(matrix4x[k]);
                    }
                    bool value = list2[j];
                    binaryWriter.Write(value);
                }
            }
        }
    }
}
