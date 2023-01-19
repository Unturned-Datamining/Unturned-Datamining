using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SDG.Framework.Devkit;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageStorageV2 : IFoliageStorage
{
    private class TilePerAssetData
    {
        public AssetReference<FoliageInstancedMeshInfoAsset> assetRef;

        public List<Matrix4x4> matrices;

        public List<bool> clearWhenBaked;
    }

    private class TileData
    {
        public FoliageCoord coord;

        public List<TilePerAssetData> perAssetData;
    }

    private List<KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList>> tileInstanceListsToSave = new List<KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList>>();

    private const int FOLIAGE_FILE_VERSION_INITIAL = 1;

    private const int FOLIAGE_FILE_VERSION_ADDED_ASSET_LIST_HEADER = 2;

    private const int FOLIAGE_FILE_VERSION_NEWEST = 2;

    private byte[] GUID_BUFFER = new byte[16];

    private FileStream readerStream;

    private BinaryReader reader;

    private bool hasAllTilesInMemory;

    private List<FoliageTile> mainThreadTilesWithRelevancyChanges = new List<FoliageTile>();

    private Dictionary<FoliageCoord, long> tileBlobOffsets = new Dictionary<FoliageCoord, long>();

    private List<AssetReference<FoliageInstancedMeshInfoAsset>> assetsHeader = new List<AssetReference<FoliageInstancedMeshInfoAsset>>();

    private int loadedFileVersion;

    private long tileBlobHeaderOffset;

    private Thread workerThread;

    private bool shouldWorkerThreadContinue;

    private TileData mainThreadTileDataFromPreviousUpdate;

    private object lockObject;

    private LinkedList<FoliageCoord> workerThreadTileQueue = new LinkedList<FoliageCoord>();

    private Queue<TileData> tileDataFromWorkerThread = new Queue<TileData>();

    private List<TileData> tileDataFromMainThread = new List<TileData>();

    private Stack<TilePerAssetData> perAssetDataPool;

    private Stack<TileData> tileDataPool;

    public void Initialize()
    {
        tileBlobOffsets.Clear();
        tileBlobHeaderOffset = 0L;
        assetsHeader.Clear();
        loadedFileVersion = 0;
        string path = Level.info.path + "/Foliage.blob";
        if (!File.Exists(path))
        {
            return;
        }
        readerStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        reader = new BinaryReader(readerStream);
        using (SHA1Stream sHA1Stream = new SHA1Stream(readerStream))
        {
            using BinaryReader binaryReader = new BinaryReader(sHA1Stream);
            loadedFileVersion = binaryReader.ReadInt32();
            int num = binaryReader.ReadInt32();
            UnturnedLog.info("Found {0} foliage v2 tiles", num);
            for (int i = 0; i < num; i++)
            {
                int new_x = binaryReader.ReadInt32();
                int new_y = binaryReader.ReadInt32();
                long value = binaryReader.ReadInt64();
                tileBlobOffsets.Add(new FoliageCoord(new_x, new_y), value);
            }
            if (loadedFileVersion >= 2)
            {
                int num2 = binaryReader.ReadInt32();
                UnturnedLog.info("Found {0} foliage used assets in header", num2);
                assetsHeader.Capacity = num2;
                for (int j = 0; j < num2; j++)
                {
                    GuidBuffer guidBuffer = default(GuidBuffer);
                    binaryReader.Read(GUID_BUFFER, 0, 16);
                    guidBuffer.Read(GUID_BUFFER, 0);
                    AssetReference<FoliageInstancedMeshInfoAsset> item = new AssetReference<FoliageInstancedMeshInfoAsset>(guidBuffer.GUID);
                    assetsHeader.Add(item);
                    if (item.Find() == null)
                    {
                        ClientAssetIntegrity.ServerAddKnownMissingAsset(item.GUID, $"Foliage asset {j + 1} of {num2}");
                    }
                }
            }
            tileBlobHeaderOffset = readerStream.Position;
            Level.includeHash("Foliage", sHA1Stream.Hash);
        }
        if (Level.isEditor && loadedFileVersion < 2)
        {
            LevelHierarchy.MarkDirty();
        }
        if (!Level.isEditor && !Dedicator.IsDedicatedServer)
        {
            shouldWorkerThreadContinue = true;
            lockObject = new object();
            workerThread = new Thread(WorkerThreadMain);
            workerThread.Name = "Foliage Storage Thread";
            workerThread.Start();
        }
    }

    public void Shutdown()
    {
        if (workerThread != null)
        {
            shouldWorkerThreadContinue = false;
        }
        else
        {
            CloseReader();
        }
    }

    public void TileBecameRelevantToViewer(FoliageTile tile)
    {
        if (!hasAllTilesInMemory && !mainThreadTilesWithRelevancyChanges.Contains(tile))
        {
            mainThreadTilesWithRelevancyChanges.Add(tile);
        }
    }

    public void TileNoLongerRelevantToViewer(FoliageTile tile)
    {
        if (!hasAllTilesInMemory)
        {
            tile.clearAndReleaseInstances();
            if (!mainThreadTilesWithRelevancyChanges.Contains(tile))
            {
                mainThreadTilesWithRelevancyChanges.Add(tile);
            }
        }
    }

    public void Update()
    {
        if (workerThread == null)
        {
            return;
        }
        TileData tileData = null;
        lock (lockObject)
        {
            foreach (FoliageTile mainThreadTilesWithRelevancyChange in mainThreadTilesWithRelevancyChanges)
            {
                if (mainThreadTilesWithRelevancyChange.isRelevantToViewer)
                {
                    if (!workerThreadTileQueue.Contains(mainThreadTilesWithRelevancyChange.coord))
                    {
                        workerThreadTileQueue.AddLast(mainThreadTilesWithRelevancyChange.coord);
                    }
                }
                else
                {
                    workerThreadTileQueue.Remove(mainThreadTilesWithRelevancyChange.coord);
                }
            }
            if (tileDataFromWorkerThread.Count > 0)
            {
                tileData = tileDataFromWorkerThread.Dequeue();
            }
            if (mainThreadTileDataFromPreviousUpdate != null)
            {
                tileDataFromMainThread.Add(mainThreadTileDataFromPreviousUpdate);
                mainThreadTileDataFromPreviousUpdate = null;
            }
        }
        mainThreadTilesWithRelevancyChanges.Clear();
        if (tileData != null)
        {
            FoliageTile tile = FoliageSystem.getTile(tileData.coord);
            if (tile != null && tile.isRelevantToViewer)
            {
                tile.clearAndReleaseInstances();
                DeserializeTileOnMainThreadUsingDataFromWorkerThread(tile, tileData);
            }
            mainThreadTileDataFromPreviousUpdate = tileData;
        }
    }

    public void EditorLoadAllTiles(IEnumerable<FoliageTile> tiles)
    {
        hasAllTilesInMemory = true;
        foreach (FoliageTile tile in tiles)
        {
            DeserializeTileOnMainThread(tile);
        }
        CloseReader();
    }

    public void EditorSaveAllTiles(IEnumerable<FoliageTile> tiles)
    {
        string path = Level.info.path + "/Foliage.blob";
        if (File.Exists(path) && loadedFileVersion >= 2)
        {
            bool flag = false;
            foreach (FoliageTile tile in tiles)
            {
                if (tile.hasUnsavedChanges)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return;
            }
        }
        List<byte[]> list = new List<byte[]>();
        Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, int> assetRefToIndex = new Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, int>();
        tileBlobOffsets.Clear();
        assetsHeader.Clear();
        long num = 0L;
        foreach (FoliageTile tile2 in tiles)
        {
            if (!tile2.isEmpty())
            {
                byte[] array = SerializeTileOnMainThread(tile2, assetRefToIndex);
                if (array != null && array.Length != 0)
                {
                    list.Add(array);
                    tileBlobOffsets.Add(tile2.coord, num);
                    num += array.LongLength;
                }
            }
        }
        if (list.Count != tileBlobOffsets.Count)
        {
            UnturnedLog.error("Foliage blob count ({0}) does not match offset count ({1})", list.Count, tileBlobOffsets.Count);
            return;
        }
        using FileStream output = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        BinaryWriter binaryWriter = new BinaryWriter(output);
        binaryWriter.Write(2);
        binaryWriter.Write(tileBlobOffsets.Count);
        foreach (KeyValuePair<FoliageCoord, long> tileBlobOffset in tileBlobOffsets)
        {
            binaryWriter.Write(tileBlobOffset.Key.x);
            binaryWriter.Write(tileBlobOffset.Key.y);
            binaryWriter.Write(tileBlobOffset.Value);
        }
        UnturnedLog.info($"Foliage saving with {assetsHeader.Count} assets in header");
        binaryWriter.Write(assetsHeader.Count);
        foreach (AssetReference<FoliageInstancedMeshInfoAsset> item in assetsHeader)
        {
            new GuidBuffer(item.GUID).Write(GUID_BUFFER, 0);
            binaryWriter.Write(GUID_BUFFER, 0, 16);
        }
        foreach (byte[] item2 in list)
        {
            binaryWriter.Write(item2);
        }
    }

    private TileData GetTileDataOnWorkerThread(FoliageCoord coord)
    {
        TileData tileData;
        if (tileDataPool.Count > 0)
        {
            tileData = tileDataPool.Pop();
        }
        else
        {
            tileData = new TileData();
            tileData.perAssetData = new List<TilePerAssetData>();
        }
        tileData.coord = coord;
        if (tileBlobOffsets.TryGetValue(coord, out var value))
        {
            readerStream.Position = tileBlobHeaderOffset + value;
            int num = reader.ReadInt32();
            tileData.perAssetData.Capacity = Mathf.Max(tileData.perAssetData.Capacity, num);
            for (int i = 0; i < num; i++)
            {
                AssetReference<FoliageInstancedMeshInfoAsset> assetRef;
                if (loadedFileVersion >= 2)
                {
                    int num2 = reader.ReadInt32();
                    assetRef = ((num2 < 0 || num2 >= assetsHeader.Count) ? AssetReference<FoliageInstancedMeshInfoAsset>.invalid : assetsHeader[num2]);
                }
                else
                {
                    GuidBuffer guidBuffer = default(GuidBuffer);
                    readerStream.Read(GUID_BUFFER, 0, 16);
                    guidBuffer.Read(GUID_BUFFER, 0);
                    assetRef = new AssetReference<FoliageInstancedMeshInfoAsset>(guidBuffer.GUID);
                }
                int num3 = reader.ReadInt32();
                TilePerAssetData tilePerAssetData;
                if (perAssetDataPool.Count > 0)
                {
                    tilePerAssetData = perAssetDataPool.Pop();
                    tilePerAssetData.matrices.Capacity = Mathf.Max(tilePerAssetData.matrices.Capacity, num3);
                    tilePerAssetData.clearWhenBaked.Capacity = Mathf.Max(tilePerAssetData.clearWhenBaked.Capacity, num3);
                }
                else
                {
                    tilePerAssetData = new TilePerAssetData();
                    tilePerAssetData.matrices = new List<Matrix4x4>(num3);
                    tilePerAssetData.clearWhenBaked = new List<bool>(num3);
                }
                tilePerAssetData.assetRef = assetRef;
                for (int j = 0; j < num3; j++)
                {
                    Matrix4x4 item = default(Matrix4x4);
                    for (int k = 0; k < 16; k++)
                    {
                        item[k] = reader.ReadSingle();
                    }
                    tilePerAssetData.matrices.Add(item);
                    bool item2 = reader.ReadBoolean();
                    tilePerAssetData.clearWhenBaked.Add(item2);
                }
                if (num3 > 0)
                {
                    tileData.perAssetData.Add(tilePerAssetData);
                }
            }
        }
        return tileData;
    }

    private void DeserializeTileOnMainThreadUsingDataFromWorkerThread(FoliageTile tile, TileData tileData)
    {
        foreach (TilePerAssetData perAssetDatum in tileData.perAssetData)
        {
            if (perAssetDatum.assetRef.isNull)
            {
                UnturnedLog.error($"Foliage loaded invalid asset ref for tile {tile.coord}");
                continue;
            }
            FoliageInstanceList orAddList = tile.getOrAddList(perAssetDatum.assetRef);
            for (int i = 0; i < perAssetDatum.matrices.Count; i++)
            {
                Matrix4x4 matrix4x = perAssetDatum.matrices[i];
                bool newClearWhenBaked = perAssetDatum.clearWhenBaked[i];
                if (!tile.isInstanceCut(matrix4x.GetPosition()))
                {
                    orAddList.addInstanceAppend(new FoliageInstanceGroup(perAssetDatum.assetRef, matrix4x, newClearWhenBaked));
                }
            }
        }
    }

    private void DeserializeTileOnMainThread(FoliageTile tile)
    {
        if (tileBlobOffsets.TryGetValue(tile.coord, out var value))
        {
            readerStream.Position = tileBlobHeaderOffset + value;
            int num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                AssetReference<FoliageInstancedMeshInfoAsset> assetReference;
                bool flag;
                if (loadedFileVersion >= 2)
                {
                    int num2 = reader.ReadInt32();
                    if (num2 >= 0 && num2 < assetsHeader.Count)
                    {
                        assetReference = assetsHeader[num2];
                        flag = !assetReference.isNull;
                    }
                    else
                    {
                        assetReference = AssetReference<FoliageInstancedMeshInfoAsset>.invalid;
                        UnturnedLog.error($"Foliage loaded invalid asset index {num2} for tile {tile.coord}");
                        flag = false;
                    }
                }
                else
                {
                    GuidBuffer guidBuffer = default(GuidBuffer);
                    readerStream.Read(GUID_BUFFER, 0, 16);
                    guidBuffer.Read(GUID_BUFFER, 0);
                    assetReference = new AssetReference<FoliageInstancedMeshInfoAsset>(guidBuffer.GUID);
                    flag = !assetReference.isNull;
                }
                FoliageInstanceList orAddList = tile.getOrAddList(assetReference);
                int num3 = reader.ReadInt32();
                for (int j = 0; j < num3; j++)
                {
                    Matrix4x4 matrix4x = default(Matrix4x4);
                    for (int k = 0; k < 16; k++)
                    {
                        matrix4x[k] = reader.ReadSingle();
                    }
                    bool newClearWhenBaked = reader.ReadBoolean();
                    if (flag && !tile.isInstanceCut(matrix4x.GetPosition()))
                    {
                        orAddList.addInstanceAppend(new FoliageInstanceGroup(assetReference, matrix4x, newClearWhenBaked));
                    }
                }
            }
        }
        tile.updateBounds();
    }

    private int GetOrAddAssetIndex(AssetReference<FoliageInstancedMeshInfoAsset> assetRef, Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, int> assetRefToIndex)
    {
        if (assetRefToIndex.TryGetValue(assetRef, out var value))
        {
            return value;
        }
        int count = assetsHeader.Count;
        assetsHeader.Add(assetRef);
        assetRefToIndex.Add(assetRef, count);
        return count;
    }

    private byte[] SerializeTileOnMainThread(FoliageTile tile, Dictionary<AssetReference<FoliageInstancedMeshInfoAsset>, int> assetRefToIndex)
    {
        tileInstanceListsToSave.Clear();
        foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> instance in tile.instances)
        {
            if (!instance.Key.isNull && !instance.Value.IsListEmpty())
            {
                if (!LevelObjects.preserveMissingAssets && instance.Key.Find() == null)
                {
                    UnturnedLog.info($"Discarding missing foliage asset {instance.Key} from tile {tile.coord}");
                }
                else
                {
                    tileInstanceListsToSave.Add(instance);
                }
            }
        }
        if (tileInstanceListsToSave.Count < 1)
        {
            return null;
        }
        using MemoryStream memoryStream = new MemoryStream();
        BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
        binaryWriter.Write(tileInstanceListsToSave.Count);
        foreach (KeyValuePair<AssetReference<FoliageInstancedMeshInfoAsset>, FoliageInstanceList> item in tileInstanceListsToSave)
        {
            int orAddAssetIndex = GetOrAddAssetIndex(item.Key, assetRefToIndex);
            binaryWriter.Write(orAddAssetIndex);
            int num = 0;
            foreach (List<Matrix4x4> matrix in item.Value.matrices)
            {
                num += matrix.Count;
            }
            binaryWriter.Write(num);
            for (int i = 0; i < item.Value.matrices.Count; i++)
            {
                List<Matrix4x4> list = item.Value.matrices[i];
                List<bool> list2 = item.Value.clearWhenBaked[i];
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
        return memoryStream.ToArray();
    }

    private void CloseReader()
    {
        if (reader != null)
        {
            reader.Close();
            reader.Dispose();
            reader = null;
        }
        if (readerStream != null)
        {
            readerStream.Close();
            readerStream.Dispose();
            readerStream = null;
        }
    }

    private void WorkerThreadMain()
    {
        perAssetDataPool = new Stack<TilePerAssetData>();
        tileDataPool = new Stack<TileData>();
        TileData tileData = null;
        while (shouldWorkerThreadContinue)
        {
            FoliageCoord coord = default(FoliageCoord);
            bool flag = false;
            lock (lockObject)
            {
                if (workerThreadTileQueue.Count > 0)
                {
                    coord = workerThreadTileQueue.First.Value;
                    workerThreadTileQueue.RemoveFirst();
                    flag = true;
                }
                if (tileData != null)
                {
                    tileDataFromWorkerThread.Enqueue(tileData);
                    tileData = null;
                }
                foreach (TileData item in tileDataFromMainThread)
                {
                    foreach (TilePerAssetData perAssetDatum in item.perAssetData)
                    {
                        perAssetDatum.matrices.Clear();
                        perAssetDatum.clearWhenBaked.Clear();
                        perAssetDataPool.Push(perAssetDatum);
                    }
                    item.perAssetData.Clear();
                    tileDataPool.Push(item);
                }
                tileDataFromMainThread.Clear();
            }
            if (flag)
            {
                tileData = GetTileDataOnWorkerThread(coord);
            }
            Thread.Sleep(10);
        }
        CloseReader();
    }
}
