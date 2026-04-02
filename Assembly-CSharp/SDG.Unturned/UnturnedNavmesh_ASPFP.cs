using System;
using Pathfinding;
using Pathfinding.Graphs.Navmesh;
using Pathfinding.Util;
using UnityEngine;

namespace SDG.Unturned;

public class UnturnedNavmesh_ASPFP : IUnturnedNavmeshInterface
{
    public RecastGraph graph;

    public bool ContainsAnyBakedData
    {
        get
        {
            if (graph.tileXCount > 0)
            {
                return graph.tileZCount > 0;
            }
            return false;
        }
    }

    public UnturnedNavmesh_ASPFP()
    {
        System.Action callback = delegate
        {
            graph = AstarPath.active.data.AddGraph<RecastGraph>();
            graph.cellSize = 0.1f;
            graph.useTiles = true;
            graph.editorTileSize = 128;
            graph.minRegionSize = 64f;
            graph.walkableHeight = 2f;
            graph.walkableClimb = 0.75f;
            graph.characterRadius = 0.5f;
            graph.maxSlope = 75f;
            graph.maxEdgeLength = 16f;
            graph.contourMaxError = 2f;
            graph.collectionSettings.terrainHeightmapDownsamplingFactor = 1;
            graph.collectionSettings.rasterizeTrees = false;
            graph.collectionSettings.rasterizeMeshes = false;
            graph.collectionSettings.rasterizeColliders = true;
            graph.collectionSettings.colliderRasterizeDetail = 4f;
            graph.collectionSettings.layerMask = RayMasks.BLOCK_NAVMESH;
            graph.enableNavmeshCutting = !Level.isEditor;
        };
        AstarPath.active.AddWorkItem(callback);
        AstarPath.active.FlushWorkItems();
    }

    public void Deserialize(River river)
    {
        System.Action callback = delegate
        {
            TriangleMeshNode.SetNavmeshHolder(AstarPath.active.data.GetGraphIndex(graph), graph);
            graph.forcedBoundsCenter = river.readSingleVector3();
            graph.forcedBoundsSize = river.readSingleVector3();
            graph.tileXCount = river.readByte();
            graph.tileZCount = river.readByte();
            GraphTransform graphTransform = graph.CalculateTransform();
            TileMeshes tileMeshes = default(TileMeshes);
            tileMeshes.tileMeshes = new TileMesh[graph.tileXCount * graph.tileZCount];
            tileMeshes.tileRect = new IntRect(0, 0, graph.tileXCount - 1, graph.tileZCount - 1);
            tileMeshes.tileWorldSize = new Vector2(graph.TileWorldSizeX, graph.TileWorldSizeZ);
            for (int i = 0; i < graph.tileZCount; i++)
            {
                for (int j = 0; j < graph.tileXCount; j++)
                {
                    TileMesh tileMesh = default(TileMesh);
                    tileMesh.triangles = new int[river.readUInt16()];
                    for (int k = 0; k < tileMesh.triangles.Length; k++)
                    {
                        tileMesh.triangles[k] = river.readUInt16();
                    }
                    tileMesh.verticesInTileSpace = new Int3[river.readUInt16()];
                    for (int l = 0; l < tileMesh.verticesInTileSpace.Length; l++)
                    {
                        Int3 point = new Int3(river.readInt32(), river.readInt32(), river.readInt32());
                        Int3 @int = graphTransform.InverseTransform(point);
                        Int3 int2 = (Int3)new Vector3(graph.TileWorldSizeX * (float)j, 0f, graph.TileWorldSizeZ * (float)i);
                        tileMesh.verticesInTileSpace[l] = @int - int2;
                    }
                    tileMesh.tags = new uint[tileMesh.triangles.Length];
                    int num = j + i * graph.tileXCount;
                    tileMeshes.tileMeshes[num] = tileMesh;
                }
            }
            graph.ReplaceTiles(tileMeshes);
        };
        AstarPath.active.AddWorkItem(callback);
        AstarPath.active.FlushWorkItems();
    }

    public void Serialize(River river)
    {
        river.writeSingleVector3(graph.forcedBoundsCenter);
        river.writeSingleVector3(graph.forcedBoundsSize);
        river.writeByte((byte)graph.tileXCount);
        river.writeByte((byte)graph.tileZCount);
        NavmeshTile[] tiles = graph.GetTiles();
        for (int i = 0; i < graph.tileZCount; i++)
        {
            for (int j = 0; j < graph.tileXCount; j++)
            {
                NavmeshTile navmeshTile = tiles[j + i * graph.tileXCount];
                river.writeUInt16((ushort)navmeshTile.tris.Length);
                for (int k = 0; k < navmeshTile.tris.Length; k++)
                {
                    river.writeUInt16((ushort)navmeshTile.tris[k]);
                }
                river.writeUInt16((ushort)navmeshTile.verts.Length);
                for (int l = 0; l < navmeshTile.verts.Length; l++)
                {
                    Int3 @int = navmeshTile.verts[l];
                    river.writeInt32(@int.x);
                    river.writeInt32(@int.y);
                    river.writeInt32(@int.z);
                }
            }
        }
    }
}
