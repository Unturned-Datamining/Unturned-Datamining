using System;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Graphs.Navmesh;
using UnityEngine;

namespace SDG.Unturned;

public class UnturnedNavmeshFlag_ASPFP : IUnturnedPerNavmeshEditorInterface
{
    private Flag owner;

    private UnturnedNavmesh_ASPFP navmeshInterface;

    public int GraphIndexForUI => (int)navmeshInterface.graph.graphIndex;

    public UnturnedNavmeshFlag_ASPFP(Flag owner)
    {
        this.owner = owner;
        navmeshInterface = (UnturnedNavmesh_ASPFP)owner.navmeshInterface;
        AstarPath.OnGraphPostScan = (OnGraphDelegate)Delegate.Combine(AstarPath.OnGraphPostScan, new OnGraphDelegate(OnGraphPostScan));
        UpdateVisualMesh();
    }

    public void OnDestroy()
    {
        AstarPath.active.data.RemoveGraph(navmeshInterface.graph);
        AstarPath.OnGraphPostScan = (OnGraphDelegate)Delegate.Remove(AstarPath.OnGraphPostScan, new OnGraphDelegate(OnGraphPostScan));
    }

    public void Bake()
    {
        Bounds bounds = owner.CalculateBakingBounds();
        navmeshInterface.graph.forcedBoundsCenter = bounds.center;
        navmeshInterface.graph.forcedBoundsSize = bounds.size;
        AstarPath.active.Scan(navmeshInterface.graph);
    }

    private void OnGraphPostScan(NavGraph updated)
    {
        if (updated == navmeshInterface.graph)
        {
            owner.needsNavigationSave = true;
            UpdateVisualMesh();
        }
    }

    private void UpdateVisualMesh()
    {
        List<Vector3> list = new List<Vector3>();
        List<int> list2 = new List<int>();
        List<Vector2> list3 = new List<Vector2>();
        NavmeshTile[] tiles = navmeshInterface.graph.GetTiles();
        int num = 0;
        if (tiles == null)
        {
            return;
        }
        NavmeshTile[] array = tiles;
        foreach (NavmeshTile navmeshTile in array)
        {
            for (int j = 0; j < navmeshTile.verts.Length; j++)
            {
                Vector3 item = (Vector3)navmeshTile.verts[j];
                item.y += 0.1f;
                list.Add(item);
                list3.Add(new Vector2(item.x, item.z));
            }
            for (int k = 0; k < navmeshTile.tris.Length; k++)
            {
                list2.Add(navmeshTile.tris[k] + num);
            }
            num += navmeshTile.verts.Length;
        }
        Mesh mesh = new Mesh();
        mesh.name = "Navmesh";
        mesh.vertices = list.ToArray();
        mesh.triangles = list2.ToArray();
        mesh.normals = new Vector3[list.Count];
        mesh.uv = list3.ToArray();
        owner.VisualizationMeshFilter.transform.position = Vector3.zero;
        owner.VisualizationMeshFilter.sharedMesh = mesh;
    }
}
