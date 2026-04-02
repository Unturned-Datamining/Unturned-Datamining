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

    private LineRenderer _area;

    private LineRenderer _bounds;

    public bool needsNavigationSave;

    public Vector3 point => _point;

    public Transform model => _model;

    public MeshFilter VisualizationMeshFilter { get; private set; }

    public LineRenderer area => _area;

    public LineRenderer bounds => _bounds;

    public IUnturnedNavmeshInterface navmeshInterface { get; private set; }

    public IUnturnedPerNavmeshEditorInterface EditorFlagInterface { get; private set; }

    public FlagData data { get; private set; }

    public void move(Vector3 newPoint)
    {
        _point = newPoint;
        model.position = point;
        VisualizationMeshFilter.transform.position = Vector3.zero;
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
        EditorFlagInterface.OnDestroy();
        Object.Destroy(model.gameObject);
    }

    public Bounds CalculateBakingBounds()
    {
        float x = MIN_SIZE + width * (MAX_SIZE - MIN_SIZE);
        float z = MIN_SIZE + height * (MAX_SIZE - MIN_SIZE);
        Vector3 center;
        Vector3 size;
        if (Level.info.configData.Use_Legacy_Water && LevelLighting.seaLevel < 0.99f && !Level.info.configData.Allow_Underwater_Features)
        {
            center = new Vector3(point.x, LevelLighting.seaLevel * Level.TERRAIN + (Level.TERRAIN - LevelLighting.seaLevel * Level.TERRAIN) / 2f - 0.625f, point.z);
            size = new Vector3(x, Level.TERRAIN - LevelLighting.seaLevel * Level.TERRAIN + 1.25f, z);
        }
        else
        {
            center = new Vector3(point.x, 0f, point.z);
            size = new Vector3(x, Landscape.TILE_HEIGHT, z);
        }
        return new Bounds(center, size);
    }

    public void bakeNavigation()
    {
        VolumeManager<CullingVolume, CullingVolumeManager>.Get().ImmediatelySyncAllVolumes();
        LevelObjects.ImmediatelySyncRegionalVisibility();
        LevelRoads.ImmediatelySyncRegionalVisibility();
        EditorFlagInterface.Bake();
        LevelNavigation.updateBounds();
    }

    public Flag(Vector3 newPoint, IUnturnedNavmeshInterface newNavmesh, FlagData newData)
    {
        _point = newPoint;
        _model = Object.Instantiate(Resources.Load<GameObject>("Edit/Flag")).transform;
        model.name = "Flag";
        model.position = point;
        _area = model.Find("Area").GetComponent<LineRenderer>();
        _bounds = model.Find("Bounds").GetComponent<LineRenderer>();
        VisualizationMeshFilter = model.Find("Navmesh").GetComponent<MeshFilter>();
        width = 0f;
        height = 0f;
        navmeshInterface = newNavmesh;
        data = newData;
        buildMesh();
        EditorFlagInterface = UnturnedPathfinding.Get().CreateFlag(this);
    }

    public Flag(Vector3 newPoint, float newWidth, float newHeight, IUnturnedNavmeshInterface newNavmesh, FlagData newData)
    {
        _point = newPoint;
        _model = Object.Instantiate(Resources.Load<GameObject>("Edit/Flag")).transform;
        model.name = "Flag";
        model.position = point;
        _area = model.Find("Area").GetComponent<LineRenderer>();
        _bounds = model.Find("Bounds").GetComponent<LineRenderer>();
        VisualizationMeshFilter = model.Find("Navmesh").GetComponent<MeshFilter>();
        width = newWidth;
        height = newHeight;
        navmeshInterface = newNavmesh;
        data = newData;
        buildMesh();
        EditorFlagInterface = UnturnedPathfinding.Get().CreateFlag(this);
    }
}
