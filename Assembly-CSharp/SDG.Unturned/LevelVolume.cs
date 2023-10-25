using System;
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Interactable;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;
using UnityEngine.Rendering;

namespace SDG.Unturned;

public class LevelVolume<TVolume, TManager> : VolumeBase, IDevkitInteractableBeginSelectionHandler, IDevkitInteractableEndSelectionHandler, ITransformedHandler where TVolume : LevelVolume<TVolume, TManager> where TManager : VolumeManager<TVolume, TManager>
{
    private class Menu : SleekWrapper
    {
        private ELevelVolumeShape prevShape;

        private SleekButtonStateEnum<ELevelVolumeShape> shapeButton;

        private ISleekFloat32Field falloffField;

        private LevelVolume<TVolume, TManager> volume;

        public Menu(LevelVolume<TVolume, TManager> volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            float num = 0f;
            if (volume.supportsBoxShape && volume.supportsSphereShape)
            {
                shapeButton = new SleekButtonStateEnum<ELevelVolumeShape>();
                shapeButton.PositionOffset_Y = num;
                shapeButton.SizeOffset_X = 200f;
                shapeButton.SizeOffset_Y = 30f;
                shapeButton.SetEnum(volume.Shape);
                shapeButton.AddLabel("Shape", ESleekSide.RIGHT);
                SleekButtonStateEnum<ELevelVolumeShape> sleekButtonStateEnum = shapeButton;
                sleekButtonStateEnum.OnSwappedEnum = (Action<SleekButtonStateEnum<ELevelVolumeShape>, ELevelVolumeShape>)Delegate.Combine(sleekButtonStateEnum.OnSwappedEnum, new Action<SleekButtonStateEnum<ELevelVolumeShape>, ELevelVolumeShape>(OnShapeChanged));
                AddChild(shapeButton);
                num += shapeButton.SizeOffset_Y + 10f;
            }
            if (volume.supportsFalloff)
            {
                falloffField = Glazier.Get().CreateFloat32Field();
                falloffField.PositionOffset_Y = num;
                falloffField.SizeOffset_X = 200f;
                falloffField.SizeOffset_Y = 30f;
                falloffField.Value = volume.falloffDistance;
                falloffField.AddLabel("Falloff", ESleekSide.RIGHT);
                falloffField.OnValueChanged += OnFalloffTyped;
                AddChild(falloffField);
                num += falloffField.SizeOffset_Y + 10f;
            }
            base.SizeOffset_Y = num - 10f;
            prevShape = volume.Shape;
        }

        public override void OnUpdate()
        {
            ELevelVolumeShape shape = volume.Shape;
            if (prevShape != shape)
            {
                prevShape = shape;
                shapeButton.SetEnum(shape);
            }
        }

        private void OnShapeChanged(SleekButtonStateEnum<ELevelVolumeShape> button, ELevelVolumeShape state)
        {
            prevShape = state;
            using (new ScopedObjectUndo(volume))
            {
                volume.Shape = state;
            }
        }

        private void OnFalloffTyped(ISleekFloat32Field field, float state)
        {
            volume.falloffDistance = state;
        }
    }

    [SerializeField]
    private ELevelVolumeShape _shape;

    /// <summary>
    /// Distance inward from edge before intensity reaches 100%.
    /// </summary>
    public float falloffDistance;

    internal bool isSelected;

    [SerializeField]
    internal Collider volumeCollider;

    /// <summary>
    /// Editor-only solid/opaque child mesh renderer object.
    /// </summary>
    [SerializeField]
    protected GameObject editorGameObject;

    [SerializeField]
    protected MeshFilter editorMeshFilter;

    [SerializeField]
    protected MeshRenderer editorMeshRenderer;

    /// <summary>
    /// If true during Awake the collider component will be added.
    /// Otherwise only in the level editor. Some volume types like water use the collider in gameplay,
    /// whereas most only need the collider for general-purpose selection in the level editor.
    /// </summary>
    protected bool forceShouldAddCollider;

    protected bool supportsBoxShape = true;

    protected bool supportsSphereShape = true;

    protected bool supportsFalloff;

    private static Mesh _cubeMesh;

    private static Mesh _sphereMesh;

    public virtual ELevelVolumeShape Shape
    {
        get
        {
            return _shape;
        }
        set
        {
            if (_shape == value)
            {
                return;
            }
            _shape = value;
            if (volumeCollider != null)
            {
                bool flag = volumeCollider.enabled;
                bool isTrigger = volumeCollider.isTrigger;
                UnityEngine.Object.Destroy(volumeCollider);
                switch (value)
                {
                case ELevelVolumeShape.Box:
                    volumeCollider = base.gameObject.AddComponent<BoxCollider>();
                    break;
                case ELevelVolumeShape.Sphere:
                    volumeCollider = base.gameObject.AddComponent<SphereCollider>();
                    break;
                }
                volumeCollider.enabled = flag;
                volumeCollider.isTrigger = isTrigger;
            }
            if (editorMeshFilter != null)
            {
                SyncEditorMeshToShape();
            }
            if (value == ELevelVolumeShape.Sphere)
            {
                float max = base.transform.localScale.GetAbs().GetMax();
                base.transform.localScale = new Vector3(max, max, max);
            }
        }
    }

    public override ISleekElement CreateMenu()
    {
        if ((supportsBoxShape && supportsSphereShape) || supportsFalloff)
        {
            return new Menu(this);
        }
        return null;
    }

    public virtual void beginSelection(InteractionData data)
    {
        isSelected = true;
    }

    public virtual void endSelection(InteractionData data)
    {
        isSelected = false;
    }

    public void OnTransformed(Vector3 oldPosition, Quaternion oldRotation, Vector3 oldLocalScale, Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale, bool modifyRotation, bool modifyScale)
    {
        if (!newPosition.IsNearlyEqual(base.transform.position))
        {
            base.transform.position = newPosition;
        }
        if (modifyRotation)
        {
            base.transform.SetRotation_RoundIfNearlyAxisAligned(newRotation);
        }
        if (!modifyScale)
        {
            return;
        }
        if (Shape == ELevelVolumeShape.Sphere)
        {
            if (newLocalScale.sqrMagnitude > oldLocalScale.sqrMagnitude)
            {
                float max = newLocalScale.GetAbs().GetMax();
                newLocalScale = new Vector3(max, max, max);
            }
            else
            {
                float min = newLocalScale.GetAbs().GetMin();
                newLocalScale = new Vector3(min, min, min);
            }
        }
        base.transform.SetLocalScale_RoundIfNearlyEqualToOne(newLocalScale);
    }

    public bool IsPositionInsideVolume(Vector3 position)
    {
        switch (_shape)
        {
        case ELevelVolumeShape.Box:
        {
            Vector3 vector = base.transform.InverseTransformPoint(position);
            if (Mathf.Abs(vector.x) < 0.5f && Mathf.Abs(vector.y) < 0.5f)
            {
                return Mathf.Abs(vector.z) < 0.5f;
            }
            return false;
        }
        case ELevelVolumeShape.Sphere:
        {
            float sphereRadius = GetSphereRadius();
            float num = sphereRadius * sphereRadius;
            return (position - base.transform.position).sqrMagnitude < num;
        }
        default:
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Alpha is 0.0 outside volume and 1.0 inside inner volume.
    /// </summary>
    public bool IsPositionInsideVolumeWithAlpha(Vector3 position, out float alpha)
    {
        if (falloffDistance < 0.0001f)
        {
            alpha = 1f;
            return IsPositionInsideVolume(position);
        }
        switch (_shape)
        {
        case ELevelVolumeShape.Box:
        {
            Vector3 abs = base.transform.InverseTransformPoint(position).GetAbs();
            if (abs.x < 0.5f && abs.y < 0.5f && abs.z < 0.5f)
            {
                Vector3 a = new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 localInnerBoxExtents = GetLocalInnerBoxExtents();
                Vector3 vector = MathfEx.InverseLerp(a, localInnerBoxExtents, abs);
                alpha = vector.GetMin();
                return true;
            }
            alpha = 0f;
            return false;
        }
        case ELevelVolumeShape.Sphere:
        {
            float sphereRadius = GetSphereRadius();
            float num = sphereRadius * sphereRadius;
            float sqrMagnitude = (position - base.transform.position).sqrMagnitude;
            if (sqrMagnitude < num)
            {
                float value = Mathf.Sqrt(sqrMagnitude);
                float b = Mathf.Max(0f, sphereRadius - falloffDistance);
                alpha = Mathf.InverseLerp(sphereRadius, b, value);
                return true;
            }
            alpha = 0f;
            return false;
        }
        default:
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// World space size of the box.
    /// </summary>
    public Vector3 GetBoxSize()
    {
        return base.transform.localScale.GetAbs();
    }

    /// <summary>
    /// Half the world space size of the box.
    /// </summary>
    public Vector3 GetBoxExtents()
    {
        return base.transform.localScale.GetAbs() * 0.5f;
    }

    /// <summary>
    /// World space size of inner falloff box when falloffDistance is non-zero.
    /// For example a 24x12x6 box with a falloff of 4 has an inner box sized 16x4x0.
    /// </summary>
    public Vector3 GetInnerBoxSize()
    {
        Vector3 abs = base.transform.localScale.GetAbs();
        abs.x = Mathf.Max(0f, abs.x - falloffDistance * 2f);
        abs.y = Mathf.Max(0f, abs.y - falloffDistance * 2f);
        abs.z = Mathf.Max(0f, abs.z - falloffDistance * 2f);
        return abs;
    }

    /// <summary>
    /// World space extents of inner falloff box when falloffDistance is non-zero.
    /// </summary>
    public Vector3 GetInnerBoxExtents()
    {
        Vector3 result = base.transform.localScale.GetAbs() * 0.5f;
        result.x = Mathf.Max(0f, result.x - falloffDistance);
        result.y = Mathf.Max(0f, result.y - falloffDistance);
        result.z = Mathf.Max(0f, result.z - falloffDistance);
        return result;
    }

    /// <summary>
    /// Local space size of inner falloff box when falloffDistance is non-zero.
    /// </summary>
    public Vector3 GetLocalInnerBoxSize()
    {
        Vector3 abs = base.transform.localScale.GetAbs();
        return new Vector3(Mathf.Max(0f, abs.x - falloffDistance * 2f) / abs.x, Mathf.Max(0f, abs.y - falloffDistance * 2f) / abs.y, Mathf.Max(0f, abs.z - falloffDistance * 2f) / abs.z);
    }

    /// <summary>
    /// Local space extents of inner falloff box when falloffDistance is non-zero.
    /// </summary>
    public Vector3 GetLocalInnerBoxExtents()
    {
        Vector3 abs = base.transform.localScale.GetAbs();
        return new Vector3(Mathf.Max(0f, abs.x * 0.5f - falloffDistance) / abs.x, Mathf.Max(0f, abs.y * 0.5f - falloffDistance) / abs.y, Mathf.Max(0f, abs.z * 0.5f - falloffDistance) / abs.z);
    }

    /// <summary>
    /// World space radius of the sphere.
    /// </summary>
    public float GetSphereRadius()
    {
        return base.transform.localScale.GetAbs().GetMax() * 0.5f;
    }

    /// <summary>
    /// Local space radius of the sphere.
    /// </summary>
    public float GetLocalSphereRadius()
    {
        return 0.5f;
    }

    /// <summary>
    /// World space radius of inner falloff sphere when falloffDistance is non-zero.
    /// </summary>
    public float GetWorldSpaceInnerSphereRadius()
    {
        float num = base.transform.localScale.GetAbs().GetMax() * 0.5f;
        return Mathf.Max(0f, num - falloffDistance);
    }

    /// <summary>
    /// Local space radius of inner falloff sphere when falloffDistance is non-zero.
    /// </summary>
    public float GetLocalInnerSphereRadius()
    {
        float max = base.transform.localScale.GetAbs().GetMax();
        float num = max * 0.5f;
        return Mathf.Max(0f, num - falloffDistance) / max;
    }

    public void SetSphereRadius(float radius)
    {
        float num = radius * 2f;
        base.transform.localScale = new Vector3(num, num, num);
    }

    /// <summary>
    /// Useful for code which previously depended on creating the Unity collider to calculate bounding box.
    /// </summary>
    public Bounds CalculateWorldBounds()
    {
        Bounds result = new Bounds(base.transform.position, Vector3.zero);
        Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
        result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, -0.5f)));
        result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(new Vector3(-0.5f, -0.5f, 0.5f)));
        result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(new Vector3(-0.5f, 0.5f, -0.5f)));
        result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(new Vector3(-0.5f, 0.5f, 0.5f)));
        result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0.5f, -0.5f, -0.5f)));
        result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0.5f, -0.5f, 0.5f)));
        result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0.5f, 0.5f, -0.5f)));
        result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0.5f, 0.5f, 0.5f)));
        return result;
    }

    public Bounds CalculateLocalBounds()
    {
        return new Bounds(Vector3.zero, base.transform.localScale.GetAbs());
    }

    /// <summary>
    /// Called in the level editor during registraion and when visibility is changed.
    /// </summary>
    public virtual void UpdateEditorVisibility(ELevelVolumeVisibility visibility)
    {
        volumeCollider.enabled = visibility != ELevelVolumeVisibility.Hidden;
        editorGameObject.SetActive(visibility == ELevelVolumeVisibility.Solid);
    }

    protected virtual void OnEnable()
    {
        LevelHierarchy.addItem(this);
        VolumeManager<TVolume, TManager>.Get().AddVolume((TVolume)this);
    }

    protected virtual void OnDisable()
    {
        VolumeManager<TVolume, TManager>.Get().RemoveVolume((TVolume)this);
        LevelHierarchy.removeItem(this);
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        if (reader.containsKey("Shape"))
        {
            Shape = reader.readValue<ELevelVolumeShape>("Shape");
        }
        else
        {
            Shape = ELevelVolumeShape.Box;
        }
        if (supportsFalloff && reader.containsKey("Falloff"))
        {
            falloffDistance = reader.readValue<float>("Falloff");
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Shape", Shape);
        if (supportsFalloff)
        {
            writer.writeValue("Falloff", falloffDistance);
        }
    }

    protected virtual void Awake()
    {
        base.gameObject.layer = 30;
        if (_shape == ELevelVolumeShape.Box && !supportsBoxShape)
        {
            _shape = ELevelVolumeShape.Sphere;
        }
        else if (_shape == ELevelVolumeShape.Sphere && !supportsSphereShape)
        {
            _shape = ELevelVolumeShape.Box;
        }
        bool flag = forceShouldAddCollider || Level.isEditor;
        if (volumeCollider == null)
        {
            Collider component = GetComponent<Collider>();
            if (component != null)
            {
                bool flag2 = false;
                if (component is BoxCollider)
                {
                    if (supportsBoxShape)
                    {
                        _shape = ELevelVolumeShape.Box;
                        flag2 = flag;
                    }
                }
                else if (component is SphereCollider && supportsSphereShape)
                {
                    _shape = ELevelVolumeShape.Sphere;
                    flag2 = flag;
                }
                if (flag2)
                {
                    volumeCollider = component;
                    volumeCollider.isTrigger = true;
                }
                else
                {
                    UnityEngine.Object.Destroy(component);
                }
            }
        }
        if (flag && volumeCollider == null)
        {
            switch (_shape)
            {
            case ELevelVolumeShape.Box:
                volumeCollider = base.gameObject.AddComponent<BoxCollider>();
                break;
            case ELevelVolumeShape.Sphere:
                volumeCollider = base.gameObject.AddComponent<SphereCollider>();
                break;
            }
            volumeCollider.isTrigger = true;
        }
        if (Level.isEditor && editorGameObject == null)
        {
            editorGameObject = new GameObject("EditorPreview");
            editorGameObject.transform.SetParent(base.transform, worldPositionStays: false);
            editorGameObject.layer = 18;
            editorMeshFilter = editorGameObject.AddComponent<MeshFilter>();
            SyncEditorMeshToShape();
            editorMeshRenderer = editorGameObject.AddComponent<MeshRenderer>();
            editorMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            editorMeshRenderer.sharedMaterial = VolumeManager<TVolume, TManager>.Get().solidMaterial;
        }
    }

    protected virtual void Start()
    {
        NetId netIdFromInstanceId = GetNetIdFromInstanceId();
        if (!netIdFromInstanceId.IsNull())
        {
            NetIdRegistry.Assign(netIdFromInstanceId, this);
        }
    }

    protected virtual void OnDestroy()
    {
        NetId netIdFromInstanceId = GetNetIdFromInstanceId();
        if (!netIdFromInstanceId.IsNull())
        {
            NetIdRegistry.Release(netIdFromInstanceId);
        }
    }

    protected void AppendBaseMenu(ISleekElement childMenu)
    {
        if ((supportsBoxShape && supportsSphereShape) || supportsFalloff)
        {
            Menu menu = new Menu(this);
            menu.PositionScale_Y = 1f;
            menu.PositionOffset_Y = 0f - menu.SizeOffset_Y;
            childMenu.SizeOffset_Y += menu.SizeOffset_Y + 10f;
            childMenu.AddChild(menu);
        }
    }

    internal TManager GetVolumeManager()
    {
        return VolumeManager<TVolume, TManager>.Get();
    }

    private void SyncEditorMeshToShape()
    {
        switch (_shape)
        {
        case ELevelVolumeShape.Box:
            editorMeshFilter.sharedMesh = GetCubeMesh();
            break;
        case ELevelVolumeShape.Sphere:
            editorMeshFilter.sharedMesh = GetSphereMesh();
            break;
        default:
            editorMeshFilter.sharedMesh = null;
            break;
        }
    }

    private static Mesh GetCubeMesh()
    {
        if (_cubeMesh == null)
        {
            _cubeMesh = Resources.Load<GameObject>("Shapes/TwoSidedUnitCube").GetComponent<MeshFilter>().sharedMesh;
        }
        return _cubeMesh;
    }

    private static Mesh GetSphereMesh()
    {
        if (_sphereMesh == null)
        {
            _sphereMesh = Resources.Load<GameObject>("Shapes/TwoSidedOneDiameterSphere").GetComponent<MeshFilter>().sharedMesh;
        }
        return _sphereMesh;
    }
}
