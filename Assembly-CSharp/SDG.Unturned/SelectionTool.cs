using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Interactable;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.Rendering;
using SDG.Framework.Utilities;
using UnityEngine;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class SelectionTool : IDevkitTool
{
    public enum ESelectionMode
    {
        POSITION,
        ROTATION,
        SCALE
    }

    protected List<GameObject> copyBuffer = new List<GameObject>();

    protected List<GameObject> copySelectionDelay = new List<GameObject>();

    private ESelectionMode _mode;

    private bool wantsBoundsEditor;

    protected DevkitSelection pendingClickSelection;

    protected Vector3 handlePosition;

    protected Quaternion handleRotation;

    protected Vector3 referencePosition;

    protected Quaternion referenceRotation;

    protected Vector3 referenceScale;

    protected bool hasReferenceRotation;

    protected bool hasReferenceScale;

    private TransformHandles handles;

    protected Vector3 beginAreaSelect;

    protected float beginAreaSelectTime;

    protected bool isAreaSelecting;

    protected bool isDragging;

    protected HashSet<DevkitSelection> areaSelection = new HashSet<DevkitSelection>();

    public ESelectionMode mode
    {
        get
        {
            return _mode;
        }
        set
        {
            _mode = value;
            wantsBoundsEditor = false;
        }
    }

    protected void transformSelection()
    {
        foreach (DevkitSelection item in DevkitSelectionManager.selection)
        {
            if (!(item.gameObject == null))
            {
                item.gameObject.GetComponent<IDevkitSelectionTransformableHandler>()?.transformSelection();
            }
        }
    }

    private void OnHandlePreTransform(Matrix4x4 worldToPivot)
    {
        foreach (DevkitSelection item in DevkitSelectionManager.selection)
        {
            if (!(item.gameObject == null))
            {
                item.preTransformPosition = item.transform.position;
                item.preTransformRotation = item.transform.rotation;
                item.preTransformLocalScale = item.transform.localScale;
                item.localToWorld = item.transform.localToWorldMatrix;
                item.relativeToPivot = worldToPivot * item.localToWorld;
            }
        }
    }

    private void OnHandleTranslatedAndRotated(Vector3 worldPositionDelta, Quaternion worldRotationDelta, Vector3 pivotPosition, bool modifyRotation)
    {
        foreach (DevkitSelection item in DevkitSelectionManager.selection)
        {
            if (item.gameObject == null)
            {
                continue;
            }
            Vector3 vector2;
            if (modifyRotation)
            {
                Vector3 vector = item.preTransformPosition - pivotPosition;
                vector2 = (vector.IsNearlyZero() ? (item.preTransformPosition + worldPositionDelta) : (pivotPosition + worldRotationDelta * vector + worldPositionDelta));
            }
            else
            {
                vector2 = item.preTransformPosition + worldPositionDelta;
            }
            Quaternion quaternion = worldRotationDelta * item.preTransformRotation;
            ITransformedHandler component = item.gameObject.GetComponent<ITransformedHandler>();
            if (component != null)
            {
                component.OnTransformed(item.preTransformPosition, item.preTransformRotation, Vector3.zero, vector2, quaternion, Vector3.zero, modifyRotation, modifyScale: false);
                continue;
            }
            if (!vector2.IsNearlyEqual(item.transform.position))
            {
                item.transform.position = vector2;
            }
            if (modifyRotation)
            {
                item.transform.rotation = quaternion;
            }
        }
    }

    private void OnHandleTransformed(Matrix4x4 pivotToWorld)
    {
        foreach (DevkitSelection item in DevkitSelectionManager.selection)
        {
            if (!(item.gameObject == null))
            {
                Matrix4x4 matrix = pivotToWorld * item.relativeToPivot;
                ITransformedHandler component = item.gameObject.GetComponent<ITransformedHandler>();
                if (component != null)
                {
                    component.OnTransformed(item.preTransformPosition, item.preTransformRotation, item.preTransformLocalScale, matrix.GetPosition(), matrix.GetRotation(), matrix.lossyScale, modifyRotation: true, modifyScale: true);
                    continue;
                }
                item.transform.position = matrix.GetPosition();
                item.transform.SetRotation_RoundIfNearlyAxisAligned(matrix.GetRotation());
                item.transform.SetLocalScale_RoundIfNearlyEqualToOne(matrix.lossyScale);
            }
        }
    }

    protected void moveHandle(Vector3 position, Quaternion rotation, Vector3 scale, bool doRotation, bool hasScale)
    {
        DevkitTransactionManager.beginTransaction("Transform");
        foreach (DevkitSelection item in DevkitSelectionManager.selection)
        {
            if (!(item.gameObject == null))
            {
                DevkitTransactionUtility.recordObjectDelta(item.transform);
            }
        }
        if (DevkitSelectionManager.selection.Count == 1)
        {
            DevkitSelection devkitSelection = DevkitSelectionManager.selection.EnumerateFirst();
            if (devkitSelection != null && devkitSelection.transform != null)
            {
                ITransformedHandler component = devkitSelection.gameObject.GetComponent<ITransformedHandler>();
                if (component != null)
                {
                    component.OnTransformed(devkitSelection.preTransformPosition, devkitSelection.preTransformRotation, devkitSelection.preTransformLocalScale, position, rotation, scale, doRotation, hasScale);
                }
                else
                {
                    devkitSelection.transform.position = position;
                    if (doRotation)
                    {
                        devkitSelection.transform.rotation = rotation;
                    }
                    if (hasScale)
                    {
                        devkitSelection.transform.localScale = scale;
                    }
                }
            }
        }
        else
        {
            handles.ExternallyTransformPivot(position, rotation, doRotation);
        }
        transformSelection();
        DevkitTransactionManager.endTransaction();
    }

    protected virtual bool RaycastSelectableObjects(Ray ray, out RaycastHit hitInfo)
    {
        hitInfo = default(RaycastHit);
        return false;
    }

    protected virtual void RequestInstantiation(Vector3 position)
    {
    }

    protected virtual bool HasBoxSelectableObjects()
    {
        return false;
    }

    protected virtual IEnumerable<GameObject> EnumerateBoxSelectableObjects()
    {
        return null;
    }

    private IEnumerable<GameObject> EnumerateSelectedGameObjects()
    {
        foreach (DevkitSelection item in DevkitSelectionManager.selection)
        {
            if (item.gameObject != null)
            {
                yield return item.gameObject;
            }
        }
    }

    public virtual void update()
    {
        if (copySelectionDelay.Count > 0)
        {
            DevkitSelectionManager.clear();
            foreach (GameObject item in copySelectionDelay)
            {
                DevkitSelectionManager.add(new DevkitSelection(item, null));
            }
            copySelectionDelay.Clear();
        }
        if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
        {
            if (InputEx.GetKeyDown(KeyCode.Q))
            {
                if (mode != 0)
                {
                    mode = ESelectionMode.POSITION;
                }
                else
                {
                    wantsBoundsEditor = !wantsBoundsEditor;
                }
            }
            if (InputEx.GetKeyDown(KeyCode.W))
            {
                mode = ESelectionMode.ROTATION;
            }
            if (InputEx.GetKeyDown(KeyCode.R))
            {
                if (mode != ESelectionMode.SCALE)
                {
                    mode = ESelectionMode.SCALE;
                }
                else
                {
                    wantsBoundsEditor = !wantsBoundsEditor;
                }
            }
            Ray ray = EditorInteract.ray;
            bool flag = DevkitSelectionManager.selection.Count > 0 && handles.Raycast(ray);
            if (DevkitSelectionManager.selection.Count > 0)
            {
                handles.Render(ray);
            }
            if (InputEx.GetKeyDown(KeyCode.Mouse0))
            {
                RaycastHit hitInfo = default(RaycastHit);
                if (!flag)
                {
                    RaycastSelectableObjects(ray, out hitInfo);
                    if (hitInfo.transform != null)
                    {
                        IDevkitHierarchyItem componentInParent = hitInfo.transform.GetComponentInParent<IDevkitHierarchyItem>();
                        if (componentInParent != null && !componentInParent.CanBeSelected)
                        {
                            hitInfo = default(RaycastHit);
                        }
                    }
                }
                pendingClickSelection = new DevkitSelection((hitInfo.transform != null) ? hitInfo.transform.gameObject : null, hitInfo.collider);
                if (pendingClickSelection.isValid)
                {
                    DevkitSelectionManager.data.point = hitInfo.point;
                }
                isDragging = flag;
                if (isDragging)
                {
                    handles.MouseDown(ray);
                    DevkitTransactionManager.beginTransaction("Transform");
                    foreach (DevkitSelection item2 in DevkitSelectionManager.selection)
                    {
                        DevkitTransactionUtility.recordObjectDelta(item2.transform);
                    }
                }
                else
                {
                    beginAreaSelect = MainCamera.instance.ScreenToViewportPoint(Input.mousePosition);
                    beginAreaSelectTime = Time.time;
                }
            }
            if (InputEx.GetKey(KeyCode.Mouse0) && !isDragging && HasBoxSelectableObjects() && !isAreaSelecting && Time.time - beginAreaSelectTime > 0.1f)
            {
                isAreaSelecting = true;
                areaSelection.Clear();
                if (!InputEx.GetKey(KeyCode.LeftShift) && !InputEx.GetKey(KeyCode.LeftControl))
                {
                    DevkitSelectionManager.clear();
                }
            }
            if (isDragging)
            {
                handles.snapPositionInterval = DevkitSelectionToolOptions.instance?.snapPosition ?? 1f;
                handles.snapRotationIntervalDegrees = DevkitSelectionToolOptions.instance?.snapRotation ?? 1f;
                handles.wantsToSnap = InputEx.GetKey(ControlsSettings.snap);
                handles.MouseMove(ray);
            }
            else if (InputEx.GetKeyDown(KeyCode.E))
            {
                Physics.Raycast(ray, out var hitInfo2, 8192f, (int)DevkitSelectionToolOptions.instance.selectionMask);
                if (hitInfo2.transform != null)
                {
                    if (DevkitSelectionManager.selection.Count > 0)
                    {
                        moveHandle(hitInfo2.point, Quaternion.identity, Vector3.one, doRotation: false, hasScale: false);
                    }
                    else
                    {
                        RequestInstantiation(hitInfo2.point);
                    }
                }
            }
            if (isAreaSelecting && HasBoxSelectableObjects())
            {
                Vector3 vector = MainCamera.instance.ScreenToViewportPoint(Input.mousePosition);
                Vector2 vector2 = default(Vector2);
                Vector2 vector3 = default(Vector2);
                if (vector.x < beginAreaSelect.x)
                {
                    vector2.x = vector.x;
                    vector3.x = beginAreaSelect.x;
                }
                else
                {
                    vector2.x = beginAreaSelect.x;
                    vector3.x = vector.x;
                }
                if (vector.y < beginAreaSelect.y)
                {
                    vector2.y = vector.y;
                    vector3.y = beginAreaSelect.y;
                }
                else
                {
                    vector2.y = beginAreaSelect.y;
                    vector3.y = vector.y;
                }
                foreach (GameObject item3 in EnumerateBoxSelectableObjects())
                {
                    if (item3 == null)
                    {
                        continue;
                    }
                    Vector3 vector4 = MainCamera.instance.WorldToViewportPoint(item3.transform.position);
                    DevkitSelection devkitSelection = new DevkitSelection(item3, null);
                    if (vector4.z > 0f && vector4.x > vector2.x && vector4.x < vector3.x && vector4.y > vector2.y && vector4.y < vector3.y)
                    {
                        if (!areaSelection.Contains(devkitSelection))
                        {
                            areaSelection.Add(devkitSelection);
                            DevkitSelectionManager.add(devkitSelection);
                        }
                    }
                    else if (areaSelection.Contains(devkitSelection))
                    {
                        areaSelection.Remove(devkitSelection);
                        DevkitSelectionManager.remove(devkitSelection);
                    }
                }
            }
            if (InputEx.GetKeyUp(KeyCode.Mouse0))
            {
                if (isDragging)
                {
                    handles.MouseUp();
                    pendingClickSelection = DevkitSelection.invalid;
                    isDragging = false;
                    transformSelection();
                    DevkitTransactionManager.endTransaction();
                }
                else if (isAreaSelecting)
                {
                    isAreaSelecting = false;
                }
                else
                {
                    DevkitSelectionManager.select(pendingClickSelection);
                }
            }
        }
        if (DevkitSelectionManager.selection.Count > 0)
        {
            if (mode == ESelectionMode.POSITION)
            {
                handles.SetPreferredMode(wantsBoundsEditor ? TransformHandles.EMode.PositionBounds : TransformHandles.EMode.Position);
            }
            else if (mode == ESelectionMode.SCALE)
            {
                handles.SetPreferredMode(wantsBoundsEditor ? TransformHandles.EMode.ScaleBounds : TransformHandles.EMode.Scale);
            }
            else
            {
                handles.SetPreferredMode(TransformHandles.EMode.Rotation);
            }
            bool flag2 = mode != ESelectionMode.SCALE && !wantsBoundsEditor && !DevkitSelectionToolOptions.instance.localSpace;
            handlePosition = Vector3.zero;
            handleRotation = Quaternion.identity;
            bool flag3 = flag2;
            foreach (DevkitSelection item4 in DevkitSelectionManager.selection)
            {
                if (!(item4.gameObject == null))
                {
                    handlePosition += item4.transform.position;
                    if (!flag3)
                    {
                        handleRotation = item4.transform.rotation;
                        flag3 = true;
                    }
                }
            }
            handlePosition /= (float)DevkitSelectionManager.selection.Count;
            handles.SetPreferredPivot(handlePosition, handleRotation);
            if (wantsBoundsEditor)
            {
                handles.UpdateBoundsFromSelection(EnumerateSelectedGameObjects());
            }
            if (InputEx.GetKeyDown(KeyCode.C))
            {
                copyBuffer.Clear();
                foreach (DevkitSelection item5 in DevkitSelectionManager.selection)
                {
                    copyBuffer.Add(item5.gameObject);
                }
            }
            if (InputEx.GetKeyDown(KeyCode.V))
            {
                DevkitTransactionManager.beginTransaction("Paste");
                foreach (GameObject item6 in copyBuffer)
                {
                    IDevkitSelectionCopyableHandler component = item6.GetComponent<IDevkitSelectionCopyableHandler>();
                    GameObject gameObject = ((component == null) ? Object.Instantiate(item6) : component.copySelection());
                    IDevkitHierarchyItem component2 = gameObject.GetComponent<IDevkitHierarchyItem>();
                    if (component2 != null)
                    {
                        component2.instanceID = LevelHierarchy.generateUniqueInstanceID();
                    }
                    DevkitTransactionUtility.recordInstantiation(gameObject);
                    copySelectionDelay.Add(gameObject);
                }
                DevkitTransactionManager.endTransaction();
            }
            if (InputEx.GetKeyDown(KeyCode.Delete))
            {
                DevkitTransactionManager.beginTransaction("Delete");
                foreach (DevkitSelection item7 in DevkitSelectionManager.selection)
                {
                    DevkitTransactionUtility.recordDestruction(item7.gameObject);
                }
                DevkitSelectionManager.clear();
                DevkitTransactionManager.endTransaction();
            }
            if (InputEx.GetKeyDown(KeyCode.B))
            {
                referencePosition = handlePosition;
                referenceRotation = handleRotation;
                hasReferenceRotation = !flag2;
                referenceScale = Vector3.one;
                hasReferenceScale = false;
                if (DevkitSelectionManager.selection.Count == 1)
                {
                    foreach (DevkitSelection item8 in DevkitSelectionManager.selection)
                    {
                        if (!(item8.gameObject == null))
                        {
                            referenceScale = item8.transform.localScale;
                            hasReferenceScale = true;
                        }
                    }
                }
            }
            if (InputEx.GetKeyDown(KeyCode.N))
            {
                moveHandle(referencePosition, referenceRotation, referenceScale, hasReferenceRotation, hasReferenceScale);
            }
        }
        if (InputEx.GetKeyDown(ControlsSettings.focus))
        {
            if (DevkitSelectionManager.selection.Count > 0)
            {
                MainCamera.instance.transform.parent.position = handlePosition - 15f * MainCamera.instance.transform.forward;
            }
            else
            {
                MainCamera.instance.transform.parent.position = MainCamera.instance.transform.forward * -512f;
            }
        }
    }

    public virtual void equip()
    {
        GLRenderer.render += handleGLRender;
        mode = ESelectionMode.POSITION;
        handles = new TransformHandles();
        handles.OnPreTransform += OnHandlePreTransform;
        handles.OnTranslatedAndRotated += OnHandleTranslatedAndRotated;
        handles.OnTransformed += OnHandleTransformed;
        DevkitSelectionManager.clear();
    }

    public virtual void dequip()
    {
        GLRenderer.render -= handleGLRender;
        DevkitSelectionManager.clear();
    }

    protected void handleGLRender()
    {
        if (isAreaSelecting)
        {
            GLUtility.LINE_FLAT_COLOR.SetPass(0);
            GL.Begin(1);
            GL.Color(Color.yellow);
            GLUtility.matrix = MathUtility.IDENTITY_MATRIX;
            Vector3 vector = beginAreaSelect;
            vector.z = 16f;
            Vector3 vector2 = MainCamera.instance.ScreenToViewportPoint(Input.mousePosition);
            vector2.z = 16f;
            Vector3 position = vector;
            position.x = vector2.x;
            Vector3 position2 = vector2;
            position2.x = vector.x;
            Vector3 v = MainCamera.instance.ViewportToWorldPoint(vector);
            Vector3 v2 = MainCamera.instance.ViewportToWorldPoint(position);
            Vector3 v3 = MainCamera.instance.ViewportToWorldPoint(position2);
            Vector3 v4 = MainCamera.instance.ViewportToWorldPoint(vector2);
            GL.Vertex(v);
            GL.Vertex(v2);
            GL.Vertex(v2);
            GL.Vertex(v4);
            GL.Vertex(v4);
            GL.Vertex(v3);
            GL.Vertex(v3);
            GL.Vertex(v);
            GL.End();
        }
    }
}
