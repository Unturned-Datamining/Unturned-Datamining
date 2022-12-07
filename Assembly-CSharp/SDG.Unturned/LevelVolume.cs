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
            base.sizeOffset_X = 400;
            int num = 0;
            if (volume.supportsBoxShape && volume.supportsSphereShape)
            {
                shapeButton = new SleekButtonStateEnum<ELevelVolumeShape>();
                shapeButton.positionOffset_Y = num;
                shapeButton.sizeOffset_X = 200;
                shapeButton.sizeOffset_Y = 30;
                shapeButton.SetEnum(volume.Shape);
                shapeButton.addLabel("Shape", ESleekSide.RIGHT);
                SleekButtonStateEnum<ELevelVolumeShape> sleekButtonStateEnum = shapeButton;
                sleekButtonStateEnum.OnSwappedEnum = (Action<SleekButtonStateEnum<ELevelVolumeShape>, ELevelVolumeShape>)Delegate.Combine(sleekButtonStateEnum.OnSwappedEnum, new Action<SleekButtonStateEnum<ELevelVolumeShape>, ELevelVolumeShape>(OnShapeChanged));
                AddChild(shapeButton);
                num += shapeButton.sizeOffset_Y + 10;
            }
            if (volume.supportsFalloff)
            {
                falloffField = Glazier.Get().CreateFloat32Field();
                falloffField.positionOffset_Y = num;
                falloffField.sizeOffset_X = 200;
                falloffField.sizeOffset_Y = 30;
                falloffField.state = volume.falloffDistance;
                falloffField.addLabel("Falloff", ESleekSide.RIGHT);
                falloffField.onTypedSingle += OnFalloffTyped;
                AddChild(falloffField);
                num += falloffField.sizeOffset_Y + 10;
            }
            base.sizeOffset_Y = num - 10;
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

    public float falloffDistance;

    internal bool isSelected;

    [SerializeField]
    internal Collider volumeCollider;

    [SerializeField]
    protected GameObject editorGameObject;

    [SerializeField]
    protected MeshFilter editorMeshFilter;

    [SerializeField]
    protected MeshRenderer editorMeshRenderer;

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

    public void OnTranslatedAndRotated(Vector3 oldPosition, Quaternion oldRotation, Vector3 newPosition, Quaternion newRotation, bool modifyRotation)
    {
        if (!newPosition.IsNearlyEqual(base.transform.position))
        {
            base.transform.position = newPosition;
        }
        if (modifyRotation)
        {
            base.transform.SetRotation_RoundIfNearlyAxisAligned(newRotation);
        }
    }

    public void OnTransformed(Matrix4x4 oldLocalToWorldMatrix, Matrix4x4 newLocalToWorldMatrix)
    {
        base.transform.position = newLocalToWorldMatrix.GetPosition();
        base.transform.SetRotation_RoundIfNearlyAxisAligned(newLocalToWorldMatrix.GetRotation());
        Vector3 vector = newLocalToWorldMatrix.lossyScale;
        if (Shape == ELevelVolumeShape.Sphere)
        {
            Vector3 lossyScale = oldLocalToWorldMatrix.lossyScale;
            if (vector.sqrMagnitude > lossyScale.sqrMagnitude)
            {
                float max = vector.GetAbs().GetMax();
                vector = new Vector3(max, max, max);
            }
            else
            {
                float min = vector.GetAbs().GetMin();
                vector = new Vector3(min, min, min);
            }
        }
        base.transform.SetLocalScale_RoundIfNearlyEqualToOne(vector);
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

    public Vector3 GetBoxSize()
    {
        return base.transform.localScale.GetAbs();
    }

    public Vector3 GetBoxExtents()
    {
        return base.transform.localScale.GetAbs() * 0.5f;
    }

    public Vector3 GetInnerBoxSize()
    {
        Vector3 abs = base.transform.localScale.GetAbs();
        abs.x = Mathf.Max(0f, abs.x - falloffDistance * 2f);
        abs.y = Mathf.Max(0f, abs.y - falloffDistance * 2f);
        abs.z = Mathf.Max(0f, abs.z - falloffDistance * 2f);
        return abs;
    }

    public Vector3 GetInnerBoxExtents()
    {
        Vector3 result = base.transform.localScale.GetAbs() * 0.5f;
        result.x = Mathf.Max(0f, result.x - falloffDistance);
        result.y = Mathf.Max(0f, result.y - falloffDistance);
        result.z = Mathf.Max(0f, result.z - falloffDistance);
        return result;
    }

    public Vector3 GetLocalInnerBoxSize()
    {
        Vector3 abs = base.transform.localScale.GetAbs();
        return new Vector3(Mathf.Max(0f, abs.x - falloffDistance * 2f) / abs.x, Mathf.Max(0f, abs.y - falloffDistance * 2f) / abs.y, Mathf.Max(0f, abs.z - falloffDistance * 2f) / abs.z);
    }

    public Vector3 GetLocalInnerBoxExtents()
    {
        Vector3 abs = base.transform.localScale.GetAbs();
        return new Vector3(Mathf.Max(0f, abs.x * 0.5f - falloffDistance) / abs.x, Mathf.Max(0f, abs.y * 0.5f - falloffDistance) / abs.y, Mathf.Max(0f, abs.z * 0.5f - falloffDistance) / abs.z);
    }

    public float GetSphereRadius()
    {
        return base.transform.localScale.GetAbs().GetMax() * 0.5f;
    }

    public float GetLocalSphereRadius()
    {
        return 0.5f;
    }

    public float GetWorldSpaceInnerSphereRadius()
    {
        float num = base.transform.localScale.GetAbs().GetMax() * 0.5f;
        return Mathf.Max(0f, num - falloffDistance);
    }

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
            menu.positionScale_Y = 1f;
            menu.positionOffset_Y = -menu.sizeOffset_Y;
            childMenu.sizeOffset_Y += menu.sizeOffset_Y + 10;
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
