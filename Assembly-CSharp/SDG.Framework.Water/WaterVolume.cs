using System;
using SDG.Framework.Devkit;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Water;

public class WaterVolume : LevelVolume<WaterVolume, WaterVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private WaterVolume volume;

        public Menu(WaterVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 230f;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.PositionOffset_Y = -30f;
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = volume.isSurfaceVisible;
            sleekToggle.AddLabel("Surface Visible", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnIsSurfaceVisibleToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.PositionOffset_Y = 10f;
            sleekToggle2.SizeOffset_X = 40f;
            sleekToggle2.SizeOffset_Y = 40f;
            sleekToggle2.Value = volume.isReflectionVisible;
            sleekToggle2.AddLabel("Reflection Visible", ESleekSide.RIGHT);
            sleekToggle2.OnValueChanged += OnIsReflectionVisibleToggled;
            AddChild(sleekToggle2);
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.PositionOffset_Y = 50f;
            sleekToggle3.SizeOffset_X = 40f;
            sleekToggle3.SizeOffset_Y = 40f;
            sleekToggle3.Value = volume.isSeaLevel;
            sleekToggle3.AddLabel("Sea Level", ESleekSide.RIGHT);
            sleekToggle3.OnValueChanged += OnIsSeaLevelToggled;
            AddChild(sleekToggle3);
            SleekButtonState sleekButtonState = new SleekButtonState(new GUIContent("Clean"), new GUIContent("Salty"), new GUIContent("Dirty"));
            sleekButtonState.PositionOffset_Y = 90f;
            sleekButtonState.SizeOffset_X = 200f;
            sleekButtonState.SizeOffset_Y = 30f;
            sleekButtonState.AddLabel("Refill Type", ESleekSide.RIGHT);
            sleekButtonState.state = (int)(volume.waterType - 1);
            sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedWaterType));
            AddChild(sleekButtonState);
            SleekAssetField sleekAssetField = new SleekAssetField();
            sleekAssetField.PositionOffset_Y = 130f;
            sleekAssetField.SizeOffset_X = 200f;
            sleekAssetField.SizeOffset_Y = 60f;
            sleekAssetField.Value = volume.FishSpawnTableRef;
            sleekAssetField.AddLabel("Fish", ESleekSide.RIGHT);
            sleekAssetField.OnValueChanged += OnFishSpawnTableChanged;
            AddChild(sleekAssetField);
            ISleekFloat32Field sleekFloat32Field = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field.PositionOffset_Y = 200f;
            sleekFloat32Field.SizeOffset_X = 200f;
            sleekFloat32Field.SizeOffset_Y = 30f;
            sleekFloat32Field.Value = volume.FishingMinimumDepthOverride;
            sleekFloat32Field.AddLabel("Fishing Minimum Depth Override", ESleekSide.RIGHT);
            sleekFloat32Field.OnValueChanged += OnMinimumDepthChanged;
            AddChild(sleekFloat32Field);
        }

        private void OnIsSurfaceVisibleToggled(ISleekToggle toggle, bool state)
        {
            volume.isSurfaceVisible = state;
            LevelHierarchy.MarkDirty();
        }

        private void OnIsReflectionVisibleToggled(ISleekToggle toggle, bool state)
        {
            volume.isReflectionVisible = state;
            LevelHierarchy.MarkDirty();
        }

        private void OnIsSeaLevelToggled(ISleekToggle toggle, bool state)
        {
            volume.isSeaLevel = state;
            LevelHierarchy.MarkDirty();
        }

        private void OnSwappedWaterType(SleekButtonState button, int state)
        {
            volume.waterType = (ERefillWaterType)(state + 1);
            LevelHierarchy.MarkDirty();
        }

        private void OnFishSpawnTableChanged(SleekAssetField field)
        {
            volume.FishSpawnTableRef = field.Value;
            LevelHierarchy.MarkDirty();
        }

        private void OnMinimumDepthChanged(ISleekFloat32Field field, float value)
        {
            volume.FishingMinimumDepthOverride = value;
            LevelHierarchy.MarkDirty();
        }
    }

    public static readonly int WATER_SURFACE_TILE_SIZE = 1024;

    public GameObject waterPlane;

    /// <summary>
    /// All water tiles and the planar reflection component reference this material.
    /// </summary>
    public Material sharedMaterial;

    [SerializeField]
    protected bool _isSurfaceVisible = true;

    protected bool _editorIsSufaceVisible = true;

    [SerializeField]
    protected bool _isReflectionVisible;

    [SerializeField]
    protected bool _isSeaLevel;

    /// <summary>
    /// Not using CachingAssetRef so it works with copy-paste. (Can we implement serialization for CachingAssetRef?)
    /// </summary>
    [SerializeField]
    internal Guid _fishSpawnTableGuid;

    [SerializeField]
    private float _fishingMinimumDepthOverride = -1f;

    /// <summary>
    /// Flag for legacy sea level.
    /// </summary>
    internal bool isManagedByLighting;

    public ERefillWaterType waterType = ERefillWaterType.SALTY;

    public PlanarReflection planarReflection;

    public override ELevelVolumeShape Shape
    {
        get
        {
            return base.Shape;
        }
        set
        {
            base.Shape = value;
            SyncWaterPlaneActive();
        }
    }

    public bool isSurfaceVisible
    {
        get
        {
            return _isSurfaceVisible;
        }
        set
        {
            _isSurfaceVisible = value;
            SyncWaterPlaneActive();
        }
    }

    public bool isReflectionVisible
    {
        get
        {
            return _isReflectionVisible;
        }
        set
        {
            _isReflectionVisible = value;
            SyncPlanarReflectionEnabled();
        }
    }

    /// <summary>
    /// If true rain will be occluded below the surface on the Y axis.
    /// </summary>
    public bool isSeaLevel
    {
        get
        {
            return _isSeaLevel;
        }
        set
        {
            _isSeaLevel = value;
            if (isSeaLevel)
            {
                WaterVolumeManager.seaLevelVolume = this;
            }
        }
    }

    public CachingAssetRef FishSpawnTableRef
    {
        get
        {
            return new CachingAssetRef(_fishSpawnTableGuid);
        }
        set
        {
            _fishSpawnTableGuid = value.Guid;
        }
    }

    /// <summary>
    /// If set, depth at bobber must exceed this to catch fish.
    /// </summary>
    public float FishingMinimumDepthOverride
    {
        get
        {
            return _fishingMinimumDepthOverride;
        }
        set
        {
            _fishingMinimumDepthOverride = value;
        }
    }

    public override bool ShouldSave => !isManagedByLighting;

    public override bool CanBeSelected => !isManagedByLighting;

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    public SpawnAsset GetFishSpawnTable()
    {
        return FishSpawnTableRef.Get<SpawnAsset>();
    }

    public override void UpdateEditorVisibility(ELevelVolumeVisibility visibility)
    {
        base.UpdateEditorVisibility(visibility);
        _editorIsSufaceVisible = visibility != ELevelVolumeVisibility.Solid && LevelLighting.EditorWantsWaterSurface;
        SyncWaterPlaneActive();
    }

    internal void SyncWaterQuality()
    {
        if (!(sharedMaterial == null))
        {
            switch (GraphicsSettings.waterQuality)
            {
            case EGraphicQuality.LOW:
                sharedMaterial.shader.maximumLOD = 201;
                break;
            case EGraphicQuality.MEDIUM:
                sharedMaterial.shader.maximumLOD = 301;
                break;
            case EGraphicQuality.HIGH:
            case EGraphicQuality.ULTRA:
                sharedMaterial.shader.maximumLOD = 501;
                break;
            }
        }
    }

    private void SyncPlanarReflectionEnabledPartial()
    {
        if (planarReflection != null)
        {
            planarReflection.enabled = isReflectionVisible && GraphicsSettings.waterQuality == EGraphicQuality.ULTRA;
            if (sharedMaterial != null && !planarReflection.enabled)
            {
                sharedMaterial.DisableKeyword("WATER_REFLECTIVE");
                sharedMaterial.EnableKeyword("WATER_SIMPLE");
                sharedMaterial.SetTexture(planarReflection.reflectionSampler, null);
            }
        }
    }

    internal void SyncPlanarReflectionEnabled()
    {
        SyncPlanarReflectionEnabledPartial();
    }

    private void SyncWaterPlaneActive()
    {
        if (waterPlane != null)
        {
            waterPlane.SetActive(isSurfaceVisible && _editorIsSufaceVisible && Shape == ELevelVolumeShape.Box);
        }
    }

    private void CreateUnity4Water()
    {
        InstantiateParameters instantiateParameters = default(InstantiateParameters);
        instantiateParameters.parent = base.transform;
        instantiateParameters.worldSpace = false;
        InstantiateParameters parameters = instantiateParameters;
        waterPlane = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Level/Water_Plane"), new Vector3(0f, 0.5f, 0f), Quaternion.identity, parameters);
        waterPlane.name = "Plane";
        waterPlane.transform.localScale = Vector3.one;
        planarReflection = waterPlane.GetComponent<PlanarReflection>();
        int num = Mathf.Max(1, Mathf.FloorToInt(base.transform.localScale.x / (float)WATER_SURFACE_TILE_SIZE));
        int num2 = Mathf.Max(1, Mathf.FloorToInt(base.transform.localScale.z / (float)WATER_SURFACE_TILE_SIZE));
        float num3 = 1f / (float)num;
        float num4 = 1f / (float)num2;
        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < num2; j++)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Level/Water_Tile"));
                gameObject.name = "Tile_" + i + "_" + j;
                gameObject.transform.parent = waterPlane.transform;
                gameObject.transform.localPosition = new Vector3(-0.5f + num3 / 2f + (float)i * num3, 0f, -0.5f + num4 / 2f + (float)j * num4);
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.transform.localScale = new Vector3(0.01f * num3, 1f, 0.01f * num4);
                if (sharedMaterial == null)
                {
                    sharedMaterial = gameObject.GetComponent<Renderer>().material;
                }
                else
                {
                    gameObject.GetComponent<Renderer>().material = sharedMaterial;
                }
                gameObject.GetComponent<WaterTile>().reflection = planarReflection;
            }
        }
        planarReflection.sharedMaterial = sharedMaterial;
    }

    private void CreateFallbackWaterPlanes()
    {
        waterPlane = new GameObject("Plane");
        waterPlane.transform.parent = base.transform;
        waterPlane.transform.SetLocalPositionAndRotation(new Vector3(0f, 0.5f, 0f), Quaternion.identity);
        waterPlane.transform.localScale = Vector3.one;
        int num = Mathf.Max(1, Mathf.FloorToInt(base.transform.localScale.x / (float)WATER_SURFACE_TILE_SIZE));
        int num2 = Mathf.Max(1, Mathf.FloorToInt(base.transform.localScale.z / (float)WATER_SURFACE_TILE_SIZE));
        float num3 = 1f / (float)num;
        float num4 = 1f / (float)num2;
        GameObject original = Resources.Load<GameObject>("Level/Water_Fallback");
        InstantiateParameters instantiateParameters = default(InstantiateParameters);
        instantiateParameters.parent = waterPlane.transform;
        instantiateParameters.worldSpace = false;
        InstantiateParameters parameters = instantiateParameters;
        for (int i = 0; i < num; i++)
        {
            for (int j = 0; j < num2; j++)
            {
                Vector3 position = new Vector3(-0.5f + num3 / 2f + (float)i * num3, 0f, -0.5f + num4 / 2f + (float)j * num4);
                GameObject gameObject = UnityEngine.Object.Instantiate(original, position, Quaternion.identity, parameters);
                gameObject.name = "Tile_" + i + "_" + j;
                gameObject.transform.localScale = new Vector3(0.1f * num3, 1f, 0.1f * num4);
                if (sharedMaterial == null)
                {
                    sharedMaterial = gameObject.GetComponent<Renderer>().material;
                }
                else
                {
                    gameObject.GetComponent<Renderer>().material = sharedMaterial;
                }
            }
        }
    }

    protected void createWaterPlanes()
    {
        if (!Dedicator.IsDedicatedServer && waterPlane == null)
        {
            CreateUnity4Water();
            if (waterPlane == null)
            {
                CreateFallbackWaterPlanes();
            }
            SyncWaterQuality();
            SyncPlanarReflectionEnabled();
            SyncWaterPlaneActive();
            LevelLighting.updateLighting();
        }
    }

    public void beginCollision(Collider collider)
    {
        if (!(collider == null))
        {
            collider.gameObject.GetComponent<IWaterVolumeInteractionHandler>()?.waterBeginCollision(this);
        }
    }

    public void endCollision(Collider collider)
    {
        if (!(collider == null))
        {
            collider.gameObject.GetComponent<IWaterVolumeInteractionHandler>()?.waterEndCollision(this);
        }
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        isSurfaceVisible = reader.readValue<bool>("Is_Surface_Visible");
        isReflectionVisible = reader.readValue<bool>("Is_Reflection_Visible");
        isSeaLevel = reader.readValue<bool>("Is_Sea_Level");
        waterType = reader.readValue<ERefillWaterType>("Water_Type");
        _fishSpawnTableGuid = reader.readValue<Guid>("Fish_Spawn_Table");
        if (reader.containsKey("Fishing_Minimum_Depth_Override"))
        {
            _fishingMinimumDepthOverride = reader.readValue<float>("Fishing_Minimum_Depth_Override");
        }
        createWaterPlanes();
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Is_Surface_Visible", isSurfaceVisible);
        writer.writeValue("Is_Reflection_Visible", isReflectionVisible);
        writer.writeValue("Is_Sea_Level", isSeaLevel);
        writer.writeValue("Water_Type", waterType);
        writer.writeValue("Fish_Spawn_Table", _fishSpawnTableGuid);
        if (_fishingMinimumDepthOverride > -0.5f)
        {
            writer.writeValue("Fishing_Minimum_Depth_Override", _fishingMinimumDepthOverride);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        beginCollision(other);
    }

    public void OnTriggerExit(Collider other)
    {
        endCollision(other);
    }

    protected override void Awake()
    {
        forceShouldAddCollider = true;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        createWaterPlanes();
    }

    public string OnGetFishErrorContext()
    {
        return $"water volume at {base.transform.position}";
    }
}
