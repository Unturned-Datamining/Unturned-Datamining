using System;
using System.Collections.Generic;
using Pathfinding;
using SDG.Framework.Landscapes;
using UnityEngine;

namespace SDG.Unturned;

public class Flag
{
    public static readonly float MIN_SIZE = 32f;

    public static readonly float MAX_SIZE = 1024f;

    public float width;

    public float height;

    private Vector3 _point;

    private Transform _model;

    private MeshFilter navmesh;

    private LineRenderer _area;

    private LineRenderer _bounds;

    private RecastGraph _graph;

    public bool needsNavigationSave;

    public Vector3 point => _point;

    public Transform model => _model;

    public LineRenderer area => _area;

    public LineRenderer bounds => _bounds;

    public RecastGraph graph => _graph;

    public FlagData data { get; private set; }

    public void move(Vector3 newPoint)
    {
        _point = newPoint;
        model.position = point;
        navmesh.transform.position = Vector3.zero;
    }

    public void setEnabled(bool isEnabled)
    {
        model.gameObject.SetActive(isEnabled);
    }

    public void buildMesh()
    {
        float num = MIN_SIZE + width * (MAX_SIZE - MIN_SIZE);
        float num2 = MIN_SIZE + height * (MAX_SIZE - MIN_SIZE);
        area.SetPosition(0, new Vector3((0f - num) / 2f, 0f, (0f - num2) / 2f));
        area.SetPosition(1, new Vector3(num / 2f, 0f, (0f - num2) / 2f));
        area.SetPosition(2, new Vector3(num / 2f, 0f, num2 / 2f));
        area.SetPosition(3, new Vector3((0f - num) / 2f, 0f, num2 / 2f));
        area.SetPosition(4, new Vector3((0f - num) / 2f, 0f, (0f - num2) / 2f));
        num += LevelNavigation.BOUNDS_SIZE.x;
        num2 += LevelNavigation.BOUNDS_SIZE.z;
        bounds.SetPosition(0, new Vector3((0f - num) / 2f, 0f, (0f - num2) / 2f));
        bounds.SetPosition(1, new Vector3(num / 2f, 0f, (0f - num2) / 2f));
        bounds.SetPosition(2, new Vector3(num / 2f, 0f, num2 / 2f));
        bounds.SetPosition(3, new Vector3((0f - num) / 2f, 0f, num2 / 2f));
        bounds.SetPosition(4, new Vector3((0f - num) / 2f, 0f, (0f - num2) / 2f));
    }

    public void remove()
    {
        AstarPath.active.astarData.RemoveGraph(graph);
        UnityEngine.Object.Destroy(model.gameObject);
    }

    public void bakeNavigation()
    {
        VolumeManager<CullingVolume, CullingVolumeManager>.Get().ImmediatelySyncAllVolumes();
        LevelObjects.ImmediatelySyncRegionalVisibility();
        float x = MIN_SIZE + width * (MAX_SIZE - MIN_SIZE);
        float z = MIN_SIZE + height * (MAX_SIZE - MIN_SIZE);
        if (Level.info.configData.Use_Legacy_Water && LevelLighting.seaLevel < 0.99f && !Level.info.configData.Allow_Underwater_Features)
        {
            graph.forcedBoundsCenter = new Vector3(point.x, LevelLighting.seaLevel * Level.TERRAIN + (Level.TERRAIN - LevelLighting.seaLevel * Level.TERRAIN) / 2f - 0.625f, point.z);
            graph.forcedBoundsSize = new Vector3(x, Level.TERRAIN - LevelLighting.seaLevel * Level.TERRAIN + 1.25f, z);
        }
        else
        {
            graph.forcedBoundsCenter = new Vector3(point.x, 0f, point.z);
            graph.forcedBoundsSize = new Vector3(x, Landscape.TILE_HEIGHT, z);
        }
        AstarPath.active.ScanSpecific(graph);
        LevelNavigation.updateBounds();
    }

    private void updateNavmesh()
    {
        if (!Level.isEditor || graph == null)
        {
            return;
        }
        List<Vector3> list = new List<Vector3>();
        List<int> list2 = new List<int>();
        List<Vector2> list3 = new List<Vector2>();
        RecastGraph.NavmeshTile[] tiles = graph.GetTiles();
        int num = 0;
        if (tiles == null)
        {
            return;
        }
        foreach (RecastGraph.NavmeshTile navmeshTile in tiles)
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
        navmesh.transform.position = Vector3.zero;
        navmesh.sharedMesh = mesh;
    }

    private void OnGraphPostScan(NavGraph updated)
    {
        if (updated == graph)
        {
            needsNavigationSave = true;
            updateNavmesh();
        }
    }

    private void setupGraph()
    {
        AstarPath.OnGraphPostScan = (OnGraphDelegate)Delegate.Combine(AstarPath.OnGraphPostScan, new OnGraphDelegate(OnGraphPostScan));
    }

    public Flag(Vector3 newPoint, RecastGraph newGraph, FlagData newData)
    {
        _point = newPoint;
        _model = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Edit/Flag"))).transform;
        model.name = "Flag";
        model.position = point;
        _area = model.Find("Area").GetComponent<LineRenderer>();
        _bounds = model.Find("Bounds").GetComponent<LineRenderer>();
        navmesh = model.Find("Navmesh").GetComponent<MeshFilter>();
        width = 0f;
        height = 0f;
        _graph = newGraph;
        data = newData;
        setupGraph();
        buildMesh();
    }

    public Flag(Vector3 newPoint, float newWidth, float newHeight, RecastGraph newGraph, FlagData newData)
    {
        _point = newPoint;
        _model = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Edit/Flag"))).transform;
        model.name = "Flag";
        model.position = point;
        _area = model.Find("Area").GetComponent<LineRenderer>();
        _bounds = model.Find("Bounds").GetComponent<LineRenderer>();
        navmesh = model.Find("Navmesh").GetComponent<MeshFilter>();
        width = newWidth;
        height = newHeight;
        _graph = newGraph;
        data = newData;
        setupGraph();
        buildMesh();
        updateNavmesh();
    }
}
