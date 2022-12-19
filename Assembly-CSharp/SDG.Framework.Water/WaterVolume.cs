using System;
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
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 150;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.sizeOffset_X = 40;
            sleekToggle.sizeOffset_Y = 40;
            sleekToggle.state = volume.isSurfaceVisible;
            sleekToggle.addLabel("Surface Visible", ESleekSide.RIGHT);
            sleekToggle.onToggled += OnIsSurfaceVisibleToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.positionOffset_Y = 40;
            sleekToggle2.sizeOffset_X = 40;
            sleekToggle2.sizeOffset_Y = 40;
            sleekToggle2.state = volume.isReflectionVisible;
            sleekToggle2.addLabel("Reflection Visible", ESleekSide.RIGHT);
            sleekToggle2.onToggled += OnIsReflectionVisibleToggled;
            AddChild(sleekToggle2);
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.positionOffset_Y = 80;
            sleekToggle3.sizeOffset_X = 40;
            sleekToggle3.sizeOffset_Y = 40;
            sleekToggle3.state = volume.isSeaLevel;
            sleekToggle3.addLabel("Sea Level", ESleekSide.RIGHT);
            sleekToggle3.onToggled += OnIsSeaLevelToggled;
            AddChild(sleekToggle3);
            SleekButtonState sleekButtonState = new SleekButtonState(new GUIContent("Clean"), new GUIContent("Salty"), new GUIContent("Dirty"));
            sleekButtonState.positionOffset_Y = 120;
            sleekButtonState.sizeOffset_X = 200;
            sleekButtonState.sizeOffset_Y = 30;
            sleekButtonState.addLabel("Refill Type", ESleekSide.RIGHT);
            sleekButtonState.state = (int)(volume.waterType - 1);
            sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedWaterType));
            AddChild(sleekButtonState);
        }

        private void OnIsSurfaceVisibleToggled(ISleekToggle toggle, bool state)
        {
            volume.isSurfaceVisible = state;
        }

        private void OnIsReflectionVisibleToggled(ISleekToggle toggle, bool state)
        {
            volume.isReflectionVisible = state;
        }

        private void OnIsSeaLevelToggled(ISleekToggle toggle, bool state)
        {
            volume.isSeaLevel = state;
        }

        private void OnSwappedWaterType(SleekButtonState button, int state)
        {
            volume.waterType = (ERefillWaterType)(state + 1);
        }
    }

    public static readonly int WATER_SURFACE_TILE_SIZE = 1024;

    public GameObject waterPlane;

    public Material sharedMaterial;

    public PlanarReflection planarReflection;

    [SerializeField]
    protected bool _isSurfaceVisible = true;

    protected bool _editorIsSufaceVisible = true;

    [SerializeField]
    protected bool _isReflectionVisible;

    [SerializeField]
    protected bool _isSeaLevel;

    internal bool isManagedByLighting;

    public ERefillWaterType waterType = ERefillWaterType.SALTY;

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

    public override bool ShouldSave => !isManagedByLighting;

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
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

    internal void SyncPlanarReflectionEnabled()
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

    private void SyncWaterPlaneActive()
    {
        if (waterPlane != null)
        {
            waterPlane.SetActive(isSurfaceVisible && _editorIsSufaceVisible && Shape == ELevelVolumeShape.Box);
        }
    }

    protected void createWaterPlanes()
    {
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
        createWaterPlanes();
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Is_Surface_Visible", isSurfaceVisible);
        writer.writeValue("Is_Reflection_Visible", isReflectionVisible);
        writer.writeValue("Is_Sea_Level", isSeaLevel);
        writer.writeValue("Water_Type", waterType);
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
}
